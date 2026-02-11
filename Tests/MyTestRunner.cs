using System;
namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var tester = new TestApp.TestMainFormFunctionality();
            Console.WriteLine("彩票出手策略测试程序");
            Console.WriteLine("=====================");

            int rounds = 30;
            if (args.Length > 0 && int.TryParse(args[0], out int argRounds))
            {
                rounds = Math.Max(1, Math.Min(100, argRounds));
            }

            tester.RunTest(rounds, 8);

            Console.WriteLine("");
            Console.WriteLine("测试完成！");
        }
    }
}