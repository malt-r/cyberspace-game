using Graphs;
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
        private readonly List<Vector3Int> _occupiedCells;

        public GameObject GameObject { get; private set; }

        public Room(GameObject go)
        {
            GameObject = go;
            _occupiedCells = new List<Vector3Int>();
        }

        public Room(GameObject go, List<Vector3Int> occupiedCells)
        {
            GameObject = go;
            _occupiedCells = occupiedCells;
        }

        public List<Vector3Int> Occupiedcells { get => _occupiedCells; }

        public Vector3 MeshExtents()
        {
            var meshs = GameObject.GetComponents<MeshFilter>();
            return GameObject.GetComponent<MeshFilter>().sharedMesh.bounds.extents;
        }

        public bool HasBarrier()
        {
            var marker = GameObject.GetComponentInChildren<StoryMarker>();
            return null != marker;
        }

        public StoryMarker[] GetStoryMarkers()
        {
            return GameObject.GetComponentsInChildren<StoryMarker>();
        }

        public DoorMarker[] GetDoorMarkers()
        {
            return GameObject.GetComponentsInChildren<DoorMarker>();
        }
    }

    [Header("Grid specification")]

    [Tooltip("Dimensions of Grid in cells")]
    [SerializeField]
    Vector3Int GridDimensions = new Vector3Int(10, 1, 10);

    [Tooltip("Size of the sides of one cell")]
    [SerializeField]
    int CellSize = 4;

    [Tooltip("Number of cells to leave as bufferzone around grid")]
    [SerializeField]
    int OuterBufferZone = 2;

    [Tooltip("Number of cells to leave as bufferzone around every room")]
    [SerializeField]
    int RoomBufferZone = 2;

    [Tooltip("Show gizmos for grid?")]
    [SerializeField]
    bool ShowGrid = true;


    // temporary parameter for testing
    //[SerializeField]
    Vector2Int PlacementCell;

    // temporary parameter for testing
    //[SerializeField]
    int RoomIdx;

    [Space(20)]

    [Header("Room placement")]

    // Collider, which is used to find room-templates contained in it
    [Tooltip("Boxcollider; Is used to scan for rooms (must be contained in bounds of collider)")]
    [SerializeField]
    BoxCollider RoomFinder;

    [Tooltip("Maximum of tries for one room to be placed")]
    [SerializeField]
    int MaxPlacementTriesPerRoom = 10;

    [Tooltip("Fixed seed to use for random room placement")]
    [SerializeField]
    int RandomSeed;

    [Tooltip("Use externally generated seed for room placement")]
    [SerializeField]
    bool UseExternalEntropy;

    [Header("Graphs")]
    [SerializeField]
    bool ShowDelauney = true;

    // grid coordinates will start at local (0,0,0) and extend in positive x and
    // y coordinates
    DungeonGrid<Cell> _grid;

    // the read-in rooms of the roomfinder
    List<Room> _roomTemplates;

    List<int> _barrierRooms;

    // the instantiated gameobjects, which should be removed on cleanup
    //List<GameObject> _instantiatedRooms = new List<GameObject>();
    List<Room> _instantiatedRooms;

    Delaunay2D _delaunay;

    // TODO: find better place for this
    HashSet<Vector2Int> _triedCells = new HashSet<Vector2Int>();

    public enum NeighborDirection
    {
        leftbelow = 0,
        left = 1,
        leftabove = 2,
        above = 3,
        below = 4,
        rightabove = 5,
        right = 6,
        rightbelow = 7
    };

    Vector3Int[] _gridDirections = new Vector3Int[]
    {
        new Vector3Int(-1, 0, -1), // left below
        new Vector3Int(-1, 0,  0), // left
        new Vector3Int(-1, 0,  1), // left above
        new Vector3Int( 0, 0,  1), // above
        new Vector3Int( 0, 0, -1), // below
        new Vector3Int( 1, 0,  1), // right above
        new Vector3Int( 1, 0,  0), // right
        new Vector3Int( 1, 0, -1)  // right below
    };

    private Vector3Int GetNeighborDir(NeighborDirection dir)
    {
        return _gridDirections[((int)dir)];
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private long GetExternalSeed()
    {
        var now = System.DateTime.Now;
        var notNow = now.AddSeconds((double)Random.Range(0.0f, 100.0f));
        return now.ToBinary() ^ notNow.ToBinary();
    }

    public void Triangulate()
    {
        List<Vertex> vertices = new List<Vertex>();

        foreach (var room in _instantiatedRooms)
        {
            var mesh = room.GameObject.GetComponent<MeshFilter>().sharedMesh;
            var boundsPos = room.GameObject.transform.position + mesh.bounds.center;
            var position = new Vector2(boundsPos.x, boundsPos.z);
            vertices.Add(new Vertex<Room>(position, room));
        }

        _delaunay = Delaunay2D.Triangulate(vertices);
    }

    public void Setup()
    {
        Cleanup();
        CreateGrid();
        ReadRooms();
    }

    public void CreateGrid()
    {
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

        _triedCells = new HashSet<Vector2Int>();
    }

    public void ReadRooms()
    {
        _instantiatedRooms = new List<Room>();
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
            _roomTemplates = new List<Room>(colliders.Length - 1);

            foreach (var collider in colliders)
            {
                var roomMarker = collider.gameObject.GetComponent<RoomMarker>();
                if (null != roomMarker && RoomFinder.gameObject != collider.gameObject) // will find the finder collider
                {
                    Debug.LogWarning("Found room: " + collider.gameObject.name + "; idx: " + _roomTemplates.Count);
                    _roomTemplates.Add(new Room(collider.gameObject));
                }
            }

            Debug.LogWarning("Found " + _roomTemplates.Count.ToString() + " rooms in roomfinder");
        }
    }

    private void ReadInBarrierRooms()
    {
        Room room;
        for (int i = 0; i < _instantiatedRooms.Count; i++)
        {
            room = _instantiatedRooms[i];
            if (room.HasBarrier())
            {
                _barrierRooms.Add(i);
            }
        }
    }

    public void PlaceRoomWithSelectedIdx ()
    {
        PlaceRoom(RoomIdx, PlacementCell);
    }

    // select random cell within gird-size with border = 2 cells (to ensure,
    // that doors always can be connected on each side
    // ensure, that rooms are not directly placed besides each other (marked bufferzone in
    // between)
    // ensure, that room is completely placed within borders -> use buffer zone
    public void PlaceRooms()
    {
        if (UseExternalEntropy)
        {
            long seed = GetExternalSeed();
            Random.InitState((int)seed);
            Debug.Log("Using seed " + seed + " for placement");
        }
        else 
        {
            Random.InitState(RandomSeed);
        }

        int i;
        int iterationCount; // number of iterations to try for one room
        int roomsSkipped = 0;
        for (i = 0; i < _roomTemplates.Count; i++)
        {
            iterationCount = 0;
            var room = _roomTemplates[i];
            bool success = false;
            Vector2Int cell;
            do
            {
                // get random position
                int x = Mathf.FloorToInt(Random.Range(OuterBufferZone, (float)GridDimensions.x - OuterBufferZone));
                int y = Mathf.FloorToInt(Random.Range(OuterBufferZone, (float)GridDimensions.z - OuterBufferZone));

                cell = new Vector2Int(x, y);
                if (_triedCells.Contains(cell))
                {
                    Debug.LogWarning("Skipping cell x:" + x.ToString() + " y:" + y.ToString());
                }
                else
                {
                    _triedCells.Add(cell);
                    success = PlaceRoom(i, cell);
                }
                iterationCount++;
            } while (!success && _triedCells.Count < GridDimensions.x * GridDimensions.z * 0.8f && iterationCount <= MaxPlacementTriesPerRoom);
            if (iterationCount > MaxPlacementTriesPerRoom)
            {
                Debug.LogError("Could not place room: " + room.GameObject.name + ", to many tries");
                roomsSkipped++;
            }
        }
        if (i < _roomTemplates.Count || roomsSkipped > 0)
        {
            // TODO: in runtime code, this needs to retry the placement, until every
            // room is placed
            Debug.LogError(
                    "Could not place all rooms, tried cells: " + 
                    _triedCells.Count.ToString() + 
                    ", skipped rooms: " + 
                    roomsSkipped
                    );
        }

        ReadInBarrierRooms();
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
        Room instantiatedRoom = new Room(go, overlappingCells);
        _instantiatedRooms.Add(instantiatedRoom);

        // mark room-cells and store room idx
        List<Vector3Int> markedCells = new List<Vector3Int>();
        foreach (var overlappingCell in overlappingCells)
        {
            _grid[overlappingCell].type = CellType.Room;
            // TODO: I think this is really brittle and needs to be improved
            _grid[overlappingCell].roomIdx = _instantiatedRooms.Count - 1;

            _triedCells.Add(new Vector2Int(overlappingCell.x, overlappingCell.z));
            markedCells.Add(overlappingCell);
        }

        // create buffer zone
        // RoomBufferZone determines the number of passes
        // iterate progressively over the markedCells
        Vector3Int currentCell;
        int checkedCellIdx = 0;
        for (int i = 0; i < RoomBufferZone; i++)
        {
            // determine index range of current pass
            int maxIndex = markedCells.Count;
            for (; checkedCellIdx < maxIndex; checkedCellIdx++) 
            {
                currentCell = markedCells[checkedCellIdx];
                // check for each cell around, if a room, and if not, set to bufferzone
                foreach (var direction in _gridDirections)
                {
                    var neighborCell = currentCell + direction;
                    if (_grid[neighborCell].type != CellType.Room && 
                        _grid[neighborCell].type != CellType.Bufferzone)
                    {
                        _grid[neighborCell].type = CellType.Bufferzone;
                        markedCells.Add(neighborCell);
                    }
                }
            }
        }
    }


    private bool PlaceRoom(int roomIdx, Vector2Int cell) // cell: Zellenkoordinaten
    { 
        var room = _roomTemplates[roomIdx];
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
            return true;
        }
        return false;
    }

    public void Cleanup()
    {
        _roomTemplates = null;
        _grid = null;
        _triedCells = null;

        if (null != _instantiatedRooms)
        {
            foreach (var room in _instantiatedRooms)
            {
                DestroyImmediate(room.GameObject);
            }
        }
        _instantiatedRooms = new List<Room>();
        _delaunay = null;
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

        if (null != _delaunay && ShowDelauney)
        {
            foreach (var edge in _delaunay.Edges)
            {
                if (!edge.IsBad)
                {
                    var p1 = edge.U.Position;
                    var p2 = edge.V.Position;
                    Debug.DrawLine(
                        new Vector3(p1.x, 0, p1.y), 
                        new Vector3(p2.x, 0, p2.y),
                        Color.red
                        );
                }
            }
        }
    }
}
