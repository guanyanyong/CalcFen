using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CpCodeSelect.Core;
using CpCodeSelect.Scorer;
using CpCodeSelect.Scorer.Rules;
using CpCodeSelect.Model;
using TestApp;

namespace TestApp
{
    public partial class TestForm : Form
    {
        private LotteryProcessor globalProcessor;
        private ScoringEngine globalScoringEngine;
        private List<TestRound> testRounds;
        private int maxContinuousLosses;

        public TestForm()
        {
            InitializeComponent();
            InitializeApplication();
        }

        private void InitializeApplication()
        {
            globalProcessor = new LotteryProcessor();
            globalScoringEngine = new ScoringEngine();
            testRounds = new List<TestRound>();
            maxContinuousLosses = 8;

            // 初始化评分规则
            InitializeScoringRules();

            // 设置界面
            nudRounds.Value = 5;
            lblStatus.Text = "测试系统就绪";
        }

        private void InitializeScoringRules()
        {
            globalScoringEngine.AddRule(new KValueBelowMiddleNoBetRule());
            globalScoringEngine.AddRule(new TrendSegmentNoBetRule());
            globalScoringEngine.AddRule(new ConfirmPointBeforeTrendRule());
            globalScoringEngine.AddRule(new BigGapBetweenZeroOrOneStrongRule());
            globalScoringEngine.AddRule(new ThreeTrackSameDirectionRule());
            globalScoringEngine.AddRule(new TwoTrackSameDirectionRule());
            globalScoringEngine.AddRule(new TrackOppositeDirectionRule());
            globalScoringEngine.AddRule(new KValueBreakMiddleNotTouchUpperRule());
            globalScoringEngine.AddRule(new KValueNearUpperRailRule());
            globalScoringEngine.AddRule(new YiLouValueRule());
            globalScoringEngine.AddRule(new BollingerUpperDeclineRule());
            globalScoringEngine.AddRule(new ContinuousChuShouLimitRule());
            globalScoringEngine.AddRule(new SecondChuShouLimitRule());
            globalScoringEngine.AddRule(new StopAfterWinRule());
            globalScoringEngine.AddRule(new OpeningHornRule());
        }

        private void btnRunTest_Click(object sender, EventArgs e)
        {
            int rounds = (int)nudRounds.Value;
            RunTest(rounds, maxContinuousLosses);
        }

        private void RunTest(int rounds, int maxContinuousLosses)
        {
            testRounds.Clear();
            lbRounds.Items.Clear();

            lblStatus.Text = $"正在执行 {rounds} 轮测试...";
            Application.DoEvents();

            for (int i = 0; i < rounds; i++)
            {
                var round = new TestRound
                {
                    RoundNumber = i + 1,
                    Processor = new LotteryProcessor(),
                    ScoringEngine = new ScoringEngine()
                };

                // 复制规则到新引擎
                foreach (var rule in globalScoringEngine.GetType().GetField("_rules", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .GetValue(globalScoringEngine) as List<IScoreRule>)
                {
                    round.ScoringEngine.AddRule(rule);
                }

                // 加载数据并处理
                var realData = LoadRealData();
                if (realData != null && realData.Count > 0)
                {
                    var limitedData = realData.Take(1000).ToList(); // 限制数据量
                    
                    // 批量处理所有数据
                    round.Processor.Reset(true); // 重新生成350号码
                    var batchResults = round.Processor.ProcessMultiplePeriods(limitedData);
                    
                    // 重新计算评分和出手
                    RecalculateAllScoresConsistentWithMainForm(round.Processor, round.ScoringEngine, maxContinuousLosses);
                    
                    // 统计结果
                    var allChuShouRecords = round.Processor.HistoryData.Where(d => d.IsChuShou).ToList();
                    round.ChuShouCount = allChuShouRecords.Count;
                    
                    round.WinCount = allChuShouRecords.Count(record =>
                    {
                        int recordIndex = round.Processor.HistoryData.IndexOf(record);
                        if (recordIndex >= 0 && recordIndex < round.Processor.HistoryData.Count - 1)
                        {
                            return round.Processor.HistoryData[recordIndex + 1].IsZhongJiang;
                        }
                        return false;
                    });

                    // 统计爆掉次数
                    var cycles = allChuShouRecords.GroupBy(d => d.CycleNumber).ToList();
                    foreach (var cycleGroup in cycles)
                    {
                        var cycleRecords = cycleGroup.OrderBy(d => d.CycleStep).ToList();
                        bool isCompleted = cycleRecords.Any(d => d.IsCycleComplete);
                        bool isBurst = cycleRecords.Any(d => d.IsCycleBurst);                        
                        if (isBurst) round.BurstCount++;
                    }
                }

                testRounds.Add(round);
                lbRounds.Items.Add($"第 {round.RoundNumber} 轮 - 中:{round.WinCount} 爆:{round.BurstCount} 出:{round.ChuShouCount}");
            }

            // 更新总体统计
            UpdateOverallStats();

            lblStatus.Text = $"测试完成！共 {rounds} 轮，总中奖 {testRounds.Sum(r => r.WinCount)} 个，总爆掉 {testRounds.Sum(r => r.BurstCount)} 个";
        }

        private List<(string qiHao, string number)> LoadRealData()
        {
            try
            {
                string filePath = "..\\..\\..\\TXFFC.txt";
                if (System.IO.File.Exists(filePath))
                {
                    var lines = System.IO.File.ReadAllLines(filePath);
                    var data = new List<(string qiHao, string number)>();

                    // 文件中第一行是最新的期号，为了按时间顺序从旧到新处理，
                    // 我们需要颠倒顺序，让最早的期号在前面
                    for (int i = lines.Length - 1; i >= 0; i--)
                    {
                        var line = lines[i].Trim();
                        if (string.IsNullOrEmpty(line)) continue;
                        
                        var parts = line.Split('\t');
                        if (parts.Length >= 2)
                        {
                            string qiHao = parts[0].Trim();
                            string number = parts[1].Trim();

                            if (!string.IsNullOrEmpty(qiHao) && !string.IsNullOrEmpty(number))
                            {
                                data.Add((qiHao, number));
                            }
                        }
                    }

                    return data;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载数据时出错: {ex.Message}");
            }

            return new List<(string qiHao, string number)>();
        }

        private void RecalculateAllScoresConsistentWithMainForm(LotteryProcessor processor, ScoringEngine scoringEngine, int maxContinuousLosses)
        {
            // 重置所有周期相关字段
            for (int i = 0; i < processor.HistoryData.Count; i++)
            {
                processor.HistoryData[i].IsChuShou = false;
                processor.HistoryData[i].IsChuShouSuccess = false;
                processor.HistoryData[i].IsCycleComplete = false;
                processor.HistoryData[i].IsCycleBurst = false;
                processor.HistoryData[i].CycleNumber = 0;
                processor.HistoryData[i].CycleStep = 0;
                processor.HistoryData[i].HandNumber = 0;
                processor.HistoryData[i].IsPartOfCycle = false;
            }

            // 第一步：计算每期的评分
            for (int i = 0; i < processor.HistoryData.Count; i++)
            {
                var historyForScoring = processor.HistoryData.Take(i).ToList();
                processor.HistoryData[i].Score = scoringEngine.CalculateTotalScore(
                    processor.HistoryData[i],
                    historyForScoring
                );

                // 根据评分规则设置是否出手标志
                bool isScoreHighEnough = processor.HistoryData[i].Score >= 70;
                bool isNotInTrendSegment = !processor.HistoryData[i].IsQuShiDuan;
                bool isKValueAboveMiddle = processor.HistoryData[i].BollingerBands != null &&
                    processor.HistoryData[i].KValue >= processor.HistoryData[i].BollingerBands.MiddleValue;

                // 检查是否连续出手超过2期，如果是则必须停一期
                bool canContinueChuShou = true;
                if (i >= 2)
                {
                    bool previousTwoAreChuShou = processor.HistoryData[i - 1].IsChuShou &&
                                                 processor.HistoryData[i - 2].IsChuShou;

                    if (previousTwoAreChuShou)
                    {
                        canContinueChuShou = false;
                    }
                }

                processor.HistoryData[i].IsChuShou = isScoreHighEnough && isNotInTrendSegment && isKValueAboveMiddle && canContinueChuShou;
            }

            // 第二步：根据开奖结果确定出手成功性
            for (int i = 0; i < processor.HistoryData.Count; i++)
            {
                if (i > 0 && processor.HistoryData[i - 1].IsChuShou)
                {
                    processor.HistoryData[i - 1].IsChuShouSuccess = processor.HistoryData[i].IsZhongJiang;
                }
            }

            // 第三步：按时间顺序重新计算所有出手数据的周期信息
            for (int i = 0; i < processor.HistoryData.Count; i++)
            {
                processor.HistoryData[i].IsCycleComplete = false;
                processor.HistoryData[i].IsCycleBurst = false;
                processor.HistoryData[i].IsChuShouSuccess = false;
            }

            // 第四步：按时间顺序重新计算所有出手数据的周期和步骤信息
            for (int i = 0; i < processor.HistoryData.Count; i++)
            {
                if (processor.HistoryData[i].IsChuShou)
                {
                    processor.CalculateChuShouCycleAndHandNumber(processor.HistoryData[i]);
                }
            }

            // 第五步：再次计算出手成功性
            for (int i = 0; i < processor.HistoryData.Count; i++)
            {
                if (i > 0 && processor.HistoryData[i - 1].IsChuShou)
                {
                    processor.HistoryData[i - 1].IsChuShouSuccess = processor.HistoryData[i].IsZhongJiang;
                }
            }

            // 第六步：标记周期完成和爆掉状态
            for (int i = 0; i < processor.HistoryData.Count; i++)
            {
                if (i > 0 && processor.HistoryData[i - 1].IsChuShou)
                {
                    if (processor.HistoryData[i].IsZhongJiang)
                    {
                        processor.HistoryData[i - 1].IsCycleComplete = true;

                        int currentCycleNumber = processor.HistoryData[i - 1].CycleNumber;
                        for (int j = 0; j < processor.HistoryData.Count; j++)
                        {
                            if (processor.HistoryData[j].IsChuShou &&
                                processor.HistoryData[j].CycleNumber == currentCycleNumber)
                            {
                                processor.HistoryData[j].IsCycleComplete = true;
                                processor.HistoryData[j].IsCycleBurst = false;
                            }
                        }
                    }
                    else if (processor.HistoryData[i - 1].CycleStep == maxContinuousLosses &&
                             i < processor.HistoryData.Count &&
                             !processor.HistoryData[i].IsZhongJiang)
                    {
                        processor.HistoryData[i - 1].IsCycleBurst = true;

                        int currentCycleNumber = processor.HistoryData[i - 1].CycleNumber;
                        for (int j = 0; j < processor.HistoryData.Count; j++)
                        {
                            if (processor.HistoryData[j].IsChuShou &&
                                processor.HistoryData[j].CycleNumber == currentCycleNumber)
                            {
                                processor.HistoryData[j].IsCycleBurst = true;
                                processor.HistoryData[j].IsCycleComplete = false;
                            }
                        }
                    }
                }
            }

            // 第七步：最后再重新计算一次周期信息
            for (int i = 0; i < processor.HistoryData.Count; i++)
            {
                if (processor.HistoryData[i].IsChuShou)
                {
                    processor.CalculateChuShouCycleAndHandNumber(processor.HistoryData[i]);
                }
            }
        }

        private void UpdateOverallStats()
        {
            int totalWins = testRounds.Sum(r => r.WinCount);
            int totalBursts = testRounds.Sum(r => r.BurstCount);
            int totalChuShou = testRounds.Sum(r => r.ChuShouCount);

            lblOverallStats.Text = $"总体统计：总中奖 {totalWins} 个，总爆掉 {totalBursts} 个，总出手 {totalChuShou} 次";
        }

        private void lbRounds_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbRounds.SelectedIndex >= 0)
            {
                var selectedRound = testRounds[lbRounds.SelectedIndex];
                
                // 显示该轮的所有期数到列表中
                lbPeriods.Items.Clear();
                for (int i = 0; i < selectedRound.Processor.HistoryData.Count; i++)
                {
                    var data = selectedRound.Processor.HistoryData[i];
                    string info = $"{data.QiHao} - {data.Number} ({(data.IsZhongJiang ? "中" : "未中")})";
                    if (data.IsChuShou) info += " [出手]";
                    lbPeriods.Items.Add(info);
                }
            }
        }

        private void lbPeriods_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbRounds.SelectedIndex >= 0 && lbPeriods.SelectedIndex >= 0)
            {
                var selectedRound = testRounds[lbRounds.SelectedIndex];
                var selectedPeriod = selectedRound.Processor.HistoryData[lbPeriods.SelectedIndex];
                
                // 获取评分详情
                var historyForScoring = selectedRound.Processor.HistoryData.Take(lbPeriods.SelectedIndex).ToList();
                var scoreDetails = selectedRound.ScoringEngine.GetScoreDetails(selectedPeriod, historyForScoring);

                // 显示详细信息
                lstScores.Items.Clear();
                lstScores.Items.Add($"期号: {selectedPeriod.QiHao}");
                lstScores.Items.Add($"开奖号: {selectedPeriod.Number}");
                lstScores.Items.Add($"后三位: {selectedPeriod.Hou3Number}");
                lstScores.Items.Add($"是否中奖: {(selectedPeriod.IsZhongJiang ? "是" : "否")}");
                lstScores.Items.Add($"遗漏值: {selectedPeriod.YiLouValue}");
                lstScores.Items.Add($"K值: {selectedPeriod.KValue:F3}");
                lstScores.Items.Add($"总评分: {selectedPeriod.Score}分");

                if (selectedPeriod.BollingerBands != null)
                {
                    lstScores.Items.Add($"布林上轨: {selectedPeriod.BollingerBands.BollUpperValue:F3}");
                    lstScores.Items.Add($"布林中轨: {selectedPeriod.BollingerBands.MiddleValue:F3}");
                    lstScores.Items.Add($"布林下轨: {selectedPeriod.BollingerBands.BollLowerValue:F3}");
                }

                lstScores.Items.Add($"是否出手: {(selectedPeriod.IsChuShou ? "是" : "否")}");
                lstScores.Items.Add($"出手成功: {(selectedPeriod.IsChuShouSuccess ? "是" : "否")}");
                
                if (selectedPeriod.IsChuShou)
                {
                    lstScores.Items.Add($"周期号: {selectedPeriod.CycleNumber}");
                    lstScores.Items.Add($"周期步骤: {selectedPeriod.CycleStep}");
                    lstScores.Items.Add($"周期完成: {(selectedPeriod.IsCycleComplete ? "是" : "否")}");
                    lstScores.Items.Add($"周期爆掉: {(selectedPeriod.IsCycleBurst ? "是" : "否")}");
                }

                lstScores.Items.Add("");
                lstScores.Items.Add("--- 评分详情 ---");

                var positiveScoreRules = scoreDetails.Where(sd => sd.IsValid && sd.Score > 0).ToList();
                var negativeScoreRules = scoreDetails.Where(sd => sd.IsValid && sd.Score < 0).ToList();
                var unmetRules = scoreDetails.Where(sd => !sd.IsValid).ToList();

                // 显示加分明细
                if (positiveScoreRules.Count > 0)
                {
                    lstScores.Items.Add("【加分规则】:");
                    foreach (var detail in positiveScoreRules)
                    {
                        lstScores.Items.Add($"  +{detail.Score}分 (指标分值: {detail.ScoreValue}分): {detail.RuleName}");
                        lstScores.Items.Add($"    条件: {detail.Description}");
                    }
                }
                else
                {
                    lstScores.Items.Add("【加分规则】: 无");
                }

                // 显示减分明细
                if (negativeScoreRules.Count > 0)
                {
                    lstScores.Items.Add("【减分规则】:");
                    foreach (var detail in negativeScoreRules)
                    {
                        lstScores.Items.Add($"  {detail.Score}分 (指标分值: {detail.ScoreValue}分): {detail.RuleName}");
                        lstScores.Items.Add($"    条件: {detail.Description}");
                    }
                }
                else
                {
                    lstScores.Items.Add("【减分规则】: 无");
                }

                // 显示未触发规则
                if (unmetRules.Count > 0)
                {
                    lstScores.Items.Add("【未满足规则】:");
                    foreach (var detail in unmetRules)
                    {
                        lstScores.Items.Add($"  0分 (指标分值: {detail.ScoreValue}分): {detail.RuleName}");
                        lstScores.Items.Add($"    不满足条件: {detail.Description}");
                    }
                }
                else
                {
                    lstScores.Items.Add("【未满足规则】: 无");
                }
            }
        }

        private void btnView350Numbers_Click(object sender, EventArgs e)
        {
            // 如果没有测试数据，生成一组新的350号码用于演示
            List<string> numbers350;
            
            // 如果当前已有测试数据且选择了某个测试轮次，使用该轮的号码
            if (testRounds.Any() && lbRounds.SelectedIndex >= 0)
            {
                var selectedRound = testRounds[lbRounds.SelectedIndex];
                numbers350 = selectedRound.Processor?.Numbers350 ?? GenerateDemoNumbers350();
            }
            else
            {
                // 否则生成新的随机350号码用于演示
                var demoProcessor = new LotteryProcessor();
                demoProcessor.Reset(true); // 生成新的350号码
                numbers350 = demoProcessor.Numbers350;
            }
            
            // 创建并显示号码展示窗体
            var numbersForm = new TestApp.NumbersDisplayForm(numbers350);
            numbersForm.Show();
        }
        
        private List<string> GenerateDemoNumbers350()
        {
            var numbers = new List<string>();
            var random = new Random();
            
            for (int i = 0; i < 350; i++)
            {
                int num = random.Next(0, 1000);
                numbers.Add(num.ToString("D3"));
            }
            
            return numbers;
        }

        #region Windows Form Designer generated code
        private System.ComponentModel.IContainer components = null;

        private NumericUpDown nudRounds;
        private Button btnRunTest;
        private Button btnView350Numbers; // 添加新的按钮
        private ListBox lbRounds;
        private ListBox lbPeriods;
        private ListBox lstScores;
        private Label lblStatus;
        private Label lblOverallStats;

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.nudRounds = new System.Windows.Forms.NumericUpDown();
            this.btnRunTest = new System.Windows.Forms.Button();
            this.btnView350Numbers = new System.Windows.Forms.Button(); // 新增按钮
            this.lbRounds = new System.Windows.Forms.ListBox();
            this.lbPeriods = new System.Windows.Forms.ListBox();
            this.lstScores = new System.Windows.Forms.ListBox();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblOverallStats = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.nudRounds)).BeginInit();
            this.SuspendLayout();
            // 
            // nudRounds
            // 
            this.nudRounds.Location = new System.Drawing.Point(12, 12);
            this.nudRounds.Name = "nudRounds";
            this.nudRounds.Size = new System.Drawing.Size(60, 21);
            this.nudRounds.TabIndex = 0;
            this.nudRounds.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // btnRunTest
            // 
            this.btnRunTest.Location = new System.Drawing.Point(78, 10);
            this.btnRunTest.Name = "btnRunTest";
            this.btnRunTest.Size = new System.Drawing.Size(75, 23);
            this.btnRunTest.TabIndex = 1;
            this.btnRunTest.Text = "运行测试";
            this.btnRunTest.UseVisualStyleBackColor = true;
            this.btnRunTest.Click += new System.EventHandler(this.btnRunTest_Click);
            // 
            // btnView350Numbers
            // 
            this.btnView350Numbers.Location = new System.Drawing.Point(159, 10);
            this.btnView350Numbers.Name = "btnView350Numbers";
            this.btnView350Numbers.Size = new System.Drawing.Size(100, 23);
            this.btnView350Numbers.TabIndex = 7;
            this.btnView350Numbers.Text = "查看350注号码";
            this.btnView350Numbers.UseVisualStyleBackColor = true;
            this.btnView350Numbers.Click += new System.EventHandler(this.btnView350Numbers_Click);
            // 
            // lbRounds
            // 
            this.lbRounds.FormattingEnabled = true;
            this.lbRounds.ItemHeight = 12;
            this.lbRounds.Location = new System.Drawing.Point(12, 39);
            this.lbRounds.Name = "lbRounds";
            this.lbRounds.Size = new System.Drawing.Size(200, 268);
            this.lbRounds.TabIndex = 2;
            this.lbRounds.SelectedIndexChanged += new System.EventHandler(this.lbRounds_SelectedIndexChanged);
            // 
            // lbPeriods
            // 
            this.lbPeriods.FormattingEnabled = true;
            this.lbPeriods.ItemHeight = 12;
            this.lbPeriods.Location = new System.Drawing.Point(218, 39);
            this.lbPeriods.Name = "lbPeriods";
            this.lbPeriods.Size = new System.Drawing.Size(200, 268);
            this.lbPeriods.TabIndex = 3;
            this.lbPeriods.SelectedIndexChanged += new System.EventHandler(this.lbPeriods_SelectedIndexChanged);
            // 
            // lstScores
            // 
            this.lstScores.FormattingEnabled = true;
            this.lstScores.HorizontalScrollbar = true;
            this.lstScores.ItemHeight = 12;
            this.lstScores.Location = new System.Drawing.Point(424, 39);
            this.lstScores.Name = "lstScores";
            this.lstScores.Size = new System.Drawing.Size(400, 268);
            this.lstScores.TabIndex = 4;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(12, 319);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(113, 12);
            this.lblStatus.TabIndex = 5;
            this.lblStatus.Text = "测试系统就绪";
            // 
            // lblOverallStats
            // 
            this.lblOverallStats.AutoSize = true;
            this.lblOverallStats.Location = new System.Drawing.Point(12, 341);
            this.lblOverallStats.Name = "lblOverallStats";
            this.lblOverallStats.Size = new System.Drawing.Size(173, 12);
            this.lblOverallStats.TabIndex = 6;
            this.lblOverallStats.Text = "总体统计：总中奖 0 个，总爆掉 0 个";
            // 
            // TestForm
            // 
            this.ClientSize = new System.Drawing.Size(836, 362);
            this.Controls.Add(this.btnView350Numbers); // 添加新按钮到控件列表
            this.Controls.Add(this.lblOverallStats);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.lstScores);
            this.Controls.Add(this.lbPeriods);
            this.Controls.Add(this.lbRounds);
            this.Controls.Add(this.btnRunTest);
            this.Controls.Add(this.nudRounds);
            this.Name = "TestForm";
            this.Text = "彩票出手策略测试窗体";
            ((System.ComponentModel.ISupportInitialize)(this.nudRounds)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion
    }

    public class TestRound
    {
        public int RoundNumber { get; set; }
        public LotteryProcessor Processor { get; set; }
        public ScoringEngine ScoringEngine { get; set; }
        public int WinCount { get; set; }
        public int BurstCount { get; set; }
        public int ChuShouCount { get; set; }
    }
}