using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Day17 : MonoBehaviour
{
    // Input is too small to justify a parser today
    // public TextAsset Input;

    readonly List<int> INSTRUCTIONS = new List<int>()
    {
        // 0,3,5,4,3,0 // Test Example
        2,4,1,2,7,5,4,7,1,3,5,5,0,3,3,0
    };

    readonly long INPUT_REGISTER_A = 35200350;


    [ContextMenu("Run Pt 1")]
    public void Run()
    {
        ComputeMachine machine = new ComputeMachine();
        machine.RegisterA = INPUT_REGISTER_A;
        machine.RunProgram(INSTRUCTIONS);
        Debug.Log($"Output: {machine.GetOutput}");
    }

    [ContextMenu("Run Pt 2")]
    public void RunPt2()
    {
        ComputeMachine machine = new ComputeMachine();
        long a = machine.FindProperValueOfA(INSTRUCTIONS);
        Debug.Log($"Found match for Register A = {a}");
    }

    public class ComputeMachine
    {
        public long RegisterA = 0;

        public long RegisterB = 0;

        public long RegisterC = 0;

        public int InstructionPointer = 0;

        public List<int> Output = new List<int>();

        public string GetOutput => string.Join(",", Output);

        public void Reset()
        {
            Output.Clear();
            InstructionPointer = 0;
            RegisterA = 0;
            RegisterB = 0;
            RegisterC = 0;
        }

        public void RunProgram(List<int> instructions)
        {
            while (InstructionPointer < instructions.Count)
            {
                int opcode = instructions[InstructionPointer];
                int operand = instructions[InstructionPointer + 1];
                ExecuteInstruction(opcode, operand);
            }
        }


        // The only op that modifies A is the second-to-last operation where A is divided by 8.
        // Since A needs to be zero to end the program, and we need 16 loops total to print our output,
        // 1*(8^15) is a minimum bounds for 
        public long FindProperValueOfA(List<int> instructions)
        {
            List<long> possibilitiesOfA;
            List<long> nextIterationPossibilities = GetNextPossibleValues(0);

            for (int i = instructions.Count - 1; i >= 0; i--) // loop backwards
            {
                possibilitiesOfA = new List<long>(nextIterationPossibilities);
                nextIterationPossibilities.Clear();

                List<int> targetOutput = instructions.GetRange(i, instructions.Count - i);
                Debug.Log("Target output : " + string.Join(",", targetOutput));
                Debug.Log("Possibilities of A: " + string.Join(",", possibilitiesOfA));
                foreach (var a in possibilitiesOfA)
                {
                    Reset();
                    RegisterA = a;
                    RunProgram(instructions);

                    if (Output.SequenceEqual(instructions))
                    {
                        return a;
                    }

                    if (Output.SequenceEqual(targetOutput))
                    {
                        Debug.Log("A valid value of A was : " + a);
                        nextIterationPossibilities.AddRange(GetNextPossibleValues(a));
                    }
                }
            }

            throw new System.Exception("No valid value of A found");
        }

        private List<long> GetNextPossibleValues(long a)
        {
            var list = new List<long>();
            for (int i = 0; i < 8; i++)
            {
                list.Add(a * 8 + i);
            }
            return list;
        }

        private void ExecuteInstruction(int opcode, int operand)
        {
            switch (opcode)
            {
                case 0:
                    ExecuteAdv(operand);
                    break;
                case 1:
                    ExecuteBxl(operand);
                    break;
                case 2:
                    ExecuteBst(operand);
                    break;
                case 3:
                    ExecuteJnz(operand);
                    break;
                case 4:
                    ExecuteBxc(operand);
                    break;
                case 5:
                    ExecuteOut(operand);
                    break;
                case 6:
                    ExecuteBdv(operand);
                    break;
                case 7:
                    ExecuteCdv(operand);
                    break;
            }
        }

        private void ExecuteAdv(int operand)
        {
            long numerator = RegisterA;
            long denominator = PowerOfTwo(GetComboOperand(operand));
            RegisterA = numerator / denominator;
            InstructionPointer += 2;
        }

        private void ExecuteBxl(int operand)
        {
            RegisterB = RegisterB ^ operand;
            InstructionPointer += 2;
        }

        private void ExecuteBst(int operand)
        {
            RegisterB = GetComboOperand(operand) % 8;
            InstructionPointer += 2;
        }

        private void ExecuteJnz(int operand)
        {
            if (RegisterA == 0)
            {
                InstructionPointer += 2;
            }
            else
            {
                InstructionPointer = operand;
            }
        }

        private void ExecuteBxc(int _)
        {
            RegisterB = RegisterB ^ RegisterC;
            InstructionPointer += 2;
        }

        private void ExecuteOut(int operand)
        {
            Output.Add((int)(GetComboOperand(operand) % 8));
            InstructionPointer += 2;
        }

        private void ExecuteBdv(int operand)
        {
            long numerator = RegisterA;
            long denominator = PowerOfTwo(GetComboOperand(operand));
            RegisterB = numerator / denominator;
            InstructionPointer += 2;
        }

        private void ExecuteCdv(int operand)
        {
            long numerator = RegisterA;
            long denominator = PowerOfTwo(GetComboOperand(operand));
            RegisterC = numerator / denominator;
            InstructionPointer += 2;
        }

        private long PowerOfTwo(long operand)
        {
            if (operand == 0)
            {
                return 1;
            }
            long output = 2;
            for (long i = 1; i < operand; i++)
            {
                output *= 2;
            }
            return output;
        }

        private long GetComboOperand(int operand)
        {
            switch (operand)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                    return operand;
                case 4:
                    return RegisterA;
                case 5:
                    return RegisterB;
                case 6:
                    return RegisterC;
                case 7:
                default:
                    throw new System.Exception("Invalid combo operand 7");
            }
        }
    }
}