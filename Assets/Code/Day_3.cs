using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;

public class Day3 : MonoBehaviour
{
    public TextAsset Input;

    [ContextMenu("Run Part 1")]
    public void Run()
    {
        var input = ParseInput();

        Regex regex = new Regex(@"mul\(\d+,\d+\)");
        var matches = regex.Matches(input);

        int sum = 0;
        foreach (Match match in matches)
        {
            sum += Multiply(match.Value);
        }
        Debug.Log("Sum: " + sum);
    }

    [ContextMenu("Run Part 2")]
    public void RunPt2()
    {
        var input = ParseInput();

        Regex mulRegex = new Regex(@"mul\(\d+,\d+\)");
        Regex doRegex = new Regex(@"do\(\)");
        Regex dontRegex = new Regex(@"don\'t\(\)");

        var mulMatches = mulRegex.Matches(input);
        var doMatches = doRegex.Matches(input);
        var dontMatches = dontRegex.Matches(input);

        var allMatches = new List<Match>();
        allMatches.AddRange(mulMatches);
        allMatches.AddRange(doMatches);
        allMatches.AddRange(dontMatches);
        allMatches = allMatches.OrderBy(match => match.Index).ToList();

        int sum = 0;
        bool enabled = true;
        foreach (var match in allMatches)
        {
            if (match.Value.Contains("mul"))
            {
                if (enabled)
                {
                    sum += Multiply(match.Value);
                }
            }
            else if (match.Value.Contains("do()"))
            {
                enabled = true;
            }
            else if (match.Value.Contains("don't()"))
            {
                enabled = false;
            }
        }

        Debug.Log("Sum: " + sum);
    }

    private int Multiply(string input)
    {
        input = input.TrimStart("mul(");
        input = input.TrimEnd(")");
        var numbers = input.Split(',');
        return int.Parse(numbers[0]) * int.Parse(numbers[1]);
    }


    private string ParseInput()
    {
        return Input.text;
    }
}
