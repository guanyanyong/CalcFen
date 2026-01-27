using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CpCodeSelect.Model;
using CpCodeSelect.Model.ExModel;
using CpCodeSelect.Util;

namespace CpCodeSelect.Core
{
    public class LotteryProcessor
    {
        public List<string> Numbers350 { get; set; }
        public List<LotteryData> HistoryData { get; set; }
        public string LastExecutedQiHao { get; set; } // 记录最后执行的期号
        private Random random = new Random();

        public LotteryProcessor()
        {
            HistoryData = new List<LotteryData>();
            LastExecutedQiHao = string.Empty;
            GenerateRandom350Numbers();
        }

        /// <summary>
        /// 生成350个随机号码
        /// </summary>
        public void GenerateRandom350Numbers()
        {
            Numbers350 = new List<string>();
            HashSet<string> uniqueNumbers = new HashSet<string>();

            while (uniqueNumbers.Count < 350)
            {
                int num = random.Next(0, 1000); // 0-999
                string numStr = num.ToString("D3"); // 格式化为3位数，前面补0
                uniqueNumbers.Add(numStr);
            }

            Numbers350 = uniqueNumbers.ToList();
        }

        /// <summary>
        /// 手动设置350注号码
        /// </summary>
        /// <param name="numbers"></param>
        public void SetManual350Numbers(List<string> numbers)
        {
            Numbers350 = new List<string>();
            foreach (var num in numbers.Take(350))
            {
                // 确保是3位格式
                string formattedNum = num.PadLeft(3, '0');
                if (formattedNum.Length > 3) formattedNum = formattedNum.Substring(formattedNum.Length - 3);
                Numbers350.Add(formattedNum);
            }
        }

        /// <summary>
        /// 从文件加载开奖数据
        /// 文件中第一行是最新的期号，但需要按时间顺序从最早到最晚处理
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public List<(string qiHao, string number)> LoadLotteryDataFromFile(string filePath)
        {
            var result = new List<(string qiHao, string number)>();
            var lines = File.ReadAllLines(filePath);

            // 文件中第一行是最新的期号，为了按时间顺序从旧到新处理，
            // 我们需要颠倒顺序，让最早的期号在前面
            for (int i = lines.Length - 1; i >= 0; i--)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;
                
                var parts = line.Split('\t');
                if (parts.Length >= 2)
                {
                    result.Add((parts[0].Trim(), parts[1].Trim()));
                }
            }

            // result现在按时间顺序排列，即期号从小到大
            return result;
        }

        /// <summary>
        /// 完整执行所有数据（首次执行）
        /// </summary>
        public void ExecuteAllData(string filePath)
        {
            var allData = LoadLotteryDataFromFile(filePath);
            
            // 清空历史数据
            HistoryData.Clear();
            
            // 按时间顺序处理（从旧到新）
            foreach (var (qiHao, number) in allData)
            {
                var lotteryData = ProcessSinglePeriod(qiHao, number);
                // 更新最后执行的期号
                LastExecutedQiHao = qiHao;
            }
        }

        /// <summary>
        /// 增量执行：仅处理自上次执行后的新数据
        /// </summary>
        public void ExecuteIncrementalData(string filePath)
        {
            var allData = LoadLotteryDataFromFile(filePath);
            
            // 如果没有最后执行的期号，说明是首次执行
            if (string.IsNullOrEmpty(LastExecutedQiHao))
            {
                ExecuteAllData(filePath);
                return;
            }

            // 查找最后执行期号在数据中的位置
            int startIndex = -1;
            for (int i = 0; i < allData.Count; i++)
            {
                if (allData[i].qiHao == LastExecutedQiHao)
                {
                    startIndex = i;
                    break;
                }
            }

            // 如果没有找到最后执行的期号，或者是数据中最新的期号，则无需执行
            if (startIndex == -1)
            {
                // 最后执行的期号在数据中找不到，可能是数据有变化，重新执行全部
                ExecuteAllData(filePath);
                return;
            }
            
            // 如果最后执行的期号是数据中最新的期号，也不需要执行
            if (startIndex == 0)
            {
                return; // 数据没有更新，无需执行
            }

            // 从下一个期号开始执行新数据
            for (int i = startIndex + 1; i < allData.Count; i++) // 从较旧到较新执行
            {
                var (qiHao, number) = allData[i];
                var lotteryData = ProcessSinglePeriod(qiHao, number);
                // 更新最后执行的期号
                LastExecutedQiHao = qiHao;
            }
        }

        /// <summary>
        /// 处理单期数据
        /// </summary>
        /// <param name="qiHao">期号</param>
        /// <param name="number">开奖号码</param>
        /// <returns></returns>
        public LotteryData ProcessSinglePeriod(string qiHao, string number)
        {
            var lotteryData = new LotteryData
            {
                QiHao = qiHao,
                Number = number,
                Hou3Number = number.Substring(Math.Max(0, number.Length - 3)) // 取后3位
            };

            // 判断是否中奖
            lotteryData.IsZhongJiang = Numbers350.Contains(lotteryData.Hou3Number);

            // 计算K值
            double previousKValue = 0;
            if (HistoryData.Count > 0)
            {
                previousKValue = HistoryData.Last().KValue;
            }
            
            if (lotteryData.IsZhongJiang)
            {
                lotteryData.KValue = previousKValue + 1.857;
            }
            else
            {
                lotteryData.KValue = previousKValue - 1;
            }

            // 计算遗漏值
            CalculateYiLouValue(lotteryData);

            // 计算布林带
            CalculateBollingerBands(lotteryData);

            // 计算其他指标
            CalculateOtherIndicators(lotteryData);

            // 添加到历史数据
            HistoryData.Add(lotteryData);

            return lotteryData;
        }

        /// <summary>
        /// 计算遗漏值
        /// </summary>
        private void CalculateYiLouValue(LotteryData currentData)
        {
            // 如果是第一期，遗漏值为0
            if (HistoryData.Count == 0)
            {
                currentData.YiLouValue = 0;
                return;
            }

            if (currentData.IsZhongJiang)
            {
                // 如果当前期中奖，遗漏值为0（表示从上次中奖到现在，当前期就中奖了，没有遗漏）
                currentData.YiLouValue = 0;
            }
            else
            {
                // 如果当前期未中奖，计算从上一次中奖到当前期的未中奖期数
                // 从最新的历史数据开始向前查找，寻找上一次中奖的期
                int lastWinIndex = -1;
                for (int i = HistoryData.Count - 1; i >= 0; i--)
                {
                    if (HistoryData[i].IsZhongJiang)
                    {
                        lastWinIndex = i;
                        break;
                    }
                }

                if (lastWinIndex == -1)
                {
                    // 如果历史上从来没有中奖，当前期未中奖，遗漏值为当前期数（包含当前期）
                    currentData.YiLouValue = HistoryData.Count;
                }
                else
                {
                    // 计算从上次中奖后到当前期（包含当前期）有多少期未中奖
                    currentData.YiLouValue = HistoryData.Count - lastWinIndex;
                }
            }
        }

        /// <summary>
        /// 计算布林带
        /// </summary>
        private void CalculateBollingerBands(LotteryData currentData)
        {
            // 需要至少20个K值才能计算布林带
            if (HistoryData.Count >= 20)
            {
                // 获取最近的20个K值
                var kValues = HistoryData.Skip(Math.Max(0, HistoryData.Count - 20)).Take(20).Select(d => d.KValue).ToArray();

                if (kValues.Length >= 20)
                {
                    var (middle, upper, lower) = BollingerBandsSimple.CalculateBollingerBands(kValues);
                    
                    currentData.BollingerBands = new Bolling
                    {
                        MiddleValue = middle,
                        BollUpperValue = upper,
                        BollLowerValue = lower
                    };
                }
            }
            // 如果历史数据不足20期，currentData.BollingerBands将保持null
        }

        /// <summary>
        /// 计算其他指标
        /// </summary>
        /// <param name="currentData"></param>
        private void CalculateOtherIndicators(LotteryData currentData)
        {
            // 判断是否大遗漏
            currentData.IsDaYiLou = currentData.YiLouValue >= 2;

            // 计算连中连挂次数
            CalculateConsecutiveCounts(currentData);

            // 判断是否确认点
            CheckConfirmPoint(currentData);

            // 判断是否趋势段
            CheckTrendSegment(currentData);
            
            // 判断是否出手：根据评分规则，当总评分达到70分且不在趋势段内且K值在中轨之上时可以出手投注
            bool isScoreHighEnough = currentData.Score >= 70;
            bool isNotInTrendSegment = !currentData.IsQuShiDuan;
            bool isKValueAboveMiddle = currentData.BollingerBands != null && 
                                      currentData.KValue >= currentData.BollingerBands.MiddleValue;
            
            // 检查是否连续出手超过2期，如果是则必须停一期
            bool canContinueChuShou = true;
            if (HistoryData.Count >= 2)
            {
                // 检查前两期是否都在出手
                bool previousTwoAreChuShou = HistoryData[HistoryData.Count - 1].IsChuShou && 
                                             HistoryData[HistoryData.Count - 2].IsChuShou;
                
                if (previousTwoAreChuShou)
                {
                    // 如果前两期都在出手，则当前期不能出手，必须停一期
                    canContinueChuShou = false;
                }
            }
            
            // 在当前期决定是否出手
            if (isScoreHighEnough && isNotInTrendSegment && isKValueAboveMiddle && canContinueChuShou)
            {
                currentData.IsChuShou = true;
                
                // 计算出手周期和手数
                CalculateChuShouCycleAndHandNumber(currentData);
            }
            else
            {
                currentData.IsChuShou = false;
                currentData.HandNumber = 0;  // 未出手时手数为0
                currentData.IsPartOfCycle = false;  // 未出手时不属于周期
            }
            
            // 检查历史数据中的上一期，如果上一期决定出手，则在当前期验证出手结果
            if (HistoryData.Count > 0)
            {
                var previousData = HistoryData.Last(); // 获取上一期数据
                if (previousData.IsChuShou) // 如果上一期决定出手
                {
                    // 验证上一期出手的结果：上一期出手后，下一期（即当前期）是否中奖
                    // 在上一期出手后，当前期开奖的中奖结果决定了上一期出手是否成功
                    previousData.IsChuShouSuccess = currentData.IsZhongJiang;
                    
                    // 检查上一期出手是否完成了其所在周期
                    if (currentData.IsZhongJiang) // 如果当前期中奖，则上一期出手所在的周期完成
                    {
                        previousData.IsCycleComplete = true;
                        
                        // 同时也需要标记同一周期内的其他出手也已完成周期
                        for (int i = HistoryData.Count - 1; i >= 0; i--)
                        {
                            if (HistoryData[i].IsChuShou && 
                                HistoryData[i].CycleNumber == previousData.CycleNumber)
                            {
                                HistoryData[i].IsCycleComplete = true;
                                HistoryData[i].IsCycleBurst = false; // 完成周期，不是爆掉
                            }
                            else if (HistoryData[i].CycleNumber < previousData.CycleNumber)
                            {
                                // 如果周期号更小，说明不再同一周期内，可以停止查找
                                break;
                            }
                        }
                    }
                    else if (previousData.CycleStep == 8) // 如果上一期出手是第8步
                    {
                        // 如果当前期没有中奖，并且上一期出手是第8步，则周期爆掉
                        if (!currentData.IsZhongJiang)
                        {
                            // 标记整个周期爆掉
                            int currentCycleNumber = previousData.CycleNumber;
                            for (int i = HistoryData.Count - 1; i >= 0; i--)
                            {
                                if (HistoryData[i].IsChuShou && 
                                    HistoryData[i].CycleNumber == currentCycleNumber)
                                {
                                    HistoryData[i].IsCycleBurst = true;
                                    HistoryData[i].IsCycleComplete = false; // 爆掉，不是完成
                                }
                                else if (HistoryData[i].CycleNumber < currentCycleNumber)
                                {
                                    // 如果周期号更小，说明不再同一周期内，可以停止查找
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 计算出手周期和出手手数
        /// 严格按照"中奖或达到8期"来完成一个周期的规则
        /// </summary>
        /// <param name="currentData"></param>
        public void CalculateChuShouCycleAndHandNumber(LotteryData currentData)
        {
            // 如果没有出手，直接返回
            if (!currentData.IsChuShou)
            {
                currentData.HandNumber = 0;
                currentData.IsPartOfCycle = false;
                currentData.CycleNumber = 0;
                currentData.CycleStep = 0;
                return;
            }

            // 从头开始重新计算所有出手的周期信息，以确保准确性
            // 找出所有出手记录及其索引
            var allChuShouRecords = new List<(int index, LotteryData data)>();
            for (int i = 0; i < HistoryData.Count; i++)
            {
                if (HistoryData[i].IsChuShou)
                {
                    allChuShouRecords.Add((i, HistoryData[i]));
                    
                    // 如果已经计算到了目标出手，停止添加新记录（但我们仍需要完整循环来计算中间值）
                    if (i == HistoryData.IndexOf(currentData))
                        break;
                }
            }

            // 按照严格的周期规则进行计算：
            // 1. 每个周期从步骤1开始
            // 2. 当出现中奖或达到第8步时，当前周期完成，下一出手开始新周期
            int currentCycle = 1;
            int stepOfCurrentCycle = 1;

            for (int i = 0; i < allChuShouRecords.Count; i++)
            {
                var (index, record) = allChuShouRecords[i];
                
                // 检查是否需要开启新周期
                if (i > 0) // 不是第一个出手
                {
                    var (prevIndex, prevRecord) = allChuShouRecords[i-1];
                    
                    // 检查前一个出手是否导致周期完成
                    // 在前一个出手之后，检查是否会开启新周期
                    int nextResultIndex = prevIndex + 1; // 前一个出手的下一期结果
                    
                    if (nextResultIndex < HistoryData.Count)
                    {
                        // 如果前一个出手的下一期中奖了，或者前一个出手是第8步且下一期未中奖，则前一个周期完成
                        bool prevCycleCompleted = false;
                        
                        // 对于第一个出手，我们不能检查它的前一个出手
                        // 检查是否前一个出手导致了周期完成
                        if (HistoryData[nextResultIndex].IsZhongJiang || prevRecord.CycleStep == 8)
                        {
                            currentCycle = prevRecord.CycleNumber + 1;
                            stepOfCurrentCycle = 1;
                            prevCycleCompleted = true;
                        }
                        else
                        {
                            // 继续当前周期
                            stepOfCurrentCycle = prevRecord.CycleStep + 1;
                        }
                    }
                    else
                    {
                        // 这是当前最新的数据，无法检查是否会中奖，保守估计继续当前周期
                        stepOfCurrentCycle = prevRecord.CycleStep + 1;
                    }
                }
                
                // 限制步骤不超过8
                stepOfCurrentCycle = Math.Min(stepOfCurrentCycle, 8);
                
                // 为当前出手分配周期信息
                record.CycleNumber = currentCycle;
                record.CycleStep = stepOfCurrentCycle;
                record.IsPartOfCycle = true;
                record.HandNumber = stepOfCurrentCycle;
                
                // 如果这是我们要计算的目标数据，保存结果
                if (index == HistoryData.IndexOf(currentData))
                {
                    currentData.CycleNumber = record.CycleNumber;
                    currentData.CycleStep = record.CycleStep;
                    currentData.IsPartOfCycle = record.IsPartOfCycle;
                    currentData.HandNumber = record.HandNumber;
                }
            }
            
            // 调试输出（如果需要）
            // Console.WriteLine($"周期[{currentData.CycleNumber}] 步骤[{currentData.CycleStep}] - 期号: {currentData.QiHao}");
        }

        /// <summary>
        /// 计算连中连挂次数
        /// </summary>
        /// <param name="currentData"></param>
        private void CalculateConsecutiveCounts(LotteryData currentData)
        {
            if (HistoryData.Count == 0)
            {
                currentData.LianXuZhongJiangCount = currentData.IsZhongJiang ? 1 : 0;
                currentData.LianXuWeiZhongJiangCount = currentData.IsZhongJiang ? 0 : 1;
                return;
            }

            var prevData = HistoryData.Last();
            if (currentData.IsZhongJiang)
            {
                currentData.LianXuZhongJiangCount = prevData.IsZhongJiang ? 
                    prevData.LianXuZhongJiangCount + 1 : 1;
                currentData.LianXuWeiZhongJiangCount = 0;
            }
            else
            {
                currentData.LianXuZhongJiangCount = 0;
                currentData.LianXuWeiZhongJiangCount = prevData.IsZhongJiang ? 
                    1 : prevData.LianXuWeiZhongJiangCount + 1;
            }
        }

        /// <summary>
        /// 检查确认点
        /// </summary>
        /// <param name="currentData"></param>
        private void CheckConfirmPoint(LotteryData currentData)
        {
            // 确认点：大遗漏之后中奖，且在中奖后理论周期内再次中奖
            // 理论周期是2，即遗漏值为0或1
            if (currentData.IsZhongJiang) // 当前中奖
            {
                if (HistoryData.Count >= 1) // 至少有1期历史数据
                {
                    // 查找最近的大遗漏
                    int lastBigGapIndex = -1;
                    for (int i = HistoryData.Count - 1; i >= 0; i--)
                    {
                        if (HistoryData[i].IsDaYiLou)
                        {
                            lastBigGapIndex = i;
                            break;
                        }
                    }

                    if (lastBigGapIndex >= 0)
                    {
                        // 从大遗漏后开始找第一次中奖
                        int firstWinAfterGap = -1;
                        for (int i = lastBigGapIndex + 1; i < HistoryData.Count; i++)
                        {
                            if (HistoryData[i].IsZhongJiang)
                            {
                                firstWinAfterGap = i;
                                break;
                            }
                        }

                        // 如果找到了大遗漏后的第一次中奖
                        if (firstWinAfterGap >= 0)
                        {
                            // 当前期是否相对于第一次中奖在理论周期内
                            // 计算从第一次中奖到现在有多少期（不包含第一次中奖期本身）
                            int periodsSinceFirstWin = HistoryData.Count - firstWinAfterGap;
                            
                            // 当前期是否在理论周期内（遗漏值≤1 且 自从第一次中奖以来期数不超过2）
                            bool isInCycle = currentData.YiLouValue <= 1 && periodsSinceFirstWin <= 2;
                            
                            if (isInCycle)
                            {
                                // 检查从第一次中奖后到当前期之间是否都是在理论周期内
                                bool allInCycle = true;
                                for (int i = firstWinAfterGap; i < HistoryData.Count; i++)
                                {
                                    if (HistoryData[i].IsZhongJiang) 
                                    {
                                        // 检查该中奖期是否在理论周期内（遗漏值 <= 1）
                                        if (HistoryData[i].YiLouValue > 1)
                                        {
                                            allInCycle = false;
                                            break;
                                        }
                                    }
                                }

                                if (allInCycle)
                                {
                                    currentData.IsQueRenDian = true;
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 检查趋势段
        /// </summary>
        /// <param name="currentData"></param>
        private void CheckTrendSegment(LotteryData currentData)
        {
            // 如果当前期是大遗漏，则趋势段结束或不开始，重新计算从当前期开始
            if (currentData.IsDaYiLou)
            {
                currentData.IsQuShiDuan = false;
                currentData.QuShiDuanZhongJiangCount = 0; // 重置趋势段中奖计数
                currentData.DaYiLouHouLiLunZhouQiNeiZhongJiangShu = 0; // 重置大遗漏后理论周期内中奖数
                return;
            }

            // 如果历史数据为空，当前期不是趋势段
            if (HistoryData.Count == 0)
            {
                currentData.IsQuShiDuan = false;
                currentData.QuShiDuanZhongJiangCount = 0;
                currentData.DaYiLouHouLiLunZhouQiNeiZhongJiangShu = 0;
                return;
            }

            // 简化的趋势段逻辑：只按大遗漏后形成趋势段的逻辑
            // 大遗漏过后理论周期内开出第4个中奖（或者到达第5个期如4中1挂）的当期开始是趋势段
            int latestBigGapIndex = -1;
            for (int i = HistoryData.Count - 1; i >= 0; i--)
            {
                if (HistoryData[i].IsDaYiLou)
                {
                    latestBigGapIndex = i;
                    break;
                }
            }

            if (latestBigGapIndex >= 0)
            {
                // 重新计算：从大遗漏后的第一期开始，统计理论周期内的中奖次数
                int winsAfterBigGap = 0;
                
                // 从大遗漏后第一期开始计算，直到当前期
                for (int i = latestBigGapIndex + 1; i < HistoryData.Count; i++)
                {
                    // 只统计中奖且在理论周期内（遗漏值≤1）的
                    if (HistoryData[i].IsZhongJiang && HistoryData[i].YiLouValue <= 1)
                    {
                        winsAfterBigGap++;
                    }
                }
                
                // 如果当前期也在理论周期内（遗漏值≤1）且中奖，则计入
                if (currentData.IsZhongJiang && currentData.YiLouValue <= 1)
                {
                    winsAfterBigGap++;
                }

                // 记录大遗漏后理论周期内的中奖数
                currentData.DaYiLouHouLiLunZhouQiNeiZhongJiangShu = winsAfterBigGap;

                // 判断当前期是否在大遗漏后的理论周期内（遗漏值≤1）
                bool isWithinTheoryPeriod = currentData.YiLouValue <= 1;
                
                // 计算大遗漏后理论周期内的总期数（包括中奖和未中奖）
                int totalTheoryPeriodCount = 0;
                for (int i = latestBigGapIndex + 1; i < HistoryData.Count; i++)
                {
                    if (HistoryData[i].YiLouValue <= 1) // 在理论周期内
                    {
                        totalTheoryPeriodCount++;
                    }
                }
                if (isWithinTheoryPeriod) // 当前期也在理论周期内
                {
                    totalTheoryPeriodCount++;
                }
                
                // 趋势段条件：当前期在大遗漏后的理论周期内，且满足以下条件之一：
                // 1. 理论周期内已经有4个中奖
                // 2. 理论周期内已经有5个期（如4中1挂）
                bool shouldEnterTrendSegment = isWithinTheoryPeriod && (winsAfterBigGap >= 4 || totalTheoryPeriodCount >= 5);

                if (shouldEnterTrendSegment)
                {
                    currentData.IsQuShiDuan = true;
                    currentData.QuShiDuanZhongJiangCount = winsAfterBigGap; // 更新趋势段中奖计数
                }
                else
                {
                    currentData.IsQuShiDuan = false;
                    currentData.QuShiDuanZhongJiangCount = winsAfterBigGap; // 更新趋势段中奖计数
                }
            }
            else
            {
                // 如果没有找到大遗漏，当前期不是趋势段
                currentData.IsQuShiDuan = false;
                currentData.QuShiDuanZhongJiangCount = 0;
                currentData.DaYiLouHouLiLunZhouQiNeiZhongJiangShu = 0;
            }
        }

        /// <summary>
        /// 批量处理数据（从旧到新）
        /// </summary>
        public List<LotteryData> ProcessBatchPeriods(List<(string qiHao, string number)> dataList)
        {
            // 清空历史数据
            HistoryData.Clear();
            
            var results = new List<LotteryData>();
            
            // 按时间顺序处理（从旧到新）
            foreach (var (qiHao, number) in dataList)
            {
                var result = ProcessSinglePeriod(qiHao, number);
                results.Add(result);
                
                // 更新最后执行的期号
                LastExecutedQiHao = qiHao;
            }
            
            return results;
        }

        /// <summary>
        /// 处理多期数据（按输入顺序处理）
        /// </summary>
        /// <param name="dataList"></param>
        /// <returns></returns>
        public List<LotteryData> ProcessMultiplePeriods(List<(string qiHao, string number)> dataList)
        {
            // 清空历史数据
            HistoryData.Clear();
            
            var results = new List<LotteryData>();
            
            // 按照输入顺序处理
            foreach (var (qiHao, number) in dataList)
            {
                var result = ProcessSinglePeriod(qiHao, number);
                results.Add(result);
                
                // 更新最后执行的期号
                LastExecutedQiHao = qiHao;
            }
            
            return results;
        }

        /// <summary>
        /// 重新初始化处理器
        /// </summary>
        public void Reset()
        {
            HistoryData.Clear();
            LastExecutedQiHao = string.Empty;
            GenerateRandom350Numbers();
        }
    }
}