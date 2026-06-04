using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace New_Attenuator
{
    public partial class AttenuatorControl : UserControl
    {
        public event EventHandler<AttenuatorEventArgs> AttenuatorChanged;

        public AttenuatorControl()
        {
            InitializeComponent();

            cboChannel.Items.Clear();
            cboChannel.Items.Add($"Channel ALL");
            for (int i = 1; i <= 12; i++)
            {
                cboChannel.Items.Add($"Channel {i}");
            }
            cboChannel.SelectedIndex = 0; // 기본값: Channel ALL

            // 2. 이벤트 연결
            tbValue.Scroll += TbValue_Scroll;       // 바 움직일 때
            cboChannel.SelectedIndexChanged += CboChannel_SelectedIndexChanged;

            // [추가] 텍스트박스 이벤트 연결
            // 엔터키를 쳤을 때
            txtValue.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    ApplyTextBoxValue();        // 값 적용
                    e.SuppressKeyPress = true;  // '띵' 소리 방지
                }
            };

            // 다른 곳을 클릭해서 포커스가 나갔을 때
            txtValue.Leave += (s, e) => ApplyTextBoxValue();
        }

        public string Title
        {
            get { return grpBox.Text; }
            set { grpBox.Text = value; }
        }

        public int SelectedChannel
        {
            get { return cboChannel.SelectedIndex; }
            set
            {
                if (value >= 0 && value < cboChannel.Items.Count)
                    cboChannel.SelectedIndex = value;
            }
        }

        // 현재 감쇠 값 (0 ~ 95)
        public int Value
        {
            get { return tbValue.Value; }
            set
            {
                if (value >= tbValue.Minimum && value <= tbValue.Maximum)
                {
                    tbValue.Value = value;
                    UpdateLabel(); // [중요] 값이 바뀌면 텍스트박스도 갱신
                }
            }
        }

        private void TbValue_Scroll(object sender, EventArgs e)
        {
            UpdateLabel();  // 트랙바 움직이면 텍스트박스 숫자 변경
            NotifyChange(); // 메인 화면에 알림
        }

        private void CboChannel_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 채널 변경 시 필요하다면 알림 (선택사항)
            // NotifyChange(); 
        }

        // [수정] 트랙바 값을 텍스트박스에 표시
        private void UpdateLabel()
        {
            // 기존: txtValue.Text = "0"; (버그)
            // 수정: 현재 트랙바의 값을 문자로 변환해서 넣음
            txtValue.Text = tbValue.Value.ToString();
        }

        // [추가] 텍스트박스 값을 읽어서 트랙바에 적용하는 로직
        private void ApplyTextBoxValue()
        {
            // 숫자로 변환 시도
            if (int.TryParse(txtValue.Text, out int newValue))
            {
                // 범위 제한 (Minimum ~ Maximum)
                if (newValue > tbValue.Maximum) newValue = tbValue.Maximum;
                if (newValue < tbValue.Minimum) newValue = tbValue.Minimum;

                // 값이 실제로 다를 때만 업데이트 (무한 루프 방지)
                if (tbValue.Value != newValue)
                {
                    tbValue.Value = newValue;
                    NotifyChange(); // 메인으로 전송!
                }

                // 텍스트박스도 깔끔하게 다시 정리 (예: 999 입력 -> 95로 변환됨)
                txtValue.Text = newValue.ToString();
            }
            else
            {
                // 숫자가 아닌 걸 입력하면 원래 값으로 복구
                txtValue.Text = tbValue.Value.ToString();
            }
        }

        // 메인 화면으로 신호 쏘기
        private void NotifyChange()
        {
            if (AttenuatorChanged != null)
            {
                AttenuatorChanged(this, new AttenuatorEventArgs(SelectedChannel, tbValue.Value));
            }
        }

        public class AttenuatorEventArgs : EventArgs
        {
            public int Channel { get; }
            public int Value { get; }

            public AttenuatorEventArgs(int ch, int val)
            {
                Channel = ch;
                Value = val;
            }
        }

        private void InitializeComponent()
        {
            grpBox = new System.Windows.Forms.GroupBox();
            label2 = new Label();
            txtValue = new TextBox();
            tbValue = new TrackBar();
            label1 = new Label();
            cboChannel = new ComboBox();
            grpBox.SuspendLayout();
            ((ISupportInitialize)tbValue).BeginInit();
            SuspendLayout();
            // 
            // grpBox
            // 
            grpBox.Controls.Add(label2);
            grpBox.Controls.Add(txtValue);
            grpBox.Controls.Add(tbValue);
            grpBox.Controls.Add(label1);
            grpBox.Controls.Add(cboChannel);
            grpBox.Location = new Point(3, 3);
            grpBox.Name = "grpBox";
            grpBox.Size = new Size(166, 515);
            grpBox.TabIndex = 0;
            grpBox.TabStop = false;
            grpBox.Text = "Attenuator";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(18, 485);
            label2.Name = "label2";
            label2.Size = new Size(56, 20);
            label2.TabIndex = 4;
            label2.Text = "Value :";
            // 
            // txtValue
            // 
            txtValue.Location = new Point(80, 482);
            txtValue.Name = "txtValue";
            txtValue.Size = new Size(63, 27);
            txtValue.TabIndex = 3;
            // 
            // tbValue
            // 
            tbValue.Location = new Point(50, 80);
            tbValue.Maximum = 95;
            tbValue.Name = "tbValue";
            tbValue.Orientation = Orientation.Vertical;
            tbValue.Size = new Size(56, 396);
            tbValue.TabIndex = 2;
            tbValue.TickStyle = TickStyle.Both;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 23);
            label1.Name = "label1";
            label1.Size = new Size(133, 20);
            label1.TabIndex = 1;
            label1.Text = "Channel Selection";
            // 
            // cboChannel
            // 
            cboChannel.FormattingEnabled = true;
            cboChannel.Location = new Point(6, 46);
            cboChannel.Name = "cboChannel";
            cboChannel.Size = new Size(144, 28);
            cboChannel.TabIndex = 0;
            // 
            // AttenuatorControl
            // 
            Controls.Add(grpBox);
            Name = "AttenuatorControl";
            Size = new Size(172, 521);
            grpBox.ResumeLayout(false);
            grpBox.PerformLayout();
            ((ISupportInitialize)tbValue).EndInit();
            ResumeLayout(false);

        }
    }
}
