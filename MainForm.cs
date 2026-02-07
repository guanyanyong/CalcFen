using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CpCodeSelect.Core;
using CpCodeSelect.Model;
using CpCodeSelect.Scorer;
using CpCodeSelect.Scorer.Rules;
using CpCodeSelect.Config;

namespace CalcFen
{
    public partial class MainForm : Form
    {
        private LotteryProcessor processor;
        private ScoringEngine scoringEngine;
        private List<LotteryData> processedData;
        private string loadedFilePath; // 记录之前加载的文件路径
        private FileWatcher fileWatcher;
        private Button btnStartMonitor;
        private Button btnStopMonitor;
        private Label lblMonitorStatus;

        public MainForm()
        {
            InitializeComponent();
            InitializeApplication();
            loadedFilePath = string.Empty; // 初始化为空
        }

        private void InitializeApplication()
        {
            processor = new LotteryProcessor();
            scoringEngine = new ScoringEngine();
            processedData = new List<LotteryData>();
            fileWatcher = new FileWatcher(processor);

            // 初始化评分规则
            InitializeScoringRules();

            // 设置双击事件 - 修正方法名为正确的名称
            lstQiHao.SelectedIndexChanged += lstQiHao_SelectedIndexChanged;

            // 初始化时显示350注号码
            Display350Numbers();

            // 初始化出手统计列表框
            if (lstChuShouStats != null)
            {
                lstChuShouStats.Items.Add("暂无出手记录");
            }

            // 初始化文件监控状态标签
            if (lblMonitorStatus != null)
            {
                lblMonitorStatus.Text = "文件监控状态: 未启动";
            }

            // 订阅文件监控事件
            fileWatcher.OnFileChanged += OnFileChanged;
            fileWatcher.OnError += OnFileWatcherError;
            fileWatcher.OnStatusChanged += OnFileWatcherStatusChanged;
        }

        private void InitializeScoringRules()
        {
            scoringEngine.AddRule(new KValueBelowMiddleNoBetRule());
            scoringEngine.AddRule(new TrendSegmentNoBetRule());
            scoringEngine.AddRule(new ConfirmPointBeforeTrendRule());
            scoringEngine.AddRule(new BigGapBetweenZeroOrOneStrongRule());
            scoringEngine.AddRule(new ThreeTrackSameDirectionRule());
            scoringEngine.AddRule(new TwoTrackSameDirectionRule());
            scoringEngine.AddRule(new TrackOppositeDirectionRule());
            scoringEngine.AddRule(new KValueBreakMiddleNotTouchUpperRule());
            scoringEngine.AddRule(new KValueNearUpperRailRule()); // 添加K值接近上轨的评分规则
            scoringEngine.AddRule(new YiLouValueRule()); // 添加遗漏值评分规则
            scoringEngine.AddRule(new BollingerUpperDeclineRule()); // 添加布林上轨下降评分规则
            scoringEngine.AddRule(new ContinuousChuShouLimitRule()); // 添加连续出手限制评分规则
            scoringEngine.AddRule(new SecondChuShouLimitRule()); // 添加连续第二手限制规则
            scoringEngine.AddRule(new StopAfterWinRule()); // 添加出手中了以后停一期的规则
        }

        // 为评分引擎添加评分规则的方法
        private void InitializeScoringRulesForEngine(ScoringEngine engine)
        {
            engine.AddRule(new KValueBelowMiddleNoBetRule());
            engine.AddRule(new TrendSegmentNoBetRule());
            engine.AddRule(new ConfirmPointBeforeTrendRule());
            engine.AddRule(new BigGapBetweenZeroOrOneStrongRule());
            engine.AddRule(new ThreeTrackSameDirectionRule());
            engine.AddRule(new TwoTrackSameDirectionRule());
            engine.AddRule(new TrackOppositeDirectionRule());
            engine.AddRule(new KValueBreakMiddleNotTouchUpperRule());
            engine.AddRule(new KValueNearUpperRailRule()); // 添加K值接近上轨的评分规则
            engine.AddRule(new YiLouValueRule()); // 添加遗漏值评分规则
            engine.AddRule(new BollingerUpperDeclineRule()); // 添加布林上轨下降评分规则
            engine.AddRule(new ContinuousChuShouLimitRule()); // 添加连续出手限制评分规则
            engine.AddRule(new SecondChuShouLimitRule()); // 添加连续第二手限制规则
            engine.AddRule(new StopAfterWinRule()); // 添加出手中了以后停一期的规则
        }

        private async void btnLoadFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "文本文件|*.txt|所有文件|*.*";
                openFileDialog.Title = "选择开奖数据文件";
                openFileDialog.FileName = "TXFFC.txt";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        loadedFilePath = openFileDialog.FileName; // 保存文件路径
                        lblStatus.Text = "正在加载数据文件...";
                        Application.DoEvents(); // 刷新界面

                        string lastExecutedQiHao = processor.LastExecutedQiHao; // 记录之前的最后执行期号

                        // 先加载数据检查是否有更新
                        var allData = processor.LoadLotteryDataFromFile(openFileDialog.FileName);

                        // 检查数据是否更新
                        if (allData.Count == 0)
                        {
                            MessageBox.Show("文件中没有数据！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }

                        // 对于从旧到新的数据，最新的期号是列表的最后项
                        string latestQiHao = allData[allData.Count - 1].qiHao; // 最新的一期期号

                        if (latestQiHao == lastExecutedQiHao)
                        {
                            lblStatus.Text = $"数据没有更新，最新期号与最后执行期号相同：{latestQiHao}";
                            return;
                        }

                        lblStatus.Text = $"文件包含 {allData.Count} 条数据，正在处理...";
                        Application.DoEvents(); // 刷新界面

                        // 使用增量执行逻辑
                        processor.ExecuteIncrementalData(openFileDialog.FileName);

                        // 更新显示 - 最新数据在最上面
                        lstQiHao.Items.Clear();
                        // 反转顺序显示，让最新的在最上面
                        for (int i = processor.HistoryData.Count - 1; i >= 0; i--)
                        {
                            var result = processor.HistoryData[i];
                            lstQiHao.Items.Add($"{result.QiHao} - {result.Number} ({(result.IsZhongJiang ? "中" : "未中")})");
                        }

                        processedData = new List<LotteryData>(processor.HistoryData);

                        // 重新计算所有期的评分
                        lblStatus.Text = "正在计算评分...";
                        Application.DoEvents(); // 刷新界面
                        await RecalculateAllScoresAsync();

                        // 计算出手统计
                        lblStatus.Text = "正在计算出手统计...";
                        Application.DoEvents(); // 刷新界面
                        CalculateChuShouStatistics();

                        lblStatus.Text = $"已加载并处理 {lstQiHao.Items.Count} 期数据";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"加载文件时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private async void btnProcessSingle_Click(object sender, EventArgs e)
        {
            if (processor.HistoryData.Count == 0)
            {
                // 如果还没有历史数据，需要先加载数据
                MessageBox.Show("请先加载数据文件！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (string.IsNullOrEmpty(loadedFilePath) || !File.Exists(loadedFilePath))
            {
                MessageBox.Show("没有找到之前加载的文件，请先加载数据文件！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 从原始数据文件中查找比当前期号大1的期
            string currentQiHao = processor.HistoryData.Last().QiHao;
            long currentQiHaoNum = long.Parse(currentQiHao);
            long nextQiHaoNum = currentQiHaoNum + 1;
            string nextQiHao = nextQiHaoNum.ToString();

            // 从已加载的文本文件中查找下一期数据
            var allData = processor.LoadLotteryDataFromFile(loadedFilePath);

            // 在加载的数据中查找下一期数据
            var nextData = allData.FirstOrDefault(x => x.qiHao == nextQiHao);

            if (nextData.qiHao == null)
            {
                MessageBox.Show($"文本文件中没有找到期号为 {nextQiHao} 的数据！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 执行下一期数据
            var result = processor.ProcessSinglePeriod(nextData.qiHao, nextData.number);

            // 更新界面 - 清空并重新以最新在上的顺序添加所有数据
            lstQiHao.Items.Clear();
            // 反转顺序显示，让最新的在最上面
            for (int i = processor.HistoryData.Count - 1; i >= 0; i--)
            {
                var item = processor.HistoryData[i];
                lstQiHao.Items.Add($"{item.QiHao} - {item.Number} ({(item.IsZhongJiang ? "中" : "未中")})");
            }

            // 更新processedData
            processedData = new List<LotteryData>(processor.HistoryData);

            // 重新计算评分
            await RecalculateAllScoresAsync();

            lblStatus.Text = $"处理了单期数据: {result.QiHao}";
        }

        private async void btnProcessMultiple_Click(object sender, EventArgs e)
        {
            if (processor.HistoryData.Count == 0)
            {
                // 如果还没有历史数据，需要先加载数据
                MessageBox.Show("请先加载数据文件！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (string.IsNullOrEmpty(loadedFilePath) || !File.Exists(loadedFilePath))
            {
                MessageBox.Show("没有找到之前加载的文件，请先加载数据文件！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (NumericUpDown numUpDown = new NumericUpDown())
            {
                numUpDown.Minimum = 1;
                // 将最大值增加到2000
                numUpDown.Maximum = 2000;
                numUpDown.Value = 10;

                using (Form inputForm = new Form())
                {
                    inputForm.Text = "输入处理期数";
                    inputForm.Size = new Size(250, 100);
                    inputForm.StartPosition = FormStartPosition.CenterParent;

                    Label label = new Label();
                    label.Text = "请输入要处理的期数:";
                    label.Location = new Point(10, 10);
                    label.Size = new Size(150, 20);

                    numUpDown.Location = new Point(10, 30);
                    numUpDown.Size = new Size(100, 20);

                    Button okButton = new Button();
                    okButton.Text = "确定";
                    okButton.Location = new Point(10, 60);
                    okButton.Size = new Size(75, 25);
                    okButton.DialogResult = DialogResult.OK;

                    Button cancelButton = new Button();
                    cancelButton.Text = "取消";
                    cancelButton.Location = new Point(90, 60);
                    cancelButton.Size = new Size(75, 25);
                    cancelButton.DialogResult = DialogResult.Cancel;

                    inputForm.Controls.Add(label);
                    inputForm.Controls.Add(numUpDown);
                    inputForm.Controls.Add(okButton);
                    inputForm.Controls.Add(cancelButton);
                    inputForm.AcceptButton = okButton;
                    inputForm.CancelButton = cancelButton;

                    if (inputForm.ShowDialog() == DialogResult.OK)
                    {
                        int count = (int)numUpDown.Value;

                        // 从原始数据文件中查找后续期数据
                        string currentQiHao = processor.HistoryData.Last().QiHao;
                        long currentQiHaoNum = long.Parse(currentQiHao);

                        var allData = processor.LoadLotteryDataFromFile(loadedFilePath);

                        // 查找从当前期号+1开始的多个数据
                        var dataToProcess = new List<(string qiHao, string number)>();

                        for (int i = 0; i < count; i++)
                        {
                            long nextQiHaoNum = currentQiHaoNum + i + 1;
                            string nextQiHao = nextQiHaoNum.ToString();

                            // 从文件中查找这期数据
                            var foundData = allData.FirstOrDefault(x => x.qiHao == nextQiHao);

                            if (foundData.qiHao != null)
                            {
                                dataToProcess.Add(foundData);
                            }
                            else
                            {
                                // 如果找不到数据，警告用户并跳出
                                MessageBox.Show($"文本文件中没有找到期号为 {nextQiHao} 的数据！只处理了 {dataToProcess.Count} 期。",
                                    "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                break;
                            }
                        }

                        if (dataToProcess.Count == 0)
                        {
                            MessageBox.Show("没有找到符合条件的后续期数据！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }

                        // 显示处理进度
                        lblStatus.Text = $"正在处理 {dataToProcess.Count} 期数据...";
                        Application.DoEvents(); // 刷新界面

                        // 分批处理大数据集，避免界面冻结
                        const int batchSize = 50; // 每批处理50期
                        for (int batchStart = 0; batchStart < dataToProcess.Count; batchStart += batchSize)
                        {
                            int currentBatchSize = Math.Min(batchSize, dataToProcess.Count - batchStart);

                            for (int i = 0; i < currentBatchSize; i++)
                            {
                                var (qiHao, number) = dataToProcess[batchStart + i];
                                var result = processor.ProcessSinglePeriod(qiHao, number);
                            }

                            // 更新UI进度
                            int processedCount = Math.Min(batchStart + batchSize, dataToProcess.Count);
                            lblStatus.Text = $"已处理 {processedCount}/{dataToProcess.Count} 期数据...";
                            Application.DoEvents(); // 刷新界面
                        }

                        // 更新UI - 最新数据在最上面
                        lstQiHao.Items.Clear();
                        // 反转顺序显示，让最新的在最上面
                        for (int i = processor.HistoryData.Count - 1; i >= 0; i--)
                        {
                            var item = processor.HistoryData[i];
                            lstQiHao.Items.Add($"{item.QiHao} - {item.Number} ({(item.IsZhongJiang ? "中" : "未中")})");
                        }

                        processedData = new List<LotteryData>(processor.HistoryData);
                        await RecalculateAllScoresAsync(); // 异步重新计算评分

                        // 计算出手统计
                        CalculateChuShouStatistics();

                        lblStatus.Text = $"处理了 {dataToProcess.Count} 期数据";
                    }
                }
            }
        }

        private void lstQiHao_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedIndex = lstQiHao.SelectedIndex;
            if (selectedIndex >= 0 && selectedIndex < processedData.Count)
            {
                // 实际数据在列表中是按时间顺序排列的，但显示顺序是反的（最新在上）
                // 所以 selectedData 对应的是显示顺序中的选择项
                var selectedData = processedData[processedData.Count - 1 - selectedIndex]; // 因为显示顺序是反的

                // 重新计算该期的评分详情以显示完整信息
                var tempScoringEngine = new ScoringEngine();
                InitializeScoringRulesForEngine(tempScoringEngine); // 为临时评分引擎添加评分规则

                var historyForScoring = processedData.Take(processedData.Count - 1 - selectedIndex).ToList();

                // 获取评分详情
                var scoreDetails = tempScoringEngine.GetScoreDetails(selectedData, historyForScoring);

                // 显示评分详情在右侧列表框
                lstScores.Items.Clear();

                lstScores.Items.Add($"期号: {selectedData.QiHao}");
                lstScores.Items.Add($"开奖号: {selectedData.Number}");
                lstScores.Items.Add($"后三位: {selectedData.Hou3Number}");
                lstScores.Items.Add($"是否中奖: {(selectedData.IsZhongJiang ? "是" : "否")}");
                lstScores.Items.Add($"遗漏值: {selectedData.YiLouValue}");
                lstScores.Items.Add($"K值: {selectedData.KValue:F3}");

                if (selectedData.BollingerBands != null)
                {
                    lstScores.Items.Add($"布林上轨: {selectedData.BollingerBands.BollUpperValue:F3}");
                    lstScores.Items.Add($"布林中轨: {selectedData.BollingerBands.MiddleValue:F3}");
                    lstScores.Items.Add($"布林下轨: {selectedData.BollingerBands.BollLowerValue:F3}");

                    // 计算K值与布林三轨的关系
                    if (selectedData.KValue > selectedData.BollingerBands.MiddleValue)
                    {
                        lstScores.Items.Add($"K值位置: 中轨上");
                    }
                    else if (selectedData.KValue < selectedData.BollingerBands.MiddleValue)
                    {
                        lstScores.Items.Add($"K值位置: 中轨下");
                    }
                    else
                    {
                        lstScores.Items.Add($"K值位置: 与中轨持平");
                    }

                    // 检查K值是否接近上轨或中轨（0.3范围内）
                    double distanceToUpper = Math.Abs(selectedData.KValue - selectedData.BollingerBands.BollUpperValue);
                    double distanceToMiddle = Math.Abs(selectedData.KValue - selectedData.BollingerBands.MiddleValue);
                    double distanceToLower = Math.Abs(selectedData.KValue - selectedData.BollingerBands.BollLowerValue);

                    if (distanceToUpper <= 0.3)
                    {
                        lstScores.Items.Add($"K值状态: 靠近上轨 ({distanceToUpper:F3})");
                    }

                    if (distanceToMiddle <= 0.3)
                    {
                        lstScores.Items.Add($"K值状态: 靠近中轨 ({distanceToMiddle:F3})");
                    }

                    if (distanceToLower <= 0.3)
                    {
                        lstScores.Items.Add($"K值状态: 靠近下轨 ({distanceToLower:F3})");
                    }
                }
                else
                {
                    lstScores.Items.Add("布林三轨: 无（前20期无法计算或不满足计算条件）");
                }

                lstScores.Items.Add($"是否大遗漏: {(selectedData.IsDaYiLou ? "是" : "否")}");
                lstScores.Items.Add($"是否确认点: 【{(selectedData.IsQueRenDian ? "是" : "否")}】");
                lstScores.Items.Add($"是否趋势段: 【{(selectedData.IsQuShiDuan ? "是" : "否")}】");
                lstScores.Items.Add($"大遗漏后理论周期内中奖数: {selectedData.DaYiLouHouLiLunZhouQiNeiZhongJiangShu}");

                // 根据业务逻辑，当前期做出出手决定，但是否中奖要看下一期的结果
                // 所以显示出手决策和中奖结果分开显示
                lstScores.Items.Add($"是否出手: 【{(selectedData.IsChuShou ? "是" : "否")}】");
                lstScores.Items.Add($"是否中奖: {(selectedData.IsZhongJiang ? "是" : "否")}");
                lstScores.Items.Add($"出手后是否成功: 【{(selectedData.IsChuShouSuccess ? "中" : "不中")}】");

                // 显示周期相关信息
                if (selectedData.IsChuShou)
                {
                    lstScores.Items.Add($"所属周期: {selectedData.CycleNumber}");
                    lstScores.Items.Add($"周期步骤: {selectedData.CycleStep}/{processor.GetCycleLength()}");
                    lstScores.Items.Add($"周期是否完成: {(selectedData.IsCycleComplete ? "是" : "否")}");
                    lstScores.Items.Add($"周期是否爆掉: {(selectedData.IsCycleBurst ? "是" : "否")}");

                    lstScores.Items.Add($"出手后结果: {(selectedData.IsChuShouSuccess ? "中奖" : "未中奖")}");
                }

                // 显示出手验证结果（从下一期获取，如果有下一期的话）
                int currentDataIndex = processedData.FindIndex(d => d.QiHao == selectedData.QiHao);
                if (currentDataIndex >= 0 && currentDataIndex < processedData.Count - 1)
                {
                    // 如果当前不是最后一期，可以看下一期验证出手结果
                    var nextData = processedData[currentDataIndex + 1];
                    if (selectedData.IsChuShou)
                    {
                        lstScores.Items.Add($"下一期验证结果: {(selectedData.IsChuShouSuccess ? "出手成功" : "出手失败")}");
                    }
                }
                else if (currentDataIndex == processedData.Count - 1)
                {
                    lstScores.Items.Add($"当前为最后一期，出手结果待验证");
                }

                // 统计加分和减分情况
                var positiveScoreRules = scoreDetails.Where(sd => sd.IsValid && sd.Score > 0).ToList();
                var negativeScoreRules = scoreDetails.Where(sd => sd.IsValid && sd.Score < 0).ToList();
                var unmetRules = scoreDetails.Where(sd => !sd.IsValid).ToList();

                // 重新计算总分以确认评分准确性
                int recalculatedTotalScore = tempScoringEngine.CalculateTotalScore(selectedData, historyForScoring);

                lstScores.Items.Add($"总评分: {recalculatedTotalScore}分");

                lstScores.Items.Add("");
                lstScores.Items.Add("--- 评分详情 ---");

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

                // 显示未触发规则（条件不满足的规则）
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

                // 显示触发规则总数
                int activeRulesCount = positiveScoreRules.Count + negativeScoreRules.Count;
                lstScores.Items.Add($"");
                lstScores.Items.Add($"满足规则数量: {activeRulesCount} (共{scoreDetails.Count}个规则)");

                // 添加一条分隔线，使评分更清晰
                lstScores.Items.Add("");
                lstScores.Items.Add($"最终总评分: {recalculatedTotalScore}分");
            }
        }

        private void btnStartMonitor_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(loadedFilePath))
            {
                MessageBox.Show("请先加载一个文件！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                fileWatcher.StartMonitoring(loadedFilePath);
                lblMonitorStatus.Text = $"文件监控状态: 正在监控 {Path.GetFileName(loadedFilePath)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"启动文件监控失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnStopMonitor_Click(object sender, EventArgs e)
        {
            try
            {
                fileWatcher.StopMonitoring();
                lblMonitorStatus.Text = "文件监控状态: 未启动";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"停止文件监控失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnFileChanged(string message)
        {
            // 在UI线程上调用
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => OnFileChanged(message)));
                return;
            }

            // 使用Task.Run在后台线程重新计算所有评分，避免阻塞UI线程
            Task.Run(async () =>
            {
                try
                {
                    await RecalculateAllScoresAsync();

                    // 计算完成后回到UI线程更新UI
                    this.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            // 确保processor.HistoryData和processedData是同步的
                            processedData = new List<LotteryData>(processor.HistoryData);
                            UpdateUiDisplay();
                            // 重新计算出手统计数据 - 在UI线程上
                            CalculateChuShouStatistics();
                            lblStatus.Text = message;
                        }
                        catch (Exception uiEx)
                        {
                            Console.WriteLine($"UI更新错误: {uiEx.Message}");
                            lblStatus.Text = "UI更新错误: " + uiEx.Message;
                        }
                    }));
                }
                catch (Exception calcEx)
                {
                    Console.WriteLine($"评分计算错误: {calcEx.Message}");
                    this.BeginInvoke(new Action(() =>
                    {
                        lblStatus.Text = "评分计算错误: " + calcEx.Message;
                    }));
                }
            });
        }

        private void OnFileWatcherError(string errorMessage)
        {
            // 在UI线程上调用
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => OnFileWatcherError(errorMessage)));
                return;
            }

            MessageBox.Show(errorMessage, "文件监控错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            lblStatus.Text = $"文件监控错误: {errorMessage}";
        }

        private void OnFileWatcherStatusChanged(string statusMessage)
        {
            // 在UI线程上调用
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => OnFileWatcherStatusChanged(statusMessage)));
                return;
            }

            lblStatus.Text = statusMessage;
        }

        private void UpdateUiDisplay()
        {
            // 首先记录当前数据量进行对比
            int previousCount = lstQiHao.Items.Count;
            int newHistoryCount = processor.HistoryData.Count;

            // 临时禁用选择事件，防止在更新期间触发不必要的处理
            lstQiHao.SelectedIndexChanged -= lstQiHao_SelectedIndexChanged;

            // 更新期号列表显示
            lstQiHao.Items.Clear();
            // 反转顺序显示，让最新的在最上面
            for (int i = processor.HistoryData.Count - 1; i >= 0; i--)
            {
                var result = processor.HistoryData[i];
                lstQiHao.Items.Add($"{result.QiHao} - {result.Number} ({(result.IsZhongJiang ? "中" : "未中")})");
            }

            // 先更新processedData，确保在选择事件触发时使用的是最新数据
            processedData = new List<LotteryData>(processor.HistoryData);

            // 重新计算出手统计
            CalculateChuShouStatistics();

            // 如果有数据，选中最新一期（即列表中的第一项）
            if (lstQiHao.Items.Count > 0)
            {
                lstQiHao.SelectedIndex = 0; // 选中最新一期（列表顶部）
            }

            // 重新启用选择事件
            lstQiHao.SelectedIndexChanged += lstQiHao_SelectedIndexChanged;

            // 刷新控件以确保显示正确
            lstQiHao.Refresh();

            // 强制刷新界面，确保变更立即可见
            Application.DoEvents();

            // 输出调试信息
            Console.WriteLine($"UI更新: 之前条目数={previousCount}, 当前历史数据数={newHistoryCount}, 列表当前条目数={lstQiHao.Items.Count}");
            if (processor.HistoryData.Count > 0)
            {
                var latestData = processor.HistoryData[processor.HistoryData.Count - 1]; // 最新添加的数据
                Console.WriteLine($"最新期号: {latestData.QiHao}, 最新号码: {latestData.Number}");
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            // 停止文件监控
            fileWatcher.StopMonitoring();
            lblMonitorStatus.Text = "文件监控状态: 未启动";

            Reset(true);
        }
        /// <summary>
        /// 重新设置应用程序状态
        /// </summary>
        /// <param name="needReset350code">是否需要重新生成350注号码</param>
        private void Reset(bool needReset350code)
        {
            // 停止文件监控
            fileWatcher.StopMonitoring();
            lblMonitorStatus.Text = "文件监控状态: 未启动";

            processor.Reset(needReset350code);
            processedData.Clear();
            lstQiHao.Items.Clear();
            lstScores.Items.Clear();
            lstChuShouStats.Items.Clear(); // 清空出手统计
            loadedFilePath = string.Empty; // 重置文件路径
            Display350Numbers(); // 显示重置后的350注号码
            lblStatus.Text = "已重置数据";
        }

        // 新增：显示350注号码
        private void Display350Numbers()
        {
            if (processor?.Numbers350 != null)
            {
                // 将号码转换为整数，排序后转回字符串，用空格分隔
                var sortedNumbers = processor.Numbers350
                    .Select(int.Parse)  // 转换为整数
                    .OrderBy(n => n)    // 按升序排序
                    .Select(n => n.ToString("D3")); // 转回3位数字格式

                txt350Numbers.Text = string.Join(" ", sortedNumbers);
            }
        }

        // 新增：设置350注号码
        private void btnSet350Numbers_Click(object sender, EventArgs e)
        {
            try
            {
                string input = txt350Numbers.Text.Trim();
                if (string.IsNullOrEmpty(input))
                {
                    MessageBox.Show("请输入号码！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string[] numberArray = input.Split(new char[] { ',', ' ', ';', '\t', '\n', '\r' },
                    StringSplitOptions.RemoveEmptyEntries);

                var numbers = new List<string>();
                foreach (string numStr in numberArray)
                {
                    string cleanNum = numStr.Trim();
                    if (int.TryParse(cleanNum, out int num))
                    {
                        numbers.Add(num.ToString("D3")); // 转换为3位格式
                    }
                }

                if (numbers.Count > 0)
                {
                    processor.SetManual350Numbers(numbers);
                    MessageBox.Show($"成功设置{numbers.Count}个号码", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    //Display350Numbers();
                    Reset(false);
                }
                else
                {
                    MessageBox.Show("未识别到有效的号码！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"设置号码时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 新增：生成随机350注
        private void btnGenerate350Numbers_Click(object sender, EventArgs e)
        {
            processor.GenerateRandom350Numbers();
            Display350Numbers();
            lblStatus.Text = "已生成随机350注号码";
        }

        // 重新计算所有期的评分
        private void RecalculateAllScores()
        {
            for (int i = 0; i < processor.HistoryData.Count; i++)
            {
                var historyForScoring = processor.HistoryData.Take(i).ToList();

                // 重新计算当前期的总评分
                processor.HistoryData[i].Score = scoringEngine.CalculateTotalScore(
                    processor.HistoryData[i],
                    historyForScoring
                );

                // 根据评分规则设置是否出手标志
                // 需要考虑评分>=70且不在趋势段内且K值在中轨上
                bool isScoreHighEnough = processor.HistoryData[i].Score >= 70;
                bool isNotInTrendSegment = !processor.HistoryData[i].IsQuShiDuan;
                bool isKValueAboveMiddle = processor.HistoryData[i].BollingerBands != null &&
                    processor.HistoryData[i].KValue >= processor.HistoryData[i].BollingerBands.MiddleValue;

                processor.HistoryData[i].IsChuShou = isScoreHighEnough && isNotInTrendSegment && isKValueAboveMiddle;
            }

            // 计算出手后的中奖结果（在所有期的评分和出手决策都确定后再计算）
            for (int i = 0; i < processor.HistoryData.Count; i++)
            {
                // 检查当前期的前一期（上一期）是否出手
                if (i > 0 && processor.HistoryData[i - 1].IsChuShou)
                {
                    // 上一期出手，当前期开奖结果决定上一期出手是否成功
                    processor.HistoryData[i - 1].IsChuShouSuccess = processor.HistoryData[i].IsZhongJiang;
                }
            }
        }

        private async Task RecalculateAllScoresAsync()
        {
            await Task.Run(() =>
            {
                // 首先重置所有周期相关字段，确保重新计算不会受到之前结果的影响
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
                    // 需要考虑评分>=70且不在趋势段内且K值在中轨上
                    bool isScoreHighEnough = processor.HistoryData[i].Score >= 70;
                    bool isNotInTrendSegment = !processor.HistoryData[i].IsQuShiDuan;
                    bool isKValueAboveMiddle = processor.HistoryData[i].BollingerBands != null &&
                        processor.HistoryData[i].KValue >= processor.HistoryData[i].BollingerBands.MiddleValue;

                    // 检查是否连续出手超过2期，如果是则必须停一期
                    bool canContinueChuShou = true;
                    if (i >= 2)
                    {
                        // 检查前两期是否都在出手
                        bool previousTwoAreChuShou = processor.HistoryData[i - 1].IsChuShou &&
                                                     processor.HistoryData[i - 2].IsChuShou;

                        if (previousTwoAreChuShou)
                        {
                            // 如果前两期都在出手，则当前期不能出手，必须停一期
                            canContinueChuShou = false;
                        }
                    }

                    processor.HistoryData[i].IsChuShou = isScoreHighEnough && isNotInTrendSegment && isKValueAboveMiddle && canContinueChuShou;
                }

                // 第二步：根据开奖结果确定出手成功性（先只设置出手成功状态）
                for (int i = 0; i < processor.HistoryData.Count; i++)
                {
                    // 检查当前期的前一期（上一期）是否出手
                    if (i > 0 && processor.HistoryData[i - 1].IsChuShou)
                    {
                        // 上一期出手，当前期开奖结果决定上一期出手是否成功
                        processor.HistoryData[i - 1].IsChuShouSuccess = processor.HistoryData[i].IsZhongJiang;
                    }
                }

                // 第三步：按时间顺序重新计算所有出手数据的周期信息
                // 先重置周期相关状态
                for (int i = 0; i < processor.HistoryData.Count; i++)
                {
                    processor.HistoryData[i].IsCycleComplete = false;
                    processor.HistoryData[i].IsCycleBurst = false;
                    processor.HistoryData[i].IsChuShouSuccess = false;
                }

                // 第四步：按时间顺序重新计算所有出手数据的周期和步骤信息
                // 严格按顺序执行，确保每次调用CalculateChuShouCycleAndHandNumber时依赖的数据已经计算好
                for (int i = 0; i < processor.HistoryData.Count; i++)
                {
                    if (processor.HistoryData[i].IsChuShou)
                    {
                        processor.CalculateChuShouCycleAndHandNumber(processor.HistoryData[i]);
                    }
                }

                // 第五步：然后计算出手成功性
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
                        // 检查上一期出手是否完成了其所在周期
                        if (processor.HistoryData[i].IsZhongJiang) // 如果当前期中奖，则上一期出手所在的周期完成
                        {
                            // 标记上一期出手完成其周期
                            processor.HistoryData[i - 1].IsCycleComplete = true;

                            // 同时标记同一周期内的所有出手为完成
                            int currentCycleNumber = processor.HistoryData[i - 1].CycleNumber;
                            for (int j = 0; j < processor.HistoryData.Count; j++)
                            {
                                if (processor.HistoryData[j].IsChuShou &&
                                    processor.HistoryData[j].CycleNumber == currentCycleNumber)
                                {
                                    processor.HistoryData[j].IsCycleComplete = true;
                                    processor.HistoryData[j].IsCycleBurst = false; // 完成周期，不是爆掉
                                }
                            }
                        }
                        // 检查周期是否因第N步未中奖而爆掉
                        else if (processor.HistoryData[i - 1].CycleStep == processor.GetCycleLength() &&
                                i < processor.HistoryData.Count &&
                                !processor.HistoryData[i].IsZhongJiang)
                        {
                            // 标记上一期出手导致周期爆掉
                            processor.HistoryData[i - 1].IsCycleBurst = true;

                            // 同时标记整个周期爆掉
                            int currentCycleNumber = processor.HistoryData[i - 1].CycleNumber;
                            for (int j = 0; j < processor.HistoryData.Count; j++)
                            {
                                if (processor.HistoryData[j].IsChuShou &&
                                    processor.HistoryData[j].CycleNumber == currentCycleNumber)
                                {
                                    processor.HistoryData[j].IsCycleBurst = true;
                                    processor.HistoryData[j].IsCycleComplete = false; // 爆掉，不是完成
                                }
                            }
                        }
                    }
                }

                // 第七步：最后再重新计算一次周期信息，确保一致性
                for (int i = 0; i < processor.HistoryData.Count; i++)
                {
                    if (processor.HistoryData[i].IsChuShou)
                    {
                        processor.CalculateChuShouCycleAndHandNumber(processor.HistoryData[i]);
                    }
                }
            });
        }

        // 计算出手统计信息
        private void CalculateChuShouStatistics()
        {
            // 清空之前的统计数据
            if (this.lstChuShouStats != null)
            {
                try
                {
                    this.lstChuShouStats.Items.Clear();

                    // 统计出手记录
                    var chuShouRecords = processedData?.Where(d => d.IsChuShou).ToList() ?? new List<LotteryData>();

                    if (chuShouRecords.Count == 0)
                    {
                        this.lstChuShouStats.Items.Add("暂无出手记录");
                        return;
                    }

                    this.lstChuShouStats.Items.Add($"总出手次数: {chuShouRecords.Count}");
                    this.lstChuShouStats.Items.Add($"总中奖次数: {chuShouRecords.Count(d => d.IsChuShouSuccess)}");
                    this.lstChuShouStats.Items.Add($"中奖率: {(chuShouRecords.Count > 0 ? (double)chuShouRecords.Count(d => d.IsChuShouSuccess) / chuShouRecords.Count * 100 : 0):F2}%");
                    this.lstChuShouStats.Items.Add("");

                    // 按周期分组，然后基于分组结果进行统计
                    var cycles = chuShouRecords
                        .GroupBy(d => d.CycleNumber)
                        .OrderBy(g => g.Key)
                        .ToList();

                    if (cycles.Count > 0)
                    {
                        // 获取周期长度，用于整个统计过程
                        int cycleLength = processor.GetCycleLength();

                        // 统计完成的周期数和爆掉的周期数
                        int completedCycles = cycles.Count(c => c.Any(d => d.IsCycleComplete));
                        int burstCycles = cycles.Count(c => c.Any(d => d.IsCycleBurst));
                        int totalCycles = cycles.Count;

                        this.lstChuShouStats.Items.Add($"=== 周期统计数据 ===");
                        this.lstChuShouStats.Items.Add($"总周期数: {totalCycles}");
                        this.lstChuShouStats.Items.Add($"完成周期数(中奖完成): {completedCycles}");
                        this.lstChuShouStats.Items.Add($"爆掉周期数({cycleLength}期不中奖): {burstCycles}");
                        this.lstChuShouStats.Items.Add($"周期成功率: {(totalCycles > 0 ? (double)(totalCycles - burstCycles) / totalCycles * 100 : 0):F2}%");

                        // 重新基于周期分组进行统计第1-N期的出手分布
                        this.lstChuShouStats.Items.Add("");
                        this.lstChuShouStats.Items.Add($"=== 第1-{cycleLength}期出手分布 ===");
                        for (int step = 1; step <= cycleLength; step++)
                        {
                            // 遍历每个周期中的每个出手来统计步骤分布
                            var stepRecords = new List<LotteryData>();
                            foreach (var cycleGroup in cycles)
                            {
                                var cycleSteps = cycleGroup.Where(d => d.CycleStep == step).ToList();
                                stepRecords.AddRange(cycleSteps);
                            }

                            int successCount = stepRecords.Count(d => d.IsChuShouSuccess);
                            this.lstChuShouStats.Items.Add($"第{step}期中: {stepRecords.Count}次。(中奖{successCount}次)");
                        }

                        // 统计出手周期完成情况：从第1期到第N期中奖的各种情况
                        this.lstChuShouStats.Items.Add("");
                        this.lstChuShouStats.Items.Add($"=== 出手周期完成统计 (1-{cycleLength}期中奖或{cycleLength}期全不中) ===");

                        // 初始化统计数组，索引0-N分别代表在第1-N期中奖和N期都不中奖
                        int[] completionStats = new int[cycleLength + 1];
                        for (int i = 0; i < cycleLength + 1; i++)
                        {
                            completionStats[i] = 0;
                        }

                        // 遍历每个周期，统计在第几步完成或失败
                        foreach (var cycleGroup in cycles)
                        {
                            var cycleRecords = cycleGroup.OrderBy(d => d.CycleStep).ToList();

                            // 检查这个周期是完成还是爆掉
                            bool isCompleted = cycleRecords.Any(d => d.IsCycleComplete);
                            bool isBurst = cycleRecords.Any(d => d.IsCycleBurst);

                            if (isCompleted)
                            {
                                // 周期完成，找出在哪一步中奖的
                                var winningRecord = cycleRecords.FirstOrDefault(d => d.IsChuShouSuccess);
                                if (winningRecord != null)
                                {
                                    int winStep = winningRecord.CycleStep;
                                    if (winStep >= 1 && winStep <= cycleLength)
                                    {
                                        completionStats[winStep - 1]++;
                                    }
                                }
                            }
                            else if (isBurst)
                            {
                                // 周期爆掉（N期都没中）
                                completionStats[cycleLength]++;
                            }
                        }

                        // 显示统计结果
                        for (int i = 0; i < cycleLength; i++)
                        {
                            this.lstChuShouStats.Items.Add($"第{i + 1}期中奖完成: {completionStats[i]}次");
                        }
                        this.lstChuShouStats.Items.Add($"第{cycleLength}期仍未中奖: {completionStats[cycleLength]}次");

                        this.lstChuShouStats.Items.Add("");
                    }

                    this.lstChuShouStats.Items.Add("=== 按周期分组的出手记录 ===");

                    // 按周期分组显示记录
                    foreach (var cycleGroup in cycles)
                    {
                        var cycleRecords = cycleGroup.OrderBy(d => d.CycleStep).ToList();
                        bool isCompleted = cycleRecords.Any(d => d.IsCycleComplete);
                        bool isBurst = cycleRecords.Any(d => d.IsCycleBurst);

                        // 需要获取配置的周期长度
                        int currentCycleLength = processor.GetCycleLength();
                        string cycleStatus = isCompleted ? " (完成-中奖)" : (isBurst ? $" (爆掉-{currentCycleLength}期不中)" : " (进行中)");

                        this.lstChuShouStats.Items.Add($"周期 {cycleGroup.Key}{cycleStatus}: 共{cycleRecords.Count}次出手");

                        // 显示该周期内每一次出手
                        foreach (var record in cycleRecords)
                        {
                            string zhongjiangStatus = record.IsChuShouSuccess ? "中奖" : "未中";
                            string nextPeriodInfo = "";

                            // 获取下一期的开奖号，用于验证出手结果
                            int currentIndex = processedData.IndexOf(record);
                            if (currentIndex >= 0 && currentIndex < processedData.Count - 1)
                            {
                                var nextRecord = processedData[currentIndex + 1];
                                nextPeriodInfo = $"[下期{nextRecord.QiHao}:{nextRecord.Number}]";
                            }

                            this.lstChuShouStats.Items.Add($"  步骤{record.CycleStep} - 期号 {record.QiHao}: {record.Number} (出手->{zhongjiangStatus}) {nextPeriodInfo}");
                        }

                        this.lstChuShouStats.Items.Add("");
                    }
                }
                catch (Exception ex)
                {
                    // 如果出现异常，添加错误信息
                    if (this.lstChuShouStats != null)
                    {
                        this.lstChuShouStats.Items.Add($"计算出手统计时出错: {ex.Message}");
                    }
                }
            }
        }
    }
}