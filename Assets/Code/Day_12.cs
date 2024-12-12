using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Day12 : MonoBehaviour
{
    public TextAsset Input;

    [ContextMenu("Run Pt 1")]
    public void Run()
    {
        GardenMap map = new GardenMap(Input.text);
        int price = map.FindPrices();
        Debug.Log("Total price: " + price);
    }

    [ContextMenu("Run Pt 2")]
    public void RunPt2()
    {
        GardenMap map = new GardenMap(Input.text);
        int price = map.FindBulkPrices();
        Debug.Log("Total price: " + price);
    }

    public class GardenMap
    {
        char[][] Map;

        int Height => Map.Length;
        int Width => Map[0].Length;

        private List<HashSet<Vector2Int>> _plots = new List<HashSet<Vector2Int>>();

        public GardenMap(string input)
        {
            Map = input.Split('\n').Select(x => x.Trim().ToCharArray()).ToArray();
            OrganizeByPlot();
        }

        public void OrganizeByPlot()
        {
            HashSet<Vector2Int> plotAssigned = new HashSet<Vector2Int>();
            for (int x = 0; x < Height; x++)
            {
                for (int y = 0; y < Width; y++)
                {
                    Vector2Int location = new Vector2Int(x, y);
                    if (plotAssigned.Contains(location))
                    {
                        continue;
                    }

                    // Find the rest of the plot, add it to the list, and prevent us from checking it again
                    HashSet<Vector2Int> plot = FindRestOfPlot(location);
                    _plots.Add(plot);
                    foreach (var plottedLocation in plot)
                    {
                        plotAssigned.Add(plottedLocation);
                    }
                }
            }
        }

        private HashSet<Vector2Int> FindRestOfPlot(Vector2Int location)
        {
            HashSet<Vector2Int> plot = new HashSet<Vector2Int>();
            Queue<Vector2Int> toCheck = new Queue<Vector2Int>();
            char plant = Map[location.x][location.y];
            plot.Add(location);
            toCheck.Enqueue(location);

            while (toCheck.Count > 0)
            {
                Vector2Int current = toCheck.Dequeue();
                foreach (var neighbor in GetInBoundsNeighbors(current))
                {
                    if ((Map[neighbor.x][neighbor.y] == plant) && (!plot.Contains(neighbor)))
                    {
                        plot.Add(neighbor);
                        toCheck.Enqueue(neighbor);
                    }
                }
            }

            return plot;
        }

        public int FindBulkPrices()
        {
            int price = 0;
            for (int i = 0; i < _plots.Count; i++)
            {
                price += FindPlotArea(i) * FindPlotSides(i);
            }
            return price;
        }

        public int FindPrices()
        {
            int price = 0;
            for (int i = 0; i < _plots.Count; i++)
            {
                price += FindPlotArea(i) * FindPlotPerimter(i);
            }
            return price;
        }

        public int FindPlotArea(int plotIdx)
        {
            return _plots[plotIdx].Count;
        }

        public int FindPlotSides(int plotIdx)
        {
            HashSet<Tuple<Vector2Int, Vector2Int>> edges = new HashSet<Tuple<Vector2Int, Vector2Int>>();
            foreach (var location in _plots[plotIdx])
            {
                foreach (var neighbor in GetNeighbors(location))
                {
                    if (!_plots[plotIdx].Contains(neighbor))
                    {
                        edges.Add(new Tuple<Vector2Int, Vector2Int>(location, neighbor));
                    }
                }
            }

            int sides = 0;
            while (edges.Count > 0)
            {
                Tuple<Vector2Int, Vector2Int> edge = edges.First();
                edges.Remove(edge);

                var restOfEdge = FindRestOfEdge(edges, edge);

                foreach (var edgeToRemove in restOfEdge)
                {
                    edges.Remove(edgeToRemove);
                }
                sides++;
            }
            return sides;
        }

        private HashSet<Tuple<Vector2Int, Vector2Int>> FindRestOfEdge(HashSet<Tuple<Vector2Int, Vector2Int>> edges, Tuple<Vector2Int, Vector2Int> edge)
        {
            HashSet<Tuple<Vector2Int, Vector2Int>> restOfEdge = new HashSet<Tuple<Vector2Int, Vector2Int>>();
            Queue<Tuple<Vector2Int, Vector2Int>> toCheck = new Queue<Tuple<Vector2Int, Vector2Int>>();
            restOfEdge.Add(edge);
            toCheck.Enqueue(edge);

            while (toCheck.Count > 0)
            {
                Tuple<Vector2Int, Vector2Int> current = toCheck.Dequeue();
                foreach (var neighbor in GetAdjacentEdges(current))
                {
                    if (edges.Contains(neighbor) && !restOfEdge.Contains(neighbor))
                    {
                        restOfEdge.Add(neighbor);
                        toCheck.Enqueue(neighbor);
                    }
                }
            }

            return restOfEdge;
        }

        private List<Tuple<Vector2Int, Vector2Int>> GetAdjacentEdges(Tuple<Vector2Int, Vector2Int> edge)
        {
            bool vertical = edge.Item1.x == edge.Item2.x; // edge is horizontal, so looking for a vertical side
            List<Tuple<Vector2Int, Vector2Int>> adjacentEdges = new List<Tuple<Vector2Int, Vector2Int>>();
            if (vertical)
            {
                adjacentEdges.Add(new Tuple<Vector2Int, Vector2Int>(new Vector2Int(edge.Item1.x - 1, edge.Item1.y), new Vector2Int(edge.Item2.x - 1, edge.Item2.y)));
                adjacentEdges.Add(new Tuple<Vector2Int, Vector2Int>(new Vector2Int(edge.Item1.x + 1, edge.Item1.y), new Vector2Int(edge.Item2.x + 1, edge.Item2.y)));
            }
            else
            {
                adjacentEdges.Add(new Tuple<Vector2Int, Vector2Int>(new Vector2Int(edge.Item1.x, edge.Item1.y - 1), new Vector2Int(edge.Item2.x, edge.Item2.y - 1)));
                adjacentEdges.Add(new Tuple<Vector2Int, Vector2Int>(new Vector2Int(edge.Item1.x, edge.Item1.y + 1), new Vector2Int(edge.Item2.x, edge.Item2.y + 1)));
            }
            return adjacentEdges;
        }

        public int FindPlotPerimter(int plotIdx)
        {
            int perimeterCt = 0;
            foreach (var location in _plots[plotIdx])
            {
                foreach (var neighbor in GetNeighbors(location))
                {
                    if (!_plots[plotIdx].Contains(neighbor))
                    {
                        perimeterCt++;
                    }
                }
            }
            return perimeterCt;
        }

        public List<Vector2Int> GetNeighbors(Vector2Int location)
        {
            List<Vector2Int> neighbors = new List<Vector2Int>();

            neighbors.Add(new Vector2Int(location.x - 1, location.y));
            neighbors.Add(new Vector2Int(location.x + 1, location.y));
            neighbors.Add(new Vector2Int(location.x, location.y - 1));
            neighbors.Add(new Vector2Int(location.x, location.y + 1));

            return neighbors;
        }

        public List<Vector2Int> GetInBoundsNeighbors(Vector2Int location)
        {
            List<Vector2Int> neighbors = GetNeighbors(location);
            return neighbors.Where(x => x.x >= 0 && x.x < Height && x.y >= 0 && x.y < Width).ToList();
        }
    }

}
