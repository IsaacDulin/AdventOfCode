using UnityEngine;

public static class Utils
{
    public static void PrintCharArray(char[][] array)
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
}
