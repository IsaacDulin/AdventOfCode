using System.Collections.Generic;
using UnityEngine;

public class Day5 : MonoBehaviour
{
    public TextAsset Input;

    [ContextMenu("Run Part 1")]
    public void Run()
    {
        ParseInput(out var ruleList, out var pageUpdates);
        int outputSum = 0;
        foreach (var page in pageUpdates)
        {
            if (PageUpdateMatchesAllRules(page, ruleList))
            {
                outputSum += page.MiddlePage;
            }
        }

        Debug.Log("Output sum: " + outputSum);
    }

    private bool PageUpdateMatchesAllRules(PageUpdate pageUpdate, List<Rule> rules)
    {
        foreach (var rule in rules)
        {
            if (!rule.Matches(pageUpdate))
            {
                return false;
            }
        }

        return true;
    }

    [ContextMenu("Run Part 2")]
    public void RunPt2()
    {
        ParseInput(out var ruleList, out var pageUpdates);
        int outputSum = 0;
        foreach (var page in pageUpdates)
        {
            if (!PageUpdateMatchesAllRules(page, ruleList))
            {
                PageUpdate newPageUpdate = FixPageUpdate(page, ruleList);
                outputSum += newPageUpdate.MiddlePage;
            }
        }

        Debug.Log("Output sum: " + outputSum);
    }

    private PageUpdate FixPageUpdate(PageUpdate pageUpdate, List<Rule> ruleList)
    {
        do
        {
            foreach (var rule in ruleList)
            {
                if (!rule.Matches(pageUpdate))
                {
                    pageUpdate.SwapBasedOnRule(rule);
                }
            }
        } while (!PageUpdateMatchesAllRules(pageUpdate, ruleList));

        return pageUpdate;
    }

    private void ParseInput(out List<Rule> ruleList, out List<PageUpdate> pageUpdates)
    {
        ruleList = new List<Rule>();
        var lines = Input.text.Split('\n');
        int idx = 0;
        while (!string.IsNullOrEmpty(lines[idx].Trim()))
        {
            Debug.Log("Adding rule: " + lines[idx]);
            ruleList.Add(new Rule(lines[idx]));
            idx++;
        }

        // Skip the blank line
        idx += 1;

        // Now parse the page updates
        pageUpdates = new List<PageUpdate>();
        for (int i = idx; i < lines.Length; i++)
        {
            Debug.Log("Adding page update: " + lines[i]);
            pageUpdates.Add(new PageUpdate(lines[i]));
        }
    }

    public class Rule
    {
        public int BeforePage;
        public int AfterPage;

        public Rule(string rule)
        {
            var parts = rule.Split("|");
            BeforePage = int.Parse(parts[0]);
            AfterPage = int.Parse(parts[1]);
        }

        public bool Matches(PageUpdate page)
        {
            if (page.PageNumbers.Contains(BeforePage) && page.PageNumbers.Contains(AfterPage))
            {
                if (page.PageNumbers.IndexOf(BeforePage) >= page.PageNumbers.IndexOf(AfterPage))
                {
                    return false;
                }
            }
            return true;
        }
    }

    public class PageUpdate
    {
        public List<int> PageNumbers = new List<int>();

        public PageUpdate(string update)
        {
            var parts = update.Split(",");
            foreach (var part in parts)
            {
                PageNumbers.Add(int.Parse(part));
            }
        }

        public void SwapBasedOnRule(Rule rule)
        {
            var beforeIdx = PageNumbers.IndexOf(rule.BeforePage);
            var afterIdx = PageNumbers.IndexOf(rule.AfterPage);
            var temp = PageNumbers[beforeIdx];
            PageNumbers[beforeIdx] = PageNumbers[afterIdx];
            PageNumbers[afterIdx] = temp;
        }

        public int MiddlePage => PageNumbers[PageNumbers.Count / 2];
    }
}
