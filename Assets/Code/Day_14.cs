using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine;

public class Day14  : MonoBehaviour
{
    public TextAsset Input;
    
    public GameObject VisualizedRobotPrefab;

    [SerializeField]
    private List<GameObject> _visualizedRobots;

    [ContextMenu("Run Pt 1")]
    public void Run()
    {
        RobotMap map = new RobotMap(Input.text);
        map.Advance(100);
        Debug.Log(map.FindSafetyFactor());
    }

    public void Start()
    { 
        StartCoroutine(Part2Coroutine());
    }

    // Wouldn't have done pt 2 via manual inspection if I thought the first instance was at 8k lol
    public IEnumerator Part2Coroutine()
    {
        RobotMap map = new RobotMap(Input.text);
        _visualizedRobots = new List<GameObject>();
        Dictionary<int, int> safetyFactors = new Dictionary<int, int>();

        for (int i=0; i< 10000; i++)
        {
            map.Advance(1);
            UpdateVisualization(map);
            Debug.Log($"Iteration: {i}");
            if ( i >= 8148) 
            {
                yield return null;
            }
        }
    }

    private void UpdateVisualization(RobotMap map)
    {
        for (int i=0; i < map.Robots.Count; i++ )
        {
            if (_visualizedRobots.Count <= i)
            {
                var go = Instantiate(VisualizedRobotPrefab, new Vector3(map.Robots[i].Position.x, map.Robots[i].Position.y, 0), Quaternion.identity);
                _visualizedRobots.Add(go);
            }
            _visualizedRobots[i].transform.position = new Vector3(map.Robots[i].Position.x, -map.Robots[i].Position.y, 0);
        }
    }
    
    public class RobotMap
    {
        public int Height = 103;
        public int Width = 101;
        public List<Robot> Robots;


        public RobotMap(string input) 
        {
            var lines = input.Split('\n').ToList();
            Robots = new List<Robot>();
            for (int i = 0; i < lines.Count; i++)
            {
                Robots.Add(new Robot(lines[i]));
            }
        }

        public void Advance(int iterations) 
        {
            for (int i = 0; i < iterations; i++)
            {
                foreach (var robot in Robots)
                {
                    Advance(robot);
                }
            }
        }

        public void Advance(Robot robot) 
        {
            robot.Position += robot.Velocity;

            if (robot.Position.x < 0)
            {
                robot.Position.x = Width + robot.Position.x;
            }
            if (robot.Position.y < 0)
            {
                robot.Position.y = Height + robot.Position.y;
            }

            int rolledOverX = robot.Position.x % Width;
            int rolledOverY = robot.Position.y % Height;
            robot.Position = new Vector2Int(rolledOverX, rolledOverY);
        }

        public int FindSafetyFactor()
        {
            Dictionary<int, int> quadrantMap = new Dictionary<int, int>();
            foreach (var robot in Robots)
            {
                int quadrant = GetQuadrant(robot);
                if (quadrantMap.ContainsKey(quadrant))
                {
                    quadrantMap[quadrant]++;
                }
                else
                {
                    quadrantMap.Add(quadrant, 1);
                }
            }
            return quadrantMap[0] * quadrantMap[1] * quadrantMap[2] * quadrantMap[3];
        }
        
        public int GetQuadrant(Robot robot)
        {
            if (robot.Position.x < Width / 2)
            {
                if (robot.Position.y < Height / 2)
                {
                    return 0;
                }
                else if (robot.Position.y > ((Height / 2)))
                {
                    return 1;
                }
            }
            else if (robot.Position.x > (Width / 2))
            {
                if (robot.Position.y < Height / 2)
                {
                    return 2;
                }
                else if (robot.Position.y > ((Height / 2)))
                {
                    return 3;
                }
            }
            return -1;
        }

    }

    public class Robot 
    {
        public Vector2Int Position;
        public Vector2Int Velocity;
        public Robot(string inputLine)
        {
            var parts = inputLine.Split(' ').ToList();
            var pos = (parts[0].Trim().TrimStart('p','=')).Split(',');
            Position = new Vector2Int(int.Parse(pos[0]), int.Parse(pos[1]));
            var vel = (parts[1].Trim().TrimStart('v','=')).Split(',');
            Velocity = new Vector2Int(int.Parse(vel[0]), int.Parse(vel[1]));
        }

        public override string ToString() 
        {
            return $"Pos: {Position} Vel: {Velocity}";
        }
    }
}
