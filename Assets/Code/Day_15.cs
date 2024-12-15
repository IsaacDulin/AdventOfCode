using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Day15 : MonoBehaviour
{
    public TextAsset Input;

    [ContextMenu("Run Pt 1")]
    public void Run()
    {
        WarehouseMap map = new WarehouseMap();
        ParseInput(map, out var directions);
        map.SimulateRobot(directions);
        Debug.Log(map.ToString());
        Debug.Log(map.CalculateGPSSum());
    }

    [ContextMenu("Run Pt 2")]
    public void RunPt2()
    {
        WarehouseMapWide map = new WarehouseMapWide();
        ParseInput(map, out var directions);
        map.SimulateRobot(directions);
        Debug.Log(map.ToString());
        Debug.Log(map.CalculateGPSSum());
    }

    /// <summary>
    /// Populates the passed in map via the ParseMap function, and parses the directions
    /// </summary>
    public void ParseInput(WarehouseMap map, out List<Vector2Int> directions)
    {
        List<string> mapLines = new List<string>();
        directions = new List<Vector2Int>();
        var lines = Input.text.Split('\n');
        bool parseMap = true;
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (line == "")
            {
                parseMap = false; // switch to parsing directions
                continue;
            }

            if (parseMap)
            {
                mapLines.Add(line);
            }
            else
            {
                char[] directionChars = line.Trim().ToCharArray();
                foreach (var c in directionChars)
                {
                    directions.Add(_directionMap[c]);
                }
            }
        }
        map.ParseMap(mapLines);
    }

    public enum WarehouseItem
    {
        Empty,
        Wall,
        Box,
        Robot,
        BoxLeft,
        BoxRight,
    }

    public Dictionary<char, Vector2Int> _directionMap = new Dictionary<char, Vector2Int>()
    {
        {'^', new Vector2Int(-1,0)}, // Up is negative x (top left is 0,0)
        {'v', new Vector2Int(1, 0)}, // Down is positive x
        {'<', new Vector2Int(0, -1)}, // Left is negative y
        {'>', new Vector2Int(0, 1)}, // Right is positive y
    };

    public class WarehouseMap
    {
        public Dictionary<Vector2Int, WarehouseItem> ObjectMap;
        public Vector2Int RobotPosition => ObjectMap.First(x => x.Value == WarehouseItem.Robot).Key;
        public int Height;
        public int Width;

        private Dictionary<char, WarehouseItem> _objectMap = new Dictionary<char, WarehouseItem>() {
            {'.', WarehouseItem.Empty},
            {'#', WarehouseItem.Wall},
            {'O', WarehouseItem.Box},
            {'@', WarehouseItem.Robot},
        };

        public virtual void ParseMap(List<string> mapLines)
        {
            ObjectMap = new Dictionary<Vector2Int, WarehouseItem>();
            for (int i = 0; i < mapLines.Count; i++)
            {
                var line = mapLines[i];
                for (int j = 0; j < line.Length; j++)
                {
                    var c = line[j];
                    var pos = new Vector2Int(i, j);
                    ObjectMap[pos] = _objectMap[c];
                }
            }
            Height = ObjectMap.Max(x => x.Key.x);
            Width = ObjectMap.Max(x => x.Key.y);
        }

        public void SimulateRobot(List<Vector2Int> directions)
        {
            foreach (var direction in directions)
            {
                TryPush(RobotPosition, direction);
            }
        }

        protected virtual bool TryPush(Vector2Int position, Vector2Int direction)
        {
            var newPosition = position + direction;
            switch (ObjectMap[newPosition])
            {
                case WarehouseItem.Empty:
                    ObjectMap[newPosition] = ObjectMap[position];
                    ObjectMap[position] = WarehouseItem.Empty;
                    return true;
                case WarehouseItem.Box:
                    bool pushed = TryPush(newPosition, direction);
                    if (pushed)
                    {
                        ObjectMap[newPosition] = ObjectMap[position];
                        ObjectMap[position] = WarehouseItem.Empty;
                    }
                    return pushed;
                case WarehouseItem.Wall:
                    return false;
                case WarehouseItem.Robot:
                    throw new System.Exception("Somehow pushed ourselves");
            }
            return false;
        }

        public long CalculateGPSSum()
        {
            long sum = 0;
            foreach (var pos in ObjectMap.Keys)
            {
                if (ObjectMap[pos] == WarehouseItem.Box || ObjectMap[pos] == WarehouseItem.BoxLeft)
                {
                    int distFromTop = pos.x;
                    int distFromLeft = pos.y;
                    sum += 100 * distFromTop + distFromLeft;
                }
            }
            return sum;
        }


        public override string ToString()
        {
            string result = "";
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    var pos = new Vector2Int(i, j);
                    switch (ObjectMap[pos])
                    {
                        case WarehouseItem.Empty:
                            result += ".";
                            break;
                        case WarehouseItem.Wall:
                            result += "#";
                            break;
                        case WarehouseItem.Box:
                            result += "O";
                            break;
                        case WarehouseItem.Robot:
                            result += "@";
                            break;
                        case WarehouseItem.BoxLeft:
                            result += "[";
                            break;
                        case WarehouseItem.BoxRight:
                            result += "]";
                            break;
                    }
                }
                result += "\n";
            }
            return result;
        }
    }


    public class WarehouseMapWide : WarehouseMap
    {
        private Dictionary<char, Tuple<WarehouseItem, WarehouseItem>> _objectMap = new Dictionary<char, Tuple<WarehouseItem, WarehouseItem>>()
        {
            {'.', new Tuple<WarehouseItem,WarehouseItem>(WarehouseItem.Empty, WarehouseItem.Empty)},
            {'#', new Tuple<WarehouseItem,WarehouseItem>(WarehouseItem.Wall, WarehouseItem.Wall)},
            {'O', new Tuple<WarehouseItem,WarehouseItem>(WarehouseItem.BoxLeft, WarehouseItem.BoxRight)},
            {'@', new Tuple<WarehouseItem,WarehouseItem>(WarehouseItem.Robot, WarehouseItem.Empty)},
        };

        public override void ParseMap(List<string> mapLines)
        {
            ObjectMap = new Dictionary<Vector2Int, WarehouseItem>();
            for (int i = 0; i < mapLines.Count; i++)
            {
                var line = mapLines[i];
                for (int j = 0; j < line.Length; j++)
                {
                    var c = line[j];
                    var posLeft = new Vector2Int(i, 2 * j);
                    var posRight = new Vector2Int(i, 2 * j + 1);
                    ObjectMap[posLeft] = _objectMap[c].Item1;
                    ObjectMap[posRight] = _objectMap[c].Item2;
                }
            }
            Height = ObjectMap.Max(x => x.Key.x);
            Width = ObjectMap.Max(x => x.Key.y);
        }

        protected override bool TryPush(Vector2Int position, Vector2Int direction)
        {
            if (CanPush(position, direction))
            {
                Move(position, direction);
            }
            return true;
        }

        // Already confirmed we can move - moves whatever's in position in direction
        private void Move(Vector2Int position, Vector2Int direction)
        {
            //var newPosition = position + direction;
            switch (ObjectMap[position])
            {
                case WarehouseItem.Empty:
                    // Nothing to move - done here
                    break;
                case WarehouseItem.Robot:
                    var newPosition = position + direction;

                    // Push anything in the new position
                    Move(newPosition, direction);

                    // Move the robot
                    ObjectMap[newPosition] = WarehouseItem.Robot;
                    ObjectMap[position] = WarehouseItem.Empty;
                    break;
                case WarehouseItem.BoxLeft:
                case WarehouseItem.BoxRight:
                    var otherBoxPtPosition = GetOtherBoxHalfPosition(position);
                    var newPositionBoxPt1 = position + direction;
                    var newPositionBoxPt2 = otherBoxPtPosition + direction;
                    WarehouseItem boxPt1Type = ObjectMap[position];
                    WarehouseItem boxPt2Type = ObjectMap[otherBoxPtPosition];

                    // Move subsequent boxes
                    bool vertical = direction.x != 0;
                    if (vertical)
                    {
                        Move(newPositionBoxPt1, direction);
                        Move(newPositionBoxPt2, direction);

                        // Both halves move out of the way, leaving both original positions empty
                        ObjectMap[newPositionBoxPt1] = boxPt1Type;
                        ObjectMap[newPositionBoxPt2] = boxPt2Type;
                        ObjectMap[otherBoxPtPosition] = WarehouseItem.Empty;
                        ObjectMap[position] = WarehouseItem.Empty;
                    }
                    else
                    {
                        // First half will never 'push' anything
                        Move(newPositionBoxPt2, direction);

                        ObjectMap[newPositionBoxPt2] = boxPt2Type;
                        ObjectMap[newPositionBoxPt1] = boxPt1Type;
                        ObjectMap[position] = WarehouseItem.Empty;
                    }

                    break;
                case WarehouseItem.Wall:
                    throw new Exception("Tried to actually move a wall");
            }
        }

        private bool CanPush(Vector2Int position, Vector2Int direction)
        {
            var newPosition = position + direction;
            bool vertical = direction.x != 0;
            switch (ObjectMap[newPosition])
            {
                case WarehouseItem.Empty:
                    return true;
                case WarehouseItem.BoxLeft:
                case WarehouseItem.BoxRight:
                    if (vertical)
                    {
                        // Check if we can push both halves of the box
                        return CanPush(newPosition, direction) && CanPush(GetOtherBoxHalfPosition(newPosition), direction);
                    }
                    else
                    {
                        // Pushing horizontally - just propagate one further 
                        return CanPush(newPosition + direction, direction);
                    }
                case WarehouseItem.Wall:
                    return false;
                case WarehouseItem.Robot:
                    throw new Exception("Somehow pushed ourselves");
            }
            return false;
        }

        private Vector2Int GetOtherBoxHalfPosition(Vector2Int position)
        {
            if (ObjectMap[position] == WarehouseItem.BoxLeft)
            {
                return position + new Vector2Int(0, 1);
            }
            else if (ObjectMap[position] == WarehouseItem.BoxRight)
            {
                return position + new Vector2Int(0, -1);
            }
            else
            {
                throw new Exception("Not a box half");
            }
        }
    }
}