using System;
using System.Collections.Generic;
using System.Linq;
using CpCodeSelect.Model;

namespace CpCodeSelect.Engine
{
    /// <summary>
    /// 彩票决策引擎 - 用于对彩票号码进行评分、决策是否出手，并跟踪历史表现
    /// </summary>
    public class LotteryDecisionEngine
    {
        /// <summary>
        /// 存储彩票号码历史评分和出手信息的字典
        /// Key: 期号，Value: 决策记录
        /// </summary>
        private Dictionary<string, DecisionRecord> decisionHistory;

        /// <summary>
        /// 当前的350注号码列表
        /// </summary>
        private List<string> numbers350;

        /// <summary>
        /// 决策阈值 - 评分高于此值的号码才会考虑出手
        /// </summary>
        public int DecisionThreshold { get; set; } = 70;

        /// <summary>
        /// 构造函数
        /// </summary>
        public LotteryDecisionEngine()
        {
            decisionHistory = new Dictionary<string, DecisionRecord>();
            numbers350 = new List<string>();
        }

        /// <summary>
        /// 设置350注号码
        /// </summary>
        /// <param name="numbers">350注号码列表</param>
        public void SetNumbers350(List<string> numbers)
        {
            if (numbers != null)
            {
                // 确保号码格式为3位数字格式
                numbers350 = numbers.Select(num => num.ToString().PadLeft(3, '0')).Take(350).ToList();
            }
            else
            {
                numbers350 = new List<string>();
            }
        }

        /// <summary>
        /// 通过逗号或空格分割的字符串设置350注号码
        /// </summary>
        /// <param name="numbersString">包含号码的字符串，可以用逗号或空格分割</param>
        public void SetNumbers350FromString(string numbersString)
        {
            if (string.IsNullOrWhiteSpace(numbersString))
            {
                numbers350 = new List<string>();
                return;
            }

            // 使用逗号和空格作为分隔符拆分字符串
            char[] separators = { ',', ' ', ';', '\t', '\n', '\r' };
            var numberStrings = numbersString.Split(separators, StringSplitOptions.RemoveEmptyEntries);

            // 过滤有效号码并转换格式
            numbers350 = numberStrings
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim())
                .Where(s => IsValidNumber(s)) // 验证号码格式
                .Select(num => num.PadLeft(3, '0')) // 格式化为3位数
                .Take(350) // 限制为最多350个
                .ToList();
        }

        /// <summary>
        /// 验证号码格式是否有效
        /// </summary>
        /// <param name="number">号码字符串</param>
        /// <returns>是否为有效号码</returns>
        private bool IsValidNumber(string number)
        {
            // 检查是否为纯数字，且长度不超过3位
            return !string.IsNullOrEmpty(number) && 
                   number.All(char.IsDigit) && 
                   number.Length <= 3;
        }

        /// <summary>
        /// 获取当前的350注号码
        /// </summary>
        /// <returns>350注号码列表</returns>
        public List<string> GetNumbers350()
        {
            return new List<string>(numbers350);
        }

        /// <summary>
        /// 基于历史记录和当前期进行评分并判断是否出手
        /// </summary>
        /// <param name="currentQiHao">当前期号</param>
        /// <param name="currentNumber">当前开奖号码</param>
        /// <param name="historyData">历史开奖数据列表</param>
        /// <returns>DecisionResult对象，包含评分、是否出手等信息</returns>
        public DecisionResult EvaluateCurrentPeriod(string currentQiHao, string currentNumber, List<LotteryData> historyData)
        {
            // 计算整体策略评分
            var strategyData = new LotteryData
            {
                QiHao = currentQiHao,
                Number = currentNumber  // 使用当前开奖号进行分析
            };

            // 计算评分（基于当前期号和历史数据的分析）
            int score = CalculateScore(strategyData, historyData);

            // 确定是否出手（从350注号码池中进行投注）
            bool shouldChuShou = ShouldChuShou(score, strategyData, historyData);

            // 创建决策记录
            var decisionRecord = new DecisionRecord
            {
                QiHao = currentQiHao,
                Number = currentNumber,  // 记录实际开奖号码，用于后续中奖判断参考
                Score = score,
                ShouldChuShou = shouldChuShou,
                DecisionTime = DateTime.Now,
                HistoryDataUsed = historyData.ToList(), // 保存当时的历史数据快照
                Numbers350AtDecision = GetNumbers350() // 保存当时的350注号码
            };

            // 保存决策记录
            decisionHistory[currentQiHao] = decisionRecord;

            // 返回决策结果
            return new DecisionResult
            {
                QiHao = currentQiHao,
                Number = currentNumber,
                Score = score,
                ShouldChuShou = shouldChuShou,
                Message = $"评分为{score}分，{(shouldChuShou ? "建议出手（从350注号码池中进行投注）" : "不建议出手")}"
            };
        }

        /// <summary>
        /// 评估350注号码中每一注的评分和出手建议
        /// </summary>
        /// <param name="currentQiHao">当前期号</param>
        /// <param name="historyData">历史开奖数据列表</param>
        /// <returns>包含每注号码评分和建议的列表</returns>
        public List<NumberEvaluationResult> EvaluateNumbers350(string currentQiHao, List<LotteryData> historyData)
        {
            var evaluationResults = new List<NumberEvaluationResult>();

            if (numbers350 == null || !numbers350.Any())
            {
                return evaluationResults;
            }

            // 为了评估350注号码，我们需要为每一注创建一个模拟的当前期数据，
            // 然后使用相同的评分逻辑来为每一注打分
            for (int i = 0; i < numbers350.Count; i++)
            {
                var number = numbers350[i];
                
                // 创建模拟当前期数据用于评分 - 这里使用当前期号和350注中的号码
                var mockCurrentData = new LotteryData
                {
                    QiHao = currentQiHao,
                    Number = number
                };

                // 计算该号码的评分
                int score = CalculateScore(mockCurrentData, historyData);

                // 创建一个临时的LotteryData对象用于评估是否应出手（不实际保存到决策历史）
                var tempMockData = new LotteryData
                {
                    QiHao = currentQiHao,
                    Number = number
                };

                // 确定是否建议出手该号码（使用当前的350注号码列表）
                // 由于ShouldChuShou内部会检查350注号码列表，这里需要确保使用的是模拟数据
                bool shouldChuShou = ShouldChuShou(score, tempMockData, historyData);

                evaluationResults.Add(new NumberEvaluationResult
                {
                    Number = number,
                    Score = score,
                    ShouldChuShou = shouldChuShou,
                    Position = i + 1
                });
            }

            // 按评分降序排序
            evaluationResults = evaluationResults.OrderByDescending(r => r.Score).ToList();

            return evaluationResults;
        }

        /// <summary>
        /// 更新出手结果 - 下一期开奖后调用
        /// </summary>
        /// <param name="previousQiHao">上一期期号（出手期）</param>
        /// <param name="nextQiHao">下一期期号（验证期）</param>
        /// <param name="nextNumber">下一期开奖号码</param>
        /// <returns>更新结果</returns>
        public UpdateResult UpdateOutcome(string previousQiHao, string nextQiHao, string nextNumber)
        {
            // 验证上一期是否确实出手了
            if (!decisionHistory.ContainsKey(previousQiHao))
            {
                return new UpdateResult
                {
                    Success = false,
                    Message = $"未找到期号 {previousQiHao} 的出手记录"
                };
            }

            var previousRecord = decisionHistory[previousQiHao];
            
            if (!previousRecord.ShouldChuShou)
            {
                return new UpdateResult
                {
                    Success = false,
                    Message = $"期号 {previousQiHao} 当时并未出手，无需更新结果"
                };
            }

            // 判断是否中奖：开奖号码是否在350注号码池中
            bool isWinning = false;
            if (previousRecord.Numbers350AtDecision != null && previousRecord.Numbers350AtDecision.Contains(nextNumber))
            {
                isWinning = true;
            }

            // 更新决策记录的中奖状态
            previousRecord.IsWinning = isWinning;
            previousRecord.NextPeriodNumber = nextNumber;
            previousRecord.UpdateTime = DateTime.Now;

            // 更新决策历史
            decisionHistory[previousQiHao] = previousRecord;

            return new UpdateResult
            {
                Success = true,
                IsWinning = isWinning,
                Message = $"期号 {previousQiHao} 出手结果更新成功：{(isWinning ? "中奖" : "未中奖")}"
            };
        }

        /// <summary>
        /// 获取指定号码的历史评分和出手信息
        /// </summary>
        /// <param name="qiHao">期号</param>
        /// <returns>决策记录</returns>
        public DecisionRecord GetHistoricalDecision(string qiHao)
        {
            if (decisionHistory.ContainsKey(qiHao))
            {
                return decisionHistory[qiHao];
            }
            return null;
        }

        /// <summary>
        /// 获取所有历史决策记录
        /// </summary>
        /// <returns>所有决策记录</returns>
        public List<DecisionRecord> GetAllHistoricalDecisions()
        {
            return decisionHistory.Values.ToList();
        }

        /// <summary>
        /// 计算评分的核心方法
        /// </summary>
        /// <param name="currentData">当前期数据</param>
        /// <param name="historyData">历史数据</param>
        /// <returns>综合评分</returns>
        private int CalculateScore(LotteryData currentData, List<LotteryData> historyData)
        {
            int score = 0;

            // 基础评分逻辑（可根据实际需求调整）
            // 1. 如果历史数据显示整体趋势适合出手，则加分
            if (historyData.Any())
            {
                var recentPeriods = 10; // 检查最近10期
                var recentHistory = historyData.TakeLast(recentPeriods).ToList();
                
                // 分析最近10期的整体模式
                // 计算号码分布的均匀性
                var uniqueNumbers = recentHistory.Select(h => h.Number).Distinct().Count();
                if (uniqueNumbers >= 8) // 如果最近10期中有较多不同号码，则认为分布较均匀，可出手
                {
                    score += 10;
                }
            }

            // 2. 考虑号码的规律性（比如尾号、和值等特征）
            // 这里可以根据具体的彩票规律进行扩展
            if (!string.IsNullOrEmpty(currentData.Number))
            {
                var lastDigit = currentData.Number.Substring(currentData.Number.Length - 1);
                var sumOfDigits = currentData.Number.Sum(c => c - '0');
                
                // 示例：如果和值在平均范围附近，加5分
                if (sumOfDigits >= 10 && sumOfDigits <= 20)
                {
                    score += 5;
                }
            }

            // 3. 考虑历史趋势
            if (historyData.Count >= 3)
            {
                // 检查最近三期的趋势模式
                var recentThree = historyData.TakeLast(3).ToList();
                
                // 分析最近几期的波动情况
                if (recentThree.Count >= 2)
                {
                    var prevNumber = recentThree[recentThree.Count - 2].Number;
                    var lastNumber = recentThree[recentThree.Count - 1].Number;
                    
                    // 如果最近两期号码差异较大，可能表示趋势变化
                    if (prevNumber != lastNumber)
                    {
                        score += 3;
                    }
                }
            }

            // 4. 考虑350注号码池的特性
            if (numbers350 != null && numbers350.Count > 0)
            {
                // 如果350注号码池覆盖了较好的号码分布，增加分数
                score += Math.Min(15, numbers350.Count / 25); // 基于号码池大小给予一定分数
            }

            // 5. 确保评分在合理范围内
            score = Math.Max(0, Math.Min(100, score));

            return score;
        }

        /// <summary>
        /// 判断是否应该出手（从350注号码池中进行投注）
        /// </summary>
        /// <param name="score">当前评分</param>
        /// <param name="currentData">当前期数据</param>
        /// <param name="historyData">历史数据</param>
        /// <returns>是否出手</returns>
        private bool ShouldChuShou(int score, LotteryData currentData, List<LotteryData> historyData)
        {
            // 基本条件：评分达到阈值
            if (score < DecisionThreshold)
            {
                return false;
            }

            // 检查350注号码列表是否存在
            if (numbers350 == null || !numbers350.Any())
            {
                return false; // 没有350注号码，则无法出手
            }

            // 检查连续出手限制
            if (historyData.Count >= 2)
            {
                var lastTwoRecords = historyData.TakeLast(2).ToList();
                
                // 如果前两期都已经出手，则当前期不建议出手（防止连续出手风险）
                if (lastTwoRecords.All(r => 
                    decisionHistory.ContainsKey(r.QiHao) && 
                    decisionHistory[r.QiHao].ShouldChuShou))
                {
                    return false;
                }
            }

            // 检查确认点和趋势段
            if (historyData.Any())
            {
                // 如果当前期处于趋势段内，不建议出手
                var lastData = historyData.Last();
                if (lastData.IsQuShiDuan)
                {
                    return false;
                }
                
                // 检查是否大遗漏后理论周期内
                if (lastData.IsDaYiLou)
                {
                    return false;  // 在大遗漏状态下暂时不出手
                }
            }

            // 其他业务逻辑可以根据需要添加

            return true;
        }
    }

    /// <summary>
    /// 决策记录类 - 存储每次决策的详细信息
    /// </summary>
    public class DecisionRecord
    {
        /// <summary>
        /// 期号
        /// </summary>
        public string QiHao { get; set; }

        /// <summary>
        /// 开奖号码
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// 评分
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// 是否决定出手
        /// </summary>
        public bool ShouldChuShou { get; set; }

        /// <summary>
        /// 是否中奖
        /// </summary>
        public bool? IsWinning { get; set; }

        /// <summary>
        /// 决策时间
        /// </summary>
        public DateTime DecisionTime { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 下一期开奖号码（用于验证出手结果）
        /// </summary>
        public string NextPeriodNumber { get; set; }

        /// <summary>
        /// 决策时使用的历史数据快照
        /// </summary>
        public List<LotteryData> HistoryDataUsed { get; set; }

        /// <summary>
        /// 决策时的350注号码快照
        /// </summary>
        public List<string> Numbers350AtDecision { get; set; }
    }

    /// <summary>
    /// 决策结果类 - 返回给调用者的信息
    /// </summary>
    public class DecisionResult
    {
        /// <summary>
        /// 期号
        /// </summary>
        public string QiHao { get; set; }

        /// <summary>
        /// 开奖号码
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// 评分
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// 是否应该出手
        /// </summary>
        public bool ShouldChuShou { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; }
    }

    /// <summary>
    /// 更新结果类 - 返回更新中奖结果的操作结果
    /// </summary>
    public class UpdateResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 是否中奖
        /// </summary>
        public bool? IsWinning { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; }
    }

    /// <summary>
    /// 号码评估结果类 - 存储350注号码中每注的评估信息
    /// </summary>
    public class NumberEvaluationResult
    {
        /// <summary>
        /// 号码
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// 评分
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// 是否建议出手
        /// </summary>
        public bool ShouldChuShou { get; set; }

        /// <summary>
        /// 在350注中的位置
        /// </summary>
        public int Position { get; set; }
    }
}