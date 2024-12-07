using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Day7 : MonoBehaviour
{
    public TextAsset Input;

    [ContextMenu("Run Part 1")]
    public void Run()
    {
        var equations = ParseInput();
        Int64 count = 0;
        foreach (var equation in equations)
        {
            if (equation.CanProduceResult())
            {
                count += equation.Result;
            }
        }
        Debug.Log($"Valid equations sum: {count}");

    }

    [ContextMenu("Run Part 2")]
    public void RunPt2()
    {
        var equations = ParseInput();
        Int64 count = 0;
        foreach (var equation in equations)
        {
            equation.AllowConcat = true;

            if (equation.CanProduceResult())
            {
                count += equation.Result;
            }
        }
        Debug.Log($"Valid equations sum: {count}");
    }


    private List<Equation> ParseInput()
    {
        var lines = Input.text.Split("\n");
        return lines.Select(line => new Equation(line)).ToList();
    }

    public class Equation
    {
        public bool AllowConcat = false;
        public int Radix => AllowConcat ? 3 : 2;
        public Int64 Result;
        public List<Int64> Operands;
        public Equation(string input)
        {
            var split = input.Split(": ");
            Result = Int64.Parse(split[0].Trim());
            Operands = split[1].Trim().Split(" ").Select(Int64.Parse).ToList();
        }

        public bool CanProduceResult()
        {
            var iterator = IterateOverPossibilities();
            while (iterator.MoveNext())
            {
                if (iterator.Current == Result)
                {
                    return true;
                }
            }
            return false;
        }

        public IEnumerator<Int64> IterateOverPossibilities()
        {
            Int64 possibilities = (Int64)Math.Pow(Radix, Operands.Count - 1);
            for (int i = 0; i < possibilities; i++)
            {
                yield return Evaluate(i);
            }
        }

        private Int64 Evaluate(int operators)
        {
            Int64 result = Operands[0];
            for (int i = 1; i < Operands.Count; i++)
            {
                var op = GetOperator(i - 1, operators);
                switch (op)
                {
                    case Operator.Add:
                        result += Operands[i];
                        break;
                    case Operator.Multiply:
                        result *= Operands[i];
                        break;
                    case Operator.Concat:
                        result = Int64.Parse(result.ToString() + Operands[i].ToString());
                        break;
                }
            }
            return result;
        }

        private Operator GetOperator(int operatorIdx, int operatorDefinition)
        {
            int op = operatorDefinition / (int)Math.Pow(Radix, operatorIdx) % Radix;
            switch (op)
            {
                case 0:
                    return Operator.Add;
                case 1:
                    return Operator.Multiply;
                case 2:
                    return Operator.Concat;
            }
            return Operator.Multiply;
        }

        public override string ToString()
        {
            return $"{string.Join(" ", Operands)} = {Result}";
        }

        public enum Operator
        {
            Add,
            Multiply,
            Concat,
        }

    }
}
