using Graphs;
using System.Collections.Generic;
using UnityEngine;

public partial class DungeonGenerator : MonoBehaviour
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

    public struct RoomTemplateCandiate
    {
        public int TemplateIdx;
        public Vector2Int Cell;
        public List<Vector3Int> OccupiedCells;

        public RoomTemplateCandiate(int templateIdx, Vector2Int cell, List<Vector3Int> occupiedCells)
        {
            TemplateIdx = templateIdx;
            Cell = cell;
            OccupiedCells = occupiedCells;
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

    [Tooltip("Maximum of tries to place all rooms")]
    [SerializeField]
    int MaxTotalPlacementTries = 10;

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
    List<Room> _roomTemplates = new List<Room>();


    // the instantiated gameobjects, which should be removed on cleanup
    List<Room> _instantiatedRooms = new List<Room>();

    List<RoomTemplateCandiate> _roomTemplateIdxsToInstantiate = new List<RoomTemplateCandiate>();

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
        var notNow = now.AddDays((double)Random.Range(0.0f, 100.0f));
        return now.ToBinary() ^ notNow.ToBinary();
    }

    public void Triangulate()
    {
        List<Vertex> vertices = new List<Vertex>();

        foreach (var room in _instantiatedRooms)
        {
            var meshCenter = room.GetMeshCenter();
            var position = new Vector2(meshCenter.x, meshCenter.z);
            vertices.Add(new Vertex<Room>(position, room));
        }

        _delaunay = Delaunay2D.Triangulate(vertices);
    }

    public void CreateContextGraph()
    {
        Triangulate();
        // sort list of rooms based on story-index
        // -> store rooms with no index at the beginning

        bool yeah = true;
    }

    public void Setup()
    {
        Cleanup();
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
                    _roomTemplates.Add(new Room(collider.gameObject));
                }
            }

            _roomTemplates.Sort(new RoomComparer());
            Debug.LogWarning("Found " + _roomTemplates.Count.ToString() + " rooms in roomfinder");
        }
    }

    // try, until it works
    public bool SudoPlaceRooms()
    {
        int count = 1;
        while (!PlaceRooms() && count <= MaxTotalPlacementTries) count++;
        if (count > MaxTotalPlacementTries)
        {
            Debug.Log("Could not place all rooms with multiple tries");
            return false;
        }
        else
        {
            Debug.Log("Placement took " + count.ToString() + " tries");
            return true;
        }
    }

    // select random cell within gird-size with border = 2 cells (to ensure,
    // that doors always can be connected on each side
    // ensure, that rooms are not directly placed besides each other (marked bufferzone in
    // between)
    // ensure, that room is completely placed within borders -> use buffer zone
    public bool PlaceRooms()
    {
        CreateGrid();
        UnmarkTemplatesForInstantiation();

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
                if (!_triedCells.Contains(cell))
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
            Debug.LogError(
                    "Could not place all rooms, tried cells: " + 
                    _triedCells.Count.ToString() + 
                    ", skipped rooms: " + 
                    roomsSkipped
                    );
            CreateGrid();
            UnmarkTemplatesForInstantiation();
            return false;
        }

        InstantiateRooms();
        return true;
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
                return false;
            }
        }
        return true;
    }

    // instantiate room templates at location and mark the overlapping cells 
    // as occupied
    private void InstantiateRooms()
    {
        foreach (var placementCandidate in _roomTemplateIdxsToInstantiate)
        {
            var room = _roomTemplates[placementCandidate.TemplateIdx];
            // location: grid-coords
            var location = placementCandidate.Cell * CellSize; // TODO: this should be encapsulated in method and take into account the offset

            var go = Instantiate(room.GameObject, new Vector3(location.x, 0, location.y), this.transform.rotation, this.transform);
            Room instantiatedRoom = new Room(go);
            _instantiatedRooms.Add(instantiatedRoom);

            foreach (var occupiedCell in placementCandidate.OccupiedCells)
            {
                // TODO: I think this is really brittle and needs to be improved
                _grid[occupiedCell].roomIdx = _instantiatedRooms.Count - 1;
            }
        }
    }


    private bool PlaceRoom(int roomIdx, Vector2Int cell) // cell: Zellenkoordinaten
    { 
        var room = _roomTemplates[roomIdx];
        var occupiedCells = GetOccupiedCells(room, cell, Vector3.zero);

        if (AreCellsFree(occupiedCells))
        {
            // deferred instantiation
            _roomTemplateIdxsToInstantiate.Add(new RoomTemplateCandiate(roomIdx, cell, occupiedCells));

            Debug.LogWarning(
                string.Format(
                    "Placing Room: {0} at location {1} in cell {2}",
                    room.GameObject.name,
                    cell * CellSize,
                    cell)
                );

            // mark room-cells and store room idx
            List<Vector3Int> markedCells = new List<Vector3Int>();
            foreach (var occupyingCell in occupiedCells)
            {
                _grid[occupyingCell].type = CellType.Room;


                _triedCells.Add(new Vector2Int(occupyingCell.x, occupyingCell.z));
                markedCells.Add(occupyingCell);
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

            return true;
        }
        return false;
    }

    public void Cleanup()
    {
        _roomTemplates.Clear();
        _grid = null;
        DestroyRooms();
        _delaunay = null;
    }

    private void DestroyRooms()
    {
        if (null != _instantiatedRooms)
        {
            foreach (var room in _instantiatedRooms)
            {
                DestroyImmediate(room.GameObject);
            }
        }
        _instantiatedRooms = new List<Room>();
        _triedCells = new HashSet<Vector2Int>();
    }

    private void UnmarkTemplatesForInstantiation()
    {
        _triedCells = new HashSet<Vector2Int>();
        _roomTemplateIdxsToInstantiate.Clear();
    }

    #region Visualization

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
    #endregion
}
