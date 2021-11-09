using Graphs;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

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
        public List<DoorMarker> doorMarkers;
        public Path path;
    }

    public struct Path
    {
        public List<Vector2Int> cells;
        public int partitionIdx;
    }

    public enum CellType
    {
        Free,
        Room,
        Hallway,
        Door,
        DoorDock,
        Bufferzone // adjacent to room or on border of grid
    };

    public struct Partition
    {
        public int StartIdxInclusive;
        public int EndIdxExclusive;
        public bool IsRelevantForStory;

        public Partition(int startIdxInclusive, int endIdxExclusive, bool isRelevantForStory)
        {
            StartIdxInclusive = startIdxInclusive;
            EndIdxExclusive = endIdxExclusive;
            IsRelevantForStory = isRelevantForStory;
        }

        public Partition(int startIdxInclusive, int endIdxExclusive) : this(startIdxInclusive, endIdxExclusive, true)
        {

        }
    }

    public struct RoomTemplateCandiate
    {
        public int TemplateIdx;
        public Vector2Int Cell;
        public List<Vector3Int> OccupiedCells;
        public Vector3 Rotation;

        public RoomTemplateCandiate(int templateIdx, Vector2Int cell, List<Vector3Int> occupiedCells, Vector3 rotation)
        {
            TemplateIdx = templateIdx;
            Cell = cell;
            OccupiedCells = occupiedCells;
            Rotation = rotation;
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

    [SerializeField]
    bool AllowRoomRotation = true;

    [Tooltip("Fixed seed to use for random room placement")]
    [SerializeField]
    int RandomSeed;

    [Tooltip("Use externally generated seed for room placement")]
    [SerializeField]
    bool UseExternalEntropy;

    [Header("Graphs")]
    [SerializeField]
    bool ShowDelauney = true;

    [SerializeField]
    bool ShowPartition = false;

    [SerializeField]
    int PartitionIdxToShow = 0;

    [SerializeField]
    bool ShowDoorPartition = false;

    [SerializeField]
    int DoorPartitionIdxToShow = 0;

    [SerializeField]
    bool ShowMST = true;

    [SerializeField]
    bool ShowPaths = true;

    // grid coordinates will start at local (0,0,0) and extend in positive x and
    // y coordinates
    DungeonGrid<Cell> _grid;

    // the read-in rooms of the roomfinder
    List<Room> _roomTemplates = new List<Room>();


    // the instantiated gameobjects, which should be removed on cleanup
    List<Room> _instantiatedRooms = new List<Room>();

    List<RoomTemplateCandiate> _roomTemplateIdxsToInstantiate = new List<RoomTemplateCandiate>();

    List<Delaunay2D> _delaunays;

    List<Partition> _partitions = new List<Partition>();

    //List<List<DoorMarker>> _doorPartitions = new List<List<DoorMarker>>();
    List<List<Vector3Int>> _doorPartitions = new List<List<Vector3Int>>();

    List<List<Prim.Edge>> _msts = new List<List<Prim.Edge>>();

    List<Path> _paths = new List<Path>();

    // TODO: find better place for this
    HashSet<Vector2Int> _triedCells = new HashSet<Vector2Int>();

    public enum NeighborDirection
    {
        left = 0,
        above = 1,
        below = 2,
        right = 3,
        leftbelow = 4,
        leftabove = 5,
        rightabove = 6,
        rightbelow = 7
    };

    Vector3Int[] _gridDirections = new Vector3Int[]
    {
        new Vector3Int(-1, 0,  0), // left
        new Vector3Int( 0, 0,  1), // above
        new Vector3Int( 0, 0, -1), // below
        new Vector3Int( 1, 0,  0), // right
        new Vector3Int(-1, 0, -1), // left below
        new Vector3Int(-1, 0,  1), // left above
        new Vector3Int( 1, 0,  1), // right above
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

    #region context graph

    public Delaunay2D TriangulateDoorPartition(List<Vector3Int> doorCells)
    {
        List<Vertex> vertices = new List<Vertex>();

        foreach (var cell in doorCells)
        {
            //var meshCenter = room.GetMeshCenter();
            // TODO: check, conversions... maybe, this could be done completely with cells

            var position = new Vector2(cell.x, cell.z);
            vertices.Add(new Vertex<Vector3Int>(position, cell));
        }

        return Delaunay2D.Triangulate(vertices);
    }

    public Delaunay2D PseudoTriangulate(List<Vector3Int> twoDoorCells)
    {
        List<Vertex> vertices = new List<Vertex>();
        foreach (var cell in twoDoorCells)
        {
            var position = new Vector2(cell.x, cell.z);
            vertices.Add(new Vertex<Vector3Int>(position, cell));
        }
        var del = Delaunay2D.Triangulate(vertices);

        // manually create the one possible edge
        if (del.Vertices.Count == 2)
        {
            del.Edges.Add(new Delaunay2D.Edge(del.Vertices[0], del.Vertices[1]));
        }
        return del;
    }

    public Delaunay2D TriangulateDoorPartition(List<DoorMarker> doorMarkers)
    {
        List<Vertex> vertices = new List<Vertex>();

        foreach (var marker in doorMarkers)
        {
            //var meshCenter = room.GetMeshCenter();
            // TODO: check, conversions... maybe, this could be done completely with cells
            var doorCell = GlobalToCellIdx(marker.transform.position);

            var position = new Vector2(doorCell.x, doorCell.z);
            vertices.Add(new Vertex<Vector3Int>(position, doorCell));
        }

        return Delaunay2D.Triangulate(vertices);
    }


    public void CreateContextGraph()
    {
        // sort list of rooms based on story-index
        // -> store rooms with no index at the beginning

        // create door-dock-cells (the cell which is supposed to connect to a door)
        foreach(var room in _instantiatedRooms)
        {
            var doorCells = GetDoorCellsOfPlacedRoom(room, true);
            foreach (var cell in doorCells)
            {
                room.AddDoorCell(cell);
            }

            var dockCells = GetDoorDockCells(doorCells);
            foreach (var dockCell in dockCells)
            {
                // temporary, for testing
                _grid[dockCell].type = CellType.DoorDock;
            }
        }

        // partition rooms
        List<Partition> partitions = new List<Partition>();
        int i;
        bool prevRoomStoryRelevant = IsMarkerRelevantForStory(_instantiatedRooms[0].GetStoryMarker());
        for (i = 1; i < _instantiatedRooms.Count; i++)
        {
            Room room = _instantiatedRooms[i];
            if (!prevRoomStoryRelevant && IsMarkerRelevantForStory(room.GetStoryMarker()))
            {
                partitions.Add(new Partition(0, i, false));
                prevRoomStoryRelevant = true;
                continue;
            }

            // check for barriers
            // TODO: check, if room has only doors in front of barrier -> unite partitions
            if (room.HasBarrier())
            {
                partitions.Add(new Partition(partitions[partitions.Count - 1].EndIdxExclusive, i+1));
            }
        }
        partitions.Add(new Partition(partitions[partitions.Count - 1].EndIdxExclusive, i));

        _partitions = partitions;

        // -> randomly put doors of non-story-relevant rooms in partitions (in the same per room obviously)
        Dictionary<int, List<int>> nonRelevantRooms = new Dictionary<int, List<int>>();
        if (!_partitions[0].IsRelevantForStory)
        {
            for (int j = 0; j < _partitions[0].EndIdxExclusive; j++ )
            {
                int partitionIdx = Mathf.RoundToInt(Random.Range(1f, _partitions.Count));
                if (!nonRelevantRooms.ContainsKey(partitionIdx))
                {
                    nonRelevantRooms.Add(partitionIdx, new List<int>());
                }
                nonRelevantRooms[partitionIdx].Add(j);
            }
        }
        // partition doors-cells

        List<List<Vector3Int>> doorPartitions = new List<List<Vector3Int>>();
        int firstRelevantPartition = _partitions[0].IsRelevantForStory ? 0 : 1;

        // init
        for (int j = firstRelevantPartition; j < _partitions.Count; j++)
        {
            //doorPartitions.Add(new List<DoorMarker>());
            doorPartitions.Add(new List<Vector3Int>());
        }

        int currentDoorPartition = 0;
        // partition over every relevant partitions
        for (int j = firstRelevantPartition; j < _partitions.Count; j++)
        {
            var partition = _partitions[j];

            // iterate over every room in partition and put doors in partition
            for (int n = partition.StartIdxInclusive; n < partition.EndIdxExclusive; n++)
            {
                var room = _instantiatedRooms[n];
                if (!room.HasBarrier())
                {
                    // if the room has no barriers, put every door into current partition

                    doorPartitions[currentDoorPartition].AddRange(GetDoorDockCells(room.DoorCells));
                }
                else
                {
                    var doorCellsAfterBarrier = GetDoorsOnSideOfBarrier(room, beforeBarrier: false);
                    doorPartitions[currentDoorPartition].AddRange(GetDoorDockCells(doorCellsAfterBarrier));

                    var doorCellsBeforeBarrier = GetDoorsOnSideOfBarrier(room, beforeBarrier: true);
                    doorPartitions[currentDoorPartition].AddRange(GetDoorDockCells(doorCellsBeforeBarrier));

                    // put doors of barrier room also in next partition, if no doors are behind barriers

                    // TODO: unite the two partitions
                    // this should probably be done earlier, when the rooms are partitioned?
                    if (doorCellsAfterBarrier.Count == 0)
                    {
                        doorPartitions[currentDoorPartition + 1].AddRange(GetDoorDockCells(doorCellsBeforeBarrier));
                    }
                }
            }

            if (nonRelevantRooms.ContainsKey(j))
            {
                foreach (var roomIdx in nonRelevantRooms[j])
                {
                    var room = _instantiatedRooms[roomIdx];
                    doorPartitions[currentDoorPartition].AddRange(GetDoorDockCells(room.DoorCells));
                }
            }
            currentDoorPartition++;
        }

        _doorPartitions = doorPartitions;


        // create delaunay between doors of one partition
        foreach(var partition in _doorPartitions)
        {
            if (partition.Count > 2)
            {
                _delaunays.Add(TriangulateDoorPartition(partition));
            }
            else
            {
                _delaunays.Add(PseudoTriangulate(partition));
            }
        }

        // select paths -> minimum spanning tree? oder irgendwie anders
        // TODO: erkennen, wenn zwei trueren zum gleichen Raum gehoeren und nur mit geringer wahrscheinlichkeit direkt verbinden
        // TODO: store edge-length together with edge to be able to sort path creation from shortest to longest path?
        foreach (var del in _delaunays)
        {
            if (del.Edges.Count > 2)
            {
                List<Prim.Edge> edges = new List<Prim.Edge>();

                foreach (var edge in del.Edges)
                {
                    edges.Add(new Prim.Edge(edge.U, edge.V));
                }

                List<Prim.Edge> mst = Prim.MinimumSpanningTree(edges, edges[0].U);
                _msts.Add(mst);
            }
            else if (del.Edges.Count == 1)
            {
                _msts.Add(new List<Prim.Edge>{ new Prim.Edge(del.Edges[0].U, del.Edges[0].V) });
            }
        }

        DungeonPathfinder2D astar = new DungeonPathfinder2D(new Vector2Int(GridDimensions.x, GridDimensions.z));
        int pathPartitionIdx = 0;
        foreach (var mst in _msts)
        {
            foreach (var edge in mst)
            {
                var startCellDoorf = edge.U.Position;
                var startCellDoor = new Vector2Int(Mathf.RoundToInt(startCellDoorf.x), Mathf.RoundToInt(startCellDoorf.y));
                var endCellDoorf = edge.V.Position;
                var endCellDoor = new Vector2Int(Mathf.RoundToInt(endCellDoorf.x), Mathf.RoundToInt(endCellDoorf.y));

                // TODO: this should include the partition, of which some other path is part of
                var path = astar.FindPath(startCellDoor, endCellDoor, (DungeonPathfinder2D.Node a, DungeonPathfinder2D.Node b) => {
                    var pathCost = new DungeonPathfinder2D.PathCost();

                    var bPosVec3 = new Vector3Int(b.Position.x, 0, b.Position.y);
                    pathCost.cost = Vector2Int.Distance(b.Position, endCellDoor);    //heuristic

                    if (_grid[bPosVec3].type == CellType.Room || _grid[bPosVec3].type == CellType.Door)
                    {
                        pathCost.cost += 1000;
                    }
                    else if (_grid[bPosVec3].type == CellType.Free || _grid[bPosVec3].type == CellType.Bufferzone)
                    {
                        pathCost.cost += 5;
                    }
                    else if (_grid[bPosVec3].type == CellType.Hallway && _grid[bPosVec3].path.partitionIdx == pathPartitionIdx)
                    {
                        pathCost.cost += 1;
                    }
                    else if (_grid[bPosVec3].type == CellType.Hallway && _grid[bPosVec3].path.partitionIdx != pathPartitionIdx ||
                             _grid[bPosVec3].type == CellType.DoorDock && _grid[bPosVec3].path.partitionIdx != pathPartitionIdx ) // TODO: this does not work for doors, which are in more than one partition
                    {
                        pathCost.cost += 1000;
                        //Debug.Log("Trying to pass through cell of other partition at cell x: " + bPosVec3.x + ", z: " + bPosVec3.z);
                    }

                    pathCost.traversable = true;

                    return pathCost;
                });

                if (null != path)
                {
                    Path newPath = new Path();
                    newPath.partitionIdx = pathPartitionIdx;
                    newPath.cells = path;
                    _paths.Add(newPath);

                    foreach (var cell in path)
                    {
                        _grid[cell.x, 0, cell.y].path = newPath;
                        if (_grid[cell.x, 0, cell.y].type != CellType.DoorDock) // don't convert to hallway
                        {
                            _grid[cell.x, 0, cell.y].type = CellType.Hallway;
                            Debug.Log("Included DoorDock");
                        }
                    }
                }
            }
            pathPartitionIdx++;
        }
    }

    List<Vector3Int> GetDoorsOnSideOfBarrier(Room room, bool beforeBarrier)
    {
        var barrierMarker = room.GetStoryMarker();
        var barrierDir = barrierMarker.GetBarrierDirection();
        var barrierLoc = barrierMarker.transform.position;
        List<Vector3Int> doorCellsOnSide = new List<Vector3Int>();
        foreach (var cell in room.DoorCells)
        {
            var cellGlobalLoc = CellIdxToGlobal(cell);
            var diff = cellGlobalLoc - barrierLoc;
            var dot = Vector3.Dot(barrierDir, diff);
            if (dot > 0 && !beforeBarrier)
            {
                doorCellsOnSide.Add(cell);
            } 
            else if (dot < 0 && beforeBarrier)
            {
                doorCellsOnSide.Add(cell);
            }
        }
        return doorCellsOnSide;
    }



    #endregion

    #region setup
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
    #endregion

    #region cleanup
    public void Cleanup()
    {
        _roomTemplates.Clear();
        _grid = null;
        DestroyRooms();
        _delaunays = new List<Delaunay2D>();
        _partitions = new List<Partition>();
        //_doorPartitions = new List<List<DoorMarker>>();
        _doorPartitions = new List<List<Vector3Int>>();
        _msts = new List<List<Prim.Edge>>();
        _paths = new List<Path>();
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
    #endregion

    #region room placement
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

    // TODO: do partitioning of rooms before placement and create sectors of grid, 
    // in which most of the rooms of the same partition are placed, to create more stringent
    // path creation
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

                // pick random rotation
                float rotationAroundY = 0.0f;
                if (AllowRoomRotation)
                {
                    var rotIdx = Mathf.FloorToInt(Random.Range(0, 3));
                    switch (rotIdx)
                    {
                        case 1:
                            rotationAroundY = 90.0f;
                            break;
                        case 2:
                            rotationAroundY = 180.0f;
                            break;
                        case 3:
                            rotationAroundY = 270.0f;
                            break;
                        default:
                            rotationAroundY = 0.0f;
                            break;
                    }
                }
                Vector3 rotation = new Vector3(0, rotationAroundY, 0);

                cell = new Vector2Int(x, y);
                if (!_triedCells.Contains(cell))
                {
                    _triedCells.Add(cell);
                    success = PlaceRoom(i, cell, rotation);
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

    private bool PlaceRoom(int roomIdx, Vector2Int cell, Vector3 rotation) // cell: Zellenkoordinaten
    { 
        var room = _roomTemplates[roomIdx];
        var occupiedCells = GetOccupiedCells(room, cell, rotation);

        if (AreCellsFree(occupiedCells))
        {
            // deferred instantiation
            _roomTemplateIdxsToInstantiate.Add(new RoomTemplateCandiate(roomIdx, cell, occupiedCells, rotation));

            Debug.LogWarning(
                string.Format(
                    "Placing Room: {0} at location {1} in cell {2}",
                    room.GameObject.name,
                    cell * CellSize,
                    cell)
                );

            // mark room-cells and store room idx
            List<Vector3Int> markedCells = new List<Vector3Int>();
            foreach (var occupiedCell in occupiedCells)
            {
                _grid[occupiedCell].type = CellType.Room;

                _triedCells.Add(new Vector2Int(occupiedCell.x, occupiedCell.z));
                markedCells.Add(occupiedCell);
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
                            _grid[neighborCell].type != CellType.Bufferzone && 
                            _grid[neighborCell].type != CellType.Door)
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

    // instantiate room templates at location and mark the overlapping cells 
    // as occupied
    private void InstantiateRooms()
    {
        foreach (var placementCandidate in _roomTemplateIdxsToInstantiate)
        {
            var room = _roomTemplates[placementCandidate.TemplateIdx];
            // location: grid-coords
            var location = placementCandidate.Cell * CellSize; // TODO: this should be encapsulated in method and take into account the offset

            // because the rotation should be applied cell-wise and not coninously
            // an offset is required for the non-0 deg rotations
            // got no time to be smart, hard code
            Vector3Int offset = GetRotationRelatedCellOffset(placementCandidate.Rotation);
            offset = offset * CellSize;

            var go = Instantiate(
                room.GameObject,
                new Vector3(location.x, 0, location.y) + offset,
                Quaternion.Euler(placementCandidate.Rotation),
                this.transform
                );

            Room instantiatedRoom = new Room(go);
            _instantiatedRooms.Add(instantiatedRoom);

            foreach (var occupiedCell in placementCandidate.OccupiedCells)
            {
                // TODO: I think this is really brittle and needs to be improved
                _grid[occupiedCell].roomIdx = _instantiatedRooms.Count - 1;
            }
        }
    }
    #endregion

    #region cell 'management'

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
        Vector3 Rotation
        ) 
    {
        List<Vector3Int> overlappingCells = new List<Vector3Int>();

        // get mesh extents
        var actualExtents = room.MeshExtents() * 2;

        var roomObjectPosition = room.GameObject.transform.position; // global coord
        var placementLoc3 = new Vector3(placementCell.x, 0, placementCell.y);

        var rotation = Quaternion.Euler(Rotation);

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
                        // guess, this would be the place to apply rotation to room to
                        // translate rotation to the cell locations

                        var gridCell =  placementLoc3 + rotation * new Vector3(x, 0, z);
                        overlappingCells.Add(Vector3Int.RoundToInt(gridCell));
                        break;
                    }
                }
            }
        }
        return overlappingCells;
    }

    // this should be called, when the rooms were placed
    private List<Vector3Int> GetDoorCellsOfPlacedRoom(Room room, bool overwriteCellType)
    {
        HashSet<Vector3Int> doorCells = new HashSet<Vector3Int>();

        var doorMarkers = room.GetDoorMarkers();
        var roomPosition = room.GameObject.transform.position;
        var roomRotation = room.GameObject.transform.rotation;

        foreach (var marker in doorMarkers)
        {
            // get room-local position of marker
            var markerLocalPos = marker.transform.localPosition;

            // get gridcell
            var scaledPos = (roomPosition + roomRotation * markerLocalPos) / CellSize;
            var gridCell = Vector3Int.FloorToInt(scaledPos);

            // the doorCell must be an occupied room cell or another door
            if (_grid[gridCell].type != CellType.Room &&
                _grid[gridCell].type != CellType.Door)
            {
                // if it is not, select nearest room-cell
                foreach (var dir in _gridDirections)
                {
                    if (_grid[gridCell + dir].type == CellType.Room ||
                        _grid[gridCell + dir].type == CellType.Door)
                    {
                        gridCell = gridCell + dir;
                        break;
                    }
                }
            }
            doorCells.Add(gridCell);
            _grid[gridCell].type = CellType.Door;
            if (_grid[gridCell].doorMarkers == null)
            {
                _grid[gridCell].doorMarkers = new List<DoorMarker>();
            }
            _grid[gridCell].doorMarkers.Add(marker);
        }

        List<Vector3Int> doorCellList = new List<Vector3Int>();
        foreach (var cell in doorCells)
        {
            doorCellList.Add(cell);
        }

        return doorCellList;
    }

    private List<Vector3Int> GetDoorDockCells(List<Vector3Int> doorCells)
    {
        List<Vector3Int> doorDockCells = new List<Vector3Int>();
        foreach (var doorCell in doorCells)
        {
            var doorMarkers = _grid[doorCell].doorMarkers;
            foreach (var marker in doorMarkers)
            {
                var markerDirection = marker.gameObject.transform.forward;
                doorDockCells.Add(doorCell - Vector3Int.RoundToInt(markerDirection));
            }
        }
        return doorDockCells;
    }

    // returns false, if one of the passed cells is already occupied
    private bool AreCellsFree(List<Vector3Int> cellCoords)
    {
        foreach (var cellCoord in cellCoords)
        {
            try
            {
                var cellType = _grid[cellCoord].type; 

                if (cellType != CellType.Free)
                {
                    return false;
                }
            } catch (System.Exception ex ) // TODO: this does occur, if the cellcoord is negative or exceeds the boundaries, which may happen due to rotation (should be caught earlier)
            {
                return false;
            }
        }
        return true;
    }

    private Vector3Int GetRotationRelatedCellOffset(Vector3 rotation)
    {
        int rot = Mathf.RoundToInt(rotation.y);
        int rotMod = rot % 360;

        Vector3Int offset = new Vector3Int(0,0,0);
        if (rotMod == 90)
        {
            offset = new Vector3Int(0, 0, 1);
        }
        else if (rotMod == 180)
        {
            offset = new Vector3Int(1, 0, 1);
        } 
        else if (rotMod == 270)
        {
            offset = new Vector3Int(1, 0, 0);
        }
        return offset;
    }

    #endregion

    #region convenience
    private bool IsMarkerRelevantForStory(StoryMarker marker)
    {
        return marker.RelevantForStory && marker.IndexInStory >= 0;
    }

    private long GetExternalSeed()
    {
        var now = System.DateTime.Now;
        var notNow = now.AddDays((double)Random.Range(0.0f, 100.0f));
        return now.ToBinary() ^ notNow.ToBinary();
    }

    public Vector3Int GlobalToCellIdx(Vector3 globalCoord)
    {
        var downScaled = globalCoord / CellSize;
        return Vector3Int.RoundToInt(downScaled);
    }

    public Vector3 CellIdxToGlobal(Vector3Int cell)
    {
        // this should account for transform of parent 
        var coord = cell * CellSize + new Vector3Int(1,0,1) * CellSize / 2;
        return coord; //?
    }
    #endregion

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
        else if (CellType.Door == cell.type)
        {
            Debug.DrawLine(lowerCorner, upperCorner, Color.cyan);
        }
        else if (CellType.Hallway == cell.type)
        {
            Debug.DrawLine(lowerCorner, upperCorner, Color.green);
        }
        else if (CellType.DoorDock == cell.type)
        {
            Debug.DrawLine(lowerCorner, upperCorner, Color.magenta);
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
                        Debug.DrawLine(new Vector3(0, y, z) * CellSize, new Vector3(_grid.Size.x, y, z) * CellSize, Color.gray);
                    }

                    for (int x = 0; x <= _grid.Size.x; x++)
                    {
                        Debug.DrawLine(new Vector3(x, y, 0) * CellSize, new Vector3(x, y, _grid.Size.z) * CellSize, Color.gray);
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


        if (null != _partitions && PartitionIdxToShow < _partitions.Count && ShowPartition)
        {
            var partition = _partitions[PartitionIdxToShow];
            for (int i = partition.StartIdxInclusive; i < partition.EndIdxExclusive; i++)
            {
                var room = _instantiatedRooms[i];
                Handles.DrawWireDisc(room.GetMeshCenter(), room.GameObject.transform.up, 10, 10);
            }
        }

        if (null != _doorPartitions && DoorPartitionIdxToShow < _doorPartitions.Count && ShowDoorPartition)
        {
            var partition = _doorPartitions[DoorPartitionIdxToShow];
            //foreach (var doorMarker in partition)
            //{
            //    var trans = doorMarker.transform;
            //    Handles.DrawSolidDisc(trans.position, trans.up, 5);
            //}
            foreach (var cell in partition)
            {
                var trans = cell * CellSize + new Vector3Int(1,0,1) * CellSize / 2;
                Handles.DrawSolidDisc(trans, Vector3.up, 2);
            }

            if (null != _delaunays && ShowDelauney && DoorPartitionIdxToShow < _delaunays.Count)
            {
                var delaunay = _delaunays[DoorPartitionIdxToShow];
                foreach (var edge in delaunay.Edges)
                {
                    if (!edge.IsBad)
                    {
                        var p1 = edge.U.Position * CellSize + new Vector3(1,1,1) * CellSize / 2;
                        var p2 = edge.V.Position * CellSize + new Vector3(1,1,1) * CellSize / 2;
                        Debug.DrawLine(
                            new Vector3(p1.x, 0, p1.y), 
                            new Vector3(p2.x, 0, p2.y),
                            Color.red
                            );
                    }
                }
                if (delaunay.Vertices.Count == 2)
                {
                    var p1 = delaunay.Vertices[0].Position * CellSize;
                    var p2 = delaunay.Vertices[1].Position * CellSize;
                    Debug.DrawLine(
                        new Vector3(p1.x, 0, p1.y), 
                        new Vector3(p2.x, 0, p2.y),
                        Color.red
                        );
                }
            }

            if (null != _msts && ShowMST && DoorPartitionIdxToShow < _msts.Count)
            {
                var mst = _msts[DoorPartitionIdxToShow];
                foreach (var edge in mst)
                {
                    var p1 = edge.U.Position * CellSize + new Vector3(1,1,0) * CellSize / 2;
                    var p2 = edge.V.Position * CellSize + new Vector3(1,1,0) * CellSize / 2;
                    Debug.DrawLine(
                        new Vector3(p1.x, 0, p1.y), 
                        new Vector3(p2.x, 0, p2.y),
                        Color.magenta
                        );
                }
            }

            // only show paths for current partition
            if (null != _paths && ShowPaths)
            {
                var pathsForPartition = _paths.Where(path => path.partitionIdx == DoorPartitionIdxToShow);
                if (pathsForPartition.Any())
                {
                    foreach(var path in pathsForPartition)
                    {
                        Vector3 c1;
                        Vector3 c2;
                        c1 = new Vector3(path.cells[0].x, 0, path.cells[0].y) * CellSize + new Vector3(1,1,1) * CellSize / 2;
                        foreach(var node in path.cells)
                        {
                            c2 = new Vector3(node.x, 0, node.y) * CellSize + new Vector3(1,1,1) * CellSize / 2;
                            Debug.DrawLine(
                                new Vector3(c1.x, c1.y, c1.z), 
                                new Vector3(c2.x, c2.y, c2.z),
                                Color.red
                                );
                            c1 = c2;
                        }
                    }
                }

            }
        }

    }
    #endregion
}
