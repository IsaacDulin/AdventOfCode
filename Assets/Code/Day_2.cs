using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Day2 : MonoBehaviour
{
    public TextAsset Input;

    [ContextMenu("Run Part 1")]
    public void Run()
    {
        var reports = ParseInput();
        int count = reports.Count(report => report.LevelIsSafe());
        Debug.Log($"{count} reports are safe.");
    }

    [ContextMenu("Run Part 2")]
    public void RunPt2()
    {
        var reports = ParseInput();
        int count = reports.Count(report => report.LevelIsSafeWithDampener());
        Debug.Log($"{count} reports are safe with the dampener on.");
    }

    private List<Report> ParseInput()
    {
        List<Report> reports = new List<Report>();
        var lines = Input.text.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            reports.Add(new Report(lines[i]));
        }
        return reports;
    }

    public class Report
    {
        const int MAX_DELTA = 3;
        const int MIN_DELTA = 1;

        List<int> Levels;

        public Report(string line)
        {
            Levels = new List<int>();
            var splitLine = line.Split(" ");
            foreach (var level in splitLine)
            {
                Levels.Add(int.Parse(level));
            }
        }

        public Report(List<int> levels)
        {
            Levels = levels;
        }

        public List<Report> GetProblemDampenedReports()
        {
            List<Report> reports = new List<Report>();
            for (int i = 0; i < Levels.Count; i++)
            {
                List<int> newLevels = new List<int>(Levels);
                newLevels.RemoveAt(i);
                reports.Add(new Report(newLevels));
            }
            return reports;
        }

        public bool LevelIsSafeWithDampener()
        {
            if (LevelIsSafe())
            {
                return true;
            }

            var dampenedReports = GetProblemDampenedReports();
            foreach (var report in dampenedReports)
            {
                if (report.LevelIsSafe())
                {
                    return true;
                }
            }

            return false;
        }

        public bool LevelIsSafe()
        {
            return LevelsAreAscendingSlowly() || LevelsAreDescendingSlowly();
        }

        public bool LevelsAreAscendingSlowly()
        {
            for (int i = 0; i < Levels.Count - 1; i++)
            {
                int delta = Levels[i + 1] - Levels[i];
                if ((delta < MIN_DELTA) || (delta > MAX_DELTA))
                {
                    return false;
                }
            }
            return true;
        }

        public bool LevelsAreDescendingSlowly()
        {
            for (int i = 0; i < Levels.Count - 1; i++)
            {
                int delta = Levels[i] - Levels[i + 1];
                if ((delta < MIN_DELTA) || (delta > MAX_DELTA))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
