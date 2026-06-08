using System;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using static New_Attenuator.AttenuatorControl;

namespace New_Attenuator
{
    public partial class Form1 : Form
    {
        private SerialPort serialPort = new SerialPort();
        private System.Windows.Forms.Timer sendTimer = new System.Windows.Forms.Timer();
        private ToolTip modeToolTip = new ToolTip();

        private bool isRunning = false;
        private int pendingCh = 0;
        private int pendingVal = 0;
        private const string ConfigVersion = "2.0";
        private bool hasPendingDeviceApply = false;
        public Form1()
        {
            InitializeComponent();
            UpdateWindowTitle();
            SetupModeToolTips();

        }

        private void UpdateWindowTitle()
        {
            string version = Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion
                ?? "1.0.0";

            Text = $"New_Attenuator {version}";
        }

        private void SetupModeToolTips()
        {
            modeToolTip.InitialDelay = 300;
            modeToolTip.AutoPopDelay = 10000;
            modeToolTip.ReshowDelay = 100;
            modeToolTip.ShowAlways = true;

            modeToolTip.SetToolTip(rbBasic1, "[2 AP] / 두 AP를 서로 반대 방향으로 감쇠시키는 교차 테스트입니다.");
            modeToolTip.SetToolTip(rbBasic2, "[1~N AP] / 선택된 AP를 하나씩 순서대로 스윕하는 테스트입니다.");
            modeToolTip.SetToolTip(rbBasic3, "[N AP] / 활성 AP 전체를 같은 값으로 동시에 올리고 내리는 테스트입니다.");
            modeToolTip.SetToolTip(rbTrans1, "[4 AP] / AP1/AP3와 AP2/AP4를 두 쌍으로 나눠 순차 전환합니다.");
            modeToolTip.SetToolTip(rbTrans2, "[4 AP] / 2개 AP 쌍을 동시에 교차 전환하는 테스트입니다.");
            modeToolTip.SetToolTip(rbTrans3, "[4 AP] / 4개 AP를 순서대로 하나씩 전환하는 테스트입니다.");
            modeToolTip.SetToolTip(rbTrans4, "[4 AP] / AP가 겹치며 다음 AP로 자연스럽게 넘어가는 테스트입니다.");
            modeToolTip.SetToolTip(rbStepHandover, "[4 AP] / AP를 한 칸씩 순차적으로 넘기는 테스트입니다.");
            modeToolTip.SetToolTip(rbPingPong, "[2 AP] / 두 AP 사이를 반복 전환하는 안정성 테스트입니다.");
            modeToolTip.SetToolTip(rbDiagonal, "[4 AP] / 비연속 AP로 건너뛰는 전환 경로 테스트입니다.");
            modeToolTip.SetToolTip(rbFailover, "[4 AP] / 주 AP 장애 시 백업 AP로 복구하는 장애 대응 테스트입니다.");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // 포트 목록 불러오기
            grpBand.Enabled = false;
            grpMode.Enabled = false;
            grpBtn.Enabled = false;

            // Enable Ant 콤보박스 아이템 초기화 및 기본값 설정
            cboEnableAnt.Items.Clear();
            cboEnableAnt.Items.AddRange(new string[] { "1", "2", "3", "4", "5", "6" });
            cboEnableAnt.DropDownStyle = ComboBoxStyle.DropDownList; // 사용자가 임의의 글자를 타이핑하지 못하게 막음

            // 기본으로 4개가 모두 보이도록 인덱스 3("4") 선택
            cboEnableAnt.SelectedIndex = 3;

            string[] ports = SerialPort.GetPortNames();
            cboPort.Items.Clear();
            cboPort.Items.AddRange(ports);
            if (ports.Length > 0) cboPort.SelectedIndex = 0;

            // 시리얼 포트 기본 설정 (장비 스펙에 맞춤)
            serialPort.BaudRate = 115200;
            serialPort.DataBits = 8;
            serialPort.StopBits = StopBits.One;
            serialPort.Parity = Parity.None;
            serialPort.Handshake = Handshake.None;
            serialPort.ReadTimeout = 500; // 0.5초 안에 응답 없으면 넘어감 (멈춤 방지)

            // UI 초기화
            txtLow.Enabled = false;
            txtHigh.Enabled = false;
            txtStep.Enabled = false;
            txtTimeout.Enabled = false;

            // 기본 2.4GHz 선택
            rbBand24.Checked = true;
            ApplyPreset();

            // 감쇠기 이벤트 연결 (AttenuatorControl이 있다고 가정)
            // attr1, attr2 등이 없으면 이 부분은 주석 처리하거나 맞춰주세요.
            if (attr1 != null) attr1.AttenuatorChanged += (s, v) => SendCommand(v.Channel, v.Value);
            if (attr2 != null) attr2.AttenuatorChanged += (s, v) => SendCommand(v.Channel, v.Value);
            if (attr3 != null) attr3.AttenuatorChanged += (s, v) => SendCommand(v.Channel, v.Value);
            if (attr4 != null) attr4.AttenuatorChanged += (s, v) => SendCommand(v.Channel, v.Value);
            // attr3, attr4...
        }

        // 3. 연결 버튼
        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                if (serialPort.IsOpen)
                {
                    grpBand.Enabled = false;
                    grpMode.Enabled = false;
                    grpBtn.Enabled = false;
                    serialPort.Close();
                    btnConnect.Text = "Connection";
                    btnConnect.BackColor = SystemColors.Control;
                    cboPort.Enabled = true;
                }
                else
                {
                    grpBand.Enabled = true;
                    grpMode.Enabled = true;
                    grpBtn.Enabled = true;
                    btnStop.Enabled = false;
                    if (cboPort.SelectedItem == null) { MessageBox.Show("Select Com Port"); return; }
                    serialPort.PortName = cboPort.SelectedItem.ToString();
                    serialPort.Open();
                    btnConnect.Text = "Disconnect";
                    cboPort.Enabled = false;

                    if (hasPendingDeviceApply)
                    {
                        ApplyCurrentAttenuatorSettingsToDevice();
                        hasPendingDeviceApply = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection Error: " + ex.Message);
            }
        }

        // 4. Band 선택 및 프리셋 로직
        private void rbBand_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb != null && rb.Checked)
            {
                ApplyPreset();
            }
        }

        private void valEdit_CheckedChanged(object sender, EventArgs e)
        {
            bool canEdit = valEdit.Checked;
            txtLow.Enabled = canEdit;
            txtHigh.Enabled = canEdit;
            txtStep.Enabled = canEdit;
            txtTimeout.Enabled = canEdit;

            if (!canEdit) ApplyPreset();
        }

        private void ApplyPreset()
        {
            if (valEdit.Checked) return;

            txtLow.Text = "0";
            txtStep.Text = "1";
            txtTimeout.Text = "0"; // 0 = 무한

            if (rbBand24.Checked) txtHigh.Text = "40";
            else if (rbBand5.Checked) txtHigh.Text = "30";
            else if (rbBand6.Checked) txtHigh.Text = "50";
        }

        // 5. [핵심] Start 버튼 (자동화)
        private async void btnStart_Click(object sender, EventArgs e)
        {
            if (isRunning) return;

            try
            {
                // 1. 설정값 읽기
                int startVal = int.Parse(txtLow.Text);
                int endVal = int.Parse(txtHigh.Text);
                int stepVal = int.Parse(txtStep.Text);
                int durationSeconds = int.Parse(txtTimeout.Text);

                // 유효성 검사
                if (stepVal <= 0) { MessageBox.Show("Step size must be > 0"); return; }
                if (startVal > endVal) { MessageBox.Show("Low must be <= High"); return; }

                // 2. 실행 준비
                isRunning = true;
                ToggleUI(false);

                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                sw.Start();

                Console.WriteLine("=== Automation Started ===");

                // 3. 무한 반복 루프 (여기서는 계속 함수를 호출만 함)
                while (isRunning)
                {
                    // [핵심] 모드에 맞는 동작을 실행하는 함수 호출!
                    // 이 함수가 한 사이클(끝까지 갔다가 복귀)을 다 돌 때까지 기다림(await)
                    await RunAutoCycle(startVal, endVal, stepVal, durationSeconds, sw);

                    // 시간 제한 체크 (한 바퀴 돌고 나서도 시간이 오버됐는지 확인)
                    if (CheckTimeout(sw, durationSeconds)) break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                isRunning = false;
                ToggleUI(true);
                Console.WriteLine("=== Automation Stopped ===");
            }
        }

        // 6. Stop 버튼
        private void btnStop_Click(object sender, EventArgs e)
        {
            isRunning = false; // 깃발 내리기 -> 루프 즉시 중단
        }

        // 7. 명령 전송 함수 (*OPC? 적용됨)
        private void SendCommand(int ch, int val)
        {
            if (serialPort == null || !serialPort.IsOpen)
            {
                HandleDisconnection("Port is closed or disconnected.");
                return;
            }

            try
            {
                string channelStr = (ch == 0) ? "ALL" : ch.ToString();

                // (1) 명령 보내기
                string cmd = $"ATTN {channelStr} {val}\r\n";
                serialPort.Write(cmd);

                // (2) 확인 명령 보내기
                serialPort.Write("*OPC?\r\n");

                // (3) 응답 대기
                try
                {
                    string response = serialPort.ReadLine();
                }
                catch (TimeoutException)
                {
                    // 타임아웃은 정상적인 딜레이로 간주하고 무시
                }
            }
            catch (IOException ex)
            {
                // 장비 연결이 물리적으로 끊어졌을 때 (케이블 뽑힘 등)
                HandleDisconnection("Connection lost (Device unplugged): " + ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                // 포트가 예기치 않게 닫혔을 때
                HandleDisconnection("Serial port closed unexpectedly: " + ex.Message);
            }
            catch (Exception ex)
            {
                // 기타 치명적이지 않은 에러는 로그만 남김
                Console.WriteLine("Serial Error: " + ex.Message);
            }
        }

        // 8. UI 제어 보조 함수
        private void ToggleUI(bool enable)
        {
            btnStart.Enabled = enable;
            btnStop.Enabled = !enable;

            // 실행 중엔 설정 변경 불가
            valEdit.Enabled = enable;
            grpBand.Enabled = enable;
            grpMode.Enabled = enable;

            if (valEdit.Checked)
            {
                txtLow.Enabled = enable;
                txtHigh.Enabled = enable;
                txtStep.Enabled = enable;
                txtTimeout.Enabled = enable;
            }
        }

        private void UpdateAllSliders(int val)
        {
            // attr 객체가 있다면 값 업데이트
            if (attr1 != null) attr1.Value = val;
            if (attr2 != null) attr2.Value = val;
            if (attr3 != null) attr3.Value = val;
            if (attr4 != null) attr4.Value = val;
        }
        private void UpdateSliders(int v1, int v2)
        {
            if (attr1 != null) attr1.Value = v1;
            if (attr2 != null) attr2.Value = v2;
            // attr3, attr4는 필요하다면 v1이나 v2 중 하나를 따라가거나 0으로 설정
        }
        private bool CheckTimeout(System.Diagnostics.Stopwatch sw, int durationSeconds)
        {
            // 0이면 무한이므로 체크 안 함 (return false)
            if (durationSeconds > 0 && sw.Elapsed.TotalSeconds >= durationSeconds)
            {
                Console.WriteLine("[System] Time limit reached.");
                isRunning = false; // 깃발 내리기
                return true;       // "시간 다 됐어!" 라고 알려줌
            }
            return false;
        }
        // [새로운 보조 함수] 특정 번호(1~4)의 감쇠기 슬라이더 값만 업데이트
        private void UpdateAttr(int index, int value)
        {
            switch (index)
            {
                case 1: if (attr1 != null) attr1.Value = value; break;
                case 2: if (attr2 != null) attr2.Value = value; break;
                case 3: if (attr3 != null) attr3.Value = value; break;
                case 4: if (attr4 != null) attr4.Value = value; break;
                case 5: if (attr5 != null) attr5.Value = value; break;
                case 6: if (attr6 != null) attr6.Value = value; break;
            }
        }

        private int GetSelectedAntCount()
        {
            int antCount = 1;
            if (cboEnableAnt.InvokeRequired) // UI 스레드 안전 접근
            {
                cboEnableAnt.Invoke(new Action(() => int.TryParse(cboEnableAnt.SelectedItem?.ToString(), out antCount)));
            }
            else
            {
                int.TryParse(cboEnableAnt.SelectedItem?.ToString(), out antCount);
            }
            return antCount;
        }

        private List<int> GetActiveChannels()
        {
            int antCount = GetSelectedAntCount();
            List<int> activeChannels = new List<int>();

            if (antCount >= 1 && attr1 != null) activeChannels.Add(attr1.SelectedChannel);
            if (antCount >= 2 && attr2 != null) activeChannels.Add(attr2.SelectedChannel);
            if (antCount >= 3 && attr3 != null) activeChannels.Add(attr3.SelectedChannel);
            if (antCount >= 4 && attr4 != null) activeChannels.Add(attr4.SelectedChannel);
            if (antCount >= 5 && attr5 != null) activeChannels.Add(attr5.SelectedChannel);
            if (antCount >= 6 && attr6 != null) activeChannels.Add(attr6.SelectedChannel);

            return activeChannels;
        }

        private string NormalizeModeKey(string mode)
        {
            if (string.IsNullOrWhiteSpace(mode))
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder(mode.Length);
            foreach (char ch in mode)
            {
                if (char.IsLetterOrDigit(ch))
                {
                    builder.Append(char.ToLowerInvariant(ch));
                }
            }

            return builder.ToString();
        }

        private bool EnsureChannelCount(List<int> channels, int required, string modeName)
        {
            if (channels.Count < required)
            {
                MessageBox.Show($"{modeName} mode requires at least {required} active antennas.", "Mode Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void SetChannelValue(int channel, int attrIndex, int value)
        {
            SendCommand(channel, value);
            UpdateAttr(attrIndex, value);
        }

        private void SetActiveChannels(List<int> channels, int value)
        {
            for (int i = 0; i < channels.Count; i++)
            {
                SendCommand(channels[i], value);
                UpdateAttr(i + 1, value);
            }
        }

        private async Task RunAutoCycle(int start, int end, int step, int duration, System.Diagnostics.Stopwatch sw)
        {
            List<int> activeChannels = GetActiveChannels();
            int n = activeChannels.Count;
            if (n == 0)
            {
                return;
            }

            string modeKey = NormalizeModeKey(GetSelectedMode());

            switch (modeKey)
            {
                case "basic1":
                    if (!EnsureChannelCount(activeChannels, 2, "Basic1")) return;
                    await RunBasic1Async(activeChannels[0], activeChannels[1], start, end, step, duration, sw);
                    break;

                case "basic2":
                    await RunBasic2Async(activeChannels, start, end, step, duration, sw);
                    break;

                case "basic3":
                    await RunBasic3Async(activeChannels, start, end, step, duration, sw);
                    break;

                case "transform1":
                    if (!EnsureChannelCount(activeChannels, 4, "Transform1")) return;
                    await RunTransform1Async(activeChannels, start, end, step, duration, sw);
                    break;

                case "transform2":
                    if (!EnsureChannelCount(activeChannels, 4, "Transform2")) return;
                    await RunTransform2Async(activeChannels, start, end, step, duration, sw);
                    break;

                case "transform3":
                    if (!EnsureChannelCount(activeChannels, 4, "Transform3")) return;
                    await RunTransform3Async(activeChannels, start, end, step, duration, sw);
                    break;

                case "transform4":
                    if (!EnsureChannelCount(activeChannels, 4, "Transform4")) return;
                    await RunTransform4Async(activeChannels, start, end, step, duration, sw);
                    break;

                case "stephandover":
                    if (!EnsureChannelCount(activeChannels, 4, "Step Handover")) return;
                    await RunStepHandoverAsync(activeChannels, start, end, step, duration, sw);
                    break;

                case "pingponghandover":
                    if (!EnsureChannelCount(activeChannels, 2, "Ping-Pong Handover")) return;
                    await RunPingPongAsync(activeChannels, start, end, step, duration, sw);
                    break;

                case "diagonalhandover":
                    if (!EnsureChannelCount(activeChannels, 4, "Diagonal Handover")) return;
                    await RunDiagonalAsync(activeChannels, start, end, step, duration, sw);
                    break;

                case "failoverrecovery":
                    if (!EnsureChannelCount(activeChannels, 2, "Failover Recovery")) return;
                    await RunFailoverRecoveryAsync(activeChannels, start, end, step, duration, sw);
                    break;

                default:
                    await RunDefaultSweepAsync(start, end, step, duration, sw);
                    break;
            }
        }

        private async Task RunBasic1Async(int ch1, int ch2, int start, int end, int step, int duration, System.Diagnostics.Stopwatch sw)
        {
            for (int i = start; i <= end; i += step)
            {
                if (!isRunning || CheckTimeout(sw, duration)) return;

                int valA = end - (i - start);
                int valB = i;

                SendCommand(ch1, valA);
                SendCommand(ch2, valB);
                UpdateSliders(valA, valB);

                Console.WriteLine($"[Basic1] Ch{ch1}:{valA}, Ch{ch2}:{valB}");
                await Task.Delay(1000);
            }

            if (isRunning && !CheckTimeout(sw, duration))
            {
                await Task.Delay(5000);
            }

            for (int i = end - step; i >= start; i -= step)
            {
                if (!isRunning || CheckTimeout(sw, duration)) return;

                int valA = end - (i - start);
                int valB = i;

                SendCommand(ch1, valA);
                SendCommand(ch2, valB);
                UpdateSliders(valA, valB);

                Console.WriteLine($"[Basic1 Return] Ch{ch1}:{valA}, Ch{ch2}:{valB}");
                await Task.Delay(1000);
            }

            if (isRunning && !CheckTimeout(sw, duration))
            {
                await Task.Delay(5000);
            }
        }

        private async Task RunBasic2Async(List<int> activeChannels, int start, int end, int step, int duration, System.Diagnostics.Stopwatch sw)
        {
            Console.WriteLine($"[Basic2] {activeChannels.Count}-AP Sequential Sweep Start");

            SetActiveChannels(activeChannels, start);
            await Task.Delay(1000);

            for (int currentIdx = 0; currentIdx < activeChannels.Count; currentIdx++)
            {
                int ch = activeChannels[currentIdx];
                Console.WriteLine($"[Basic2] Ch{ch} (Index {currentIdx + 1}) start");

                for (int val = start; val <= end; val += step)
                {
                    if (!isRunning || CheckTimeout(sw, duration)) return;

                    SendCommand(ch, val);
                    UpdateAttr(currentIdx + 1, val);
                    await Task.Delay(1000);
                }

                if (isRunning && !CheckTimeout(sw, duration))
                {
                    await Task.Delay(10000);
                }

                SendCommand(ch, start);
                UpdateAttr(currentIdx + 1, start);
                await Task.Delay(1000);
            }
        }

        private async Task RunBasic3Async(List<int> activeChannels, int start, int end, int step, int duration, System.Diagnostics.Stopwatch sw)
        {
            Console.WriteLine("[Basic3] Group sweep start");

            SetActiveChannels(activeChannels, start);
            await Task.Delay(1000);

            for (int val = start; val <= end; val += step)
            {
                if (!isRunning || CheckTimeout(sw, duration)) return;

                SetActiveChannels(activeChannels, val);
                Console.WriteLine($"[Basic3] ALL active channels: {val}");
                await Task.Delay(1000);
            }

            if (isRunning && !CheckTimeout(sw, duration))
            {
                await Task.Delay(5000);
            }

            for (int val = end - step; val >= start; val -= step)
            {
                if (!isRunning || CheckTimeout(sw, duration)) return;

                SetActiveChannels(activeChannels, val);
                Console.WriteLine($"[Basic3 Return] ALL active channels: {val}");
                await Task.Delay(1000);
            }

            if (isRunning && !CheckTimeout(sw, duration))
            {
                await Task.Delay(5000);
            }
        }

        private async Task RunTransform1Async(List<int> activeChannels, int start, int end, int step, int duration, System.Diagnostics.Stopwatch sw)
        {
            int ch1 = activeChannels[0];
            int ch2 = activeChannels[1];
            int ch3 = activeChannels[2];
            int ch4 = activeChannels[3];

            SetChannelValue(ch1, 1, start);
            SetChannelValue(ch2, 2, start);
            SetChannelValue(ch3, 3, start);
            SetChannelValue(ch4, 4, start);
            await Task.Delay(2000);

            Console.WriteLine("[Transform1] Pair 1: AP1/AP3 fade out");
            for (int val = start; val <= end; val += step)
            {
                if (!isRunning || CheckTimeout(sw, duration)) return;

                SetChannelValue(ch1, 1, val);
                SetChannelValue(ch3, 3, val);
                await Task.Delay(1000);
            }

            if (isRunning && !CheckTimeout(sw, duration))
            {
                await Task.Delay(5000);
            }

            SetChannelValue(ch1, 1, start);
            SetChannelValue(ch3, 3, start);

            Console.WriteLine("[Transform1] Pair 2: AP2/AP4 fade out");
            for (int val = start; val <= end; val += step)
            {
                if (!isRunning || CheckTimeout(sw, duration)) return;

                SetChannelValue(ch2, 2, val);
                SetChannelValue(ch4, 4, val);
                await Task.Delay(1000);
            }

            if (isRunning && !CheckTimeout(sw, duration))
            {
                await Task.Delay(5000);
            }
        }

        private async Task RunTransform2Async(List<int> activeChannels, int start, int end, int step, int duration, System.Diagnostics.Stopwatch sw)
        {
            int ch1 = activeChannels[0];
            int ch2 = activeChannels[1];
            int ch3 = activeChannels[2];
            int ch4 = activeChannels[3];

            SetChannelValue(ch1, 1, start);
            SetChannelValue(ch2, 2, end);
            SetChannelValue(ch3, 3, start);
            SetChannelValue(ch4, 4, end);
            await Task.Delay(2000);

            Console.WriteLine("[Transform2] Pair cross fade start");
            for (int val = start; val <= end; val += step)
            {
                if (!isRunning || CheckTimeout(sw, duration)) return;

                int valOut = val;
                int valIn = end - (val - start);

                SetChannelValue(ch1, 1, valOut);
                SetChannelValue(ch2, 2, valIn);
                SetChannelValue(ch3, 3, valOut);
                SetChannelValue(ch4, 4, valIn);
                await Task.Delay(1000);
            }

            if (isRunning && !CheckTimeout(sw, duration))
            {
                await Task.Delay(5000);
            }

            Console.WriteLine("[Transform2] Pair cross fade return");
            for (int val = start; val <= end; val += step)
            {
                if (!isRunning || CheckTimeout(sw, duration)) return;

                int valOut = val;
                int valIn = end - (val - start);

                SetChannelValue(ch1, 1, valIn);
                SetChannelValue(ch2, 2, valOut);
                SetChannelValue(ch3, 3, valIn);
                SetChannelValue(ch4, 4, valOut);
                await Task.Delay(1000);
            }

            if (isRunning && !CheckTimeout(sw, duration))
            {
                await Task.Delay(5000);
            }
        }

        private async Task RunTransform3Async(List<int> activeChannels, int start, int end, int step, int duration, System.Diagnostics.Stopwatch sw)
        {
            int ch1 = activeChannels[0];
            int ch2 = activeChannels[1];
            int ch3 = activeChannels[2];
            int ch4 = activeChannels[3];

            SetChannelValue(ch1, 1, start);
            SetChannelValue(ch2, 2, end);
            SetChannelValue(ch3, 3, end);
            SetChannelValue(ch4, 4, end);
            await Task.Delay(2000);

            int[] channels = { ch1, ch2, ch3, ch4 };
            for (int idx = 0; idx < channels.Length; idx++)
            {
                if (!isRunning || CheckTimeout(sw, duration)) return;

                int currentAttrIndex = idx + 1;
                int currentChannel = channels[idx];
                int nextAttrIndex = (idx + 1) % channels.Length + 1;
                int nextChannel = channels[(idx + 1) % channels.Length];

                Console.WriteLine($"[Transform3] AP{currentAttrIndex} roaming phase");
                for (int val = start; val <= end; val += step)
                {
                    if (!isRunning || CheckTimeout(sw, duration)) return;

                    SendCommand(currentChannel, val);
                    UpdateAttr(currentAttrIndex, val);
                    await Task.Delay(1000);
                }

                if (isRunning && !CheckTimeout(sw, duration))
                {
                    await Task.Delay(5000);
                }

                SendCommand(currentChannel, end);
                UpdateAttr(currentAttrIndex, end);
                SendCommand(nextChannel, start);
                UpdateAttr(nextAttrIndex, start);
            }
        }

        private async Task RunTransform4Async(List<int> activeChannels, int start, int end, int step, int duration, System.Diagnostics.Stopwatch sw)
        {
            int ch1 = activeChannels[0];
            int ch2 = activeChannels[1];
            int ch3 = activeChannels[2];
            int ch4 = activeChannels[3];

            SetChannelValue(ch1, 1, start);
            SetChannelValue(ch2, 2, end);
            SetChannelValue(ch3, 3, end);
            SetChannelValue(ch4, 4, end);
            await Task.Delay(2000);

            Console.WriteLine("[Transform4] Smooth roaming phase 1");
            for (int val = start; val <= end; val += step)
            {
                if (!isRunning || CheckTimeout(sw, duration)) return;

                int valOut = val;
                int valIn = end - (val - start);
                SetChannelValue(ch1, 1, valOut);
                SetChannelValue(ch2, 2, valIn);
                await Task.Delay(1000);
            }

            if (isRunning && !CheckTimeout(sw, duration))
            {
                await Task.Delay(5000);
            }

            Console.WriteLine("[Transform4] Smooth roaming phase 2");
            for (int val = start; val <= end; val += step)
            {
                if (!isRunning || CheckTimeout(sw, duration)) return;

                int valOut = val;
                int valIn = end - (val - start);
                SetChannelValue(ch2, 2, valOut);
                SetChannelValue(ch3, 3, valIn);
                await Task.Delay(1000);
            }

            if (isRunning && !CheckTimeout(sw, duration))
            {
                await Task.Delay(5000);
            }

            Console.WriteLine("[Transform4] Smooth roaming phase 3");
            for (int val = start; val <= end; val += step)
            {
                if (!isRunning || CheckTimeout(sw, duration)) return;

                int valOut = val;
                int valIn = end - (val - start);
                SetChannelValue(ch3, 3, valOut);
                SetChannelValue(ch4, 4, valIn);
                await Task.Delay(1000);
            }

            if (isRunning && !CheckTimeout(sw, duration))
            {
                await Task.Delay(5000);
            }

            Console.WriteLine("[Transform4] Smooth roaming phase 4");
            for (int val = start; val <= end; val += step)
            {
                if (!isRunning || CheckTimeout(sw, duration)) return;

                int valOut = val;
                int valIn = end - (val - start);
                SetChannelValue(ch4, 4, valOut);
                SetChannelValue(ch1, 1, valIn);
                await Task.Delay(1000);
            }

            if (isRunning && !CheckTimeout(sw, duration))
            {
                await Task.Delay(5000);
            }
        }

        private async Task RunStepHandoverAsync(List<int> activeChannels, int start, int end, int step, int duration, System.Diagnostics.Stopwatch sw)
        {
            int ch1 = activeChannels[0];
            int ch2 = activeChannels[1];
            int ch3 = activeChannels[2];
            int ch4 = activeChannels[3];

            SetChannelValue(ch1, 1, start);
            SetChannelValue(ch2, 2, end);
            SetChannelValue(ch3, 3, end);
            SetChannelValue(ch4, 4, end);
            await Task.Delay(1500);

            Console.WriteLine("[StepHandover] AP1 -> AP2 -> AP3 -> AP4");
            int[] channels = { ch1, ch2, ch3, ch4 };
            for (int idx = 0; idx < channels.Length; idx++)
            {
                if (!isRunning || CheckTimeout(sw, duration)) return;

                int currentAttrIndex = idx + 1;
                int currentChannel = channels[idx];
                int nextAttrIndex = (idx + 1) % channels.Length + 1;
                int nextChannel = channels[(idx + 1) % channels.Length];

                SendCommand(currentChannel, end);
                UpdateAttr(currentAttrIndex, end);
                await Task.Delay(700);

                SendCommand(nextChannel, start);
                UpdateAttr(nextAttrIndex, start);
                await Task.Delay(1200);
            }
        }

        private async Task RunPingPongAsync(List<int> activeChannels, int start, int end, int step, int duration, System.Diagnostics.Stopwatch sw)
        {
            int ch1 = activeChannels[0];
            int ch2 = activeChannels[1];

            SetChannelValue(ch1, 1, start);
            SetChannelValue(ch2, 2, end);
            await Task.Delay(1500);

            Console.WriteLine("[PingPong] AP1 <-> AP2");
            while (isRunning && !CheckTimeout(sw, duration))
            {
                for (int val = start; val <= end; val += step)
                {
                    if (!isRunning || CheckTimeout(sw, duration)) return;

                    int valOut = val;
                    int valIn = end - (val - start);
                    SetChannelValue(ch1, 1, valOut);
                    SetChannelValue(ch2, 2, valIn);
                    await Task.Delay(1000);
                }

                if (isRunning && !CheckTimeout(sw, duration))
                {
                    await Task.Delay(3000);
                }

                for (int val = start; val <= end; val += step)
                {
                    if (!isRunning || CheckTimeout(sw, duration)) return;

                    int valOut = val;
                    int valIn = end - (val - start);
                    SetChannelValue(ch1, 1, valIn);
                    SetChannelValue(ch2, 2, valOut);
                    await Task.Delay(1000);
                }

                if (isRunning && !CheckTimeout(sw, duration))
                {
                    await Task.Delay(3000);
                }
            }
        }

        private async Task RunDiagonalAsync(List<int> activeChannels, int start, int end, int step, int duration, System.Diagnostics.Stopwatch sw)
        {
            int ch1 = activeChannels[0];
            int ch2 = activeChannels[1];
            int ch3 = activeChannels[2];
            int ch4 = activeChannels[3];

            SetChannelValue(ch1, 1, start);
            SetChannelValue(ch2, 2, end);
            SetChannelValue(ch3, 3, end);
            SetChannelValue(ch4, 4, end);
            await Task.Delay(1500);

            int[] pathChannels = { ch1, ch3, ch2, ch4 };
            int[] pathAttrs = { 1, 3, 2, 4 };

            Console.WriteLine("[Diagonal] AP1 -> AP3 -> AP2 -> AP4");
            for (int idx = 0; idx < pathChannels.Length; idx++)
            {
                if (!isRunning || CheckTimeout(sw, duration)) return;

                int currentChannel = pathChannels[idx];
                int currentAttrIndex = pathAttrs[idx];
                int nextChannel = pathChannels[(idx + 1) % pathChannels.Length];
                int nextAttrIndex = pathAttrs[(idx + 1) % pathAttrs.Length];

                for (int val = start; val <= end; val += step)
                {
                    if (!isRunning || CheckTimeout(sw, duration)) return;

                    int valOut = val;
                    int valIn = end - (val - start);
                    SetChannelValue(currentChannel, currentAttrIndex, valOut);
                    SetChannelValue(nextChannel, nextAttrIndex, valIn);
                    await Task.Delay(1000);
                }

                if (isRunning && !CheckTimeout(sw, duration))
                {
                    await Task.Delay(4000);
                }
            }
        }

        private async Task RunFailoverRecoveryAsync(List<int> activeChannels, int start, int end, int step, int duration, System.Diagnostics.Stopwatch sw)
        {
            int ch1 = activeChannels[0];
            int ch2 = activeChannels[1];
            int ch3 = activeChannels[2];
            int ch4 = activeChannels[3];

            SetChannelValue(ch1, 1, start);
            SetChannelValue(ch2, 2, end);
            SetChannelValue(ch3, 3, end);
            SetChannelValue(ch4, 4, end);
            await Task.Delay(1500);

            int[] primaryOrder = { ch1, ch2, ch3, ch4 };
            Console.WriteLine("[Failover] Primary to backup recovery sequence");

            for (int idx = 0; idx < primaryOrder.Length; idx++)
            {
                if (!isRunning || CheckTimeout(sw, duration)) return;

                int currentChannel = primaryOrder[idx];
                int currentAttrIndex = idx + 1;
                int nextChannel = primaryOrder[(idx + 1) % primaryOrder.Length];
                int nextAttrIndex = (idx + 1) % primaryOrder.Length + 1;

                SendCommand(currentChannel, end);
                UpdateAttr(currentAttrIndex, end);
                await Task.Delay(10000);

                SendCommand(nextChannel, start);
                UpdateAttr(nextAttrIndex, start);
                await Task.Delay(10000);
            }
        }

        private async Task RunDefaultSweepAsync(int start, int end, int step, int duration, System.Diagnostics.Stopwatch sw)
        {
            for (int i = start; i <= end; i += step)
            {
                if (!isRunning || CheckTimeout(sw, duration)) return;

                SendCommand(0, i);
                UpdateAllSliders(i);
                Console.WriteLine($"[Normal Sweep] ALL: {i}");
                await Task.Delay(1000);
            }
        }

        private void cboPort_DropDown(object sender, EventArgs e)
        {
            // 기존에 선택된 포트 이름 기억
            string currentSelection = cboPort.SelectedItem?.ToString();

            // 현재 PC에 연결된 포트 다시 불러오기
            string[] ports = SerialPort.GetPortNames();
            cboPort.Items.Clear();
            cboPort.Items.AddRange(ports);

            // 이전에 선택했던 포트가 여전히 존재하면 다시 선택, 없으면 첫 번째 항목 선택
            if (!string.IsNullOrEmpty(currentSelection) && cboPort.Items.Contains(currentSelection))
            {
                cboPort.SelectedItem = currentSelection;
            }
            else if (cboPort.Items.Count > 0)
            {
                cboPort.SelectedIndex = 0;
            }
        }

        private void HandleDisconnection(string reason)
        {
            // UI 스레드 접근 오류 방지 (InvokeRequired 체크)
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => HandleDisconnection(reason)));
                return;
            }

            Console.WriteLine($"[Error] {reason}");

            isRunning = false; // 자동화 루프 강제 종료
            ToggleUI(true);    // UI 잠금 해제

            // 포트가 열려있다면 강제로 닫기 시도
            if (serialPort != null && serialPort.IsOpen)
            {
                try { serialPort.Close(); } catch { }
            }

            // 연결 버튼 및 UI 초기화
            btnConnect.Text = "Connection";
            btnConnect.BackColor = SystemColors.Control;
            cboPort.Enabled = true;

            grpBand.Enabled = false;
            grpMode.Enabled = false;
            grpBtn.Enabled = false;

            MessageBox.Show(reason, "Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private string GetSettingRootDirectory()
        {
            DirectoryInfo? dir = new DirectoryInfo(AppContext.BaseDirectory);

            while (dir != null)
            {
                if (File.Exists(Path.Combine(dir.FullName, "New_Attenuator.slnx")) ||
                    File.Exists(Path.Combine(dir.FullName, "New_Attenuator.sln")))
                {
                    return Path.Combine(dir.FullName, "New_Attenuator");
                }

                dir = dir.Parent;
            }

            return AppContext.BaseDirectory;
        }

        private void btnSaveConfig_Click(object sender, EventArgs e)
        {
            using SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "INI files (*.ini)|*.ini|All files (*.*)|*.*",
                DefaultExt = "ini",
                InitialDirectory = GetSettingRootDirectory(),
                FileName = $"attenuator_setting_{DateTime.Now:yyyyMMddHHmm}.ini",
                Title = "Save environment setting"
            };

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            try
            {
                var ini = new IniFile();
                SaveCurrentSettings(ini);
                ini.Save(dialog.FileName);
                MessageBox.Show($"Environment setting saved.\n{dialog.FileName}", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Save failed: " + ex.Message, "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnLoadConfig_Click(object sender, EventArgs e)
        {
            using OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "INI files (*.ini)|*.ini|All files (*.*)|*.*",
                Title = "Load environment setting",
                InitialDirectory = GetSettingRootDirectory()
            };

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            try
            {
                var ini = IniFile.Load(dialog.FileName);
                LoadSettings(ini);
                hasPendingDeviceApply = true;

                if (serialPort != null && serialPort.IsOpen)
                {
                    ApplyCurrentAttenuatorSettingsToDevice();
                    hasPendingDeviceApply = false;
                    MessageBox.Show("Environment setting loaded and applied.", "Load", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Environment setting loaded. Connect the attenuator to apply values.", "Load", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Load failed: " + ex.Message, "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveCurrentSettings(IniFile ini)
        {
            ini.Write("Profile", "ConfigVersion", ConfigVersion);
            ini.Write("Profile", "Name", "DQA_Environment");

            ini.Write("Serial", "Port", cboPort.SelectedItem?.ToString() ?? "");

            ini.Write("Test", "Band", GetSelectedBand());
            ini.Write("Test", "Mode", GetSelectedMode());
            ini.Write("Test", "ModeText", GetSelectedModeText());
            ini.WriteBool("Test", "OverrideParameter", valEdit.Checked);
            ini.Write("Test", "EnableAnt", cboEnableAnt.SelectedItem?.ToString() ?? "4");
            ini.Write("Test", "Low", txtLow.Text);
            ini.Write("Test", "High", txtHigh.Text);
            ini.Write("Test", "Step", txtStep.Text);
            ini.Write("Test", "Timeout", txtTimeout.Text);

            SaveAttenuator(ini, 1, attr1);
            SaveAttenuator(ini, 2, attr2);
            SaveAttenuator(ini, 3, attr3);
            SaveAttenuator(ini, 4, attr4);
            SaveAttenuator(ini, 5, attr5);
            SaveAttenuator(ini, 6, attr6);
        }

        private void LoadSettings(IniFile ini)
        {
            string version = ini.Read("Profile", "ConfigVersion", ConfigVersion);
            if (version != "1.0" && version != ConfigVersion)
            {
                throw new InvalidOperationException($"Unsupported config version: {version}");
            }

            SetComboBoxSelectedText(cboPort, ini.Read("Serial", "Port", cboPort.SelectedItem?.ToString() ?? ""));
            SetBand(ini.Read("Test", "Band", GetSelectedBand()));
            string modeValue = ini.Read("Test", "Mode", ini.Read("Test", "ModeText", GetSelectedMode()));
            SetMode(modeValue);

            valEdit.Checked = ini.ReadBool("Test", "OverrideParameter", valEdit.Checked);
            SetComboBoxSelectedText(cboEnableAnt, ini.Read("Test", "EnableAnt", cboEnableAnt.SelectedItem?.ToString() ?? "4"));

            txtLow.Text = ini.Read("Test", "Low", txtLow.Text);
            txtHigh.Text = ini.Read("Test", "High", txtHigh.Text);
            txtStep.Text = ini.Read("Test", "Step", txtStep.Text);
            txtTimeout.Text = ini.Read("Test", "Timeout", txtTimeout.Text);

            LoadAttenuator(ini, 1, attr1);
            LoadAttenuator(ini, 2, attr2);
            LoadAttenuator(ini, 3, attr3);
            LoadAttenuator(ini, 4, attr4);
            LoadAttenuator(ini, 5, attr5);
            LoadAttenuator(ini, 6, attr6);
        }

        private void SaveAttenuator(IniFile ini, int index, AttenuatorControl control)
        {
            string section = $"Attenuator{index}";
            ini.WriteInt(section, "Channel", control.SelectedChannel);
            ini.WriteInt(section, "Value", control.Value);
        }

        private void LoadAttenuator(IniFile ini, int index, AttenuatorControl control)
        {
            string section = $"Attenuator{index}";
            control.SelectedChannel = Clamp(ini.ReadInt(section, "Channel", control.SelectedChannel), 0, 12);
            control.Value = Clamp(ini.ReadInt(section, "Value", control.Value), 0, 95);
        }

        private void ApplyCurrentAttenuatorSettingsToDevice()
        {
            int antCount = 4;
            if (!int.TryParse(cboEnableAnt.SelectedItem?.ToString(), out antCount))
            {
                antCount = 4;
            }

            if (antCount >= 1) SendCommand(attr1.SelectedChannel, attr1.Value);
            if (antCount >= 2) SendCommand(attr2.SelectedChannel, attr2.Value);
            if (antCount >= 3) SendCommand(attr3.SelectedChannel, attr3.Value);
            if (antCount >= 4) SendCommand(attr4.SelectedChannel, attr4.Value);
            if (antCount >= 5) SendCommand(attr5.SelectedChannel, attr5.Value);
            if (antCount >= 6) SendCommand(attr6.SelectedChannel, attr6.Value);
        }

        private string GetSelectedBand()
        {
            if (rbBand5.Checked) return "5GHz";
            if (rbBand6.Checked) return "6GHz";
            return "2.4GHz";
        }

        private void SetBand(string band)
        {
            rbBand24.Checked = false;
            rbBand5.Checked = false;
            rbBand6.Checked = false;

            if (band == "5GHz") rbBand5.Checked = true;
            else if (band == "6GHz") rbBand6.Checked = true;
            else rbBand24.Checked = true;
        }

        private string GetSelectedMode()
        {
            if (rbBasic2.Checked) return "Basic2";
            if (rbBasic3.Checked) return "Basic3";
            if (rbTrans1.Checked) return "Transform1";
            if (rbTrans2.Checked) return "Transform2";
            if (rbTrans3.Checked) return "Transform3";
            if (rbTrans4.Checked) return "Transform4";
            if (rbStepHandover.Checked) return "StepHandover";
            if (rbPingPong.Checked) return "PingPongHandover";
            if (rbDiagonal.Checked) return "DiagonalHandover";
            if (rbFailover.Checked) return "FailoverRecovery";
            return "Basic1";
        }

        private string GetSelectedModeText()
        {
            if (rbBasic1.Checked) return rbBasic1.Text;
            if (rbBasic2.Checked) return rbBasic2.Text;
            if (rbBasic3.Checked) return rbBasic3.Text;
            if (rbTrans1.Checked) return rbTrans1.Text;
            if (rbTrans2.Checked) return rbTrans2.Text;
            if (rbTrans3.Checked) return rbTrans3.Text;
            if (rbTrans4.Checked) return rbTrans4.Text;
            if (rbStepHandover.Checked) return rbStepHandover.Text;
            if (rbPingPong.Checked) return rbPingPong.Text;
            if (rbDiagonal.Checked) return rbDiagonal.Text;
            if (rbFailover.Checked) return rbFailover.Text;
            return rbBasic1.Text;
        }

        private void SetMode(string mode)
        {
            rbBasic1.Checked = false;
            rbBasic2.Checked = false;
            rbBasic3.Checked = false;
            rbTrans1.Checked = false;
            rbTrans2.Checked = false;
            rbTrans3.Checked = false;
            rbTrans4.Checked = false;
            rbStepHandover.Checked = false;
            rbPingPong.Checked = false;
            rbDiagonal.Checked = false;
            rbFailover.Checked = false;

            string normalized = NormalizeModeKey(mode);

            if (normalized == "basic2") rbBasic2.Checked = true;
            else if (normalized == "basic3") rbBasic3.Checked = true;
            else if (normalized == "transform1") rbTrans1.Checked = true;
            else if (normalized == "transform2") rbTrans2.Checked = true;
            else if (normalized == "transform3") rbTrans3.Checked = true;
            else if (normalized == "transform4") rbTrans4.Checked = true;
            else if (normalized == "stephandover") rbStepHandover.Checked = true;
            else if (normalized == "pingponghandover") rbPingPong.Checked = true;
            else if (normalized == "diagonalhandover") rbDiagonal.Checked = true;
            else if (normalized == "failoverrecovery") rbFailover.Checked = true;
            else rbBasic1.Checked = true;
        }

        private void SetComboBoxSelectedText(ComboBox comboBox, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            if (!comboBox.Items.Contains(value))
            {
                comboBox.Items.Add(value);
            }

            comboBox.SelectedItem = value;
        }

        private int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
        private void cboEnableAnt_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 콤보박스에서 선택된 텍스트를 숫자로 변환 시도
            if (int.TryParse(cboEnableAnt.SelectedItem?.ToString(), out int antCount))
            {
                // 1. 선택한 숫자에 맞춰 감쇠기 컨트롤의 Visible 속성 켜고 끄기
                if (attr1 != null) attr1.Visible = (antCount >= 1);
                if (attr2 != null) attr2.Visible = (antCount >= 2);
                if (attr3 != null) attr3.Visible = (antCount >= 3);
                if (attr4 != null) attr4.Visible = (antCount >= 4);
                if (attr5 != null) attr5.Visible = (antCount >= 5);
                if (attr6 != null) attr6.Visible = (antCount >= 6);

                // 2. 폼 사이즈(너비) 동적 조절 로직
                int paddingRight = 20; // 우측 끝에 약간의 여백(Margin)을 줍니다.
                int newWidth = this.ClientSize.Width;

                // 가장 우측에 배치되는(보이는) 감쇠기의 Right 속성(x좌표 + 너비)을 기준으로 폼 너비 계산
                if (antCount == 6 && attr6 != null) newWidth = attr6.Right + paddingRight;
                else if (antCount == 5 && attr5 != null) newWidth = attr5.Right + paddingRight;
                else if (antCount == 4 && attr4 != null) newWidth = attr4.Right + paddingRight;
                else if (antCount == 3 && attr3 != null) newWidth = attr3.Right + paddingRight;
                else if (antCount == 2 && attr2 != null) newWidth = attr2.Right + paddingRight;
                else if (antCount == 1 && attr1 != null) newWidth = attr1.Right + paddingRight;

                // Form의 내부 그리기 영역(ClientSize)을 변경. 높이는 기존 높이 유지.
                this.ClientSize = new Size(newWidth, this.ClientSize.Height);
            }
        }

    }
}
