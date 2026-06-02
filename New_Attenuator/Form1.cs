using System;
using System.Drawing;
using System.IO;
using System.IO.Ports;
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
        private const string ConfigVersion = "1.0";
        private bool hasPendingDeviceApply = false;
        public Form1()
        {
            InitializeComponent();
            SetupModeToolTips();

        }

        private void SetupModeToolTips()
        {
            modeToolTip.InitialDelay = 300;
            modeToolTip.AutoPopDelay = 10000;
            modeToolTip.ReshowDelay = 100;
            modeToolTip.ShowAlways = true;

            modeToolTip.SetToolTip(rbBasic1, " AP 2개를 교차 제어합니다. 한쪽 감쇠 값은 증가하고 다른 쪽은 감소한 뒤 반대로 복귀합니다.");
            modeToolTip.SetToolTip(rbBasic2, " 선택된 안테나 채널을 순서대로 하나씩 start 값에서 end 값까지 스윕합니다.");
            modeToolTip.SetToolTip(rbBasic3, " 현재 자동 제어 동작이 구현되어 있지 않습니다.");
            modeToolTip.SetToolTip(rbTrans1, " AP 1/3 그룹과 AP 2/4 그룹을 차례로 약화시키는 전환 테스트 모드입니다.");
            modeToolTip.SetToolTip(rbTrans2, " 현재 자동 제어 동작이 구현되어 있지 않습니다.");
            modeToolTip.SetToolTip(rbTrans3, " 4개 AP를 순차적으로 전환하며 로밍 상황을 반복 테스트합니다.");
            modeToolTip.SetToolTip(rbTrans4, " AP 1에서 시작해 AP 2, AP 3, AP 4 방향으로 순차 전환하는 로밍 테스트 모드입니다.");
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

        private async Task RunAutoCycle(int start, int end, int step, int duration, System.Diagnostics.Stopwatch sw)
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

            // 2. 동작할 실제 채널 리스트 구성 (attr 객체가 살아있고 표시되는 것만)
            List<int> activeChannels = new List<int>();
            if (antCount >= 1 && attr1 != null) activeChannels.Add(attr1.SelectedChannel);
            if (antCount >= 2 && attr2 != null) activeChannels.Add(attr2.SelectedChannel);
            if (antCount >= 3 && attr3 != null) activeChannels.Add(attr3.SelectedChannel);
            if (antCount >= 4 && attr4 != null) activeChannels.Add(attr4.SelectedChannel);
            if (antCount >= 5 && attr5 != null) activeChannels.Add(attr5.SelectedChannel);
            if (antCount >= 6 && attr6 != null) activeChannels.Add(attr6.SelectedChannel);

            int n = activeChannels.Count;
            if (n == 0) return;

            // [핵심 변경] 하드코딩(1, 2) 대신, UI에서 선택된 채널 번호를 가져옵니다.
            // attr1, attr2가 null이 아닌지 체크하고 가져옵니다 (없으면 기본값 1, 2)
            int ch1 = (attr1 != null) ? attr1.SelectedChannel : 1;
            int ch2 = (attr2 != null) ? attr2.SelectedChannel : 2;
            int ch3 = (attr3 != null) ? attr3.SelectedChannel : 3;
            int ch4 = (attr4 != null) ? attr4.SelectedChannel : 4;
            int ch5 = (attr5 != null) ? attr5.SelectedChannel : 5;
            int ch6 = (attr6 != null) ? attr6.SelectedChannel : 6;

            if (rbBasic1.Checked)
            {
                // [PHASE 1] 교차 진행 (A: 감소 / B: 증가)
                for (int i = start; i <= end; i += step)
                {
                    if (!isRunning || CheckTimeout(sw, duration)) return;

                    int valA = end - (i - start); // Max -> Low (감소)
                    int valB = i;                 // Low -> Max (증가)

                    // 선택된 채널로 전송!
                    SendCommand(ch1, valA);
                    SendCommand(ch2, valB);

                    // 화면 슬라이더도 같이 움직여줌
                    UpdateSliders(valA, valB);

                    Console.WriteLine($"[Basic1 Step1] Ch{ch1}:{valA}, Ch{ch2}:{valB}");
                    await Task.Delay(1000);
                }

                if (isRunning && !CheckTimeout(sw, duration))
                {
                    await Task.Delay(5000);
                }

                // [PHASE 2] 원위치 복귀 (A: 증가 / B: 감소)
                for (int i = end - step; i >= start; i -= step)
                {
                    if (!isRunning || CheckTimeout(sw, duration)) return;

                    int valA = end - (i - start); // Low -> Max (증가)
                    int valB = i;                 // Max -> Low (감소)

                    SendCommand(ch1, valA);
                    SendCommand(ch2, valB);

                    UpdateSliders(valA, valB);

                    Console.WriteLine($"[Basic1 Step2] Ch{ch1}:{valA}, Ch{ch2}:{valB}");
                    await Task.Delay(1000);
                }

                if (isRunning && !CheckTimeout(sw, duration))
                {
                    await Task.Delay(5000);
                }
            }
            else if (rbBasic2.Checked)
            {
                Console.WriteLine($"[Basic2] {n}-AP Sequential Sweep Start");

                // 전체 채널 초기화 (모두 Start 값으로 대기)
                for (int i = 0; i < n; i++)
                {
                    SendCommand(activeChannels[i], start);
                    UpdateAttr(i + 1, start); // 슬라이더 UI는 1번부터 매핑됨
                }
                await Task.Delay(1000);

                // 활성화된 채널 개수(n)만큼 순서대로 스윕 진행
                for (int currentIdx = 0; currentIdx < n; currentIdx++)
                {
                    int ch = activeChannels[currentIdx];
                    Console.WriteLine($"[Basic2] Ch{ch} (Index {currentIdx + 1}) 동작 시작");

                    // 현재 채널 Sweep
                    for (int val = start; val <= end; val += step)
                    {
                        if (!isRunning || CheckTimeout(sw, duration)) return;

                        SendCommand(ch, val);
                        UpdateAttr(currentIdx + 1, val);

                        await Task.Delay(1000);
                    }

                    if (isRunning)
                    {
                        Console.WriteLine($">> Ch{ch} Max 도달! 5초 대기");
                        await Task.Delay(10000);
                    }

                    // 동작 끝난 채널은 다시 원래 상태(start)로 복귀
                    SendCommand(ch, start);
                    UpdateAttr(currentIdx + 1, start);
                    await Task.Delay(1000);
                }
            }
            // 모드 1
            else if (rbTrans1.Checked)
            {
                // [초기 상태 설정] AP1,3 Strong(start), AP2, 4는 Weak(end)
                SendCommand(ch1, start); UpdateAttr(1, start);
                SendCommand(ch2, start); UpdateAttr(2, start);
                SendCommand(ch3, start); UpdateAttr(3, start);
                SendCommand(ch4, start); UpdateAttr(4, start);
                await Task.Delay(2000); // 초기화 안정 시간

                Console.WriteLine("[Trans4] Phase 1: AP1↗ AP2, AP3, AP4 start");
                for (int i = start; i <= end; i += step)
                {
                    if (!isRunning || CheckTimeout(sw, duration)) return;
                    int valOut = i;                 // start -> end (약해짐)

                    SendCommand(ch1, valOut); UpdateAttr(1, valOut); // AP1 Out
                    SendCommand(ch3, valOut); UpdateAttr(3, valOut); // AP3 Out

                    await Task.Delay(1000);
                }
                if (isRunning) { Console.WriteLine(">> [Hold] Handover 완료. 5초 대기"); await Task.Delay(5000); }

                SendCommand(ch1, start); UpdateAttr(1, start);
                SendCommand(ch3, start); UpdateAttr(3, start);

                Console.WriteLine("[Trans4] Phase 1: AP1↗ AP2, AP3, AP4 start");
                for (int i = start; i <= end; i += step)
                {
                    if (!isRunning || CheckTimeout(sw, duration)) return;
                    int valOut = i;                 // start -> end (약해짐)

                    SendCommand(ch2, valOut); UpdateAttr(2, valOut); // AP2 Out
                    SendCommand(ch4, valOut); UpdateAttr(4, valOut); // AP4 Out

                    await Task.Delay(1000);
                }
                if (isRunning) { Console.WriteLine(">> [Hold] Handover 완료. 5초 대기"); await Task.Delay(5000); }
            }
            else if (rbTrans2.Checked)
            {

            }
            // 모드 3
            else if (rbTrans3.Checked)
            {
                Console.WriteLine(">>> [Trans4] 4-AP Roaming Cycle Start");

                // [초기 상태 설정] AP1만 Strong(start), 나머지는 Weak(end)
                SendCommand(ch1, start); UpdateAttr(1, start);
                SendCommand(ch2, start); UpdateAttr(2, start);
                SendCommand(ch3, start); UpdateAttr(3, start);
                SendCommand(ch4, start); UpdateAttr(4, start);
                await Task.Delay(2000); // 초기화 안정 시간

                // --- PHASE 1: AP1(Fade Out) -> AP2(Fade In) ---
                Console.WriteLine("[Trans4] Phase 1: AP1↗ AP2, AP3, AP4 start");
                for (int i = start; i <= end; i += step)
                {
                    if (!isRunning || CheckTimeout(sw, duration)) return;
                    int valOut = i;                 // start -> end (약해짐)

                    SendCommand(ch1, valOut); UpdateAttr(1, valOut); // AP1 Out

                    await Task.Delay(1000);
                }
                if (isRunning) { Console.WriteLine(">> [Hold] Handover 완료. 5초 대기"); await Task.Delay(5000); }

                Console.WriteLine("[Trans4] Phase 2: AP2↗ AP1, AP3, AP4 start");
                SendCommand(ch1, start); UpdateAttr(1, start);
                SendCommand(ch3, start); UpdateAttr(3, start);
                SendCommand(ch4, start); UpdateAttr(4, start);
                for (int i = start; i <= end; i += step)
                {
                    if (!isRunning || CheckTimeout(sw, duration)) return;
                    int valOut = i;

                    SendCommand(ch2, valOut); UpdateAttr(2, valOut); // AP2 Out

                    await Task.Delay(1000);
                }
                if (isRunning) { Console.WriteLine(">> [Hold] Handover 완료. 5초 대기"); await Task.Delay(5000); }


                // --- PHASE 3: AP3(Fade Out) -> AP4(Fade In) ---
                Console.WriteLine("[Trans4] Phase 3: AP3↗ AP1, AP2, AP4 start");
                SendCommand(ch1, start); UpdateAttr(1, start);
                SendCommand(ch2, start); UpdateAttr(2, start);
                SendCommand(ch4, start); UpdateAttr(4, start);
                for (int i = start; i <= end; i += step)
                {
                    if (!isRunning || CheckTimeout(sw, duration)) return;
                    int valOut = i;

                    SendCommand(ch3, valOut); UpdateAttr(3, valOut); // AP3 Out

                    await Task.Delay(1000);
                }
                if (isRunning) { Console.WriteLine(">> [Hold] Handover 완료. 5초 대기"); await Task.Delay(5000); }


                // --- PHASE 4: AP4(Fade Out) -> AP1(Fade In) [Loop Back] ---
                Console.WriteLine("[Trans4] Phase 4: AP4↗ AP1, AP2, AP3 start");
                SendCommand(ch1, start); UpdateAttr(1, start);
                SendCommand(ch2, start); UpdateAttr(2, start);
                SendCommand(ch3, start); UpdateAttr(3, start);
                for (int i = start; i <= end; i += step)
                {
                    if (!isRunning || CheckTimeout(sw, duration)) return;
                    int valOut = i;

                    SendCommand(ch4, valOut); UpdateAttr(4, valOut); // AP4 Out

                    await Task.Delay(1000);
                }
                if (isRunning) { Console.WriteLine(">> [Hold] Cycle 완료 (AP1 Strong 복귀). 5초 대기"); await Task.Delay(5000); }
            }
            // ========================================================
            // [모드 4] Transform 4: 4-AP Sequential Roaming (순차 중첩 로밍)
            // (AP1->AP2->AP3->AP4->AP1 반복)
            // ========================================================
            else if (rbTrans4.Checked)
            {
                Console.WriteLine(">>> [Trans4] 4-AP Roaming Cycle Start");

                // [초기 상태 설정] AP1만 Strong(start), 나머지는 Weak(end)
                SendCommand(ch1, start); UpdateAttr(1, start);
                SendCommand(ch2, end); UpdateAttr(2, end);
                SendCommand(ch3, end); UpdateAttr(3, end);
                SendCommand(ch4, end); UpdateAttr(4, end);
                await Task.Delay(2000); // 초기화 안정 시간

                // --- PHASE 1: AP1(Fade Out) -> AP2(Fade In) ---
                Console.WriteLine("[Trans4] Phase 1: AP1↘ AP2↗ (AP3,4 Max)");
                for (int i = start; i <= end; i += step)
                {
                    if (!isRunning || CheckTimeout(sw, duration)) return;
                    int valOut = i;                 // start -> end (약해짐)
                    int valIn = end - (i - start);  // end -> start (강해짐)

                    SendCommand(ch1, valOut); UpdateAttr(1, valOut); // AP1 Out
                    SendCommand(ch2, valIn); UpdateAttr(2, valIn);  // AP2 In
                                                                    // AP3, AP4는 이미 Max 상태 유지

                    await Task.Delay(1000);
                }
                if (isRunning) { Console.WriteLine(">> [Hold] Handover 완료. 5초 대기"); await Task.Delay(5000); }


                // --- PHASE 2: AP2(Fade Out) -> AP3(Fade In) ---
                Console.WriteLine("[Trans4] Phase 2: AP2↘ AP3↗ (AP1,4 Max)");
                for (int i = start; i <= end; i += step)
                {
                    if (!isRunning || CheckTimeout(sw, duration)) return;
                    int valOut = i;
                    int valIn = end - (i - start);

                    SendCommand(ch2, valOut); UpdateAttr(2, valOut); // AP2 Out
                    SendCommand(ch3, valIn); UpdateAttr(3, valIn);  // AP3 In
                                                                    // AP1(이미 Max), AP4(계속 Max) 유지

                    await Task.Delay(1000);
                }
                if (isRunning) { Console.WriteLine(">> [Hold] Handover 완료. 5초 대기"); await Task.Delay(5000); }


                // --- PHASE 3: AP3(Fade Out) -> AP4(Fade In) ---
                Console.WriteLine("[Trans4] Phase 3: AP3↘ AP4↗ (AP1,2 Max)");
                for (int i = start; i <= end; i += step)
                {
                    if (!isRunning || CheckTimeout(sw, duration)) return;
                    int valOut = i;
                    int valIn = end - (i - start);

                    SendCommand(ch3, valOut); UpdateAttr(3, valOut); // AP3 Out
                    SendCommand(ch4, valIn); UpdateAttr(4, valIn);  // AP4 In
                                                                    // AP1, AP2 Max 유지

                    await Task.Delay(1000);
                }
                if (isRunning) { Console.WriteLine(">> [Hold] Handover 완료. 5초 대기"); await Task.Delay(5000); }


                // --- PHASE 4: AP4(Fade Out) -> AP1(Fade In) [Loop Back] ---
                Console.WriteLine("[Trans4] Phase 4: AP4↘ AP1↗ (AP2,3 Max)");
                for (int i = start; i <= end; i += step)
                {
                    if (!isRunning || CheckTimeout(sw, duration)) return;
                    int valOut = i;
                    int valIn = end - (i - start);

                    SendCommand(ch4, valOut); UpdateAttr(4, valOut); // AP4 Out
                    SendCommand(ch1, valIn); UpdateAttr(1, valIn);  // AP1 In (다시 강해짐)
                                                                    // AP2, AP3 Max 유지

                    await Task.Delay(1000);
                }
                if (isRunning) { Console.WriteLine(">> [Hold] Cycle 완료 (AP1 Strong 복귀). 5초 대기"); await Task.Delay(5000); }
            }
            // 2. 그 외 (Normal Sweep Mode)
            else
            {
                // 단순 증가 (0 -> Max)
                for (int i = start; i <= end; i += step)
                {
                    if (!isRunning || CheckTimeout(sw, duration)) return;

                    // 일반 모드는 "Channel ALL(0)"로 보낼지, 아니면 각자 채널로 보낼지 결정해야 함.
                    // 일단은 'ALL(0)'로 전체 제어하도록 유지합니다.
                    SendCommand(0, i);
                    UpdateAllSliders(i);

                    Console.WriteLine($"[Normal Sweep] ALL: {i}");
                    await Task.Delay(1000);
                }
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
            if (version != ConfigVersion)
            {
                throw new InvalidOperationException($"Unsupported config version: {version}");
            }

            SetComboBoxSelectedText(cboPort, ini.Read("Serial", "Port", cboPort.SelectedItem?.ToString() ?? ""));
            SetBand(ini.Read("Test", "Band", GetSelectedBand()));
            SetMode(ini.Read("Test", "Mode", GetSelectedMode()));

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
            return "Basic1";
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

            if (mode == "Basic2") rbBasic2.Checked = true;
            else if (mode == "Basic3") rbBasic3.Checked = true;
            else if (mode == "Transform1") rbTrans1.Checked = true;
            else if (mode == "Transform2") rbTrans2.Checked = true;
            else if (mode == "Transform3") rbTrans3.Checked = true;
            else if (mode == "Transform4") rbTrans4.Checked = true;
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
