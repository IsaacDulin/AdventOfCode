using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Day21 : MonoBehaviour
{
    public TextAsset Input;

    [ContextMenu("Run Pt 1")]
    public void Run()
    {
    }

    [ContextMenu("Run Pt 2")]
    public void RunPt2()
    {
    }

    public class RobotController
    {
        public Vector2Int StartPos;
        public int Height { get; private set; }
        public int Width { get; private set; }

        public RobotController(string input)
        {
        }

    }
}
