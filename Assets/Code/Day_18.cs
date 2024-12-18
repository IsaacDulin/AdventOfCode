using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;

public class Day18 : MonoBehaviour
{
    public TextAsset Input;

    [ContextMenu("Run Pt 1")]
    public void Run()
    {
        var fallingMemory = ParseFallingMemory(Input.text);
        MemorySpace memorySpace = new MemorySpace();
        memorySpace.SimulateMemoryFalling(fallingMemory.GetRange(0, 1024));
        int shortestPath = memorySpace.Solve();
        Debug.Log($"Shortest path: {shortestPath}");
    }

    [ContextMenu("Run Pt 2")]
    public void RunPt2()
    {
        var fallingMemory = ParseFallingMemory(Input.text);
        MemorySpace memorySpace = new MemorySpace();
        foreach (var location in fallingMemory)
        {
            memorySpace.SimulateMemoryFalling(new List<Vector2Int> { location });
            try
            {
                memorySpace.Solve();
            }
            catch
            {
                Debug.Log("The first byte that blocks the path is: " + location);
                break;
            }
        }
    }

    public List<Vector2Int> ParseFallingMemory(string input)
    {
        List<Vector2Int> fallingMemoryLocations = new List<Vector2Int>();
        foreach (var line in input.Split('\n'))
        {
            var split = line.Split(',');
            fallingMemoryLocations.Add(new Vector2Int(int.Parse(split[0].Trim()), int.Parse(split[1].Trim())));
        }
        return fallingMemoryLocations;
    }

    public class MemorySpace
    {
        public int Width = 71;
        public int Height = 71;
        private HashSet<Vector2Int> _corruptedMemory = new();

        public Vector2Int StartVector2Int;

        public MemorySpace() { }

        public void SimulateMemoryFalling(List<Vector2Int> memory)
        {
            foreach (var location in memory)
            {
                _corruptedMemory.Add(location);
            }
        }

        public int Solve()
        {
            var queue = new Queue<Vector2Int>();
            Dictionary<Vector2Int, int> shortestPath = new();
            Vector2Int startState = new Vector2Int(0, 0);
            shortestPath[startState] = 0;
            queue.Enqueue(startState);

            while (queue.Count > 0)
            {
                Vector2Int currentState = queue.Dequeue();
                if (currentState == new Vector2(Width - 1, Height - 1))
                {
                    return shortestPath[currentState];
                }

                foreach (Vector2Int adjacentState in GetAdjacentSpaces(currentState))
                {
                    int shortestDistance = shortestPath[currentState] + 1;
                    if (!shortestPath.TryGetValue(adjacentState, out int currentShortestDistance) || shortestDistance < currentShortestDistance)
                    {
                        // Enqueue the adjacent state path
                        queue.Enqueue(adjacentState);
                        shortestPath[adjacentState] = shortestDistance;
                    }
                }
            }
            throw new Exception("No solution found");
        }

        public List<Vector2Int> GetAdjacentSpaces(Vector2Int position)
        {
            List<Vector2Int> spaces = new List<Vector2Int>();

            // Move up
            if (position.y < Height - 1)
            {
                spaces.Add(new Vector2Int(position.x, position.y + 1));
            }

            // Move down
            if (position.y > 0)
            {
                spaces.Add(new Vector2Int(position.x, position.y - 1));
            }

            // Move right
            if (position.x < Width - 1)
            {
                spaces.Add(new Vector2Int(position.x + 1, position.y));
            }
            // Move left
            if (position.x > 0)
            {
                spaces.Add(new Vector2Int(position.x - 1, position.y));
            }

            return spaces.Where((x) => !_corruptedMemory.Contains(x)).ToList();
        }
    }

    public class Path : List<Vector2Int>
    {
        public Path() : base() { }
        // public int Cost = 0;
    }
}