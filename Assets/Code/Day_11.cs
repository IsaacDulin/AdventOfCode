using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Day11 : MonoBehaviour
{
    public TextAsset Input;

    [ContextMenu("Run Pt 1")]
    public void Run()
    {
        MagicStones magicStones = new MagicStones(Input.text);
        magicStones.RegisterRule(new ZeroToOneRule());
        magicStones.RegisterRule(new EvenDigitsRule());
        magicStones.RegisterRule(new CatchAllRule());

        Int64 ct = magicStones.BlinkXTimes(25);

        Debug.Log("Total stones: " + ct);
    }

    [ContextMenu("Run Pt 2")]
    public void RunPt2()
    {
        MagicStones magicStones = new MagicStones(Input.text);
        magicStones.RegisterRule(new ZeroToOneRule());
        magicStones.RegisterRule(new EvenDigitsRule());
        magicStones.RegisterRule(new CatchAllRule());

        Int64 ct = magicStones.BlinkXTimes(75);

        Debug.Log("Total stones: " + ct);
    }

    public class ZeroToOneRule : IRule
    {
        public bool RuleApplies(Int64 stone)
        {
            return stone == 0;
        }

        public List<Int64> ApplyRule(Int64 stone)
        {
            return new List<Int64> { 1 };
        }
    }

    public class EvenDigitsRule : IRule
    {
        public bool RuleApplies(Int64 stone)
        {
            return stone.ToString().Length % 2 == 0;
        }

        public List<Int64> ApplyRule(Int64 stone)
        {
            string stoneString = stone.ToString();
            Int64 firstHalf = Int64.Parse(stoneString.Substring(0, stoneString.Length / 2));
            Int64 secondHalf = Int64.Parse(stoneString.Substring(stoneString.Length / 2));
            return new List<Int64> { firstHalf, secondHalf };
        }
    }

    public class CatchAllRule : IRule
    {
        public bool RuleApplies(Int64 stone)
        {
            return true;
        }

        public List<Int64> ApplyRule(Int64 stone)
        {
            return new List<Int64> { 2024 * stone };
        }
    }

    public interface IRule
    {
        public bool RuleApplies(Int64 stone);

        public List<Int64> ApplyRule(Int64 stone);
    }

    public class MagicStones
    {
        public List<Int64> Stones = new List<Int64>();
        public List<IRule> Rules = new List<IRule>();

        // <Starting Stone, Iteration Times> , Stone Count after Iteration Times
        private Dictionary<Tuple<Int64, int>, Int64> StoneCountHash = new Dictionary<Tuple<Int64, int>, Int64>();

        public MagicStones(string input)
        {
            Stones = input.Split(' ').Select((s) => Int64.Parse(s.Trim())).ToList();
        }

        public void RegisterRule(IRule rule)
        {
            Rules.Add(rule);
        }

        public Int64 BlinkXTimes(int times)
        {
            Int64 sum = 0;
            foreach (var stone in Stones)
            {
                sum += Blink(stone, times);
            }
            return sum;
        }

        private Int64 Blink(Int64 singleStone, int times)
        {
            var key = new Tuple<Int64, int>(singleStone, times);
            if (StoneCountHash.ContainsKey(key))
            {
                return StoneCountHash[key];
            }

            if (times == 0)
            {
                return 1;
            }
            else
            {
                foreach (var rule in Rules)
                {
                    if (rule.RuleApplies(singleStone))
                    {
                        List<Int64> newStones = rule.ApplyRule(singleStone);

                        Int64 sum = 0;
                        foreach (var newStone in newStones)
                        {
                            Int64 count = Blink(newStone, times - 1);
                            // Cache here
                            sum += count;
                        }

                        StoneCountHash[new Tuple<Int64, int>(singleStone, times)] = sum;
                        return sum;
                    }
                }
            }

            throw new Exception("No rule applied to stone: " + singleStone);
        }

        public override string ToString()
        {
            string output = "";
            foreach (var stone in Stones)
            {
                output += stone + "; ";
            }
            return output;
        }
    }
}
