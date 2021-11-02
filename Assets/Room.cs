using UnityEngine;

public partial class DungeonGenerator
{
    public class Room
    {
        public GameObject GameObject { get; private set; }

        public Room(GameObject go)
        {
            GameObject = go;
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
            return GameObject.transform.position + mesh.bounds.center;
        }

        public bool HasBarrier()
        {
            var marker = GameObject.GetComponentInChildren<StoryMarker>();
            return null != marker;
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
