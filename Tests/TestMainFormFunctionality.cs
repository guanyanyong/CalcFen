using System;
using System.Collections.Generic;
using System.Linq;
using CpCodeSelect.Core;
using CpCodeSelect.Scorer;
using CpCodeSelect.Scorer.Rules;
using System.IO;

namespace TestApp
{
    /// <summary>
    /// 测试MainForm功能的完整测试用例
    /// 1. 每次生成一组纯随机的350注号码
    /// 2. 执行出手判断和是否中奖判断
    /// 3. 不中奖就继续出手，最多连续8次不中为爆掉一个计划
    /// 4. 设置循环执行步骤1和2多少次
    /// 5. 统计爆掉次数和中奖次数
    /// </summary>
    public class TestMainFormFunctionality
    {
        private LotteryProcessor processor;
        private ScoringEngine scoringEngine;
        
        public TestMainFormFunctionality()
        {
            processor = new LotteryProcessor();
            scoringEngine = new ScoringEngine();
            
            // 初始化评分规则
            InitializeScoringRules();
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
            scoringEngine.AddRule(new OpeningHornRule()); // 添加开口型喇叭评分规则
        }

        /// <summary>
        /// 测试主方法：执行指定轮次的测试
        /// </summary>
        /// <param name="rounds">要执行的轮次数</param>
        /// <param name="maxContinuousLosses">最大连续不中奖次数（超过此数视为爆掉）</param>
        public void RunTest(int rounds, int maxContinuousLosses = 8)
        {
            Console.WriteLine($"开始执行测试，共 {rounds} 轮，最大连续不中奖次数：{maxContinuousLosses}");
            Console.WriteLine("==================================================");

            int totalWins = 0;      // 总中奖次数
            int totalBursts = 0;    // 总爆掉次数
            int totalChuShou = 0;   // 总出手次数

            for (int round = 1; round <= rounds; round++)
            {
                Console.WriteLine($"第 {round} 轮测试开始...");
                
                // 重置处理器（生成新的随机350注号码）
                processor.Reset(true);
                
                // 模拟执行一轮测试：通过模拟一系列开奖号来测试出手策略
                var testResult = SimulateRound(maxContinuousLosses);
                
                totalWins += testResult.Wins;
                totalBursts += testResult.Bursts;
                totalChuShou += testResult.ChuShouCount;
                
                Console.WriteLine($"第 {round} 轮结果：中奖 {testResult.Wins} 次，爆掉 {testResult.Bursts} 次");
                Console.WriteLine($"  本轮出手 {testResult.ChuShouCount} 次，当前历史数据 {processor.HistoryData.Count} 期");
            }

            Console.WriteLine("==================================================");
            Console.WriteLine("测试完成！总体统计结果：");
            Console.WriteLine($"总共执行 {rounds} 轮测试");
            Console.WriteLine($"总中奖次数：{totalWins}");
            Console.WriteLine($"总爆掉次数：{totalBursts}");
            Console.WriteLine($"总出手次数：{totalChuShou}");
            Console.WriteLine($"平均每轮中奖：{totalWins / (double)rounds:F2} 次");
            Console.WriteLine($"平均每轮爆掉：{totalBursts / (double)rounds:F2} 次");
        }

        /// <summary>
        /// 模拟一轮测试（使用TXFFC.txt中的真实开奖数据序列并测试出手策略）
        /// 采用与MainForm相同的完整计算流程
        /// </summary>
        /// <param name="maxContinuousLosses">最大连续不中奖次数</param>
        /// <returns>测试结果统计</returns>
        private (int Wins, int Bursts, int ChuShouCount) SimulateRound(int maxContinuousLosses)
        {
            int wins = 0;
            int bursts = 0;
            int chuShouCount = 0;
            
            // 加载真实开奖数据
            var realData = LoadRealDataFromTXFFC();
            if (realData == null || realData.Count == 0)
            {
                Console.WriteLine("无法加载TXFFC.txt文件数据，使用模拟数据进行测试");
                return SimulateWithRandomData(maxContinuousLosses);
            }
            
            // 限制处理数据量以避免处理太多数据
            int maxDataCount = Math.Min(realData.Count, 1000); // 最多处理1000期数据
            var limitedData = realData.Take(maxDataCount).ToList();
            
            // 首先批量处理所有数据，建立完整的历史记录
            processor.Reset(true); // 重新生成350号码
            var batchResults = processor.ProcessMultiplePeriods(limitedData);
            
            // 然后执行与MainForm相同的完整重新计算流程
            RecalculateAllScoresConsistentWithMainForm();
            
            // 最后统计结果
            var allChuShouRecords = processor.HistoryData.Where(d => d.IsChuShou).ToList();
            chuShouCount = allChuShouRecords.Count;
            
            // 统计中奖次数：出手并且在下一期开奖中中奖
            wins = allChuShouRecords.Count(record =>
            {
                int recordIndex = processor.HistoryData.IndexOf(record);
                // 检查下一期是否中奖
                if (recordIndex >= 0 && recordIndex < processor.HistoryData.Count - 1)
                {
                    return processor.HistoryData[recordIndex + 1].IsZhongJiang;
                }
                return false;
            });
            
            // 统计爆掉次数：根据周期逻辑
            var cycles = allChuShouRecords.GroupBy(d => d.CycleNumber).ToList();
            foreach (var cycleGroup in cycles)
            {
                var cycleRecords = cycleGroup.OrderBy(d => d.CycleStep).ToList();
                bool isCompleted = cycleRecords.Any(d => d.IsCycleComplete);
                bool isBurst = cycleRecords.Any(d => d.IsCycleBurst);
                
                if (isBurst)
                {
                    bursts++;
                }
            }
            
            return (wins, bursts, chuShouCount);
        }
        
        /// <summary>
        /// 与MainForm一致的重新计算所有评分的流程
        /// </summary>
        private void RecalculateAllScoresConsistentWithMainForm()
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

            // 第二步：根据开奖结果确定出手成功性
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
                    else if (processor.HistoryData[i - 1].CycleStep == processor.GetCycleLength() && // 使用配置的周期长度
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
        }
        
        /// <summary>
        /// 从TXFFC.txt文件加载真实数据
        /// </summary>
        /// <returns>包含期号和开奖号码的元组列表</returns>
        private List<(string qiHao, string number)> LoadRealDataFromTXFFC()
        {
            try
            {
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\TXFFC.txt");
                
                // 如果相对路径不存在，尝试在当前目录查找
                if (!File.Exists(filePath))
                {
                    filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TXFFC.txt");
                }
                
                if (!File.Exists(filePath))
                {
                    Console.WriteLine("TXFFC.txt文件未找到，尝试在项目根目录查找...");
                    return null;
                }
                
                var lines = File.ReadAllLines(filePath);
                var data = new List<(string qiHao, string number)>();
                
                foreach(var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    
                    var parts = line.Split('\t');
                    if (parts.Length >= 2)
                    {
                        string qiHao = parts[0].Trim();
                        string number = parts[1].Trim();
                        
                        // 验证数据格式
                        if (!string.IsNullOrEmpty(qiHao) && !string.IsNullOrEmpty(number))
                        {
                            data.Add((qiHao, number));
                        }
                    }
                }
                
                Console.WriteLine($"从TXFFC.txt成功加载 {data.Count} 条真实开奖数据");
                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载TXFFC.txt文件时出错: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// 使用随机数据模拟（备用方法）
        /// </summary>
        /// <param name="maxContinuousLosses">最大连续不中奖次数</param>
        /// <returns>测试结果统计</returns>
        private (int Wins, int Bursts, int ChuShouCount) SimulateWithRandomData(int maxContinuousLosses)
        {
            int wins = 0;
            int bursts = 0;
            int chuShouCount = 0;
            int continuousLosses = 0;  // 当前连续不中奖次数
            
            // 模拟一定数量的开奖期数（例如100期）
            int simulationPeriods = 100;
            
            for (int period = 1; period <= simulationPeriods; period++)
            {
                // 生成随机开奖号码
                string qiHao = (1000000 + period).ToString(); // 模拟期号，如 1000001, 1000002...
                string number = GenerateRandomWinningNumber();
                
                // 处理这一期数据
                var lotteryData = processor.ProcessSinglePeriod(qiHao, number);
                
                // 重新计算评分（这里我们简化为使用预设的规则）
                var historyForScoring = processor.HistoryData.Take(processor.HistoryData.Count - 1).ToList();
                lotteryData.Score = scoringEngine.CalculateTotalScore(lotteryData, historyForScoring);
                
                // 判断是否出手（参考MainForm中的逻辑）
                bool isScoreHighEnough = lotteryData.Score >= 70;
                bool isNotInTrendSegment = !lotteryData.IsQuShiDuan;
                bool isKValueAboveMiddle = lotteryData.BollingerBands != null &&
                                          lotteryData.KValue >= lotteryData.BollingerBands.MiddleValue;
                
                // 检查是否连续出手超过2期，如果是则必须停一期
                bool canContinueChuShou = true;
                if (processor.HistoryData.Count >= 2)
                {
                    bool previousTwoAreChuShou = processor.HistoryData[processor.HistoryData.Count - 1].IsChuShou &&
                                                 processor.HistoryData[processor.HistoryData.Count - 2].IsChuShou;
                    if (previousTwoAreChuShou)
                    {
                        canContinueChuShou = false;
                    }
                }
                
                bool shouldChuShou = isScoreHighEnough && isNotInTrendSegment && isKValueAboveMiddle && canContinueChuShou;
                
                if (shouldChuShou)
                {
                    chuShouCount++;
                    
                    // 应用出手逻辑
                    lotteryData.IsChuShou = true;
                    processor.CalculateChuShouCycleAndHandNumber(lotteryData);
                    
                    // 检查出手结果（当前期出手，下一期开奖决定是否中奖）
                    // 在这个模拟中，我们检查当前出手是否成功
                    if (processor.HistoryData.Count > 1)
                    {
                        var previousData = processor.HistoryData[processor.HistoryData.Count - 2];
                        if (previousData.IsChuShou)
                        {
                            // 上一期出手，当前期开奖验证是否成功
                            previousData.IsChuShouSuccess = lotteryData.IsZhongJiang;
                            
                            if (lotteryData.IsZhongJiang)
                            {
                                // 中奖了，重置连续不中奖计数
                                wins++;
                                continuousLosses = 0;
                                
                                // 标记周期完成
                                previousData.IsCycleComplete = true;
                                
                                // 同一周期内其他出手也标记完成
                                int currentCycleNumber = previousData.CycleNumber;
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
                            else
                            {
                                // 未中奖，增加连续不中奖计数
                                continuousLosses++;
                                
                                // 检查是否达到连续出手最大次数，如果是则爆掉
                                if (previousData.CycleStep >= maxContinuousLosses)
                                {
                                    // 周期爆掉
                                    bursts++;
                                    
                                    // 标记整个周期爆掉
                                    int currentCycleNumber = previousData.CycleNumber;
                                    for (int j = 0; j < processor.HistoryData.Count; j++)
                                    {
                                        if (processor.HistoryData[j].IsChuShou &&
                                            processor.HistoryData[j].CycleNumber == currentCycleNumber)
                                        {
                                            processor.HistoryData[j].IsCycleBurst = true;
                                            processor.HistoryData[j].IsCycleComplete = false;
                                        }
                                    }
                                    
                                    // 重置连续不中奖计数
                                    continuousLosses = 0;
                                }
                            }
                        }
                    }
                }
            }
            
            return (wins, bursts, chuShouCount);
        }

        /// <summary>
        /// 生成随机开奖号码（3位数字）
        /// </summary>
        private readonly Random _random = new Random();

        private string GenerateRandomWinningNumber()
        {
            int number = _random.Next(0, 1000);
            return number.ToString("D3");
        }
    }
}