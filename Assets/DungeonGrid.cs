using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGrid<T> 
{
    T[] data;

    public Vector3Int Size { get; private set; }
    public Vector3Int Offset { get; set; }

    public DungeonGrid(Vector3Int size, Vector3Int offset)
    {
        Size = size;
        Offset = offset;

        data = new T[size.x * size.y * size.z];
    }

    public int GetIndex(Vector3Int pos)
    {
        var scaledPos = pos;/// CellSize;
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
}
