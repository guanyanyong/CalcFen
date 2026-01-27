namespace CalcFen
{
    partial class MainForm
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
            btnLoadFile = new Button();
            btnProcessSingle = new Button();
            btnProcessMultiple = new Button();
            lstQiHao = new ListBox();
            lstScores = new ListBox();
            lblQiHaoTitle = new Label();
            lblScoreTitle = new Label();
            lblStatus = new Label();
            btnReset = new Button();
            txt350Numbers = new TextBox();
            lbl350Numbers = new Label();
            btnSet350Numbers = new Button();
            btnGenerate350Numbers = new Button();
            lstChuShouStats = new ListBox();
            lblChuShouStatsTitle = new Label();
            SuspendLayout();
            // 
            // btnLoadFile
            // 
            btnLoadFile.Location = new Point(12, 12);
            btnLoadFile.Name = "btnLoadFile";
            btnLoadFile.Size = new Size(100, 30);
            btnLoadFile.TabIndex = 0;
            btnLoadFile.Text = "加载文件";
            btnLoadFile.UseVisualStyleBackColor = true;
            btnLoadFile.Click += btnLoadFile_Click;
            // 
            // btnProcessSingle
            // 
            btnProcessSingle.Location = new Point(118, 12);
            btnProcessSingle.Name = "btnProcessSingle";
            btnProcessSingle.Size = new Size(100, 30);
            btnProcessSingle.TabIndex = 1;
            btnProcessSingle.Text = "处理单期";
            btnProcessSingle.UseVisualStyleBackColor = true;
            btnProcessSingle.Click += btnProcessSingle_Click;
            // 
            // btnProcessMultiple
            // 
            btnProcessMultiple.Location = new Point(224, 12);
            btnProcessMultiple.Name = "btnProcessMultiple";
            btnProcessMultiple.Size = new Size(100, 30);
            btnProcessMultiple.TabIndex = 2;
            btnProcessMultiple.Text = "处理多期";
            btnProcessMultiple.UseVisualStyleBackColor = true;
            btnProcessMultiple.Click += btnProcessMultiple_Click;
            // 
            // lstQiHao
            // 
            lstQiHao.FormattingEnabled = true;
            lstQiHao.ItemHeight = 17;
            lstQiHao.Location = new Point(12, 163);
            lstQiHao.Name = "lstQiHao";
            lstQiHao.Size = new Size(300, 310);
            lstQiHao.TabIndex = 3;
            lstQiHao.SelectedIndexChanged += lstQiHao_SelectedIndexChanged;
            // 
            // lstScores
            // 
            lstScores.FormattingEnabled = true;
            lstScores.ItemHeight = 17;
            lstScores.Location = new Point(330, 163);
            lstScores.Name = "lstScores";
            lstScores.Size = new Size(450, 310);
            lstScores.TabIndex = 4;
            // 
            // lblQiHaoTitle
            // 
            lblQiHaoTitle.AutoSize = true;
            lblQiHaoTitle.Location = new Point(12, 143);
            lblQiHaoTitle.Name = "lblQiHaoTitle";
            lblQiHaoTitle.Size = new Size(80, 17);
            lblQiHaoTitle.TabIndex = 5;
            lblQiHaoTitle.Text = "期号和开奖号";
            // 
            // lblScoreTitle
            // 
            lblScoreTitle.AutoSize = true;
            lblScoreTitle.Location = new Point(330, 143);
            lblScoreTitle.Name = "lblScoreTitle";
            lblScoreTitle.Size = new Size(92, 17);
            lblScoreTitle.TabIndex = 6;
            lblScoreTitle.Text = "评分及评分说明";
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(12, 480);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(32, 17);
            lblStatus.TabIndex = 7;
            lblStatus.Text = "就绪";
            // 
            // btnReset
            // 
            btnReset.Location = new Point(330, 12);
            btnReset.Name = "btnReset";
            btnReset.Size = new Size(100, 30);
            btnReset.TabIndex = 8;
            btnReset.Text = "重置";
            btnReset.UseVisualStyleBackColor = true;
            btnReset.Click += btnReset_Click;
            // 
            // txt350Numbers
            // 
            txt350Numbers.Location = new Point(12, 70);
            txt350Numbers.Multiline = true;
            txt350Numbers.Name = "txt350Numbers";
            txt350Numbers.ScrollBars = ScrollBars.Vertical;
            txt350Numbers.Size = new Size(768, 70);
            txt350Numbers.TabIndex = 9;
            // 
            // lbl350Numbers
            // 
            lbl350Numbers.AutoSize = true;
            lbl350Numbers.Location = new Point(12, 50);
            lbl350Numbers.Name = "lbl350Numbers";
            lbl350Numbers.Size = new Size(137, 17);
            lbl350Numbers.TabIndex = 10;
            lbl350Numbers.Text = "350注号码 (用逗号分隔)";
            // 
            // btnSet350Numbers
            // 
            btnSet350Numbers.Location = new Point(436, 12);
            btnSet350Numbers.Name = "btnSet350Numbers";
            btnSet350Numbers.Size = new Size(100, 30);
            btnSet350Numbers.TabIndex = 11;
            btnSet350Numbers.Text = "设置350注";
            btnSet350Numbers.UseVisualStyleBackColor = true;
            btnSet350Numbers.Click += btnSet350Numbers_Click;
            // 
            // btnGenerate350Numbers
            // 
            btnGenerate350Numbers.Location = new Point(542, 12);
            btnGenerate350Numbers.Name = "btnGenerate350Numbers";
            btnGenerate350Numbers.Size = new Size(120, 30);
            btnGenerate350Numbers.TabIndex = 12;
            btnGenerate350Numbers.Text = "生成随机350注";
            btnGenerate350Numbers.UseVisualStyleBackColor = true;
            btnGenerate350Numbers.Click += btnGenerate350Numbers_Click;
            // 
            // lstChuShouStats
            // 
            lstChuShouStats.FormattingEnabled = true;
            lstChuShouStats.ItemHeight = 17;
            lstChuShouStats.Location = new Point(12, 556);
            lstChuShouStats.Name = "lstChuShouStats";
            lstChuShouStats.Size = new Size(768, 259);
            lstChuShouStats.TabIndex = 14;
            // 
            // lblChuShouStatsTitle
            // 
            lblChuShouStatsTitle.AutoSize = true;
            lblChuShouStatsTitle.Location = new Point(12, 536);
            lblChuShouStatsTitle.Name = "lblChuShouStatsTitle";
            lblChuShouStatsTitle.Size = new Size(56, 17);
            lblChuShouStatsTitle.TabIndex = 13;
            lblChuShouStatsTitle.Text = "出手统计";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1244, 822);
            Controls.Add(btnGenerate350Numbers);
            Controls.Add(btnSet350Numbers);
            Controls.Add(lbl350Numbers);
            Controls.Add(txt350Numbers);
            Controls.Add(lstChuShouStats);
            Controls.Add(lblChuShouStatsTitle);
            Controls.Add(btnReset);
            Controls.Add(lblStatus);
            Controls.Add(lblScoreTitle);
            Controls.Add(lblQiHaoTitle);
            Controls.Add(lstScores);
            Controls.Add(lstQiHao);
            Controls.Add(btnProcessMultiple);
            Controls.Add(btnProcessSingle);
            Controls.Add(btnLoadFile);
            Name = "MainForm";
            Text = "彩票K线及布林评分系统";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button btnLoadFile;
        private System.Windows.Forms.Button btnProcessSingle;
        private System.Windows.Forms.Button btnProcessMultiple;
        private System.Windows.Forms.ListBox lstQiHao;
        private System.Windows.Forms.ListBox lstScores;
        private System.Windows.Forms.Label lblQiHaoTitle;
        private System.Windows.Forms.Label lblScoreTitle;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.TextBox txt350Numbers;
        private System.Windows.Forms.Label lbl350Numbers;
        private System.Windows.Forms.Button btnSet350Numbers;
        private System.Windows.Forms.Button btnGenerate350Numbers;
        private System.Windows.Forms.ListBox lstChuShouStats;
        private System.Windows.Forms.Label lblChuShouStatsTitle;
    }
}