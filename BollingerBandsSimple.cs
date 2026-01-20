using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CpCodeSelect.Util
{
    public class BollingerBandsSimple
    {
        /// <summary>
        /// 计算布林带的单一函数
        /// </summary>
        public static (double middle, double upper, double lower) CalculateBollingerBands(
            double[] prices,
            int period = 20,
            double stdDevMultiplier = 2.0)
        {
            if (prices == null || prices.Length < period)
            {
                return (double.NaN, double.NaN, double.NaN);
            }

            // 确保只取最近period个数据
            var recentPrices = prices.Skip(prices.Length - period).Take(period).ToArray();

            // 1. 计算中轨（简单移动平均）
            double middleBand = recentPrices.Average();

            // 2. 计算标准差
            double variance = 0.0;
            foreach (var price in recentPrices)
            {
                variance += Math.Pow(price - middleBand, 2);
            }
            double standardDeviation = Math.Sqrt(variance / period);

            // 3. 计算上下轨
            double upperBand = middleBand + (stdDevMultiplier * standardDeviation);
            double lowerBand = middleBand - (stdDevMultiplier * standardDeviation);

            return (middleBand, upperBand, lowerBand);
        }

        /// <summary>
        /// 为价格序列计算所有布林带值
        /// </summary>
        public static (double[] middles, double[] uppers, double[] lowers) CalculateAllBollingerBands(
            double[] prices,
            int period = 20,
            double stdDevMultiplier = 2.0)
        {
            int length = prices.Length;
            double[] middles = new double[length];
            double[] uppers = new double[length];
            double[] lowers = new double[length];

            for (int i = 0; i < length; i++)
            {
                if (i < period - 1)
                {
                    // 数据不足，设置为NaN
                    middles[i] = double.NaN;
                    uppers[i] = double.NaN;
                    lowers[i] = double.NaN;
                }
                else
                {
                    // 取最近period个价格
                    var window = new double[period];
                    Array.Copy(prices, i - period + 1, window, 0, period);

                    var (middle, upper, lower) = CalculateBollingerBands(window, period, stdDevMultiplier);
                    middles[i] = middle;
                    uppers[i] = upper;
                    lowers[i] = lower;
                }
            }

            return (middles, uppers, lowers);
        }
    }
}
