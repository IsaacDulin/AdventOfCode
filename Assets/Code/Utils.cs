using UnityEngine;
using System;
using System.Collections.Generic;

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


public class PriorityQueue<T>
{
    private readonly List<(T Item, int Priority)> _heap;
    private readonly IComparer<int> _comparer;

    public PriorityQueue(IComparer<int> comparer = null)
    {
        _heap = new List<(T, int)>();
        _comparer = comparer ?? Comparer<int>.Default;
    }

    public int Count => _heap.Count;

    public void Enqueue(T item, int priority)
    {
        _heap.Add((item, priority));
        HeapifyUp(_heap.Count - 1);
    }

    public T Dequeue()
    {
        if (Count == 0)
        {
            throw new InvalidOperationException("The priority queue is empty.");
        }

        T item = _heap[0].Item;
        _heap[0] = _heap[^1];
        _heap.RemoveAt(_heap.Count - 1);

        if (_heap.Count > 0)
        {
            HeapifyDown(0);
        }

        return item;
    }

    public T Peek()
    {
        if (Count == 0)
        {
            throw new InvalidOperationException("The priority queue is empty.");
        }

        return _heap[0].Item;
    }

    private void HeapifyUp(int index)
    {
        while (index > 0)
        {
            int parentIndex = (index - 1) / 2;
            if (_comparer.Compare(_heap[index].Priority, _heap[parentIndex].Priority) >= 0)
            {
                break;
            }

            Swap(index, parentIndex);
            index = parentIndex;
        }
    }

    private void HeapifyDown(int index)
    {
        int lastIndex = _heap.Count - 1;

        while (true)
        {
            int leftChildIndex = 2 * index + 1;
            int rightChildIndex = 2 * index + 2;
            int smallestIndex = index;

            if (leftChildIndex <= lastIndex &&
                _comparer.Compare(_heap[leftChildIndex].Priority, _heap[smallestIndex].Priority) < 0)
            {
                smallestIndex = leftChildIndex;
            }

            if (rightChildIndex <= lastIndex &&
                _comparer.Compare(_heap[rightChildIndex].Priority, _heap[smallestIndex].Priority) < 0)
            {
                smallestIndex = rightChildIndex;
            }

            if (smallestIndex == index)
            {
                break;
            }

            Swap(index, smallestIndex);
            index = smallestIndex;
        }
    }

    private void Swap(int index1, int index2)
    {
        (T, int) temp = _heap[index1];
        _heap[index1] = _heap[index2];
        _heap[index2] = temp;
    }
}

