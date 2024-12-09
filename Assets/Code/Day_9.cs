using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Day9 : MonoBehaviour
{
    public TextAsset Input;

    [ContextMenu("Run")]
    public void Run()
    {
        var disk = new DataDisk(Input.text);
        disk.Compress();
        Debug.Log("Checksum: " + disk.Checksum());
    }


    [ContextMenu("RunPt2")]
    public void RunPt2()
    {
        var disk = new DataDisk(Input.text);
        disk.CompressWithoutFragmenting();
        Debug.Log("Disk: " + disk.ToString());
        Debug.Log("Checksum: " + disk.Checksum());
    }

    public class DataDisk
    {
        List<int> Data = new List<int>();

        int FileCt = 0;

        public DataDisk(string input)
        {
            char[] charArray = input.Trim().ToCharArray();
            bool emptySpace = false;
            FileCt = charArray.Length / 2;
            for (int i = 0; i < charArray.Length; i++)
            {
                int blockSize = int.Parse(charArray[i].ToString());
                int blockId = i / 2;

                if (emptySpace)
                {
                    Data.AddRange(Enumerable.Repeat(-1, blockSize));
                }
                else
                {
                    Data.AddRange(Enumerable.Repeat(blockId, blockSize));
                }

                // Alternate
                emptySpace = !emptySpace;
            }
        }

        public void Compress()
        {
            int sz = Data.Count;
            for (int i = sz - 1; i >= 0; i--)
            {
                int firstOpenIdx = Data.IndexOf(-1);
                if (firstOpenIdx > i)
                {
                    //Done
                    return;
                }

                if (Data[i] != -1)
                {
                    Data[firstOpenIdx] = Data[i];
                    Data[i] = -1;
                }
            }
        }

        public void CompressWithoutFragmenting()
        {
            for (int i = FileCt; i >= 0; i--)
            {
                int startIdx = Data.FindIndex(a => a == i);
                int endIdx = Data.FindLastIndex(a => a == i);

                int size = endIdx - startIdx + 1;
                int openSpaceIdx = FindContiguousOpenSpace(size, startIdx);
                if (openSpaceIdx != -1)
                {
                    for (int j = 0; j < size; j++)
                    {
                        Data[openSpaceIdx + j] = Data[startIdx + j];
                        Data[startIdx + j] = -1;
                    }
                }
            }
        }

        private int FindContiguousOpenSpace(int minSize, int maxIdx)
        {
            int openSpaceIdx = -1;

            int idx = 0;
            while (idx < maxIdx)
            {
                if (Data[idx] == -1)
                {
                    int openSpaceSize = 0;
                    int openSpaceStart = idx;
                    while (idx < maxIdx && Data[idx] == -1)
                    {
                        openSpaceSize++;
                        idx++;
                    }

                    if (openSpaceSize >= minSize)
                    {
                        openSpaceIdx = openSpaceStart;
                        break;
                    }
                }
                idx++;
            }
            return openSpaceIdx;
        }

        public Int64 Checksum()
        {
            Int64 sum = 0;
            for (int i = 0; i < Data.Count; i++)
            {
                if (Data[i] != -1)
                {
                    sum += i * Data[i];
                }
            }
            return sum;
        }

        public override string ToString()
        {
            string data = "";
            for (int i = 0; i < Data.Count; i++)
            {
                data += Data[i].ToString();
            }
            return data;
        }
    }
}
