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
            this.btnLoadFile = new System.Windows.Forms.Button();
            this.btnProcessSingle = new System.Windows.Forms.Button();
            this.btnProcessMultiple = new System.Windows.Forms.Button();
            this.lstQiHao = new System.Windows.Forms.ListBox();
            this.lstScores = new System.Windows.Forms.ListBox();
            this.lblQiHaoTitle = new System.Windows.Forms.Label();
            this.lblScoreTitle = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnReset = new System.Windows.Forms.Button();
            this.txt350Numbers = new System.Windows.Forms.TextBox();
            this.lbl350Numbers = new System.Windows.Forms.Label();
            this.btnSet350Numbers = new System.Windows.Forms.Button();
            this.btnGenerate350Numbers = new System.Windows.Forms.Button();
            this.lstChuShouStats = new System.Windows.Forms.ListBox();
            this.lblChuShouStatsTitle=new Label();
            this.SuspendLayout();
            // 
            // btnLoadFile
            // 
            this.btnLoadFile.Location = new System.Drawing.Point(12, 12);
            this.btnLoadFile.Name = "btnLoadFile";
            this.btnLoadFile.Size = new System.Drawing.Size(100, 30);
            this.btnLoadFile.TabIndex = 0;
            this.btnLoadFile.Text = "加载文件";
            this.btnLoadFile.UseVisualStyleBackColor = true;
            this.btnLoadFile.Click += new System.EventHandler(this.btnLoadFile_Click);
            // 
            // btnProcessSingle
            // 
            this.btnProcessSingle.Location = new System.Drawing.Point(118, 12);
            this.btnProcessSingle.Name = "btnProcessSingle";
            this.btnProcessSingle.Size = new System.Drawing.Size(100, 30);
            this.btnProcessSingle.TabIndex = 1;
            this.btnProcessSingle.Text = "处理单期";
            this.btnProcessSingle.UseVisualStyleBackColor = true;
            this.btnProcessSingle.Click += new System.EventHandler(this.btnProcessSingle_Click);
            // 
            // btnProcessMultiple
            // 
            this.btnProcessMultiple.Location = new System.Drawing.Point(224, 12);
            this.btnProcessMultiple.Name = "btnProcessMultiple";
            this.btnProcessMultiple.Size = new System.Drawing.Size(100, 30);
            this.btnProcessMultiple.TabIndex = 2;
            this.btnProcessMultiple.Text = "处理多期";
            this.btnProcessMultiple.UseVisualStyleBackColor = true;
            this.btnProcessMultiple.Click += new System.EventHandler(this.btnProcessMultiple_Click);
            // 
            // btnReset
            // 
            this.btnReset.Location = new System.Drawing.Point(330, 12);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(100, 30);
            this.btnReset.TabIndex = 8;
            this.btnReset.Text = "重置";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // btnSet350Numbers
            // 
            this.btnSet350Numbers.Location = new System.Drawing.Point(436, 12);
            this.btnSet350Numbers.Name = "btnSet350Numbers";
            this.btnSet350Numbers.Size = new System.Drawing.Size(100, 30);
            this.btnSet350Numbers.TabIndex = 11;
            this.btnSet350Numbers.Text = "设置350注";
            this.btnSet350Numbers.UseVisualStyleBackColor = true;
            this.btnSet350Numbers.Click += new System.EventHandler(this.btnSet350Numbers_Click);
            // 
            // btnGenerate350Numbers
            // 
            this.btnGenerate350Numbers.Location = new System.Drawing.Point(542, 12);
            this.btnGenerate350Numbers.Name = "btnGenerate350Numbers";
            this.btnGenerate350Numbers.Size = new System.Drawing.Size(120, 30);
            this.btnGenerate350Numbers.TabIndex = 12;
            this.btnGenerate350Numbers.Text = "生成随机350注";
            this.btnGenerate350Numbers.UseVisualStyleBackColor = true;
            this.btnGenerate350Numbers.Click += new System.EventHandler(this.btnGenerate350Numbers_Click);
            // 
            // lbl350Numbers
            // 
            this.lbl350Numbers.AutoSize = true;
            this.lbl350Numbers.Location = new System.Drawing.Point(12, 50);
            this.lbl350Numbers.Name = "lbl350Numbers";
            this.lbl350Numbers.Size = new System.Drawing.Size(139, 17);
            this.lbl350Numbers.TabIndex = 10;
            this.lbl350Numbers.Text = "350注号码 (用逗号分隔)";
            // 
            // txt350Numbers
            // 
            this.txt350Numbers.Location = new System.Drawing.Point(12, 70);
            this.txt350Numbers.Multiline = true;
            this.txt350Numbers.Name = "txt350Numbers";
            this.txt350Numbers.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txt350Numbers.Size = new System.Drawing.Size(768, 40);
            this.txt350Numbers.TabIndex = 9;
            // 
            // lstQiHao
            // 
            this.lstQiHao.FormattingEnabled = true;
            this.lstQiHao.ItemHeight = 17;
            this.lstQiHao.Location = new System.Drawing.Point(12, 130);
            this.lstQiHao.Name = "lstQiHao";
            this.lstQiHao.Size = new System.Drawing.Size(300, 250);
            this.lstQiHao.TabIndex = 3;
            this.lstQiHao.SelectedIndexChanged += new System.EventHandler(this.lstQiHao_SelectedIndexChanged);
            // 
            // lstScores
            // 
            this.lstScores.FormattingEnabled = true;
            this.lstScores.ItemHeight = 17;
            this.lstScores.Location = new System.Drawing.Point(330, 130);
            this.lstScores.Name = "lstScores";
            this.lstScores.Size = new System.Drawing.Size(450, 250);
            this.lstScores.TabIndex = 4;
            // 
            // lblQiHaoTitle
            // 
            this.lblQiHaoTitle.AutoSize = true;
            this.lblQiHaoTitle.Location = new System.Drawing.Point(12, 110);
            this.lblQiHaoTitle.Name = "lblQiHaoTitle";
            this.lblQiHaoTitle.Size = new System.Drawing.Size(106, 17);
            this.lblQiHaoTitle.TabIndex = 5;
            this.lblQiHaoTitle.Text = "期号和开奖号";
            // 
            // lblScoreTitle
            // 
            this.lblScoreTitle.AutoSize = true;
            this.lblScoreTitle.Location = new System.Drawing.Point(330, 110);
            this.lblScoreTitle.Name = "lblScoreTitle";
            this.lblScoreTitle.Size = new System.Drawing.Size(122, 17);
            this.lblScoreTitle.TabIndex = 6;
            this.lblScoreTitle.Text = "评分及评分说明";
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(12, 390);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(82, 17);
            this.lblStatus.TabIndex = 7;
            this.lblStatus.Text = "就绪";
            // 
            // lstChuShouStats
            // 
            this.lstChuShouStats.FormattingEnabled = true;
            this.lstChuShouStats.ItemHeight = 17;
            this.lstChuShouStats.Location = new System.Drawing.Point(12, 435);
            this.lstChuShouStats.Name = "lstChuShouStats";
            this.lstChuShouStats.Size = new System.Drawing.Size(768, 157);
            this.lstChuShouStats.TabIndex = 14;
            // 
            // lblChuShouStatsTitle
            // 
            this.lblChuShouStatsTitle.AutoSize = true;
            this.lblChuShouStatsTitle.Location = new System.Drawing.Point(12, 415);
            this.lblChuShouStatsTitle.Name = "lblChuShouStatsTitle";
            this.lblChuShouStatsTitle.Size = new System.Drawing.Size(82, 17);
            this.lblChuShouStatsTitle.TabIndex = 13;
            this.lblChuShouStatsTitle.Text = "出手统计";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 600);
            this.Controls.Add(this.btnGenerate350Numbers);
            this.Controls.Add(this.btnSet350Numbers);
            this.Controls.Add(this.lbl350Numbers);
            this.Controls.Add(this.txt350Numbers);
            this.Controls.Add(this.lstChuShouStats);
            this.Controls.Add(this.lblChuShouStatsTitle);
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.lblScoreTitle);
            this.Controls.Add(this.lblQiHaoTitle);
            this.Controls.Add(this.lstScores);
            this.Controls.Add(this.lstQiHao);
            this.Controls.Add(this.btnProcessMultiple);
            this.Controls.Add(this.btnProcessSingle);
            this.Controls.Add(this.btnLoadFile);
            this.Name = "MainForm";
            this.Text = "彩票K线及布林评分系统";
            this.ResumeLayout(false);
            this.PerformLayout();

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