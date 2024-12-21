using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Day20 : MonoBehaviour
{
    public const int REQUIRED_IMPROVEMENT = 100;
    public TextAsset Input;

    [ContextMenu("Run Pt 1")]
    public void Run()
    {
        var maze = new Maze(Input.text);
        List<Path> paths = maze.Solve(maze.StartPos);
        int baseLineSpeed = paths[0].Count;
        int ct = maze.FindShortcuts(paths[0], 2, REQUIRED_IMPROVEMENT).Count;
        Debug.Log($"Number of good cheats: {ct}");
    }

    [ContextMenu("Run Pt 2")]
    public void RunPt2()
    {
        var maze = new Maze(Input.text);
        List<Path> paths = maze.Solve(maze.StartPos);
        int baseLineSpeed = paths[0].Count;
        var shortcuts = maze.FindShortcuts(paths[0], 20, REQUIRED_IMPROVEMENT);
        Debug.Log($"Number of good cheats {shortcuts.Count}");
    }

    public enum MazeItem
    {
        Empty,
        Wall,
        Start,
        End
    }

    public class Maze
    {
        private Dictionary<Vector2Int, MazeItem> _map = new Dictionary<Vector2Int, MazeItem>();

        private Dictionary<char, MazeItem> _objectMap = new Dictionary<char, MazeItem>() {
            {'.', MazeItem.Empty},
            {'#', MazeItem.Wall},
            {'S', MazeItem.Start},
            {'E', MazeItem.End},
        };

        public Vector2Int StartPos;

        public int Height { get; private set; }
        public int Width { get; private set; }

        public Maze(string input)
        {
            var lines = input.Split('\n');
            for (int i = 0; i < lines.Length; i++) // height
            {
                var line = lines[i].Trim();
                for (int j = 0; j < line.Length; j++) // width
                {
                    var c = line[j];
                    _map.Add(new Vector2Int(j, i), _objectMap[c]);
                    if (_objectMap[c] == MazeItem.Start)
                    {
                        StartPos = new Vector2Int(j, i);
                    }
                }
            }
            Width = _map.Max(x => x.Key.x);
            Height = _map.Max(x => x.Key.y);
        }

        public List<Path> Solve(Vector2Int startState)
        {
            Dictionary<Vector2Int, Path> shortestPath = new Dictionary<Vector2Int, Path>();
            shortestPath[startState] = new Path() { startState };

            var queue = new Queue<Vector2Int>();
            queue.Enqueue(startState);

            List<Path> bestPaths = new List<Path>();
            while (queue.Count > 0)
            {
                Vector2Int currentState = queue.Dequeue();

                if (_map[currentState] == MazeItem.End)
                {
                    bestPaths.Add(shortestPath[currentState]);
                    continue;
                }

                foreach (Vector2Int adjacentState in GetAdjacentStates(currentState))
                {
                    int newPathLength = shortestPath[currentState].Count + 1;
                    if (!shortestPath.TryGetValue(adjacentState, out Path path) || newPathLength < path.Count)
                    {
                        // Document the path to this new state
                        var newPath = new Path();
                        newPath.AddRange(shortestPath[currentState]);
                        newPath.Add(adjacentState);

                        // Enqueue the adjacent state path
                        queue.Enqueue(adjacentState);
                        shortestPath[adjacentState] = newPath;
                    }
                }
            }

            return bestPaths;
        }

        public HashSet<Tuple<Vector2Int, Vector2Int>> FindShortcuts(Path path, int cheatSeconds, int timeToSave)
        {
            // Get index along path based on positional states
            Dictionary<Vector2Int, int> positionByTime = new();
            for (int i = 0; i < path.Count; i++)
            {
                positionByTime[path[i]] = i;
            }

            // Find all good cheats
            var goodCheats = new HashSet<Tuple<Vector2Int, Vector2Int>>();
            foreach (var state in path)
            {
                var cheatPossibilities = GetCheatPossibilities(state, cheatSeconds + 1);
                int fromIdx = positionByTime[state];
                foreach (var cheat in cheatPossibilities)
                {
                    int toIdx = positionByTime[cheat.Item2];
                    int originalDisatnce = path.Count;
                    int newDistance = path.Count - (toIdx - fromIdx) + GetDistance(state, cheat.Item2);
                    if (originalDisatnce - newDistance >= timeToSave)
                    {
                        goodCheats.Add(cheat);
                    }
                }
            }
            return goodCheats;
        }

        public HashSet<Tuple<Vector2Int, Vector2Int>> GetCheatPossibilities(Vector2Int position, int cheatSeconds)
        {
            HashSet<Tuple<Vector2Int, Vector2Int>> cheatPossibilities = new HashSet<Tuple<Vector2Int, Vector2Int>>();
            for (int i = 0; i < cheatSeconds; i++)
            {
                for (int j = 0; j < (cheatSeconds - i); j++)
                {
                    Vector2Int endPos = new Vector2Int(position.x + i, position.y + j);
                    if (IsInBounds(endPos))
                    {
                        if (_map[endPos] != MazeItem.Wall)
                        {
                            cheatPossibilities.Add(new Tuple<Vector2Int, Vector2Int>(position, endPos));
                        }
                    }

                    endPos = new Vector2Int(position.x - i, position.y + j);
                    if (IsInBounds(endPos))
                    {
                        if (_map[endPos] != MazeItem.Wall)
                        {
                            cheatPossibilities.Add(new Tuple<Vector2Int, Vector2Int>(position, endPos));
                        }
                    }

                    endPos = new Vector2Int(position.x + i, position.y - j);
                    if (IsInBounds(endPos))
                    {
                        if (_map[endPos] != MazeItem.Wall)
                        {
                            cheatPossibilities.Add(new Tuple<Vector2Int, Vector2Int>(position, endPos));
                        }
                    }

                    endPos = new Vector2Int(position.x - i, position.y - j);
                    if (IsInBounds(endPos))
                    {
                        if (_map[endPos] != MazeItem.Wall)
                        {
                            cheatPossibilities.Add(new Tuple<Vector2Int, Vector2Int>(position, endPos));
                        }
                    }
                }
            }
            return cheatPossibilities;
        }

        public int GetDistance(Vector2Int pos, Vector2Int pos2)
        {
            return Math.Abs(pos.x - pos2.x) + Math.Abs(pos.y - pos2.y);
        }

        public List<Vector2Int> GetAdjacentStates(Vector2Int state)
        {
            List<Vector2Int> allAdjacentSquares = GetAdjacentSquares(state);
            return allAdjacentSquares.Where(x => _map[x] != MazeItem.Wall).ToList();
        }

        public List<Vector2Int> GetAdjacentSquares(Vector2Int position)
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

            return spaces;
        }

        public bool IsInBounds(Vector2Int position)
        {
            return position.x >= 0 && position.x <= Width && position.y >= 0 && position.y <= Height;
        }
    }

    public class Path : List<Vector2Int> { }
}