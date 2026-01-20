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
            return currentData.IsQueRenDian && !currentData.IsQuShiDuan;
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
}