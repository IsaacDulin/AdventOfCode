using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Day6 : MonoBehaviour
{
    public TextAsset Input;

    [ContextMenu("Run Part 1")]
    public void Run()
    {
        var map = ParseInput();
        Utils.PrintCharArray(map.MapData);
        var visited = new HashSet<Vector2Int>
        {
            map.GuardPose.Position
        };
        while (map.MoveGuard() != null)
        {
            visited.Add(map.GuardPose.Position);
            Debug.Log("New Guard Position: " + map.GuardPose.Position);
        }

        Utils.PrintCharArray(map.MapData);

        Debug.Log($"Visited {visited.Count} unique positions");
    }

    [ContextMenu("Run Part 2")]
    // This takes ~7 minutes to run - lol
    public void RunPt2()
    {
        var maps = ModifiedMaps();
        int newMapsWithLoops = 0;

        foreach (var map in maps)
        {
            var visited = new HashSet<Pose>
            {
                map.GuardPose
            };
            while (map.MoveGuard() != null)
            {
                if (visited.Contains(map.GuardPose))
                {
                    newMapsWithLoops++;
                    break;
                }
                visited.Add(map.GuardPose);
            }

        }
        Debug.Log($"Possible new maps with loops: {newMapsWithLoops}");
    }

    List<Map> ModifiedMaps()
    {
        var maps = new List<Map>();
        var map = ParseInput();
        for (int i = 0; i < map.MapData.Length; i++)
        {
            for (int j = 0; j < map.MapData[i].Length; j++)
            {
                if (map.MapData[i][j] == '.')
                {
                    var newMap = new Map(Input.text);
                    newMap.MapData[i][j] = '#';
                    maps.Add(newMap);
                }
            }
        }

        return maps;
    }


    private Map ParseInput()
    {
        return new Map(Input.text);
    }

    public class Map
    {
        public char[][] MapData;

        public Pose GuardPose;

        public char this[Vector2Int pos]
        {
            get => MapData[pos.x][pos.y];
            set => MapData[pos.x][pos.y] = value;
        }

        public Map(string input)
        {
            var lines = input.Split('\n');
            MapData = new char[lines.Length][];
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim().ToCharArray();
                if (line.Contains('^'))
                {
                    int idx = line.ToList().IndexOf('^');
                    GuardPose = new Pose()
                    {
                        Position = new Vector2Int(i, idx),
                        Direction = new Vector2Int(-1, 0) // North
                    };
                }

                MapData[i] = line;
            }
        }

        public Vector2Int? MoveGuard()
        {
            var aheadPos = GuardPose.Position + GuardPose.Direction;
            if (OutOfBounds(aheadPos))
            {
                return null;
            }

            if (this[aheadPos] == '#') // Obstacle, turn
            {
                RotateGuard();
            }
            else
            {
                GuardPose.Position = aheadPos;
            }

            this[GuardPose.Position] = 'X';
            return GuardPose.Position;
        }

        private bool OutOfBounds(Vector2Int pos)
        {
            return pos.x < 0 || pos.x >= MapData.Length || pos.y < 0 || pos.y >= MapData[0].Length;
        }

        public void RotateGuard()
        {
            if (GuardPose.Direction == Vector2Int.up)
            {
                GuardPose.Direction = Vector2Int.right;
            }
            else if (GuardPose.Direction == Vector2Int.right)
            {
                GuardPose.Direction = Vector2Int.down;
            }
            else if (GuardPose.Direction == Vector2Int.down)
            {
                GuardPose.Direction = Vector2Int.left;
            }
            else if (GuardPose.Direction == Vector2Int.left)
            {
                GuardPose.Direction = Vector2Int.up;
            }
        }
    }

    public struct Pose : System.IEquatable<Pose>
    {
        public Vector2Int Position;
        public Vector2Int Direction;

        public override bool Equals(object obj)
        {
            if (obj is Pose other)
            {
                return other.Position == Position && other.Direction == Direction;
            }

            return false;
        }

        public bool Equals(Pose other)
        {
            if (other.Position == Position && other.Direction == Direction)
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode() * 17 + Direction.GetHashCode();
        }
    }
}
