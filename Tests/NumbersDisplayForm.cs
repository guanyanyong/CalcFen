using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace TestApp
{
    public partial class NumbersDisplayForm : Form
    {
        private System.ComponentModel.IContainer components = null;
        private TextBox textBoxNumbers;
        private Label lblTotalCount;
        private List<string> numbersToShow;
        
        public NumbersDisplayForm(List<string> numbers350)
        {
            InitializeComponent();
            
            numbersToShow = numbers350 ?? new List<string>();
            
            // 设置窗体标题
            this.Text = $"350注号码列表 (共{numbersToShow.Count}注)";
            
            // 将号码以空格分割的方式显示在文本框中
            DisplayNumbersAsSpaceSeparated(numbersToShow);
            
            // 设置总数统计
            lblTotalCount.Text = $"总数：{numbersToShow.Count} 注";
        }
        
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.textBoxNumbers = new TextBox();
            this.lblTotalCount = new Label();
            
            this.SuspendLayout();
            
            // 
            // textBoxNumbers
            // 
            this.textBoxNumbers.Location = new System.Drawing.Point(12, 12);
            this.textBoxNumbers.Multiline = true;
            this.textBoxNumbers.ScrollBars = ScrollBars.Vertical;
            this.textBoxNumbers.Size = new System.Drawing.Size(600, 400);
            this.textBoxNumbers.Name = "textBoxNumbers";
            this.textBoxNumbers.ReadOnly = true;
            
            // 
            // lblTotalCount
            // 
            this.lblTotalCount.AutoSize = true;
            this.lblTotalCount.Location = new System.Drawing.Point(12, 420);
            this.lblTotalCount.Name = "lblTotalCount";
            this.lblTotalCount.Size = new System.Drawing.Size(100, 12);
            this.lblTotalCount.TabIndex = 1;
            
            // 
            // NumbersDisplayForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 440);
            this.Controls.Add(this.textBoxNumbers);
            this.Controls.Add(this.lblTotalCount);
            this.Name = "NumbersDisplayForm";
            this.Text = "350注号码列表";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        
        private void DisplayNumbersAsSpaceSeparated(List<string> numbers)
        {
            // 确保所有号码都是3位数字格式
            var formattedNumbers = new List<string>();
            foreach (string number in numbers)
            {
                string formattedNum = number;
                // 确保号码是3位数字格式
                if (formattedNum.Length < 3)
                {
                    formattedNum = formattedNum.PadLeft(3, '0');
                }
                else if (formattedNum.Length > 3)
                {
                    formattedNum = formattedNum.Substring(formattedNum.Length - 3);
                }
                formattedNumbers.Add(formattedNum);
            }
            
            // 对号码进行从小到大排序
            formattedNumbers.Sort((x, y) => int.Parse(x).CompareTo(int.Parse(y)));
            
            // 将所有号码用空格连接起来
            string allNumbers = string.Join(" ", formattedNumbers);
            textBoxNumbers.Text = allNumbers;
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}