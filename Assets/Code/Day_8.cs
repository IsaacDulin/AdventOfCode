using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Day8 : MonoBehaviour
{
    public TextAsset Input;

    [ContextMenu("Run")]
    public void Run()
    {
        var map = new Map(Input.text);
        var labels = map.GetUniqueAtneannaLabels();
        Utils.PrintCharArray(map.MapData);
        HashSet<Vector2Int> antinodes = new HashSet<Vector2Int>();
        foreach (var label in labels)
        {
            var antennas = map.GetAntennas(label);
            Debug.Log("Antennas for " + label + ": " + antennas[0].ToString() + " " + antennas[1].ToString());
            antinodes.AddRange(map.GetAntinodes(antennas));
        }
        int ct = antinodes.Count;
        Debug.Log($"Total antinodes: {ct}");
    }

    public class Map
    {
        public char[][] MapData;

        public Map(string input)
        {
            MapData = input.Split("\n").Select(line => line.Trim().ToCharArray()).ToArray();
        }

        public List<char> GetUniqueAtneannaLabels()
        {
            var list = MapData.SelectMany(row => row).Distinct().ToList();
            list.Remove('.');
            return list;
        }

        public List<Vector2Int> GetAntinodes(List<Vector2Int> antennas)
        {
            var antinodes = new List<Vector2Int>();
            for (int i = 0; i < antennas.Count; i++)
            {
                for (int j = 0; j < antennas.Count; j++)
                {
                    if (i == j)
                    {
                        continue;
                    }
                    Vector2Int delta = antennas[i] - antennas[j];
                    Vector2Int nextNode = antennas[i] + delta;

                    // This part was converted from part 1 for part 2
                    while (nextNode.x >= 0 && nextNode.x < MapData.Length && nextNode.y >= 0 && nextNode.y < MapData[0].Length)
                    {
                        antinodes.Add(nextNode);
                        nextNode += delta;
                    }
                    nextNode = antennas[i] - delta;
                    while (nextNode.x >= 0 && nextNode.x < MapData.Length && nextNode.y >= 0 && nextNode.y < MapData[0].Length)
                    {
                        antinodes.Add(nextNode);
                        nextNode -= delta;
                    }
                    // End conversion
                }
            }

            // Filter by map bounds
            antinodes = antinodes.Distinct().ToList();
            //antinodes = antinodes.Where(antinode => antinode.x >= 0 && antinode.x < MapData.Length && antinode.y >= 0 && antinode.y < MapData[0].Length).ToList();
            return antinodes;
        }


        public List<Vector2Int> GetAntennas(char antennaLabel)
        {
            var antennas = new List<Vector2Int>();
            for (int x = 0; x < MapData.Length; x++)
            {
                for (int y = 0; y < MapData[x].Length; y++)
                {
                    if (MapData[x][y] == antennaLabel)
                    {
                        antennas.Add(new Vector2Int(x, y));
                    }
                }
            }
            return antennas;
        }
    }
}
