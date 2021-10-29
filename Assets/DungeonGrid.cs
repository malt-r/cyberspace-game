﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGrid<T> 
{
    T[] data;

    public Vector3Int Size { get; private set; }
    public Vector3Int Offset { get; set; }
    public int CellSize { get; private set; }


    public DungeonGrid(Vector3Int size, Vector3Int offset, int cellsize)
    {
        Size = size;
        Offset = offset;
        CellSize = cellsize;

        data = new T[size.x * size.y * size.z];
    }

    public int GetIndex(Vector3Int pos)
    {
        var scaledPos = pos / CellSize;
        return scaledPos.x + (Size.x * scaledPos.y) + (Size.x * Size.y * scaledPos.z);
    }

    public bool InBounds(Vector3Int pos)
    {
        return new BoundsInt(Vector3Int.zero, Size).Contains(pos + Offset);
    }

    public ref T this[int x, int y, int z]
    {
        get
        {
            return ref this[new Vector3Int(x, y, z)];
        }
    }

    public ref T this[Vector3Int pos]
    {
        get
        {
            pos += Offset;
            return ref data[GetIndex(pos)];
        }
    }

    public void DrawGrid()
    {
        // draw grid
        if (true)
        {
            for (int y = 0; y < Size.y; y++)
            {
                for (int z = 0; z <= Size.z; z++)
                {
                    Debug.DrawLine(new Vector3(0, y, z) * CellSize, new Vector3(Size.x, y, z) * CellSize, Color.green);
                }

                for (int x = 0; x <= Size.x; x++)
                {
                    Debug.DrawLine(new Vector3(x, y, 0) * CellSize, new Vector3(x, y, Size.z) * CellSize, Color.green);
                }
            }
        }
    }
}
