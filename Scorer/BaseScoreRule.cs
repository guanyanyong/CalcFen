using System;
using System.Collections.Generic;
using CpCodeSelect.Model;

namespace CpCodeSelect.Scorer
{
    /// <summary>
    /// 评分规则接口
    /// </summary>
    public interface IScoreRule
    {
        string RuleName { get; }
        string Description { get; }
        int CalculateScore(LotteryData currentData, List<LotteryData> historyData);
        bool IsValid(LotteryData currentData, List<LotteryData> historyData);
    }

    /// <summary>
    /// 抽象评分规则基类
    /// </summary>
    public abstract class BaseScoreRule : IScoreRule
    {
        public abstract string RuleName { get; }
        public abstract string Description { get; }
        public abstract int ScoreValue { get; } // 评分值字段

        public virtual bool IsValid(LotteryData currentData, List<LotteryData> historyData)
        {
            return true;
        }

        public virtual int CalculateScore(LotteryData currentData, List<LotteryData> historyData)
        {
            if (IsValid(currentData, historyData))
            {
                return ScoreValue;
            }
            return 0; // 如果条件不满足，返回0分
        }
    }

    /// <summary>
    /// 评分引擎
    /// </summary>
    public class ScoringEngine
    {
        private List<IScoreRule> _rules;

        public ScoringEngine()
        {
            _rules = new List<IScoreRule>();
        }

        public void AddRule(IScoreRule rule)
        {
            _rules.Add(rule);
        }

        public void RemoveRule(IScoreRule rule)
        {
            _rules.Remove(rule);
        }

        public int CalculateTotalScore(LotteryData currentData, List<LotteryData> historyData)
        {
            int totalScore = 0;
            // 清空之前的评分详情
            currentData.ScoreDetails.Clear();
            
            foreach (var rule in _rules)
            {
                bool isValid = rule.IsValid(currentData, historyData);
                
                if (isValid)
                {
                    int ruleScore = rule.CalculateScore(currentData, historyData);
                    totalScore += ruleScore;
                    
                    // 添加评分详情 (触发的规则)
                    int expectedScore = 0;
                    if (rule is BaseScoreRule baseRule)
                    {
                        expectedScore = baseRule.ScoreValue;
                    }
                    
                    currentData.ScoreDetails.Add(new ScoreDetail
                    {
                        RuleName = rule.RuleName,
                        Score = ruleScore, // 触发的规则，显示实际分数
                        Description = rule.Description,
                        IsValid = isValid,
                        ExpectedScore = expectedScore
                    });
                }
                else
                {
                    // 添加评分详情 (未触发的规则)
                    int expectedScore = 0;
                    if (rule is BaseScoreRule baseRule)
                    {
                        expectedScore = baseRule.ScoreValue;
                    }
                    
                    currentData.ScoreDetails.Add(new ScoreDetail
                    {
                        RuleName = rule.RuleName,
                        Score = 0, // 未触发的规则，显示0分
                        Description = rule.Description,
                        IsValid = isValid,
                        ExpectedScore = expectedScore
                    });
                }
            }
            
            return totalScore;
        }

        public List<ScoreDetail> GetScoreDetails(LotteryData currentData, List<LotteryData> historyData)
        {
            var details = new List<ScoreDetail>();
            
            foreach (var rule in _rules)
            {
                bool isValid = rule.IsValid(currentData, historyData);
                int actualScore = 0;
                
                if (isValid)
                {
                    actualScore = rule.CalculateScore(currentData, historyData);
                }
                
                int expectedScore = 0;
                int scoreValue = 0;
                
                if (rule is BaseScoreRule baseRule)
                {
                    expectedScore = baseRule.ScoreValue;
                    scoreValue = baseRule.ScoreValue; // 使用评分规则的分值
                }
                
                details.Add(new ScoreDetail
                {
                    RuleName = rule.RuleName,
                    Score = actualScore,
                    Description = rule.Description,
                    IsValid = isValid,
                    ExpectedScore = expectedScore,
                    ScoreValue = scoreValue
                });
            }
            
            return details;
        }
    }
}