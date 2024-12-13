using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine;

public class Day13 : MonoBehaviour
{
    const Int64 PRIZE_OFFSET = 10000000000000;
    public TextAsset Input;

    [ContextMenu("Run Pt 1")]
    public void Run()
    {
        var configs = ParseInput();
        Int64 totalCost = 0;
        foreach (var config in configs)
        {
            if (SolveSystem(config, out Vector<double> solution))
            {
                totalCost += config.CalculateCost(solution);
            }
        }
        Debug.Log("Total Cost: " + totalCost);
    }

    [ContextMenu("Run Pt 2")]
    public void RunPt2()
    {
        var configs = ParseInput();
        Int64 totalCost = 0;
        foreach (var config in configs)
        {
            // Add the part 2 prize offset
            config.Prize.X += PRIZE_OFFSET;
            config.Prize.Y += PRIZE_OFFSET;

            if (SolveSystem(config, out Vector<double> solution))
            {
                totalCost += config.CalculateCost(solution);
            }
        }
        Debug.Log("Total Cost: " + totalCost);
    }

    public bool SolveSystem(ClawMachineConfig machine, out Vector<double> bestSolution)
    {
        var M = machine.GetMatrix();
        var P = machine.GetPrizeVector();

        if (Math.Abs(M.Determinant()) < 1e-10)
        {
            // Oh my god I can't believe we didn't have to handle infinite solutions. 
            // I spent so long on this section.
            bestSolution = null;
            return false;
        }
        else
        {
            // There's a unique solution
            var x = M.Inverse() * P;
            bestSolution = x;
        }

        if (!machine.CheckSolution(bestSolution))
        {
            Debug.Log("Solution was probably not integers");
            Debug.Log($"{bestSolution[0]}, {bestSolution[1]}");
            return false;
        }

        return true;
    }

    public double CalculateButtonEfficiency(Button button)
    {
        double length = button.Motion.Norm(2);
        return length / (double)button.Price;
    }

    public bool VectorIsIntegers(Vector<double> v)
    {
        for (int i = 0; i < v.Count; i++)
        {
            double delta = v[i] - Math.Round(v[i]);
            if (Math.Abs(delta) > 1e-5)
            {
                return false;
            }
        }
        return true;
    }

    public List<ClawMachineConfig> ParseInput()
    {
        var lines = Input.text.Split('\n').ToList();
        var clawMachineConfigs = new List<ClawMachineConfig>();
        for (int i = 0; i < lines.Count; i += 4)
        {
            clawMachineConfigs.Add(new ClawMachineConfig(string.Join("\n", lines.GetRange(i, 3))));
        }
        return clawMachineConfigs;
    }

    public class ClawMachineConfig
    {
        public Button ButtonA;
        public Button ButtonB;
        public Prize Prize;

        public ClawMachineConfig(string config)
        {
            //expects 3 lines
            var lines = config.Split('\n');
            ButtonA = new Button(lines[0].Split(':')[1].Trim(), 3);
            ButtonB = new Button(lines[1].Split(':')[1].Trim(), 1);
            Prize = new Prize(lines[2].Split(':')[1].Trim());
        }

        public Matrix<double> GetMatrix()
        {
            return Matrix<double>.Build.DenseOfArray(new double[,]
            {
                { ButtonA.Motion[0], ButtonB.Motion[0] },
                { ButtonA.Motion[1], ButtonB.Motion[1] }
            });
        }

        public bool CheckSolution(Vector<double> v)
        {
            return CheckSolution((long)Math.Round(v[0]), (long)Math.Round(v[1]));
        }

        public bool CheckSolution(Int64 aPresses, Int64 bPresses)
        {
            return ButtonA.X * aPresses + ButtonB.X * bPresses == Prize.X && ButtonA.Y * aPresses + ButtonB.Y * bPresses == Prize.Y;
        }

        public Vector<double> GetPrizeVector()
        {
            return Vector<double>.Build.DenseOfArray(new double[] { Prize.X, Prize.Y });
        }

        public Int64 CalculateCost(Vector<double> x)
        {
            return ButtonA.Price * (long)Math.Round(x[0]) + (ButtonB.Price * (long)Math.Round(x[1]));
        }

        public override string ToString()
        {
            return $"ButtonA: {ButtonA.Motion.ToString()}; ButtonB: {ButtonB.Motion.ToString()}; Prize: {Prize.X}; {Prize.Y}";
        }
    }

    public class Button
    {
        public Vector<double> Motion => Vector<double>.Build.DenseOfArray(new double[] { X, Y });
        public int Price;
        public long X;
        public long Y;

        public Button(string config, int price)
        {
            var split = config.Split(',');
            X = int.Parse(split[0].Trim().TrimStart('X', '+'));
            Y = int.Parse(split[1].Trim().TrimStart('Y', '+'));
            Price = price;
        }
    }

    public class Prize
    {
        public Int64 X;
        public Int64 Y;
        public Prize(string config)
        {
            var split = config.Split(',');
            X = int.Parse(split[0].Trim().TrimStart('X', '='));
            Y = int.Parse(split[1].Trim().TrimStart('Y', '='));
        }
    }
}
