using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Day24 : MonoBehaviour
{
    public readonly int NUM_BITS = 45; // 45 in, 46 out
    public TextAsset Input;

    [ContextMenu("Run Pt 1")]
    public void Run()
    {
        WireMap.Clear();
        Gates.Clear();
        ParseInput();
        RunDevice();
        foreach (var wire in WireMap.Values)
        {
            if (wire.Id.StartsWith("z"))
            {
                Debug.Log(wire.Id + " " + wire.Value);
            }
        }
        Debug.Log("Output: " + GetOutput());
    }

    [ContextMenu("Run Pt 2")]
    public void RunPt2()
    {
        WireMap.Clear();
        Gates.Clear();
        ParseInput();
        AssessDevice();

    }

    public Dictionary<string, Wire> WireMap = new Dictionary<string, Wire>();
    public List<Gate> Gates = new List<Gate>();

    public void RunDevice()
    {
        var sortedGates = Gates.OrderBy(x => x.Depth).ToList();
        foreach (var gate in sortedGates)
        {
            gate.Operate();
        }
    }

    // All parsed info added to WireMap and Gates list
    public void ParseInput()
    {
        var lines = Input.text.Split('\n').Select(x => x.Trim()).ToList();
        bool allInputsParsed = false;
        foreach (var line in lines)
        {
            if (string.IsNullOrEmpty(line))
            {
                allInputsParsed = true;
                continue;
            }

            if (!allInputsParsed)
            {
                Wire wire = new Wire(line);
                WireMap.Add(wire.Id, wire);
            }
            else
            {
                Gate gate = new Gate(line, WireMap);
                Gates.Add(gate);
                Wire outputWire = new Wire(gate);
                if (WireMap.ContainsKey(outputWire.Id))
                {
                    throw new Exception("Wire already exists");
                }
                else
                {
                    WireMap.Add(outputWire.Id, outputWire);
                }
            }
        }
    }

    public long GetOutput()
    {
        List<string> outputWires = new List<string>();
        foreach (var wire in WireMap.Values)
        {
            if (wire.Id.StartsWith("z"))
            {
                outputWires.Add(wire.Id);
            }
        }
        outputWires = outputWires.OrderBy(x => int.Parse(x.TrimStart('z'))).ToList();

        long sum = 0;
        for (int i = 0; i < outputWires.Count; i++)
        {
            sum += WireMap[outputWires[i]].Value.Value ? (long)Math.Pow(2, i) : 0;
        }
        return sum;
    }

    public Wire GetWire(string Id)
    {
        return WireMap[Id];
    }

    public void AssessDevice()
    {
        for (int i = 0; i < NUM_BITS; i++)
        {
            if (!BitAdderWorks(i))
            {
                Debug.Log($"Bit Adder {i} did not work");
                Debug.Log("Relevant wires: " + string.Join(",", GetRelevantWires(i)));
            }
        }
    }

    public HashSet<string> GetRelevantWires(int idx)
    {
        Wire inputX = GetWire('x', idx);
        Wire inputY = GetWire('y', idx);
        Wire outputZ = GetWire('z', idx);

        HashSet<string> relevantWires = new HashSet<string>();
        Queue<string> queue = new Queue<string>();
        bool stopAdding = false;
        queue.Enqueue(outputZ.Id);

        while (queue.Count > 0)
        {
            string wireId = queue.Dequeue();
            var gate = GetWire(wireId).ProvidingGate;
            relevantWires.Add(wireId);

            if (GetWire(wireId) == inputX || GetWire(wireId) == inputY)
            {
                // we've gotten to the input. Stop adding more wires beyond this "depth from the output"
                stopAdding = true;
            }

            if (!stopAdding)
            {
                if (WireMap[wireId].ProvidingGate != null)
                {
                    foreach (var input in WireMap[wireId].ProvidingGate.InputIds)
                    {
                        queue.Enqueue(input);
                    }
                }
            }
        }

        return relevantWires;
    }

    // public bool TestBitAdder(int idx, string priorBitOverflow)
    // {
    //     Wire inputX = GetWire('x', idx);
    //     Wire inputY = GetWire('y', idx);
    //     Wire outputZ = GetWire('z', idx);

    //     if (idx == 0)
    //     {
    //         return TestSingleBitAdder(idx);
    //     }
    //     else
    //     {
    //         return TestBitAdder(idx - 1, priorBitOverflow);
    //     }
    // }

    public bool BitAdderWorks(int idx)
    {
        Wire inputX = GetWire('x', idx);
        Wire inputY = GetWire('y', idx);
        Wire outputZ = GetWire('z', idx);

        WireMap.Values.ToList().ForEach(x => x.Value = false);
        List<Tuple<bool, bool>> testCases = new List<Tuple<bool, bool>>()
        {
            new Tuple<bool,bool>(false,false),
            new Tuple<bool,bool>(false,true),
            new Tuple<bool,bool>(true,false),
            new Tuple<bool,bool>(true,true)
        };

        foreach (var test in testCases)
        {
            inputX.Value = test.Item1;
            inputY.Value = test.Item2;
            RunDevice();
            if (outputZ.Value != (test.Item1 ^ test.Item2))
            {
                return false;
            }
        }

        return true;
    }

    public Wire GetWire(char c, int idx)
    {
        if (idx < 10)
        {
            return GetWire(c + "0" + idx.ToString());
        }
        else
        {
            return GetWire(c + idx.ToString());
        }
    }


    public class Wire
    {
        public Wire(string line)
        {
            var parts = line.Split(':');
            Id = parts[0].Trim();
            Value = parts[1].Trim() == "1";
            ProvidingGate = null;
        }

        public Wire(Gate providingGate)
        {
            Id = providingGate.OutputId;
            ProvidingGate = providingGate;
            Value = null;
        }

        public string Id;
        public bool? Value;

        public int Depth => ProvidingGate != null ? ProvidingGate.Depth : 0;

        public Gate ProvidingGate;
    }

    public class Gate
    {
        public Gate(string line, Dictionary<string, Wire> wireMap)
        {
            var parts = line.Split("->");
            OutputId = parts[1].Trim();
            var leftParts = line.Split(" ");
            InputIds.Add(leftParts[0].Trim());
            InputIds.Add(leftParts[2].Trim());
            switch (leftParts[1].Trim())
            {
                case "AND":
                    _mode = Mode.And;
                    break;
                case "OR":
                    _mode = Mode.Or;
                    break;
                case "XOR":
                    _mode = Mode.Xor;
                    break;
            }
            _wireMap = wireMap;
        }

        public enum Mode
        {
            And,
            Or,
            Xor
        }

        private Dictionary<string, Wire> _wireMap;

        private Mode _mode;
        public int Depth => Inputs.Max(x => x.Depth) + 1;
        public List<string> InputIds = new List<string>();
        public string OutputId;
        public List<Wire> Inputs => InputIds.Select(x => _wireMap[x]).ToList();
        public Wire Output => _wireMap[OutputId];


        public void Operate()
        {
            if (Inputs.Any(x => x.Value == null))
            {
                throw new Exception("Cannot operate on null values");
            }

            switch (_mode)
            {
                case Mode.And:
                    Output.Value = Inputs[0].Value & Inputs[1].Value;
                    break;
                case Mode.Or:
                    Output.Value = Inputs[0].Value | Inputs[1].Value;
                    break;
                case Mode.Xor:
                    Output.Value = Inputs[0].Value ^ Inputs[1].Value;
                    break;
            }
        }
    }
}