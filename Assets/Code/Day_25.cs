using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Day25 : MonoBehaviour
{
    public const int MAX_PIN_SIZE = 5;
    public const int NUM_PINS = 5;

    public TextAsset Input;

    [ContextMenu("Run Pt 1")]
    public void Run()
    {
        LockAssessor la = new LockAssessor(Input.text);
        int ct = la.CompareAll();
        Debug.Log("Count: " + ct);
    }

    public class LockAssessor
    {
        public List<Lock> Locks = new List<Lock>();
        public List<Key> Keys = new List<Key>();
        public LockAssessor(string input)
        {
            var lines = input.Split('\n').Select(x => x.Trim()).ToList();
            for (int i = 0; i < lines.Count; i += 8)
            {
                if (lines[i] == "#####")
                {
                    Lock lck = new Lock(lines.GetRange(i, 7));
                    Locks.Add(lck);
                }
                else if (lines[i + 6] == "#####")
                {
                    Key key = new Key(lines.GetRange(i, 7));
                    Keys.Add(key);
                }
                else
                {
                    throw new Exception("Neither lock nor key");
                }
            }
        }

        public int CompareAll()
        {
            int count = 0;
            foreach (var lck in Locks)
            {
                foreach (var key in Keys)
                {
                    if (Compare(lck, key))
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        public bool Compare(Lock lck, Key key)
        {
            for (int i = 0; i < NUM_PINS; i++)
            {
                if (lck.PinSizes[i] + key.PinSizes[i] > MAX_PIN_SIZE)
                {
                    return false;
                }
            }
            return true;
        }
    }

    public class Lock
    {
        public List<int> PinSizes = new List<int>();
        public Lock(List<string> lines)
        {
            PinSizes.AddRange(Enumerable.Repeat(0, NUM_PINS));
            for (int i = 1; i < MAX_PIN_SIZE + 1; i++)
            {
                for (int j = 0; j < NUM_PINS; j++)
                {
                    if (lines[i][j] == '#')
                    {
                        PinSizes[j] += 1;
                    }
                }
            }
        }
    }

    public class Key
    {
        public List<int> PinSizes = new List<int>();

        public Key(List<string> lines)
        {
            PinSizes.AddRange(Enumerable.Repeat(0, NUM_PINS));
            for (int i = 1; i < MAX_PIN_SIZE + 1; i++)
            {
                for (int j = 0; j < NUM_PINS; j++)
                {
                    if (lines[i][j] == '#')
                    {
                        PinSizes[j] += 1;
                    }
                }
            }
        }
    }
}