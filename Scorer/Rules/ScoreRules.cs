using System;
using System.Collections.Generic;
using System.Linq;
using CpCodeSelect.Model;

namespace CpCodeSelect.Scorer.Rules
{
    /// <summary>
    /// K值在布林中轨以下不可投注评分规则
    /// </summary>
    public class KValueBelowMiddleNoBetRule : BaseScoreRule
    {
        public override string RuleName => "K值在中轨下不可投注";
        public override string Description => "K值在布林中轨以下时，评分为-1000分，不可投注";
        public override int ScoreValue => -1000;

        public override bool IsValid(LotteryData currentData, List<LotteryData> historyData)
        {
            if (currentData.BollingerBands == null)
                return false;
            
            return currentData.KValue < currentData.BollingerBands.MiddleValue;
        }
    }

    /// <summary>
    /// 趋势段形成后不可投注评分规则
    /// </summary>
    public class TrendSegmentNoBetRule : BaseScoreRule
    {
        public override string RuleName => "趋势段形成不可投注";
        public override string Description => "趋势段形成后，评分为-500分，不可投注";
        public override int ScoreValue => -500;

        public override bool IsValid(LotteryData currentData, List<LotteryData> historyData)
        {
            return currentData.IsQuShiDuan;
        }
    }

    /// <summary>
    /// 确认点后趋势段未形成评分规则
    /// </summary>
    public class ConfirmPointBeforeTrendRule : BaseScoreRule
    {
        public override string RuleName => "确认点后趋势段未形成";
        public override string Description => "确认点后趋势段未形成时，评分加70分，可连续出手2次";
        public override int ScoreValue => 70;

        public override bool IsValid(LotteryData currentData, List<LotteryData> historyData)
        {
            // 原始逻辑：当前期是确认点且不在趋势段内
            if (currentData.IsQueRenDian && !currentData.IsQuShiDuan)
            {
                return true;
            }
            
            // 扩展逻辑：如果当前期不是确认点，但处于确认点后的序列中，且未形成趋势段
            if (!currentData.IsZhongJiang && !currentData.IsDaYiLou) // 只考虑中奖或非大遗漏的情况
            {
                // 查找历史上最近的确认点
                for (int i = historyData.Count - 1; i >= 0; i--)
                {
                    if (historyData[i].IsQueRenDian)
                    {
                        // 检查确认点后是否有大遗漏
                        bool hasBigGapAfterConfirmPoint = false;
                        for (int j = i + 1; j < historyData.Count; j++)
                        {
                            if (historyData[j].IsDaYiLou)
                            {
                                hasBigGapAfterConfirmPoint = true;
                                break;
                            }
                        }
                        
                        // 如果确认点后还没有大遗漏，则当前仍处于确认点后的序列中
                        if (!hasBigGapAfterConfirmPoint)
                        {
                            return !currentData.IsQuShiDuan; // 只要不是趋势段就加分
                        }
                        else
                        {
                            break; // 确认点后已经有大遗漏，跳出循环
                        }
                    }
                }
            }
            else if (currentData.IsZhongJiang)
            {
                // 如果当前期是中奖，检查是否处于确认点后的序列中
                for (int i = historyData.Count - 1; i >= 0; i--)
                {
                    if (historyData[i].IsQueRenDian)
                    {
                        // 检查确认点后是否有大遗漏
                        bool hasBigGapAfterConfirmPoint = false;
                        for (int j = i + 1; j < historyData.Count; j++)
                        {
                            if (historyData[j].IsDaYiLou)
                            {
                                hasBigGapAfterConfirmPoint = true;
                                break;
                            }
                        }
                        
                        // 如果确认点后还没有大遗漏，则当前仍处于确认点后的序列中
                        if (!hasBigGapAfterConfirmPoint)
                        {
                            return !currentData.IsQuShiDuan; // 只要不是趋势段就加分
                        }
                        else
                        {
                            break; // 确认点后已经有大遗漏，跳出循环
                        }
                    }
                }
            }
            
            return false;
        }
    }

    /// <summary>
    /// 大遗漏间0遗漏强或1遗漏强评分规则
    /// </summary>
    public class BigGapBetweenZeroOrOneStrongRule : BaseScoreRule
    {
        public override string RuleName => "大遗漏间0遗漏强或1遗漏强";
        public override string Description => "两个大遗漏间0遗漏强或1遗漏强，评分加40分";
        public override int ScoreValue => 40;

        public override bool IsValid(LotteryData currentData, List<LotteryData> historyData)
        {
            if (!currentData.IsZhongJiang)
                return false;

            var previousBigGapIndex = -1;
            var currentIndex = historyData.FindIndex(x => x.QiHao == currentData.QiHao);
            
            // 向前寻找第一个大遗漏
            for (int i = currentIndex - 1; i >= 0; i--)
            {
                if (historyData[i].IsDaYiLou)
                {
                    previousBigGapIndex = i;
                    break;
                }
            }
            
            if (previousBigGapIndex <= 0)
                return false;

            // 再向前寻找第二个大遗漏
            var secondBigGapIndex = -1;
            for (int i = previousBigGapIndex - 1; i >= 0; i--)
            {
                if (historyData[i].IsDaYiLou)
                {
                    secondBigGapIndex = i;
                    break;
                }
            }

            if (secondBigGapIndex < 0)
                return false;

            // 检查两个大遗漏间的遗漏值
            bool hasOneGap = false;
            for (int i = secondBigGapIndex + 1; i < previousBigGapIndex; i++)
            {
                if (historyData[i].YiLouValue == 1)
                {
                    hasOneGap = true;
                    break;
                }
            }

            // 1遗漏强或0遗漏强都加40分
            // 根据当前遗漏值决定是否出手
            return true;
        }
    }

    /// <summary>
    /// 三轨同向评分规则
    /// </summary>
    public class ThreeTrackSameDirectionRule : BaseScoreRule
    {
        public override string RuleName => "三轨同向";
        public override string Description => "布林上轨、中轨、下轨都是上升的加80分";
        public override int ScoreValue => 80;

        public override bool IsValid(LotteryData currentData, List<LotteryData> historyData)
        {
            if (historyData.Count < 3 || currentData.BollingerBands == null)
                return false;

            // 需要至少3期数据来判断趋势
            if (historyData.Count < 3 || historyData[historyData.Count - 1].BollingerBands == null ||
                historyData[historyData.Count - 2].BollingerBands == null ||
                historyData[historyData.Count - 3].BollingerBands == null)
                return false;

            var lastData = currentData;
            var prev1 = historyData[historyData.Count - 1];
            var prev2 = historyData[historyData.Count - 2];

            // 判断上轨、中轨、下轨是否都在上升
            bool upperUp = lastData.BollingerBands.BollUpperValue > prev1.BollingerBands.BollUpperValue &&
                          prev1.BollingerBands.BollUpperValue > prev2.BollingerBands.BollUpperValue;
            
            bool middleUp = lastData.BollingerBands.MiddleValue > prev1.BollingerBands.MiddleValue &&
                           prev1.BollingerBands.MiddleValue > prev2.BollingerBands.MiddleValue;
            
            bool lowerUp = lastData.BollingerBands.BollLowerValue > prev1.BollingerBands.BollLowerValue &&
                          prev1.BollingerBands.BollLowerValue > prev2.BollingerBands.BollLowerValue;

            return upperUp && middleUp && lowerUp;
        }
    }

    /// <summary>
    /// 两轨同向评分规则
    /// </summary>
    public class TwoTrackSameDirectionRule : BaseScoreRule
    {
        public override string RuleName => "两轨同向";
        public override string Description => "布林上轨和中轨2轨上升的加70分";
        public override int ScoreValue => 70;

        public override bool IsValid(LotteryData currentData, List<LotteryData> historyData)
        {
            if (historyData.Count < 3 || currentData.BollingerBands == null)
                return false;

            if (historyData.Count < 3 || historyData[historyData.Count - 1].BollingerBands == null ||
                historyData[historyData.Count - 2].BollingerBands == null)
                return false;

            var lastData = currentData;
            var prev1 = historyData[historyData.Count - 1];
            var prev2 = historyData[historyData.Count - 2];

            // 判断上轨和中轨是否都在上升
            bool upperUp = lastData.BollingerBands.BollUpperValue > prev1.BollingerBands.BollUpperValue &&
                          prev1.BollingerBands.BollUpperValue > prev2.BollingerBands.BollUpperValue;
            
            bool middleUp = lastData.BollingerBands.MiddleValue > prev1.BollingerBands.MiddleValue &&
                           prev1.BollingerBands.MiddleValue > prev2.BollingerBands.MiddleValue;

            // 不包括三轨同向的情况
            if (historyData[historyData.Count - 1].BollingerBands != null && 
                historyData[historyData.Count - 2].BollingerBands != null)
            {
                bool lowerUp = lastData.BollingerBands.BollLowerValue > prev1.BollingerBands.BollLowerValue &&
                              prev1.BollingerBands.BollLowerValue > prev2.BollingerBands.BollLowerValue;
                              
                // 如果下轨也是上升的，则属于三轨同向，不计算在此规则中
                if (lowerUp) return false;
            }

            return upperUp && middleUp;
        }
    }

    /// <summary>
    /// 轨道反向评分规则
    /// </summary>
    public class TrackOppositeDirectionRule : BaseScoreRule
    {
        public override string RuleName => "轨道反向";
        public override string Description => "布林上轨下降，中轨和下轨上升的，如果是遗漏0则减少50分";
        public override int ScoreValue => -50;

        public override bool IsValid(LotteryData currentData, List<LotteryData> historyData)
        {
            if (historyData.Count < 3 || currentData.BollingerBands == null)
                return false;

            if (historyData.Count < 3 || historyData[historyData.Count - 1].BollingerBands == null ||
                historyData[historyData.Count - 2].BollingerBands == null)
                return false;

            var lastData = currentData;
            var prev1 = historyData[historyData.Count - 1];
            var prev2 = historyData[historyData.Count - 2];

            // 判断上轨下降，中轨和下轨上升
            bool upperDown = lastData.BollingerBands.BollUpperValue < prev1.BollingerBands.BollUpperValue &&
                            prev1.BollingerBands.BollUpperValue < prev2.BollingerBands.BollUpperValue;
            
            bool middleUp = lastData.BollingerBands.MiddleValue > prev1.BollingerBands.MiddleValue &&
                           prev1.BollingerBands.MiddleValue > prev2.BollingerBands.MiddleValue;
            
            bool lowerUp = lastData.BollingerBands.BollLowerValue > prev1.BollingerBands.BollLowerValue &&
                          prev1.BollingerBands.BollLowerValue > prev2.BollingerBands.BollLowerValue;

            return upperDown && middleUp && lowerUp && currentData.YiLouValue == 0;
        }
    }

    /// <summary>
    /// K值突破中轨未触碰上轨评分规则
    /// </summary>
    public class KValueBreakMiddleNotTouchUpperRule : BaseScoreRule
    {
        public override string RuleName => "K值突破中轨未触碰上轨";
        public override string Description => "K值从中轨下突破中轨后，没有触碰过上轨，如果是遗漏2则评分加40，可连续出手2次";
        public override int ScoreValue => 40;

        public override bool IsValid(LotteryData currentData, List<LotteryData> historyData)
        {
            if (historyData.Count < 2 || currentData.BollingerBands == null)
                return false;

            // 检查当前K值是否在中轨上方
            if (currentData.KValue < currentData.BollingerBands.MiddleValue)
                return false;

            // 检查之前的K值是否在中轨下方（即刚突破）
            var prevData = historyData[historyData.Count - 1];
            if (prevData.BollingerBands == null || 
                prevData.KValue >= prevData.BollingerBands.MiddleValue)
                return false;

            // 检查从上次在中轨下到现在的过程中，是否触碰过上轨
            // 检查距离不超过上轨0.3的范围
            bool touchedUpper = false;
            if (historyData.Count > 1)
            {
                for (int i = historyData.Count - 2; i >= 0; i--)
                {
                    var data = historyData[i];
                    if (data.BollingerBands == null)
                        continue;

                    // 如果遇到一个K值在中轨下的情况，说明是另一次突破，停止检查
                    if (data.KValue < data.BollingerBands.MiddleValue)
                        break;

                    // 检查是否接近或超过上轨
                    if (Math.Abs(data.KValue - data.BollingerBands.BollUpperValue) <= 0.3)
                    {
                        touchedUpper = true;
                        break;
                    }
                }
            }

            // 没有触碰上轨，且当前遗漏值为2
            return !touchedUpper && currentData.YiLouValue == 2;
        }
    }

    /// <summary>
    /// K值接近上轨评分规则 - 当K值距离上轨0.3以内时减300分
    /// </summary>
    public class KValueNearUpperRailRule : BaseScoreRule
    {
        public override string RuleName => "K值接近上轨";
        public override string Description => "K值离上轨的值还有0.3以内的距离时，减300分";
        public override int ScoreValue => -300;

        public override bool IsValid(LotteryData currentData, List<LotteryData> historyData)
        {
            if (currentData.BollingerBands == null)
                return false;

            // 检查K值是否距离上轨在0.3以内
            double distanceToUpper = currentData.BollingerBands.BollUpperValue - currentData.KValue;
            return distanceToUpper >= 0 && distanceToUpper <= 0.3;
        }
    }

    /// <summary>
    /// 遗漏值评分规则 - 根据不同遗漏值减去相应分数
    /// 遗漏值等于2时减30分，遗漏值等于3时减50分，遗漏值大于等于4时减100分
    /// </summary>
    public class YiLouValueRule : BaseScoreRule
    {
        public override string RuleName => "遗漏值评分";
        public override string Description => "根据遗漏值减分：遗漏2减30分，遗漏3减50分，遗漏≥4减100分";
        public override int ScoreValue => -100; // 设置一个基础值，实际计算在CalculateScore方法中

        // 重写CalculateScore方法以根据具体情况返回不同分数
        public override int CalculateScore(LotteryData currentData, List<LotteryData> historyData)
        {
            if (IsValid(currentData, historyData))
            {
                int yiLouValue = currentData.YiLouValue;
                if (yiLouValue == 2)
                    return -30;
                else if (yiLouValue == 3)
                    return -50;
                else if (yiLouValue >= 4)
                    return -100;
            }
            return 0; // 如果条件不满足，返回0分
        }

        public override bool IsValid(LotteryData currentData, List<LotteryData> historyData)
        {
            return currentData.YiLouValue >= 2; // 遗漏值大于等于2时规则有效
        }
    }

    /// <summary>
    /// 布林上轨下降评分规则 - 当布林上轨在最近5期内有下降时，减50分
    /// </summary>
    public class BollingerUpperDeclineRule : BaseScoreRule
    {
        public override string RuleName => "布林上轨下降";
        public override string Description => "布林上轨在最近5期内有下降时，减50分";
        public override int ScoreValue => -50;

        public override bool IsValid(LotteryData currentData, List<LotteryData> historyData)
        {
            // 需要有足够的数据和布林带信息
            if (historyData.Count < 2 || currentData.BollingerBands == null)
                return false;

            // 检查最近5期是否有布林上轨数据和下降情况
            int checkCount = Math.Min(5, historyData.Count);
            
            // 遍历最近的checkCount期，检查是否存在上轨下降
            for (int i = historyData.Count - checkCount; i < historyData.Count - 1; i++)
            {
                var currentBollinger = historyData[i].BollingerBands;
                var nextBollinger = historyData[i + 1].BollingerBands;
                
                if (currentBollinger != null && nextBollinger != null)
                {
                    // 如果前一期的上轨值大于后一期的上轨值，表示上轨下降
                    if (currentBollinger.BollUpperValue > nextBollinger.BollUpperValue)
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }
    }

    /// <summary>
    /// 连续出手限制评分规则 - 连续出手2期后，第3期必须停一期
    /// </summary>
    public class ContinuousChuShouLimitRule : BaseScoreRule
    {
        public override string RuleName => "连续出手限制";
        public override string Description => "连续出手2期后，第3期必须停一期";
        public override int ScoreValue => -1000; // 分数很低以阻止出手

        public override bool IsValid(LotteryData currentData, List<LotteryData> historyData)
        {
            if (historyData.Count < 2)
                return false;

            // 检查前两期是否都在出手
            bool previousTwoAreChuShou = historyData[historyData.Count - 1].IsChuShou && 
                                         historyData[historyData.Count - 2].IsChuShou;
            
            // 如果前两期都在出手，则当前期评分大幅降低，阻止出手
            return previousTwoAreChuShou;
        }
    }

    /// <summary>
    /// 连续第二手限制规则 - 如果连续出手第二期，但不在确认点后区域或已形成趋势段，减500分
    /// </summary>
    public class SecondChuShouLimitRule : BaseScoreRule
    {
        public override string RuleName => "连续第二手限制";
        public override string Description => "如果连续出手第二期，但不在确认点后区域或已形成趋势段，减500分";
        public override int ScoreValue => -500; // 减500分以阻止出手

        public override bool IsValid(LotteryData currentData, List<LotteryData> historyData)
        {
            if (historyData.Count < 1)
                return false;

            // 检查前一期是否出手
            bool previousChuShou = historyData[historyData.Count - 1].IsChuShou;

            // 检查当前期是否满足基础的出手条件（这相当于判断是否可能出手）
            // 评分规则在调用时，会基于当前数据状态来判断，无需依赖currentData.Score
            bool isNotInTrendSegment = !currentData.IsQuShiDuan;
            bool isKValueAboveMiddle = currentData.BollingerBands != null && 
                                      currentData.KValue >= currentData.BollingerBands.MiddleValue;

            // 如果前一期出手，且当前期满足基本出手条件（可能出手），检查是否在合适位置
            if (previousChuShou && isNotInTrendSegment && isKValueAboveMiddle)
            {
                // 检查当前是否不在确认点后区域
                bool notInConfirmPointArea = !IsInConfirmPointArea(currentData, historyData);
                
                // 检查是否已形成趋势段
                bool hasFormedTrendSegment = currentData.IsQuShiDuan;

                // 如果不在确认点后区域 或 已形成趋势段，则应用规则减500分
                return notInConfirmPointArea || hasFormedTrendSegment;
            }

            return false;
        }

        private bool IsInConfirmPointArea(LotteryData currentData, List<LotteryData> historyData)
        {
            // 检查当前期是否处于确认点后的序列中
            // 首先检查当前期本身是否是确认点
            if (currentData.IsQueRenDian)
            {
                return true; // 如果当前期本身就是确认点，那么肯定是在确认点后序列中
            }
            
            // 从最近的期开始往前查找，找到最近的一个确认点
            for (int i = historyData.Count - 1; i >= 0; i--)
            {
                // 如果找到了确认点
                if (historyData[i].IsQueRenDian)
                {
                    // 检查从这个确认点之后到当前期之间是否有大遗漏
                    for (int j = i + 1; j < historyData.Count; j++)
                    {
                        if (historyData[j].IsDaYiLou)
                        {
                            // 如果确认点后存在大遗漏，那么当前期不在确认点后序列中
                            return false;
                        }
                    }
                    
                    // 如果确认点后直到当前期都没有大遗漏，说明当前期在确认点后序列中
                    return true;
                }
            }
            
            // 没有找到确认点，所以不在确认点后区域
            return false;
        }
    }
}