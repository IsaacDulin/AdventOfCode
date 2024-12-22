using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Cache = System.Collections.Generic.Dictionary<(char currentKey, char nextKey, int depth), long>;

// This is the first time I really got stuck on my own. I had implemented this without a true cost solver under the assumption
// that the most direct paths would always be the same cost. After not passing the 4th test case, I understood
// that the shortest path at the higher robot-recursion levels couldn't be inferred without a real solver. 
// I did some research, finally understood what I really needed to do,
// then implemented this solution, adapted from here: https://aoc.csokavar.hu/2024/21/
public class Day21 : MonoBehaviour
{
    public TextAsset Input;

    [ContextMenu("Run Pt 1")]
    public void Run()
    {
        var codes = ParseCodes(Input.text);
        long output=Solve(codes, 2);
        Debug.Log("output : " + output);
    }

    [ContextMenu("Run Pt 2")]
    public void RunPt2()
    {
        var codes = ParseCodes(Input.text);
        long output=Solve(codes, 25);
        Debug.Log("output : " + output);
    }

    long Solve(List<string> inputCodes, int intermediateRobots) 
    {
        List<Dictionary<Vector2Int, char>> keypads = new();
        keypads.Add(NumberPadMap);
        keypads.AddRange(Enumerable.Repeat(ControlPanelMap, intermediateRobots));

        Cache cache = new Cache();
        long res = 0;

        foreach (var line in inputCodes) 
        {
            var num = GetNumericPartOfCode(line.Trim());
            res += num * Solve(line.Trim(), keypads, cache);
        }
        return res;
    }

    long Solve(string keys, List<Dictionary<Vector2Int, char>> keypads, Cache cache) 
    {
        if (keypads.Count == 0)  // no more keypads - this is our final output
        {
            return keys.Length;
        } 
        else 
        {
            char currentKey = 'A';
            long length = 0;

            foreach (var nextKey in keys) 
            {
                length += SolveSingleMove(currentKey, nextKey, keypads, cache);
                // while the sequence is entered the current key changes accordingly
                currentKey = nextKey;
            }

            // at the end the current key should be reset to 'A'
            Debug.Assert(currentKey == 'A', "The robot should point at the 'A' key");
            return length;
        }
    }

    long SolveSingleMove(char currentKey, char nextKey, List<Dictionary<Vector2Int, char>> keypads, Cache cache) 
    {
        if (cache.TryGetValue((currentKey, nextKey, keypads.Count), out long cost))
        {
            return cost;
        }
        else
        {
           Dictionary<Vector2Int, char> keypad = keypads[0];

           var currentPos = keypad.Single(kvp => kvp.Value == currentKey).Key;
           var nextPos = keypad.Single(kvp => kvp.Value == nextKey).Key;

           var dy = nextPos.y - currentPos.y;
           var vert = new string(dy < 0 ? 'v' : '^', Math.Abs(dy));

           var dx = nextPos.x - currentPos.x;
           var horiz = new string(dx < 0 ? '<' : '>', Math.Abs(dx));

           cost = long.MaxValue;

           if (keypad.ContainsKey(new Vector2Int(currentPos.x, nextPos.y)))
           {
               cost = Math.Min(cost, Solve($"{vert}{horiz}A", keypads.GetRange(1, keypads.Count -1), cache));
           }

           if (keypad.ContainsKey(new Vector2Int(nextPos.x, currentPos.y))) 
           {
               cost = Math.Min(cost, Solve($"{horiz}{vert}A", keypads.GetRange(1, keypads.Count -1), cache));
           }

            cache[(currentKey, nextKey, keypads.Count)] = cost;
            return cost;
        }
    }

    private int GetNumericPartOfCode(string code)
    {
        string trimmed = code.Trim('A');
        return int.Parse(trimmed);
    }

    private List<string> ParseCodes(string input)
    {
        return input.Split('\n').Select(x => x.Trim()).ToList();
    }

    public Dictionary<Vector2Int, char> NumberPadMap = new Dictionary<Vector2Int, char>()
    {
        {new Vector2Int(0,0), 'A' },
        {new Vector2Int(-1,0), '0' },
        {new Vector2Int(-2,1), '1' },
        {new Vector2Int(-1,1), '2' },
        {new Vector2Int(0,1), '3' },
        {new Vector2Int(-2,2), '4' },
        {new Vector2Int(-1,2), '5' },
        {new Vector2Int(0,2), '6' },
        {new Vector2Int(-2,3), '7' },
        {new Vector2Int(-1,3), '8' },
        {new Vector2Int(0,3), '9' },
    };

    public Dictionary<Vector2Int, char> ControlPanelMap = new Dictionary<Vector2Int, char>()
    {
        {new Vector2Int(0,0), 'A' },
        {new Vector2Int(-1,0), '^' },
        {new Vector2Int(-2,-1), '<' },
        {new Vector2Int(-1,-1), 'v' },
        {new Vector2Int(0,-1), '>' },
    };

    public class Path : List<char> {}
}
