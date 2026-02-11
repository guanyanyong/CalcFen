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
        private ListBox listBoxNumbers;
        private Label lblTotalCount;
        private List<string> numbersToShow;
        
        public NumbersDisplayForm(List<string> numbers350)
        {
            InitializeComponent();
            
            numbersToShow = numbers350 ?? new List<string>();
            
            // 设置窗体标题
            this.Text = $"350注号码列表 (共{numbersToShow.Count}注)";
            
            // 添加号码到列表
            PopulateNumbersList(numbersToShow);
            
            // 设置总数统计
            lblTotalCount.Text = $"总数：{numbersToShow.Count} 注";
        }
        
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.listBoxNumbers = new ListBox();
            this.lblTotalCount = new Label();
            
            this.SuspendLayout();
            
            // 
            // listBoxNumbers
            // 
            this.listBoxNumbers.FormattingEnabled = true;
            this.listBoxNumbers.ItemHeight = 12;
            this.listBoxNumbers.Location = new System.Drawing.Point(12, 12);
            this.listBoxNumbers.Size = new System.Drawing.Size(180, 400);
            this.listBoxNumbers.Name = "listBoxNumbers";
            this.listBoxNumbers.SelectionMode = SelectionMode.One; // 单选模式
            
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
            this.ClientSize = new System.Drawing.Size(204, 440);
            this.Controls.Add(this.listBoxNumbers);
            this.Controls.Add(this.lblTotalCount);
            this.Name = "NumbersDisplayForm";
            this.Text = "350注号码列表";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        
        private void PopulateNumbersList(List<string> numbers)
        {
            // 清空现有项目
            listBoxNumbers.Items.Clear();
            
            // 添加所有号码，格式化为3位数字（前面补0）
            for (int i = 0; i < numbers.Count; i++)
            {
                string number = numbers[i];
                // 确保号码是3位数字格式
                if (number.Length < 3)
                {
                    number = number.PadLeft(3, '0');
                }
                else if (number.Length > 3)
                {
                    number = number.Substring(number.Length - 3);
                }
                
                listBoxNumbers.Items.Add($"{i + 1:D3}. {number}");
            }
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