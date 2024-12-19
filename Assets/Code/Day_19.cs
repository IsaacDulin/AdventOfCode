using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Day19 : MonoBehaviour
{
    public TextAsset Input;

    [ContextMenu("Run Pt 1")]
    public void Run()
    {
        ParseInput(Input.text, out List<string> towelPatterns, out List<string> targetDesigns);
        var towelOrganizer = new TowelOrganizer(towelPatterns);
        int count = 0;
        foreach (var targetDesign in targetDesigns)
        {
            if (towelOrganizer.Solve(targetDesign) > 0)
            {
                count++;
            }
        }
        Debug.Log($"{count} designs can be made");
    }

    [ContextMenu("Run Pt 2")]
    public void RunPt2()
    {
        ParseInput(Input.text, out List<string> towelPatterns, out List<string> targetDesigns);
        var towelOrganizer = new TowelOrganizer(towelPatterns);
        long count = 0;
        foreach (var targetDesign in targetDesigns)
        {
            count += towelOrganizer.Solve(targetDesign);
        }
        Debug.Log($"Total count of design possibilites {count}");
    }

    public void ParseInput(string input, out List<string> towelPatterns, out List<string> targetDesigns)
    {
        towelPatterns = new List<string>();
        targetDesigns = new List<string>();
        var lines = input.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            if (i == 0)
            {
                towelPatterns = lines[i].Trim().Split(", ").ToList();
            }
            else
            {
                if (!string.IsNullOrEmpty(lines[i].Trim()))
                {
                    targetDesigns.Add(lines[i].Trim());
                }
            }
        }
    }

    public class TowelOrganizer
    {
        public TowelOrganizer(List<string> towelPatterns)
        {
            _towelPatterns = towelPatterns;
        }

        List<string> _towelPatterns;

        // Returns path count
        public long Solve(string targetDesign)
        {
            var queue = new PriorityQueue<string>();
            string startState = "";
            queue.Enqueue(startState, 0);
            Dictionary<string, long> pathToStateCt = new Dictionary<string, long>();
            pathToStateCt[startState] = 1;

            while (queue.Count > 0)
            {
                string currentState = queue.Dequeue();
                if (currentState == targetDesign)
                {
                    if (queue.Count > 0)
                    {
                        Debug.Log("The queue was not empty - it should be.");
                    }
                    return pathToStateCt[currentState];
                }

                foreach (string adjacentState in GetNextPossibleStrings(currentState, targetDesign))
                {
                    if (pathToStateCt.ContainsKey(adjacentState))
                    {
                        pathToStateCt[adjacentState] += pathToStateCt[currentState];
                    }
                    else
                    {
                        // Enqueue the adjacent state path
                        queue.Enqueue(adjacentState, adjacentState.Length);
                        pathToStateCt[adjacentState] = pathToStateCt[currentState];
                    }
                }
            }
            return 0;
        }

        public List<string> GetNextPossibleStrings(string currentState, string targetDesign)
        {
            List<string> nextPossibleStates = new List<string>();
            for (int i = 0; i < _towelPatterns.Count; i++)
            {
                string nextState = currentState + _towelPatterns[i];

                if (targetDesign.StartsWith(nextState))
                {
                    nextPossibleStates.Add(nextState);
                }
            }
            return nextPossibleStates;
        }
    }
}