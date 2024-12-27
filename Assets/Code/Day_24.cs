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
        SwappedWires.Clear();
        WireMap.Clear();
        Gates.Clear();
        ParseInput();
        AssessDevice();

        WireMap.Clear();
        Gates.Clear();
        ParseInput();

        for (int i = 0; i < 8; i += 2)
        {
            SwapWires(SwappedWires[i], SwappedWires[i]);
        }

        for (int i = 0; i < 10; i++)
        {
            bool working = TestWorkingDevice();
            Debug.Log("Test " + i + " " + working);
        }

        var swappedwires = SwappedWires.OrderBy(x => x.Id).ToList();
        Debug.Log("Swapped Wires: " + string.Join(",", swappedwires.Select(x => x.Id)));
    }

    public Dictionary<string, Wire> WireMap = new Dictionary<string, Wire>();
    public List<Gate> Gates = new List<Gate>();
    public List<Wire> SwappedWires = new List<Wire>();

    public bool RunDevice()
    {
        List<Gate> gatesToVisit = new List<Gate>(Gates);
        List<Gate> operableGates = Gates.Where(g => g.Inputs.All(w => w.Value != null) && g.Output.Value == null).ToList();
        gatesToVisit = gatesToVisit.Where(g => !operableGates.Contains(g)).ToList();

        while (operableGates.Count() > 0)
        {
            foreach (var gate in operableGates)
            {
                gate.Operate();
            }
            operableGates = gatesToVisit.Where(g => g.Inputs.All(w => w.Value != null) && g.Output.Value == null).ToList();
            gatesToVisit = gatesToVisit.Where(g => !operableGates.Contains(g)).ToList();
        }

        return gatesToVisit.Count == 0;
    }

    public bool TestWorkingDevice()
    {
        int x = UnityEngine.Random.Range(0, 100000);
        int y = UnityEngine.Random.Range(0, 100000);
        SetInput(x, y);
        RunDevice();
        return GetOutput() == (x + y);
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


    public void SetInput(int x, int y)
    {
        List<string> inputXWires = new List<string>();
        List<string> inputYWires = new List<string>();

        foreach (var wire in WireMap.Values)
        {
            if (wire.Id.StartsWith("x"))
            {
                inputXWires.Add(wire.Id);
            }
            if (wire.Id.StartsWith("y"))
            {
                inputYWires.Add(wire.Id);
            }
        }

        for (int i = 0; i < inputXWires.Count; i++)
        {
            GetWire(inputXWires[i]).Value = (x & (1 << i)) > 0;
            GetWire(inputYWires[i]).Value = (y & (1 << i)) > 0;
        }
    }

    public Wire GetWire(string Id)
    {
        return WireMap[Id];
    }

    public void AssessDevice()
    {
        for (int i = 0; i < NUM_BITS; i++)
        {
            if (i == 0) { continue; }
            Wire carry = FindCarryIn(i);
            Debug.Log($"Carry in for {i}: " + carry?.Id);
            Wire carryOut = FindCarryOut(i);
            Debug.Log($"Carry out for {i}: " + carryOut?.Id);
            var relevantWires = GetRelevantWires(i).ToList();
            Debug.Log("Relevant wires: " + string.Join(",", relevantWires));
            if (!BitAdderWorks(i))
            {
                Debug.Log($"Bit Adder {i} did not work");
                if (AttemptInternalFix(i))
                {
                    Debug.Log("INTERNAL FIX SUCCEEDED!");
                }
                else if (AttemptCarryPairingFix(i))
                {
                    Debug.Log("CARRY PAIRING FIX SUCCEEDED!");
                }
                else
                {
                    Debug.Log("Fix failed");
                }
            }
            Debug.Log("Swapped wires: " + string.Join(",", SwappedWires.Select(x => x.Id)));
        }
    }

    private Wire FindCarryIn(int idx)
    {
        // find the only wire "in"
        Wire outputZ = GetWire('z', idx);

        HashSet<Wire> visitedWires = new HashSet<Wire>();
        // search forward, checking ancestors of other wires
        Queue<Wire> queue = new Queue<Wire>();

        queue.Enqueue(outputZ);

        int ct = 0;
        while (queue.Count > 0 && ct < 1000)
        {
            Wire wire = queue.Dequeue();
            visitedWires.Add(wire);

            if (GetAllAncestors(wire).All(x => (!x.IsInput()) || (x.GetIndex() < idx)))
            {
                // First wire that has all ancestors from earlier indices
                return wire;
            }

            if (wire.ProvidingGate != null)
            {
                foreach (var input in wire.ProvidingGate.Inputs)
                {
                    if (!visitedWires.Contains(input))
                    {
                        visitedWires.Add(wire);
                        queue.Enqueue(input);
                    }
                }
            }
            ct++;
        }
        Debug.Log("Carry in not found " + ct);
        return null;
    }

    private Wire FindCarryOut(int idx)
    {
        return FindCarryIn(idx + 1);


        Wire inputX = GetWire('x', idx);
        Wire inputY = GetWire('y', idx);

        HashSet<Wire> visitedWires = new HashSet<Wire>();
        // search forward, checking ancestors of other wires
        Queue<Wire> queue = new Queue<Wire>();

        queue.Enqueue(inputX);
        queue.Enqueue(inputY);

        while (queue.Count > 0)
        {
            Wire wire = queue.Dequeue();
            visitedWires.Add(wire);

            if (GetAllDescendents(wire).All(x => (!x.IsOutput()) || (x.GetIndex() > idx)))
            {
                // First wire that only contains descendents of a higher index is considered the carry
                return wire;
            }

            foreach (var gate in Gates)
            {
                if (gate.InputIds.Contains(wire.Id))
                {
                    queue.Enqueue(gate.Output);
                }
            }
        }
        return null;
    }

    // get all wires that contribute to this wire
    public HashSet<Wire> GetAllAncestors(Wire wire)
    {
        HashSet<Wire> ancestors = new HashSet<Wire>();
        Queue<Wire> queue = new Queue<Wire>();
        queue.Enqueue(wire);

        while (queue.Count > 0)
        {
            Wire currentWire = queue.Dequeue();
            ancestors.Add(currentWire);

            if (currentWire.ProvidingGate != null)
            {
                foreach (var input in currentWire.ProvidingGate.Inputs)
                {
                    if (!ancestors.Contains(input))
                    {
                        queue.Enqueue(input);
                    }
                }
            }
        }

        return ancestors;
    }

    // Get all wires that are derived from / depend on this wire
    public HashSet<Wire> GetAllDescendents(Wire wire)
    {

        HashSet<Wire> descendents = new HashSet<Wire>();
        Queue<Wire> queue = new Queue<Wire>();
        queue.Enqueue(wire);

        while (queue.Count > 0)
        {
            Wire currentWire = queue.Dequeue();
            descendents.Add(currentWire);

            foreach (var gate in Gates)
            {
                if (gate.InputIds.Contains(currentWire.Id))
                {
                    queue.Enqueue(gate.Output);
                }
            }
        }

        return descendents;
    }

    public bool AttemptInternalFix(int idx)
    {
        Wire carryIn = FindCarryIn(idx);
        Wire carryOut = FindCarryOut(idx);
        var relevantWires = GetRelevantWires(idx).Where((x) => !GetWire(x).IsInput() && x != carryIn?.Id).ToList();
        for (int i = 0; i < relevantWires.Count; i++)
        {
            for (int j = i; j < relevantWires.Count; j++)
            {
                Wire wireA = GetWire(relevantWires[i]);
                Wire wireB = GetWire(relevantWires[j]);
                // No need to switch same wires or input wires
                if (i == j)
                {
                    continue;
                }

                // Swap the wires
                SwapWires(wireA, wireB);

                if (BitAdderWorks(idx, carryIn, carryOut))
                {
                    // Fix worked! 
                    Debug.Log("Fix succeeded by swapping : " + wireA.Id + ", " + wireB.Id + "!");
                    SwappedWires.Add(wireA);
                    SwappedWires.Add(wireB);
                    return true;
                }
                else
                {
                    SwapWires(wireA, wireB);
                }
            }
        }
        return false;
    }

    public bool AttemptCarryPairingFix(int idx)
    {
        var relevantAWires = GetRelevantWires(idx).Where((x) => !GetWire(x).IsInput()).ToList();
        var relevantBWires = (GetRelevantWires(idx + 1)).Where((x) => !GetWire(x).IsInput()).ToList();

        for (int i = 0; i < relevantAWires.Count; i++)
        {
            for (int j = 0; j < relevantBWires.Count; j++)
            {
                Wire wireA = GetWire(relevantAWires[i]);
                Wire wireB = GetWire(relevantBWires[j]);

                // Swap the wires
                SwapWires(wireA, wireB);

                if (BitAdderWorks(idx) && BitAdderWorks(idx + 1))
                {
                    // Fix worked! 
                    Debug.Log("Fix succeeded by swapping : " + wireA.Id + ", " + wireB.Id + "!");
                    SwappedWires.Add(wireA);
                    SwappedWires.Add(wireB);
                    return true;
                }
                else
                {
                    SwapWires(wireA, wireB);
                }
            }
        }
        return false;
    }

    public void SwapWires(Wire wireA, Wire wireB)
    {
        Gate gateA = wireA.ProvidingGate;
        Gate gateB = wireB.ProvidingGate;

        wireA.ProvidingGate = gateB;
        gateB.OutputId = wireA.Id;

        wireB.ProvidingGate = gateA;
        gateA.OutputId = wireB.Id;
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

        // Add wires "from the output back"
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

        // Add wires "from the input forward"
        queue.Enqueue(inputX.Id);
        queue.Enqueue(inputY.Id);
        stopAdding = false;
        while (queue.Count > 0)
        {
            string wireId = queue.Dequeue();
            relevantWires.Add(wireId);

            if (GetWire(wireId) == outputZ)
            {
                // we've gotten to the output. Stop adding more wires beyond this "depth from the input"
                stopAdding = true;
            }

            if (!stopAdding)
            {
                foreach (var gate in Gates)
                {
                    if (gate.InputIds.Contains(wireId))
                    {
                        queue.Enqueue(gate.Output.Id);
                    }
                }
            }
        }

        return relevantWires;
    }

    public bool BitAdderWorks(int idx)
    {
        Wire carryIn = FindCarryIn(idx);
        Wire carryOut = FindCarryOut(idx);
        return BitAdderWorks(idx, carryIn, carryOut);
    }

    public bool BitAdderWorks(int idx, Wire carryIn, Wire carryOut)
    {
        if (carryOut == null || carryIn == null)
        {
            return false;
        }

        Wire inputX = GetWire('x', idx);
        Wire inputY = GetWire('y', idx);
        Wire outputZ = GetWire('z', idx);

        // x,y,carry => z, carryout
        Dictionary<Tuple<bool, bool, bool>, Tuple<bool, bool>> testCases = new()
        {
            { new Tuple<bool, bool, bool>(false, false, false), new Tuple<bool, bool>(false, false) },
            { new Tuple<bool, bool, bool>(false, false, true), new Tuple<bool, bool>(true, false) },
            { new Tuple<bool, bool, bool>(false, true, false), new Tuple<bool, bool>(true, false) },
            { new Tuple<bool, bool, bool>(false, true, true), new Tuple<bool, bool>(false, true) },
            { new Tuple<bool, bool, bool>(true, false, false), new Tuple<bool, bool>(true, false) },
            { new Tuple<bool, bool, bool>(true, false, true), new Tuple<bool, bool>(false, true) },
            { new Tuple<bool, bool, bool>(true, true, false), new Tuple<bool, bool>(false, true) },
            { new Tuple<bool, bool, bool>(true, true, true), new Tuple<bool, bool>(true, true) }
        };

        if (carryIn == null)
        {
            testCases = testCases.Where(x => x.Key.Item3 == false).ToDictionary(x => x.Key, x => x.Value);
        }
        // if (carryOut == null)
        // {
        //     testCases = testCases.Where(x => x.Value.Item2 == false).ToDictionary(x => x.Key, x => x.Value);
        // }

        foreach (var test in testCases)
        {
            // reset
            WireMap.Values.ToList().ForEach(x => x.Value = null);

            // set inputs
            inputX.Value = test.Key.Item1;
            inputY.Value = test.Key.Item2;
            if (carryIn != null)
            {
                carryIn.Value = test.Key.Item3;
            }

            RunDevice();

            if (outputZ.Value != test.Value.Item1 || carryOut.Value != test.Value.Item2)
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

        public int GetIndex()
        {
            return int.Parse(Id.TrimStart('x', 'y', 'z'));
        }

        public bool IsInput()
        {
            return ProvidingGate == null;
        }

        public bool IsOutput()
        {
            return Id.StartsWith("z");
        }
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