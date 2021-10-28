using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;

// TODO: Calculate cells occupied by the Room and check bounds of grid
// TODO: Mark occupied cells as such
// TODO: Create context-graph
public class DungeonGenerator : MonoBehaviour
{
    public enum CellType
    {
        Free,
        Room,
        Hallway
    };

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
    Vector3Int GridSize = new Vector3Int(10, 1, 10);

    [SerializeField]
    int CellSize = 4;

    [SerializeField]
    bool ShowGrid = true;

    // grid coordinates will start at local (0,0,0) and extend in positive x and
    // y coordinates
    DungeonGrid<CellType> _grid;

    // the read-in rooms of the roomfinder
    List<Room> _rooms;

    // the instantiated gameobjects, which should be removed on cleanup
    List<GameObject> _instantiatedRooms = new List<GameObject>();

    // temporary parameter for testing
    [SerializeField]
    Vector2Int PlacementCell;

    [SerializeField]
    int RoomIdx;

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
        _grid = new DungeonGrid<CellType>(GridSize, Vector3Int.zero, CellSize);
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
        PlaceRoom(RoomIdx, PlacementCell);
    }

    // TODO: cleanup transformations

    // foreach cell which intersects with the actual extents offset by 
    // the location (and rotation, tbd), check, if the mesh overlaps with the
    // middle-coordinate of the cell -> this is not an exact measurement,
    // but if all rooms adhere to the gridsize, this should match up
    List<Vector3Int> GetOverlappingCells(GameObject room, Vector2 placementLocation, Vector3 rotation)
    {
        List<Vector3Int> overlappingCells = new List<Vector3Int>();

        var filter = room.GetComponent<MeshFilter>();
        var mesh = filter.sharedMesh;
        var actualExtents = mesh.bounds.extents * 2;

        var roomObjectPosition = room.transform.position;
        var placementLoc3 = new Vector3(placementLocation.x, 0, placementLocation.y);
        // calculate the cell occupation on original mesh location
        // and translate each position to a cell location in the actual grid
        // THIS CURRENTLY ABSOLUTELY REQUIRES NO ROTATION TO BE APPLIED TO THE 
        // ORIGINAL PREFAB/OBJECT MESH!!
        for (int x = 0; x < actualExtents.x / CellSize; x++)
        {
            for (int z = 0; z < actualExtents.z / CellSize; z++)
            {
                // location in global coord-system
                var locToCheck = roomObjectPosition + new Vector3(x * CellSize + CellSize / 2, 50 , z * CellSize + CellSize / 2);
                RaycastHit[] hits = Physics.RaycastAll(locToCheck, Vector3.down, Mathf.Infinity);

                Debug.DrawRay(locToCheck + new Vector3(0,-49,0), Vector3.down, Color.red, 1.0f);

                foreach (var hit in hits)
                {
                    // TODO: should consider offset
                    if (hit.collider.gameObject == room)
                    {
                        var gridCell = placementLoc3 + new Vector3(x * CellSize, 0 , z * CellSize);
                        overlappingCells.Add(Vector3Int.FloorToInt(gridCell));
                        break;
                    }
                }
            }
        }
        return overlappingCells;
    }

    private bool IsPlacementValid(List<Vector3Int> cellCoords)
    {
        foreach (var cellCoord in cellCoords)
        {
            var cellType = _grid[cellCoord];

            if (cellType == CellType.Room || cellType == CellType.Hallway)
            {
                Debug.LogError("Sorry, can't place room in cell " + (cellCoord/CellSize).ToString());
                return false;
            }
        }
        return true;
    }

    private void InstantiateRoom(Room room, Vector2 location, List<Vector3Int> overlappingCells)
    {
        var go = Instantiate(room.GameObject, new Vector3(location.x, 0, location.y), this.transform.rotation, this.transform);
        _instantiatedRooms.Add(go);

        foreach (var cell in overlappingCells)
        {
            _grid[cell] = CellType.Room;
        }
    }

    private void PlaceRoom(int roomIdx, Vector2Int cell)
    { 
        var room = _rooms[roomIdx];
        var location = cell * CellSize;

        Debug.LogWarning(
            string.Format(
                "Placing Room: {0} at location {1} in cell {2}",
                room.GameObject.name,
                location,
                cell)
            );

        var cells = GetOverlappingCells(room.GameObject, location, Vector3.zero);

        if (IsPlacementValid(cells))
        {
            InstantiateRoom(room, location, cells);
        }
    }

    public void Cleanup()
    {
        _grid = null;
        foreach (var room in _instantiatedRooms)
        {
            DestroyImmediate(room);
        }
    }

    private void OnDrawGizmos()
    {
        // draw grid
        if (null != _grid && ShowGrid)
        {
            _grid.DrawGrid();

            // draw filled cells
            for (int x = 0; x < _grid.Size.x; x++)
            {
                for (int z = 0; z < _grid.Size.z; z++)
                {
                    try
                    {
                        var cellInfo = _grid[x * CellSize, 0, z * CellSize];
                        var type = cellInfo;
                        if (type == CellType.Room)
                        {
                            Debug.DrawLine(new Vector3(x, 0, z) * CellSize, new Vector3(x + 1, 0, z + 1) * CellSize, Color.red);
                        }
                    }
                    catch(Exception)
                    {
                        bool b = true;
                    }
                }
            }
        }
    }
}
