using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Pose = Day6.Pose;

// Started from a copy of Day 16
public class Day20 : MonoBehaviour
{
    public const int REQUIRED_IMPROVEMENT = 100;
    public TextAsset Input;

    [ContextMenu("Run Pt 1")]
    public void Run()
    {
        var maze = new Maze(Input.text);
        List<Path> paths = maze.Solve(new State() { Position = maze.StartPos, StartCheatPos = new Vector2Int(-1, -1), EndCheatPos = new Vector2Int(-1, -1) }, int.MaxValue);
        int baseLineSpeed = paths[0].Count;
        Debug.Log($"Shortest path: {baseLineSpeed}");
        List<Path> shortcutPaths = maze.Solve(new State() { Position = maze.StartPos }, baseLineSpeed - REQUIRED_IMPROVEMENT);
        Debug.Log($"Number of good cheats: {shortcutPaths.Count}");
    }

    [ContextMenu("Run Pt 2")]
    public void RunPt2()
    {
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
            Height = _map.Max(x => x.Key.x);
            Width = _map.Max(x => x.Key.y);
        }

        public List<Path> Solve(State startState, int timeToBeat)
        {
            Dictionary<State, Path> shortestPath = new Dictionary<State, Path>();
            shortestPath[startState] = new Path() { startState };

            var queue = new Queue<State>();
            queue.Enqueue(startState);

            List<Path> bestPaths = new List<Path>();
            while (queue.Count > 0)
            {
                State currentState = queue.Dequeue();

                if (_map[currentState.Position] == MazeItem.End)
                {
                    if (shortestPath[currentState].Count <= timeToBeat)
                    {
                        bestPaths.Add(shortestPath[currentState]);
                        continue;
                    }
                }

                foreach (State adjacentState in GetAdjacentStates(currentState))
                {
                    int leastCost = shortestPath[currentState].Count + 1;
                    if (!shortestPath.TryGetValue(adjacentState, out Path path) || path.Count < leastCost)
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

        public List<State> GetAdjacentStates(State state)
        {
            List<Vector2Int> allAdjacentSquares = GetAdjacentSquares(state.Position);
            List<Vector2Int> adjacentEmptySquares = allAdjacentSquares.Where(x => _map[x] != MazeItem.Wall).ToList();
            List<Vector2Int> adjacentWalls = allAdjacentSquares.Where(x => _map[x] == MazeItem.Wall).ToList();

            List<State> adjacentStates = new List<State>();

            switch (state.CheatState)
            {
                case CheatingState.HasNotCheated:
                    foreach (Vector2Int emptySquare in adjacentEmptySquares) // could continue not cheating or start cheating
                    {
                        State newState = new State()
                        {
                            Position = emptySquare,
                        };
                        adjacentStates.Add(newState);
                    }
                    foreach (Vector2Int wall in adjacentWalls)
                    {
                        State newState = new State()
                        {
                            Position = wall,
                            StartCheatPos = wall,

                        };
                        adjacentStates.Add(newState);
                    }
                    break;
                case CheatingState.MidCheating: // progress to two no matter what
                    foreach (Vector2Int square in allAdjacentSquares)
                    {
                        State newState = new State()
                        {
                            Position = square,
                            StartCheatPos = state.StartCheatPos,
                            EndCheatPos = square,
                        };
                        adjacentStates.Add(newState);
                    }
                    break;
                case CheatingState.CheatingDone: // Done cheating now no matter what
                    foreach (Vector2Int emptySquare in adjacentEmptySquares)
                    {
                        State newState = new State()
                        {
                            Position = emptySquare,
                            StartCheatPos = state.StartCheatPos,
                            EndCheatPos = state.EndCheatPos,
                        };
                        adjacentStates.Add(newState);
                    }
                    break;
            }
            return adjacentStates;
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
    }

    public class State : IEquatable<State>
    {
        public Vector2Int Position;
        public Vector2Int? StartCheatPos = null;
        public Vector2Int? EndCheatPos = null;

        public CheatingState CheatState
        {
            get
            {
                if (StartCheatPos == null && EndCheatPos == null)
                {
                    return CheatingState.HasNotCheated;
                }
                if (StartCheatPos != null && EndCheatPos == null)
                {
                    return CheatingState.MidCheating;
                }
                if (StartCheatPos != null && EndCheatPos != null)
                {
                    return CheatingState.CheatingDone;
                }
                throw new System.Exception("Invalid cheating state");
            }
        }

        public bool Equals(State other)
        {
            return Position == other.Position && StartCheatPos == other.StartCheatPos && EndCheatPos == other.EndCheatPos;
        }

        public override bool Equals(object obj)
        {
            if (obj is State other)
            {
                return other.Position == Position && StartCheatPos == other.StartCheatPos && EndCheatPos == other.EndCheatPos;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode() * 31 + StartCheatPos.GetHashCode() * 17 + EndCheatPos.GetHashCode() * 11;
        }
    }


    public enum CheatingState
    {
        HasNotCheated,
        MidCheating,
        CheatingDone,
    }

    public class Path : List<State>
    {
        public Path() : base() { }

        public Tuple<Vector2Int, Vector2Int> GetCheatId()
        {
            Vector2Int first = new Vector2Int(-1, -1);
            Vector2Int second = new Vector2Int(-1, -1);
            foreach (var state in this)
            {
                if (state.CheatState == CheatingState.MidCheating)
                {
                    first = state.Position;
                }
                if (state.CheatState == CheatingState.CheatingDone)
                {
                    second = state.Position;
                }
            }
            return new Tuple<Vector2Int, Vector2Int>(first, second);
        }


    }
}