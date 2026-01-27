using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CpCodeSelect.Core;
using CpCodeSelect.Scorer;
using CpCodeSelect.Scorer.Rules;

namespace TestApp
{
    class Program
    {
        static void Main1(string[] args)
        {
            Console.WriteLine("彩票评分系统测试");
            
            // 创建处理器
            var processor = new LotteryProcessor();
            
            // 测试加载文件
            if (File.Exists(@"..\\TXFFC.txt"))
            {
                Console.WriteLine("正在加载开奖数据...");
                var data = processor.LoadLotteryDataFromFile(@"..\\TXFFC.txt");
                
                Console.WriteLine($"已加载 {data.Count} 条数据");
                
                // 处理前几期数据
                Console.WriteLine("正在处理前10期数据...");
                var results = processor.ProcessMultiplePeriods(data.Take(10).ToList());
                
                Console.WriteLine("处理完成！");
                Console.WriteLine($"处理了 {results.Count} 期数据");
                
                // 显示结果
                foreach (var result in results.Take(5)) // 显示前5期
                {
                    Console.WriteLine($"期号: {result.QiHao}, 开奖号: {result.Number}, 后三位: {result.Hou3Number}, " +
                                    $"中奖: {(result.IsZhongJiang ? "是" : "否")}, 遗漏: {result.YiLouValue}, " +
                                    $"K值: {result.KValue:F3}");
                }
                
                // 创建评分引擎
                var scoringEngine = new ScoringEngine();
                scoringEngine.AddRule(new KValueBelowMiddleNoBetRule());
                scoringEngine.AddRule(new TrendSegmentNoBetRule());
                scoringEngine.AddRule(new ConfirmPointBeforeTrendRule());
                scoringEngine.AddRule(new BigGapBetweenZeroOrOneStrongRule());
                scoringEngine.AddRule(new ThreeTrackSameDirectionRule());
                scoringEngine.AddRule(new TwoTrackSameDirectionRule());
                scoringEngine.AddRule(new TrackOppositeDirectionRule());
                scoringEngine.AddRule(new KValueBreakMiddleNotTouchUpperRule());
                scoringEngine.AddRule(new KValueNearUpperRailRule());
                scoringEngine.AddRule(new YiLouValueRule());
                scoringEngine.AddRule(new BollingerUpperDeclineRule());
                scoringEngine.AddRule(new ContinuousChuShouLimitRule()); // 添加连续出手限制评分规则
                scoringEngine.AddRule(new SecondChuShouLimitRule()); // 添加连续第二手限制规则
                
                // 对最后几期计算分数
                for(int i = Math.Max(0, results.Count - 3); i < results.Count; i++)
                {
                    var historyForScoring = results.Take(i).ToList();
                    var score = scoringEngine.CalculateTotalScore(results[i], historyForScoring);
                    Console.WriteLine($"期号 {results[i].QiHao} 评分: {score}");
                }
            }
            else
            {
                Console.WriteLine("..\\TXFFC.txt 文件不存在，请确保文件存在后再运行测试。");
            }
            
            Console.WriteLine("\\n按任意键退出...");
            Console.ReadKey();
        }
    }
}