namespace New_Attenuator
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            groupBox1 = new GroupBox();
            label1 = new Label();
            cboPort = new ComboBox();
            btnConnect = new Button();
            grpMode = new GroupBox();
            pnlMode = new FlowLayoutPanel();
            rbFailover = new RadioButton();
            rbDiagonal = new RadioButton();
            rbPingPong = new RadioButton();
            rbStepHandover = new RadioButton();
            rbTrans4 = new RadioButton();
            rbTrans3 = new RadioButton();
            rbBasic3 = new RadioButton();
            rbTrans2 = new RadioButton();
            rbBasic2 = new RadioButton();
            rbTrans1 = new RadioButton();
            rbBasic1 = new RadioButton();
            grpBtn = new GroupBox();
            btnStop = new Button();
            btnStart = new Button();
            valEdit = new CheckBox();
            grpEdit = new GroupBox();
            label6 = new Label();
            txtStep = new TextBox();
            label7 = new Label();
            label4 = new Label();
            txtTimeout = new TextBox();
            txtHigh = new TextBox();
            txtLow = new TextBox();
            label5 = new Label();
            attr1 = new AttenuatorControl();
            attr2 = new AttenuatorControl();
            attr3 = new AttenuatorControl();
            attr4 = new AttenuatorControl();
            groupBox2 = new GroupBox();
            cboEnableAnt = new ComboBox();
            label9 = new Label();
            attr5 = new AttenuatorControl();
            attr6 = new AttenuatorControl();
            grpConfig = new GroupBox();
            btnLoadConfig = new Button();
            btnSaveConfig = new Button();
            groupBox1.SuspendLayout();
            grpMode.SuspendLayout();
            pnlMode.SuspendLayout();
            grpBtn.SuspendLayout();
            grpEdit.SuspendLayout();
            groupBox2.SuspendLayout();
            grpConfig.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(cboPort);
            groupBox1.Controls.Add(btnConnect);
            groupBox1.Location = new Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(300, 70);
            groupBox1.TabIndex = 2;
            groupBox1.TabStop = false;
            groupBox1.Text = "Serial Setup";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 29);
            label1.Name = "label1";
            label1.Size = new Size(74, 20);
            label1.TabIndex = 3;
            label1.Text = "Com Port";
            // 
            // cboPort
            // 
            cboPort.FormattingEnabled = true;
            cboPort.Location = new Point(86, 26);
            cboPort.Name = "cboPort";
            cboPort.Size = new Size(102, 28);
            cboPort.TabIndex = 4;
            cboPort.DropDown += cboPort_DropDown;
            // 
            // btnConnect
            // 
            btnConnect.Location = new Point(194, 25);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(97, 29);
            btnConnect.TabIndex = 3;
            btnConnect.Text = "Connection";
            btnConnect.UseVisualStyleBackColor = true;
            btnConnect.Click += btnConnect_Click;
            // 
            // grpMode
            // 
            grpMode.AutoSize = true;
            grpMode.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            grpMode.Controls.Add(pnlMode);
            grpMode.Location = new Point(12, 88);
            grpMode.Name = "grpMode";
            grpMode.Padding = new Padding(8);
            grpMode.TabIndex = 5;
            grpMode.TabStop = false;
            grpMode.Text = "Select Mode";
            // 
            // pnlMode
            // 
            pnlMode.AutoSize = true;
            pnlMode.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            pnlMode.Controls.Add(rbBasic1);
            pnlMode.Controls.Add(rbBasic2);
            pnlMode.Controls.Add(rbBasic3);
            pnlMode.Controls.Add(rbTrans1);
            pnlMode.Controls.Add(rbTrans2);
            pnlMode.Controls.Add(rbTrans3);
            pnlMode.Controls.Add(rbTrans4);
            pnlMode.Controls.Add(rbStepHandover);
            pnlMode.Controls.Add(rbPingPong);
            pnlMode.Controls.Add(rbDiagonal);
            pnlMode.Controls.Add(rbFailover);
            pnlMode.FlowDirection = FlowDirection.TopDown;
            pnlMode.Location = new Point(11, 27);
            pnlMode.Margin = new Padding(0);
            pnlMode.Name = "pnlMode";
            pnlMode.Size = new Size(171, 293);
            pnlMode.TabIndex = 18;
            pnlMode.WrapContents = false;
            // 
            // rbFailover
            // 
            rbFailover.AutoSize = true;
            rbFailover.Location = new Point(3, 276);
            rbFailover.Margin = new Padding(3, 0, 3, 4);
            rbFailover.Name = "rbFailover";
            rbFailover.Size = new Size(169, 24);
            rbFailover.TabIndex = 10;
            rbFailover.Text = "Failover Recovery Test";
            rbFailover.UseVisualStyleBackColor = true;
            // 
            // rbDiagonal
            // 
            rbDiagonal.AutoSize = true;
            rbDiagonal.Location = new Point(3, 250);
            rbDiagonal.Margin = new Padding(3, 0, 3, 4);
            rbDiagonal.Name = "rbDiagonal";
            rbDiagonal.Size = new Size(150, 24);
            rbDiagonal.TabIndex = 9;
            rbDiagonal.Text = "Diagonal Handover Test";
            rbDiagonal.UseVisualStyleBackColor = true;
            // 
            // rbPingPong
            // 
            rbPingPong.AutoSize = true;
            rbPingPong.Location = new Point(3, 224);
            rbPingPong.Margin = new Padding(3, 0, 3, 4);
            rbPingPong.Name = "rbPingPong";
            rbPingPong.Size = new Size(163, 24);
            rbPingPong.TabIndex = 8;
            rbPingPong.Text = "Ping-Pong Handover Test";
            rbPingPong.UseVisualStyleBackColor = true;
            // 
            // rbStepHandover
            // 
            rbStepHandover.AutoSize = true;
            rbStepHandover.Location = new Point(3, 198);
            rbStepHandover.Margin = new Padding(3, 0, 3, 4);
            rbStepHandover.Name = "rbStepHandover";
            rbStepHandover.Size = new Size(133, 24);
            rbStepHandover.TabIndex = 7;
            rbStepHandover.Text = "Step Handover Test";
            rbStepHandover.UseVisualStyleBackColor = true;
            // 
            // rbTrans4
            // 
            rbTrans4.AutoSize = true;
            rbTrans4.Location = new Point(3, 172);
            rbTrans4.Margin = new Padding(3, 0, 3, 4);
            rbTrans4.Name = "rbTrans4";
            rbTrans4.Size = new Size(111, 24);
            rbTrans4.TabIndex = 6;
            rbTrans4.Text = "Smooth Handover Test";
            rbTrans4.UseVisualStyleBackColor = true;
            // 
            // rbTrans3
            // 
            rbTrans3.AutoSize = true;
            rbTrans3.Location = new Point(3, 146);
            rbTrans3.Margin = new Padding(3, 0, 3, 4);
            rbTrans3.Name = "rbTrans3";
            rbTrans3.Size = new Size(111, 24);
            rbTrans3.TabIndex = 5;
            rbTrans3.Text = "Sequential Roaming Test";
            rbTrans3.UseVisualStyleBackColor = true;
            // 
            // rbBasic3
            // 
            rbBasic3.AutoSize = true;
            rbBasic3.Location = new Point(3, 120);
            rbBasic3.Margin = new Padding(3, 0, 3, 4);
            rbBasic3.Name = "rbBasic3";
            rbBasic3.Size = new Size(77, 24);
            rbBasic3.TabIndex = 4;
            rbBasic3.Text = "Group Sweep Test";
            rbBasic3.UseVisualStyleBackColor = true;
            // 
            // rbTrans2
            // 
            rbTrans2.AutoSize = true;
            rbTrans2.Location = new Point(3, 94);
            rbTrans2.Margin = new Padding(3, 0, 3, 4);
            rbTrans2.Name = "rbTrans2";
            rbTrans2.Size = new Size(111, 24);
            rbTrans2.TabIndex = 3;
            rbTrans2.Text = "Parallel Pair Handover Test";
            rbTrans2.UseVisualStyleBackColor = true;
            // 
            // rbBasic2
            // 
            rbBasic2.AutoSize = true;
            rbBasic2.Location = new Point(3, 68);
            rbBasic2.Margin = new Padding(3, 0, 3, 4);
            rbBasic2.Name = "rbBasic2";
            rbBasic2.Size = new Size(77, 24);
            rbBasic2.TabIndex = 2;
            rbBasic2.Text = "Sequential Sweep Test";
            rbBasic2.UseVisualStyleBackColor = true;
            // 
            // rbTrans1
            // 
            rbTrans1.AutoSize = true;
            rbTrans1.Location = new Point(3, 42);
            rbTrans1.Margin = new Padding(3, 0, 3, 4);
            rbTrans1.Name = "rbTrans1";
            rbTrans1.Size = new Size(111, 24);
            rbTrans1.TabIndex = 1;
            rbTrans1.Text = "Dual Pair Handover Test";
            rbTrans1.UseVisualStyleBackColor = true;
            // 
            // rbBasic1
            // 
            rbBasic1.AutoSize = true;
            rbBasic1.Checked = true;
            rbBasic1.Location = new Point(3, 16);
            rbBasic1.Margin = new Padding(3, 0, 3, 4);
            rbBasic1.Name = "rbBasic1";
            rbBasic1.Size = new Size(77, 24);
            rbBasic1.TabIndex = 0;
            rbBasic1.TabStop = true;
            rbBasic1.Text = "Cross Fade Test";
            rbBasic1.UseVisualStyleBackColor = true;
            // 
            // grpBtn
            // 
            grpBtn.Controls.Add(btnStop);
            grpBtn.Controls.Add(btnStart);
            grpBtn.Controls.Add(valEdit);
            grpBtn.Location = new Point(12, 494);
            grpBtn.Name = "grpBtn";
            grpBtn.Size = new Size(300, 62);
            grpBtn.TabIndex = 8;
            grpBtn.TabStop = false;
            // 
            // btnStop
            // 
            btnStop.Location = new Point(231, 17);
            btnStop.Name = "btnStop";
            btnStop.Size = new Size(62, 36);
            btnStop.TabIndex = 9;
            btnStop.Text = "Stop";
            btnStop.UseVisualStyleBackColor = true;
            btnStop.Click += btnStop_Click;
            // 
            // btnStart
            // 
            btnStart.Location = new Point(163, 17);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(62, 36);
            btnStart.TabIndex = 1;
            btnStart.Text = "Start";
            btnStart.UseVisualStyleBackColor = true;
            btnStart.Click += btnStart_Click;
            // 
            // valEdit
            // 
            valEdit.AutoSize = true;
            valEdit.Font = new Font("맑은 고딕", 8F);
            valEdit.Location = new Point(6, 26);
            valEdit.Name = "valEdit";
            valEdit.Size = new Size(152, 23);
            valEdit.TabIndex = 0;
            valEdit.Text = "Override parameter";
            valEdit.UseVisualStyleBackColor = true;
            valEdit.CheckedChanged += valEdit_CheckedChanged;
            // 
            // grpEdit
            // 
            grpEdit.Controls.Add(label6);
            grpEdit.Controls.Add(txtStep);
            grpEdit.Controls.Add(label7);
            grpEdit.Controls.Add(label4);
            grpEdit.Controls.Add(txtTimeout);
            grpEdit.Controls.Add(txtHigh);
            grpEdit.Controls.Add(txtLow);
            grpEdit.Controls.Add(label5);
            grpEdit.Location = new Point(12, 562);
            grpEdit.Name = "grpEdit";
            grpEdit.Size = new Size(300, 78);
            grpEdit.TabIndex = 9;
            grpEdit.TabStop = false;
            grpEdit.Text = "Edit value";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("맑은 고딕", 7.5F);
            label6.Location = new Point(158, 20);
            label6.Name = "label6";
            label6.Size = new Size(61, 17);
            label6.TabIndex = 2;
            label6.Text = "Step size";
            // 
            // txtStep
            // 
            txtStep.Location = new Point(154, 40);
            txtStep.Name = "txtStep";
            txtStep.Size = new Size(68, 27);
            txtStep.TabIndex = 12;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("맑은 고딕", 7.5F);
            label7.Location = new Point(228, 20);
            label7.Name = "label7";
            label7.Size = new Size(61, 17);
            label7.TabIndex = 3;
            label7.Text = "Time out";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("맑은 고딕", 7.5F);
            label4.Location = new Point(6, 20);
            label4.Name = "label4";
            label4.Size = new Size(68, 17);
            label4.TabIndex = 0;
            label4.Text = "Low value";
            // 
            // txtTimeout
            // 
            txtTimeout.Location = new Point(228, 40);
            txtTimeout.Name = "txtTimeout";
            txtTimeout.Size = new Size(68, 27);
            txtTimeout.TabIndex = 11;
            // 
            // txtHigh
            // 
            txtHigh.Location = new Point(80, 40);
            txtHigh.Name = "txtHigh";
            txtHigh.Size = new Size(68, 27);
            txtHigh.TabIndex = 10;
            // 
            // txtLow
            // 
            txtLow.Location = new Point(6, 40);
            txtLow.Name = "txtLow";
            txtLow.Size = new Size(68, 27);
            txtLow.TabIndex = 4;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("맑은 고딕", 7.5F);
            label5.Location = new Point(80, 20);
            label5.Name = "label5";
            label5.Size = new Size(72, 17);
            label5.TabIndex = 1;
            label5.Text = "High value";
            // 
            // attr1
            // 
            attr1.Location = new Point(318, 12);
            attr1.Name = "attr1";
            attr1.SelectedChannel = 0;
            attr1.Size = new Size(172, 521);
            attr1.TabIndex = 15;
            attr1.Title = "Attenuator 1";
            attr1.Value = 0;
            // 
            // attr2
            // 
            attr2.Location = new Point(496, 12);
            attr2.Name = "attr2";
            attr2.SelectedChannel = 0;
            attr2.Size = new Size(172, 521);
            attr2.TabIndex = 16;
            attr2.Title = "Attenuator 2";
            attr2.Value = 0;
            // 
            // attr3
            // 
            attr3.Location = new Point(674, 12);
            attr3.Name = "attr3";
            attr3.SelectedChannel = 0;
            attr3.Size = new Size(172, 521);
            attr3.TabIndex = 17;
            attr3.Title = "Attenuator 3";
            attr3.Value = 0;
            // 
            // attr4
            // 
            attr4.Location = new Point(852, 12);
            attr4.Name = "attr4";
            attr4.SelectedChannel = 0;
            attr4.Size = new Size(172, 521);
            attr4.TabIndex = 18;
            attr4.Title = "Attenuator 4";
            attr4.Value = 0;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(cboEnableAnt);
            groupBox2.Controls.Add(label9);
            groupBox2.Location = new Point(12, 429);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(300, 55);
            groupBox2.TabIndex = 18;
            groupBox2.TabStop = false;
            // 
            // cboEnableAnt
            // 
            cboEnableAnt.FormattingEnabled = true;
            cboEnableAnt.Location = new Point(109, 18);
            cboEnableAnt.Name = "cboEnableAnt";
            cboEnableAnt.Size = new Size(102, 28);
            cboEnableAnt.TabIndex = 7;
            cboEnableAnt.SelectedIndexChanged += cboEnableAnt_SelectedIndexChanged;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(6, 23);
            label9.Name = "label9";
            label9.Size = new Size(97, 20);
            label9.TabIndex = 6;
            label9.Text = "Enable Ant : ";
            // 
            // attr5
            // 
            attr5.Location = new Point(1030, 12);
            attr5.Name = "attr5";
            attr5.SelectedChannel = 0;
            attr5.Size = new Size(176, 528);
            attr5.TabIndex = 19;
            attr5.Title = "Attenuator 5";
            attr5.Value = 0;
            // 
            // attr6
            // 
            attr6.Location = new Point(1212, 12);
            attr6.Name = "attr6";
            attr6.SelectedChannel = 0;
            attr6.Size = new Size(176, 528);
            attr6.TabIndex = 20;
            attr6.Title = "Attenuator 6";
            attr6.Value = 0;
            // 
            // grpConfig
            // 
            grpConfig.Controls.Add(btnLoadConfig);
            grpConfig.Controls.Add(btnSaveConfig);
            grpConfig.Location = new Point(12, 646);
            grpConfig.Name = "grpConfig";
            grpConfig.Size = new Size(300, 62);
            grpConfig.TabIndex = 21;
            grpConfig.TabStop = false;
            grpConfig.Text = "Environment Setting";
            // 
            // btnLoadConfig
            // 
            btnLoadConfig.Location = new Point(157, 22);
            btnLoadConfig.Name = "btnLoadConfig";
            btnLoadConfig.Size = new Size(136, 29);
            btnLoadConfig.TabIndex = 1;
            btnLoadConfig.Text = "Load INI";
            btnLoadConfig.UseVisualStyleBackColor = true;
            btnLoadConfig.Click += btnLoadConfig_Click;
            // 
            // btnSaveConfig
            // 
            btnSaveConfig.Location = new Point(6, 22);
            btnSaveConfig.Name = "btnSaveConfig";
            btnSaveConfig.Size = new Size(136, 29);
            btnSaveConfig.TabIndex = 0;
            btnSaveConfig.Text = "Save INI";
            btnSaveConfig.UseVisualStyleBackColor = true;
            btnSaveConfig.Click += btnSaveConfig_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1399, 790);
            Controls.Add(grpConfig);
            Controls.Add(attr6);
            Controls.Add(attr5);
            Controls.Add(groupBox2);
            Controls.Add(attr4);
            Controls.Add(attr3);
            Controls.Add(attr2);
            Controls.Add(attr1);
            Controls.Add(grpEdit);
            Controls.Add(grpBtn);
            Controls.Add(grpMode);
            Controls.Add(groupBox1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "New_Attenuator";
            Load += Form1_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            grpMode.ResumeLayout(false);
            grpMode.PerformLayout();
            pnlMode.ResumeLayout(false);
            pnlMode.PerformLayout();
            grpBtn.ResumeLayout(false);
            grpBtn.PerformLayout();
            grpEdit.ResumeLayout(false);
            grpEdit.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            grpConfig.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private GroupBox groupBox1;
        private ComboBox cboPort;
        private Button btnConnect;
        private Label label1;
        private GroupBox grpMode;
        private FlowLayoutPanel pnlMode;
        private RadioButton rbFailover;
        private RadioButton rbDiagonal;
        private RadioButton rbPingPong;
        private RadioButton rbStepHandover;
        private RadioButton rbTrans4;
        private RadioButton rbTrans3;
        private RadioButton rbBasic3;
        private RadioButton rbTrans2;
        private RadioButton rbBasic2;
        private RadioButton rbTrans1;
        private RadioButton rbBasic1;
        private GroupBox grpBtn;
        private CheckBox valEdit;
        private Button btnStop;
        private Button btnStart;
        private GroupBox grpEdit;
        private Label label5;
        private Label label4;
        private Label label6;
        private Label label7;
        private TextBox txtStep;
        private TextBox txtTimeout;
        private TextBox txtHigh;
        private TextBox txtLow;
        private AttenuatorControl attr1;
        private AttenuatorControl attr2;
        private AttenuatorControl attr3;
        private AttenuatorControl attr4;
        private GroupBox groupBox2;
        private ComboBox cboEnableAnt;
        private Label label9;
        private AttenuatorControl attr5;
        private AttenuatorControl attr6;
        private GroupBox grpConfig;
        private Button btnLoadConfig;
        private Button btnSaveConfig;
    }
}
