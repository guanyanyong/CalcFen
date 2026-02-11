using System;
using System.Collections.Generic;
using System.Linq;
using CpCodeSelect.Model;

namespace CpCodeSelect.Scorer.Rules
{
    /// <summary>
    /// 开口型喇叭评分规则 - 当上轨上升、中轨上升、下轨下降，且中轨最近5期内没有下降时，加1500分
    /// </summary>
    public class OpeningHornRule : BaseScoreRule
    {
        public override string RuleName => "开口型喇叭";
        public override string Description => "上轨上升，中轨上升，下轨下降，且中轨最近5期内没有下降，加1500分";
        public override int ScoreValue => 1500;

        public override bool IsValid(LotteryData currentData, List<LotteryData> historyData)
        {
            // 需要足够多的历史数据来判断趋势
            if (historyData.Count < 3 || currentData.BollingerBands == null)
                return false;

            // 检查当前期是否有布林带数据
            if (currentData.BollingerBands == null ||
                historyData[historyData.Count - 1].BollingerBands == null ||
                historyData[historyData.Count - 2].BollingerBands == null)
                return false;

            var lastData = currentData;
            var prev1 = historyData[historyData.Count - 1];
            var prev2 = historyData[historyData.Count - 2];

            // 检查上轨是否上升
            bool upperUp = lastData.BollingerBands.BollUpperValue > prev1.BollingerBands.BollUpperValue 
                            //&& prev1.BollingerBands.BollUpperValue > prev2.BollingerBands.BollUpperValue
                            ;

            // 检查中轨是否上升
            bool middleUp = lastData.BollingerBands.MiddleValue > prev1.BollingerBands.MiddleValue 
                            //&& prev1.BollingerBands.MiddleValue > prev2.BollingerBands.MiddleValue
                           ;

            // 检查下轨是否下降
            bool lowerDown = lastData.BollingerBands.BollLowerValue < prev1.BollingerBands.BollLowerValue 
                            //&& prev1.BollingerBands.BollLowerValue < prev2.BollingerBands.BollLowerValue
                            ;

            // 检查中轨最近5期内是否有下降
            bool middleHasDeclinedInLast5 = CheckMiddleTrackHasDeclined(historyData, 5);

            // 满足条件：上轨上升、中轨上升、下轨下降，且中轨最近5期内没有下降
            return upperUp && middleUp && lowerDown && !middleHasDeclinedInLast5;
        }

        /// <summary>
        /// 检查中轨在最近n期内是否有下降
        /// </summary>
        private bool CheckMiddleTrackHasDeclined(List<LotteryData> historyData, int periods)
        {
            if (historyData.Count < 2)
                return false;

            // 检查最近periods期内是否存在中轨下降
            int checkCount = Math.Min(periods, historyData.Count);

            for (int i = historyData.Count - checkCount; i < historyData.Count - 1; i++)
            {
                var currentBollinger = historyData[i].BollingerBands;
                var nextBollinger = historyData[i + 1].BollingerBands;

                if (currentBollinger != null && nextBollinger != null)
                {
                    // 如果前一期的中轨值大于后一期的中轨值，表示中轨下降
                    if (currentBollinger.MiddleValue > nextBollinger.MiddleValue)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}