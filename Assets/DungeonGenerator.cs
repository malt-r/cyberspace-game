using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    // coord-systems:
    // - cell-coordinates (without consideration of cellsize)
    // - grid-coordinates (with consideration of cellsize)
    // - room-coordinates (used for scanning the geometry of a room, local to room itself)
    public struct Cell
    {
        public CellType type;
        public int roomIdx;
    }

    public enum CellType
    {
        Free,
        Room,
        Hallway,
        Bufferzone // adjacent to room or on border of grid
    };

    public class Room
    {
        public GameObject GameObject { get; private set; }

        public Room(GameObject go)
        {
            GameObject = go;
        }

        public Vector3 MeshExtents()
        {
            return GameObject.GetComponent<MeshFilter>().sharedMesh.bounds.extents;
        }
    }

    // Collider, which is used to find room-templates contained in it
    [SerializeField]
    BoxCollider RoomFinder;

    [SerializeField]
    Vector3Int GridDimensions = new Vector3Int(10, 1, 10);

    [SerializeField]
    int CellSize = 4;

    [SerializeField]
    int OuterBufferZone = 2;

    [SerializeField]
    bool ShowGrid = true;

    // grid coordinates will start at local (0,0,0) and extend in positive x and
    // y coordinates
    DungeonGrid<Cell> _grid;

    // the read-in rooms of the roomfinder
    List<Room> _rooms;

    // the instantiated gameobjects, which should be removed on cleanup
    List<GameObject> _instantiatedRooms = new List<GameObject>();

    // temporary parameter for testing
    [SerializeField]
    Vector2Int PlacementCell;

    // temporary parameter for testing
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
        Random.InitState(42);
        _grid = new DungeonGrid<Cell>(GridDimensions, Vector3Int.zero);

        for (int x = 0; x < _grid.Size.x; x++)
        {
            if (x < OuterBufferZone)
            {
                for (int z = 0; z < _grid.Size.z; z++)
                {
                    _grid[x, 0, z].type = CellType.Bufferzone;
                    _grid[_grid.Size.x - x - 1, 0, _grid.Size.z - z - 1].type = CellType.Bufferzone;
                }
            }
            else
            {
                for (int z = 0; z < OuterBufferZone; z++)
                {
                    _grid[x, 0, z].type = CellType.Bufferzone;
                    _grid[_grid.Size.x - x - 1, 0, _grid.Size.z - z - 1].type = CellType.Bufferzone;
                }
            }
        }
    }

    public void ReadRooms()
    {
        if (null == RoomFinder)
        {
            Debug.LogError("RoomFinder of DungeonGenerator is null, cannot find rooms");
        }
        else 
        {
            var colliders = 
                Physics.OverlapBox
                (
                    RoomFinder.bounds.center,
                    RoomFinder.bounds.extents / 0.5f
                    );
            _rooms = new List<Room>(colliders.Length - 1);

            foreach (var collider in colliders)
            {
                if (this.RoomFinder.gameObject != collider.gameObject) // will find the finder collider
                {
                    Debug.LogWarning("Found room: " + collider.gameObject.name + "; idx: " + _rooms.Count);
                    _rooms.Add(new Room(collider.gameObject));
                }
            }

            Debug.LogWarning("Found " + _rooms.Count.ToString() + " rooms in roomfinder");
        }
    }

    public void PlaceRoomWithSelectedIdx ()
    {
        PlaceRoom(RoomIdx, PlacementCell);
    }

    // select random cell within gird-size with border = 2 cells (to ensure,
    // that doors always can be connected on each side
    // ensure, that rooms are not directly placed besides each other (one cell in
    // between)
    // ensure, that room is completely placed within borders -> use mesh-extents
    public void PlaceRooms()
    {
        foreach (var room in _rooms)
        {
            // get random position
            int x = Mathf.FloorToInt(Random.Range(2.0f, (float)GridDimensions.x) - 2);
            int y = Mathf.FloorToInt(Random.Range(2.0f, (float)GridDimensions.z) - 2);
        }
    }


    // foreach cell which intersects with the actual extents offset by 
    // the location (and rotation, tbd), check, if the mesh overlaps with the
    // middle-coordinate of the cell -> this is not an exact measurement,
    // but if all rooms adhere to the gridsize, this should match up
    //
    // The cells in the returned list will be laid out in z-columns in ascending order.
    List<Vector3Int> GetOccupiedCells
        (
        Room room, 
        Vector2 placementCell,
        Vector3 rotation
        ) 
    {
        List<Vector3Int> overlappingCells = new List<Vector3Int>();

        // get mesh extents
        var actualExtents = room.MeshExtents() * 2;

        var roomObjectPosition = room.GameObject.transform.position; // global coord
        var placementLoc3 = new Vector3(placementCell.x, 0, placementCell.y);

        const int rayStartHeight = 50;
        const int rayDrawHeight = 1 - rayStartHeight;
        // calculate the cell occupation on original mesh location
        // and translate each position to a cell location in the actual grid
        // THIS CURRENTLY ABSOLUTELY REQUIRES NO ROTATION TO BE APPLIED TO THE 
        // ORIGINAL PREFAB/OBJECT MESH!!
        for (int x = 0; x < actualExtents.x / CellSize; x++)
        {
            for (int z = 0; z < actualExtents.z / CellSize; z++)
            {
                // global coord
                var locToCheck = 
                    roomObjectPosition + 
                    new Vector3(
                        x * CellSize + CellSize / 2,
                        rayStartHeight,
                        z * CellSize + CellSize / 2
                        );
                RaycastHit[] hits = Physics.RaycastAll(locToCheck, Vector3.down, Mathf.Infinity);

                Debug.DrawRay(locToCheck + new Vector3(0, rayDrawHeight, 0), Vector3.down, Color.red, 1.0f);

                foreach (var hit in hits)
                {
                    // TODO: should consider offset
                    if (hit.collider.gameObject == room.GameObject)
                    {
                        var gridCell = placementLoc3 + new Vector3(x, 0, z);
                        overlappingCells.Add(Vector3Int.FloorToInt(gridCell));
                        break;
                    }
                }
            }
        }
        return overlappingCells;
    }


    //private bool IsPlacementValid(Room room, Vector2 location, List<Vector3Int> cellCoords)
    //{
    //    var extents = room.MeshExtents();

    //    // do room extents lay in grid-boundaries?
    //    if (extents.x > )

    //    return true;
    //}

    // returns false, if one of the passed cells is already occupied
    private bool AreCellsFree(List<Vector3Int> cellCoords)
    {
        foreach (var cellCoord in cellCoords)
        {
            var cellType = _grid[cellCoord].type; 

            if (cellType != CellType.Free)
            {
                Debug.LogError("Sorry, can't place room in cell " + cellCoord.ToString());
                return false;
            }
        }
        return true;
    }

    // instantiate room gameobject at location and mark the overlapping cells 
    // as occupied
    private void InstantiateRoom(Room room, Vector2 cell, List<Vector3Int> overlappingCells)
    {
        // location: grid-coords
        var location = cell * CellSize; // TODO: this should be encapsulated in method and take into account the offset

        var go = Instantiate(room.GameObject, new Vector3(location.x, 0, location.y), this.transform.rotation, this.transform);
        _instantiatedRooms.Add(go);

        foreach (var overlappingCell in overlappingCells)
        {
            _grid[overlappingCell].type = CellType.Room;
        }

        // TODO: designate cells around as bufferzone
        Vector3Int previousCell = overlappingCells[0];
        Vector3Int currentCell;

        Vector3Int vecLeftBelow =   new Vector3Int(-1, 0, -1);
        Vector3Int vecLeft =        new Vector3Int(-1, 0,  0);
        Vector3Int vecLeftAbove =   new Vector3Int(-1, 0,  1);
        Vector3Int vecAbove =       new Vector3Int( 0, 0,  1);
        Vector3Int vecBelow =       new Vector3Int( 0, 0, -1);
        Vector3Int vecRightAbove =  new Vector3Int( 1, 0,  1);
        Vector3Int vecRight =       new Vector3Int( 1, 0,  0);
        Vector3Int vecRightBelow =  new Vector3Int( 1, 0, -1);
        //bool firstRow = true;
        //bool switchedRow = false;
        //int lastColMaxZ = 0;
        //int lastColMinZ = 0;

        for (int i = 0; i < overlappingCells.Count; i++)
        {
            currentCell = overlappingCells[i];
            // check for each cell around, if a room, and if not, set to bufferzone
            if (_grid[currentCell + vecLeftBelow].type != CellType.Room) _grid[currentCell + vecLeftBelow].type = CellType.Bufferzone;
            if (_grid[currentCell + vecLeft].type != CellType.Room) _grid[currentCell + vecLeft].type = CellType.Bufferzone;
            if (_grid[currentCell + vecLeftAbove].type != CellType.Room) _grid[currentCell + vecLeftAbove].type = CellType.Bufferzone;
            if (_grid[currentCell + vecAbove].type != CellType.Room) _grid[currentCell + vecAbove].type = CellType.Bufferzone;
            if (_grid[currentCell + vecBelow].type != CellType.Room) _grid[currentCell + vecBelow].type = CellType.Bufferzone;
            if (_grid[currentCell + vecRightAbove].type != CellType.Room) _grid[currentCell + vecRightAbove].type = CellType.Bufferzone;
            if (_grid[currentCell + vecRight].type != CellType.Room) _grid[currentCell + vecRight].type = CellType.Bufferzone;
            if (_grid[currentCell + vecRightBelow].type != CellType.Room) _grid[currentCell + vecRightBelow].type = CellType.Bufferzone;
        }

        // lesson of today: don't try to be smart...
        //for (int i = 0; i < overlappingCells.Count; i++)
        //{
        //    currentCell = overlappingCells[i];

        //    if (currentCell.x > previousCell.x) 
        //    {
        //        // switched row, mark one above previous row and one below current
        //        _grid[previousCell + new Vector3Int(0, 0, 1)].type = CellType.Bufferzone;
        //        _grid[currentCell - new Vector3Int(0, 0, 1)].type = CellType.Bufferzone;

        //        switchedRow = true;
        //        lastColMaxZ = previousCell.z;
        //        lastColMinZ = currentCell.z;
        //    }
        //    if (firstRow)
        //    {
        //        if (0 == i) // first cell
        //        {
        //            // mark one below and left 
        //            _grid[currentCell - new Vector3Int(1, 0, 1)].type = CellType.Bufferzone;
        //            // mark one below
        //            _grid[currentCell - new Vector3Int(0, 0, 1)].type = CellType.Bufferzone;
        //        }
        //        if (switchedRow)
        //        {
        //            // one below and right
        //            _grid[previousCell - new Vector3Int(1, 0, -1)].type = CellType.Bufferzone;
        //            firstRow = false;
        //        } 
        //        else
        //        {
        //            // mark one left
        //            _grid[currentCell - new Vector3Int(1, 0, 0)].type = CellType.Bufferzone;
        //        }
        //    }

        //    if (currentCell.z > lastColMaxZ || currentCell.z < lastColMinZ)
        //    {

        //    }

        //    switchedRow = false;
        //    previousCell = currentCell;

        //    if (i == overlappingCells.Count - 1) // last cell
        //    {
        //        Debug.LogWarning("LAST CELL");
        //        // mark one above
        //        _grid[currentCell + new Vector3Int(0, 0, 1)].type = CellType.Bufferzone;

        //        // mark one above and right
        //        _grid[currentCell + new Vector3Int(1, 0, 1)].type = CellType.Bufferzone;

        //        // walk backwards through last column
        //        int lastColumnX = currentCell.x;
        //        int j;
        //        for (j = i; lastColumnX == currentCell.x; j--)
        //        {
        //            Debug.LogWarning("j" + j.ToString());
        //            // mark one right
        //            _grid[currentCell + new Vector3Int(1, 0, 0)].type = CellType.Bufferzone;
        //            currentCell = overlappingCells[j];
        //        }

        //        currentCell = overlappingCells[j+2];
        //        // mark one below and right
        //        _grid[currentCell + new Vector3Int(1, 0, -1)].type = CellType.Bufferzone;
        //        Debug.LogWarning("LEAVING");
        //    }
        //}
    }


    private void PlaceRoom(int roomIdx, Vector2Int cell) // cell: Zellenkoordinaten
    { 
        var room = _rooms[roomIdx];
        var occupiedCells = GetOccupiedCells(room, cell, Vector3.zero);

        if (AreCellsFree(occupiedCells))
        {
            Debug.LogWarning(
                string.Format(
                    "Placing Room: {0} at location {1} in cell {2}",
                    room.GameObject.name,
                    cell * CellSize,
                    cell)
                );
            InstantiateRoom(room, cell, occupiedCells);
        }
    }

    public void Cleanup()
    {
        _rooms = null;
        _grid = null;
        foreach (var room in _instantiatedRooms)
        {
            DestroyImmediate(room);
        }
    }

    private void DrawCellMarking(Cell cell, Vector3 cellLocation)
    {
        var lowerCorner = cellLocation * CellSize;
        var upperCorner = (cellLocation + new Vector3(1, 0, 1)) * CellSize;
        if (CellType.Room == cell.type)
        {
            Debug.DrawLine(lowerCorner, upperCorner, Color.red);
        } 
        else if (CellType.Bufferzone == cell.type)
        {
            Debug.DrawLine(lowerCorner, upperCorner, Color.blue);
        }
    }

    private void OnDrawGizmos()
    {
        // draw grid
        if (null != _grid && ShowGrid)
        {
            // draw grid
            if (true)
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

            // draw occupied room cells
            for (int x = 0; x < _grid.Size.x; x++)
            {
                for (int z = 0; z < _grid.Size.z; z++)
                {
                    var cellLocation = new Vector3(x, 0, z);
                    var cell = _grid[Vector3Int.FloorToInt(cellLocation)];
                    DrawCellMarking(cell, cellLocation);
                }
            }
        }
    }
}
