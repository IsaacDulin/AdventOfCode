using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Day10 : MonoBehaviour
{
    public TextAsset Input;

    [ContextMenu("Run Pt 1")]
    public void Run()
    {
        var map = new TopographicMap(Input.text);
        int sum = 0;
        foreach (var trailhead in map.Trailheads)
        {
            var reachablePeaks = map.Solve(trailhead);
            sum += reachablePeaks.Distinct().Count();
        }
        Debug.Log("Sum: " + sum);
    }

    [ContextMenu("Run Pt 2")]
    public void RunPt2()
    {
        var map = new TopographicMap(Input.text);
        int sum = 0;
        foreach (var trailhead in map.Trailheads)
        {
            var reachablePeaks = map.Solve(trailhead);
            sum += reachablePeaks.Count(); // NOT DISTINCT
        }
        Debug.Log("Sum: " + sum);
    }

    public class TopographicMap
    {
        int[][] MapData;
        public List<Vector2Int> Trailheads = new List<Vector2Int>();

        int MapHeight => MapData.Length;
        int MapWidth => MapData[0].Length;

        public TopographicMap(string input)
        {
            var lines = input.Split("\n");
            MapData = new int[lines.Length][];
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                Debug.Log(line);
                var chars = line.ToCharArray();
                int[] ints = chars.Select(c => int.Parse(c.ToString())).ToArray();
                MapData[i] = ints;
            }

            for (int i = 0; i < MapHeight; i++)
            {
                for (int j = 0; j < MapWidth; j++)
                {
                    if (MapData[i][j] == 0)
                    {
                        Trailheads.Add(new Vector2Int(i, j));
                    }
                }
            }
        }

        public List<Vector2Int> Solve(Vector2Int startingPos)
        {
            var reachablePeaks = new List<Vector2Int>();
            var queue = new Queue<Vector2Int>();
            queue.Enqueue(startingPos);

            while (queue.Count > 0)
            {
                Vector2Int currentPos = queue.Dequeue();
                if (MapData[currentPos.x][currentPos.y] == 9)
                {
                    reachablePeaks.Add(currentPos);
                }

                var adjacentStates = GetAdjacentOptions(currentPos);

                foreach (Vector2Int adjacentPos in adjacentStates)
                {
                    // Enqueue
                    queue.Enqueue(adjacentPos);
                }
            }

            return reachablePeaks;
        }

        private List<Vector2Int> GetAdjacentOptions(Vector2Int pos)
        {
            int elevation = MapData[pos.x][pos.y];
            var adjacentTiles = GetAdjacentTiles(pos);
            var adjacentOptions = new List<Vector2Int>();
            foreach (var tile in adjacentTiles)
            {
                if (MapData[tile.x][tile.y] == elevation + 1)
                {
                    adjacentOptions.Add(tile);
                }
            }
            return adjacentOptions;
        }

        private List<Vector2Int> GetAdjacentTiles(Vector2Int pos)
        {
            List<Vector2Int> adjacents = new List<Vector2Int>();
            if (pos.x > 0)
            {
                adjacents.Add(new Vector2Int(pos.x - 1, pos.y));
            }
            if (pos.x < MapHeight - 1)
            {
                adjacents.Add(new Vector2Int(pos.x + 1, pos.y));
            }
            if (pos.y > 0)
            {
                adjacents.Add(new Vector2Int(pos.x, pos.y - 1));
            }
            if (pos.y < MapWidth - 1)
            {
                adjacents.Add(new Vector2Int(pos.x, pos.y + 1));
            }
            return adjacents;
        }
    }
}
