using System.Collections.Generic;
using UnityEngine;

public class Day4 : MonoBehaviour
{
    public TextAsset Input;

    [ContextMenu("Run Part 1")]
    public void Run()
    {
        char[][] wordSearch = ParseInput();
        Kernel kernel = new Kernel(new char[1][] {
                new char[4] { 'X', 'M', 'A', 'S' },
            }, new char[4][] {
                new char[4] { 'X', '*', '*', '*' },
                new char[4] { '*', 'M', '*', '*' },
                new char[4] { '*', '*', 'A', '*' },
                new char[4] { '*', '*', '*', 'S' },
            }
        );

        int count = CountMatches(wordSearch, kernel);
        Debug.Log("Count: " + count);
    }

    [ContextMenu("Run Part 2")]
    public void RunPt2()
    {
        char[][] wordSearch = ParseInput();
        Kernel kernel = new Kernel(new char[3][] {
                new char[3] { 'M', '*', 'M' },
                new char[3] { '*', 'A', '*' },
                new char[3] { 'S', '*', 'S' },
            }, new char[3][] { // Dang, didn't actually need the 'diagonal' version
                new char[3] { '.', '.', '.' },
                new char[3] { '.', '.', '.' },
                new char[3] { '.', '.', '.' },
            }
        );

        int count = CountMatches(wordSearch, kernel);
        Debug.Log("Count: " + count);
    }

    public int CountMatches(char[][] wordSearch, Kernel kernel)
    {
        int count = 0;
        foreach (var kernelVariant in kernel.GetKernelVariants())
        {
            count += CountMatches(wordSearch, kernelVariant);
        }
        return count;
    }

    public int CountMatches(char[][] wordSearch, char[][] kernel)
    {
        int kernelHeight = kernel.Length;
        int kernelWidth = kernel[0].Length;

        int wordSearchHeight = wordSearch.Length;
        int wordSearchWidth = wordSearch[0].Length;

        int count = 0;

        for (int i = 0; i < wordSearchHeight - kernelHeight + 1; i++)
        {
            for (int j = 0; j < wordSearchWidth - kernelWidth + 1; j++)
            {
                if (KernelMatches(wordSearch, kernel, i, j))
                {
                    count++;
                }
            }
        }
        return count;
    }

    public bool KernelMatches(char[][] wordSearch, char[][] kernel, int x, int y)
    {
        int kernelHeight = kernel.Length;
        int kernelWidth = kernel[0].Length;
        for (int i = 0; i < kernelHeight; i++)
        {
            for (int j = 0; j < kernelWidth; j++)
            {
                if (kernel[i][j] != '*' && (kernel[i][j] != wordSearch[x + i][y + j]))
                {
                    return false;
                }
            }
        }
        return true;
    }

    private void PrintCharArray(char[][] array)
    {
        string output = "";
        for (int i = 0; i < array.Length; i++)
        {
            var row = array[i];
            foreach (var character in row)
            {
                output += character;
            }
            output += "\n";
        }
        Debug.Log(output);
    }

    private char[][] ParseInput()
    {
        var lines = Input.text.Split('\n');
        var wordSearch = new char[lines.Length][];
        for (int i = 0; i < lines.Length; i++)
        {
            wordSearch[i] = lines[i].Trim().ToCharArray();
        }
        return wordSearch;
    }

    public class Kernel
    {
        public Kernel(char[][] kernel, char[][] kernelAngled)
        {
            KernelArray = kernel;
            KernelArrayAngled = kernelAngled;
        }

        public char[][] KernelArray;
        public char[][] KernelArrayAngled;

        public List<char[][]> GetKernelVariants()
        {
            List<char[][]> kernelVariants = new List<char[][]>
            {
                KernelArray,
                RotateKernel(KernelArray, 1),
                RotateKernel(KernelArray, 2),
                RotateKernel(KernelArray, 3),
                KernelArrayAngled,
                RotateKernel(KernelArrayAngled, 1),
                RotateKernel(KernelArrayAngled, 2),
                RotateKernel(KernelArrayAngled, 3)
            };
            return kernelVariants;
        }

        private char[][] RotateKernel(char[][] kernel, int times)
        {
            char[][] rotatedKernel = kernel;
            for (int i = 0; i < times; i++)
            {
                rotatedKernel = RotateKernel(rotatedKernel);
            }
            return rotatedKernel;
        }

        // Rotate 90 degrees clockwise
        private char[][] RotateKernel(char[][] kernel)
        {
            int rows = kernel.Length;
            int cols = kernel[0].Length;

            char[][] rotated = new char[cols][];
            for (int i = 0; i < cols; i++)
            {
                rotated[i] = new char[rows];
            }

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    rotated[j][rows - 1 - i] = kernel[i][j];
                }
            }

            return rotated;
        }
    }
}
