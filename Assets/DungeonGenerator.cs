using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Calculate cells occupied by the Room and check bounds of grid
// TODO: Mark occupied cells as such
// TODO: Create context-graph
public class DungeonGenerator : MonoBehaviour
{
    private class Room
    {
        public GameObject GameObject { get; private set; }

        public Room(GameObject go)
        {
            GameObject = go;
        }
    }

    [SerializeField]
    BoxCollider RoomFinder;

    [SerializeField]
    Vector3Int GridSize = new Vector3Int(10,1,10);

    [SerializeField]
    int CellSize = 4;

    [SerializeField]
    bool ShowGrid = true;

    // grid coordinates will start at local (0,0,0) and extend in positive x and
    // y coordinates
    DungeonGrid<int> _grid;

    // the read-in rooms of the roomfinder
    List<Room> _rooms;

    // the instantiated gameobjects, which should be removed on cleanup
    List<GameObject> _instantiatedRooms = new List<GameObject>();

    // temporary parameter for testing
    [SerializeField]
    Vector2Int PlacementCell;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void CreateGrid()
    {
        _grid = new DungeonGrid<int>(GridSize, Vector3Int.zero);
    }

    public void ReadRooms()
    {
        if (null != RoomFinder)
        {
            var colliders = Physics.OverlapBox(RoomFinder.bounds.center, RoomFinder.bounds.extents / 0.5f);
            _rooms = new List<Room>(colliders.Length - 1);

            foreach (var collider in colliders)
            {
                if (this.RoomFinder.gameObject != collider.gameObject) // will find the finder collider
                {
                    _rooms.Add(new Room(collider.gameObject));
                }
            }

            Debug.LogWarning("Found " + _rooms.Count.ToString() + " rooms in roomfinder");
        }
    }

    public void PlaceFirstRoom ()
    {
        PlaceRoom(0, PlacementCell);
    }


    private void PlaceRoom(int roomIdx, Vector2Int cell)
    {
        var room = _rooms[0];
        var location = cell * CellSize;

        Debug.LogWarning(
            string.Format(
                "Placing Room: {0} at location {1} in cell {2}",
                room.GameObject.name,
                location,
                cell)
            );

        _instantiatedRooms.Add(Instantiate(room.GameObject, new Vector3(location.x, 0, location.y), this.transform.rotation, this.transform));
    }

    public void Cleanup()
    {
        foreach (var go in _instantiatedRooms)
        {
            DestroyImmediate(go);
        }
    }

    private void OnDrawGizmos()
    {
        // draw grid
        if (null != _grid && ShowGrid)
        {
            for (int y = 0; y < _grid.Size.y; y++)
            {
                for (int z = 0; z <= _grid.Size.z; z++)
                {
                    Debug.DrawLine(new Vector3(0, y, z) * CellSize, new Vector3(_grid.Size.x, y, z) * CellSize, Color.green);
                }

                for (int x = 0; x <= _grid.Size.x; x++)
                {
                    Debug.DrawLine(new Vector3(x, y, 0) * CellSize, new Vector3(x, y, _grid.Size.z) * CellSize, Color.green);
                }
            }
        }
    }
}
