using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;
using Pose = Day6.Pose;

public class Day16 : MonoBehaviour
{
    public TextAsset Input;

    [ContextMenu("Run Pt 1")]
    public void Run()
    {
        var maze = new Maze(Input.text);
        var paths = maze.Solve();
        Debug.Log($"Path cost: {paths[0].Cost}");
    }

    [ContextMenu("Run Pt 2")]
    public void RunPt2()
    {
        var maze = new Maze(Input.text);
        var paths = maze.Solve();
        HashSet<Vector2Int> goodSpots = new HashSet<Vector2Int>();
        foreach (var path in paths)
        {
            foreach (var pose in path)
            {
                goodSpots.Add(pose.Position);
            }
        }
        Debug.Log($"Good spots: {goodSpots.Count}");
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

        public Pose StartPose;

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
                        StartPose = new Pose()
                        {
                            Position = new Vector2Int(j, i),
                            Direction = new Vector2Int(1, 0) // facing east
                        };
                    }
                }
            }
            Height = _map.Max(x => x.Key.x);
            Width = _map.Max(x => x.Key.y);
        }

        public List<Path> Solve()
        {
            Pose currentState = StartPose;

            var queue = new Queue<Path>();
            Dictionary<Pose, int> smallestPathCost = new();

            smallestPathCost[currentState] = 0;
            queue.Enqueue(new Path() { currentState });

            List<Path> bestPaths = new List<Path>();

            while (queue.Count > 0)
            {
                Path currentStatePath = queue.Dequeue();
                currentState = currentStatePath.Last();

                if (_map[currentState.Position] == MazeItem.End)
                {
                    // this might just return the first one right now?
                    int bestCost = bestPaths.Count > 0 ? bestPaths.Min(x => x.Cost) : int.MaxValue;
                    if (currentStatePath.Cost < bestCost)
                    {
                        bestPaths = new List<Path>() { currentStatePath };
                    }
                    else if (currentStatePath.Cost == bestCost)
                    {
                        bestPaths.Add(currentStatePath);
                    }
                }

                foreach (Move move in GetAvailableMoves(currentState))
                {
                    Pose adjacentState = move.NewPose;
                    int pathCost = currentStatePath.Cost + move.Cost;
                    if (!smallestPathCost.TryGetValue(adjacentState, out int leastCost) || pathCost <= leastCost)
                    {
                        // Document the path to this new state
                        var newPath = new Path();
                        newPath.AddRange(currentStatePath);
                        newPath.Add(adjacentState);
                        newPath.Cost = pathCost;

                        // Enqueue the adjacent state path
                        queue.Enqueue(newPath);
                        smallestPathCost[adjacentState] = pathCost;
                    }
                }
            }

            return bestPaths;
        }

        public List<Move> GetAvailableMoves(Pose state)
        {
            List<Move> moves = new List<Move>();

            // Move forward
            Pose forward = new Pose()
            {
                Position = state.Position + state.Direction,
                Direction = state.Direction
            };
            if (_map[forward.Position] != MazeItem.Wall)
            {
                moves.Add(new Move() { NewPose = forward, Cost = 1 });
            }

            // Turn left
            Pose left = new Pose()
            {
                Position = state.Position,
                Direction = new Vector2Int(-state.Direction.y, state.Direction.x)
            };
            moves.Add(new Move() { NewPose = left, Cost = 1000 });

            // Turn right
            Pose right = new Pose()
            {
                Position = state.Position,
                Direction = new Vector2Int(state.Direction.y, -state.Direction.x)
            };
            moves.Add(new Move() { NewPose = right, Cost = 1000 });

            return moves;
        }
    }

    public class Move
    {
        public Pose NewPose;
        public int Cost;
    }

    public class Path : List<Pose>
    {
        public Path() : base() { }
        public int Cost = 0;
    }
}