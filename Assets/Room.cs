using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class DungeonGenerator
{
    public class Room
    {
        public GameObject GameObject { get; private set; }
        public List<Vector3Int> DoorCells { get; }

        public Room(GameObject go)
        {
            GameObject = go;
            DoorCells = new List<Vector3Int>();
        }

        public void AddDoorCell(Vector3Int cell)
        {
            if (!DoorCells.Contains(cell))
            {
                DoorCells.Add(cell);
            }
        }

        public Vector3 MeshExtents()
        {
            var meshs = GameObject.GetComponents<MeshFilter>();
            if (meshs != null)
            {
                return GameObject.GetComponent<MeshFilter>().sharedMesh.bounds.extents;
            }
            return Vector3.zero;
        }

        public Vector3 GetMeshCenter()
        {
            var mesh = GameObject.GetComponent<MeshFilter>().sharedMesh;
            return GameObject.transform.position + GameObject.transform.rotation * mesh.bounds.center;
        }

        public bool HasBarrier()
        {
            var markers = GameObject.GetComponentsInChildren<StoryMarker>();
            return markers.Where(marker => marker.IsBarrier).Any();
        }

        public StoryMarker GetStoryMarker()
        {
            return GameObject.GetComponentInChildren<StoryMarker>();
        }

        public DoorMarker[] GetDoorMarkers()
        {
            return GameObject.GetComponentsInChildren<DoorMarker>();
        }
    }
}
