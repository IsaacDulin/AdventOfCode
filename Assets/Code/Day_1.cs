using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Day1 : MonoBehaviour
{
    public TextAsset Input;

    [ContextMenu("Run Part 1")]
    public void Run()
    {
        ParseInput(out var listOne, out var listTwo);
        listOne = listOne.OrderBy(x => x).ToArray();
        listTwo = listTwo.OrderBy(x => x).ToArray();

        int summedDiff = 0;
        for (var i = 0; i < listOne.Length; i++)
        {
            int diff = Mathf.Abs(listOne[i] - listTwo[i]);
            summedDiff += Mathf.Abs(listOne[i] - listTwo[i]);
        }

        Debug.Log("Summed Difference: " + summedDiff);
    }

    [ContextMenu("Run Part 2")]
    public void RunPt2()
    {
        ParseInput(out var listOne, out var listTwo);

        Dictionary<int, int> listTwoAppearanceCts = new Dictionary<int, int>();

        for (int i = 0; i < listTwo.Length; i++)
        {
            if (listTwoAppearanceCts.ContainsKey(listTwo[i]))
            {
                listTwoAppearanceCts[listTwo[i]]++;
            }
            else
            {
                listTwoAppearanceCts.Add(listTwo[i], 1);
            }
        }

        int similarityScore = 0;
        for (var i = 0; i < listOne.Length; i++)
        {
            if (listTwoAppearanceCts.ContainsKey(listOne[i]))
            {
                similarityScore += listOne[i] * listTwoAppearanceCts[listOne[i]];
            }
            // else + 0;
        }

        Debug.Log("Similarity Score: " + similarityScore);
    }

    private void ParseInput(out int[] listOne, out int[] listTwo)
    {
        var lines = Input.text.Split('\n');
        listOne = new int[lines.Length];
        listTwo = new int[lines.Length];

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var splitLine = line.Split("   ");

            listOne[i] = int.Parse(splitLine[0].Trim().TrimStart());
            listTwo[i] = int.Parse(splitLine[1].Trim().TrimStart());
        }
    }
}
