using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using NodeLookup = System.Collections.Generic.Dictionary<string, Day23.Node>;
using Cluster = System.Tuple<string, string, string>;

public class Day23 : MonoBehaviour
{
    public TextAsset Input;

    [ContextMenu("Run Pt 1")]
    public void Run()
    {
        var nodeLookup = ParseInput(Input.text);
        var clusters = Find3Clusters(nodeLookup, 't');

        foreach (var cluster in clusters)
        {
            Debug.Log(" - " + cluster.Item1 + " " + cluster.Item2 + " " + cluster.Item3);
        }
        Debug.Log("Clusters: " + clusters.Count);
    }

    [ContextMenu("Run Pt 2")]
    public void RunPt2()
    {
        var nodeLookup = ParseInput(Input.text);
        var cluster = FindLargestCluster(nodeLookup);
        Debug.Log("Largest Cluster: " + ClusterToString(cluster));
    }

    public HashSet<string> FindLargestCluster(NodeLookup lookup)
    {
        Stack<HashSet<string>> stack = new(); // stack and largestCluster are hashsets of node ids
        HashSet<string> largestCluster = new();

        HashSet<string> visitedClusters = new(); // this is a hashset of clusters (sorted lists aggregated to a string)

        foreach (var kvp in lookup)
        {
            var cluster = new HashSet<string> { kvp.Key };
            GrowCluster(lookup, cluster);
            if (!visitedClusters.Contains(cluster.ToString()))
            {
                visitedClusters.Add(ClusterToString(cluster));
                stack.Push(cluster);
            }
        }

        while (stack.Count > 0)
        {
            HashSet<string> currentCluster = stack.Pop();
            if (currentCluster.Count > largestCluster.Count)
            {
                largestCluster = currentCluster;
            }

            foreach (var node in currentCluster)
            {
                foreach (var edge in lookup[node].Edges)
                {
                    if (currentCluster.Contains(edge.Id))
                    {
                        continue;
                    }
                    else
                    {
                        // Create a new cluster starting from the unincluded node/edge pair
                        var newCluster = new HashSet<string>() { node, edge.Id };
                        GrowCluster(lookup, newCluster);
                        string clusterString = ClusterToString(newCluster);
                        if (!visitedClusters.Contains(clusterString) && newCluster.Count < largestCluster.Count)
                        {
                            visitedClusters.Add(clusterString);
                            stack.Push(newCluster);
                        }
                    }
                }
            }
        }

        return largestCluster;
    }

    public string ClusterToString(HashSet<string> cluster)
    {
        return string.Join(",", cluster.OrderBy(x => x));
    }

    public void GrowCluster(NodeLookup lookup, HashSet<string> cluster)
    {
        var potentialAdditions = new HashSet<string>();
        var sampleNode = cluster.ToList()[0];
        foreach (var node in lookup[sampleNode].Edges)
        {
            if (cluster.Contains(node.Id))
            {
                continue;
            }
            else
            {
                if (IsInCluster(lookup, cluster, node))
                {
                    cluster.Add(node.Id);
                }
            }
        }
    }

    public bool IsInCluster(NodeLookup lookup, HashSet<string> existingCluster, Node potentialAddition)
    {
        if (potentialAddition.Edges.Count < existingCluster.Count)
        {
            return false;
        }

        foreach (var node in existingCluster)
        {
            if (!potentialAddition.Edges.Contains(lookup[node]))
            {
                return false;
            }
        }
        return true;
    }

    public HashSet<Cluster> Find3Clusters(NodeLookup lookup, char lookupChar)
    {
        HashSet<Cluster> clusters = new();
        foreach (var kvp in lookup)
        {
            if (kvp.Key.StartsWith(lookupChar))
            {
                Debug.Log("Checking " + kvp.Key);
                var nodeClusters = Find3Clusters(kvp.Value, lookup);
                foreach (var cluster in nodeClusters)
                {
                    clusters.Add(cluster);
                }
            }
        }

        return clusters;
    }

    public HashSet<Cluster> Find3Clusters(Node node, NodeLookup lookup)
    {
        HashSet<Cluster> clusters = new();
        if (node.Edges.Count >= 2)
        {
            foreach (var edge in node.Edges)
            {
                foreach (var edge2 in edge.Edges)
                {
                    foreach (var edge3 in edge2.Edges)
                    {
                        Debug.Log("Comparing " + node.Id + " " + edge.Id + " " + edge2.Id + " " + edge3.Id);
                        if (edge3.Id == node.Id)
                        {
                            Debug.Log("Found a cluster containg node: " + node.Id);
                            List<string> clusterElements = new List<string>() {
                                node.Id, edge.Id, edge2.Id
                            };
                            clusterElements = clusterElements.OrderBy(x => x).ToList();
                            clusters.Add(new Cluster(clusterElements[0], clusterElements[1], clusterElements[2]));
                        }
                    }
                }
            }
        }
        return clusters;
    }

    public NodeLookup ParseInput(string input)
    {
        var lookup = new NodeLookup();
        var lines = input.Split('\n');
        foreach (var line in lines)
        {
            var comps = line.Split('-');
            var nodeOne = comps[0].Trim();
            var nodeTwo = comps[1].Trim();

            if (!lookup.ContainsKey(nodeOne))
            {
                lookup[nodeOne] = new Node { Id = nodeOne, Edges = new List<Node>() };
            }

            if (!lookup.ContainsKey(nodeTwo))
            {
                lookup[nodeTwo] = new Node { Id = nodeTwo, Edges = new List<Node>() };
            }

            lookup[nodeOne].Edges.Add(lookup[nodeTwo]);
            lookup[nodeTwo].Edges.Add(lookup[nodeOne]);
        }

        return lookup;
    }

    public class Node
    {
        public string Id;
        public List<Node> Edges;
    }
}