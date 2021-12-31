using System;
using Graphs;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.ProGrids;
using Random = UnityEngine.Random;


// Note (Malte Reinsch, 2021/11/10):
// Generator creates walkable corridors between rooms and is thus in a workable state.
// Multiple areas of improvement:
// 
// Path improvement:
//  - mst currenlty only creates shortes paths, the prim-algorithm could be modified to
//    detect connection to same room and only create it with smaller than 1 probability
//  - store length of mst edge to be able to create paths in specific order
//  - Astar algo creates squiggly paths; these don't really hinder playability but aren't
//    visually appealing; astar could be modified to put cost on often changing direction
//  - astar could perform multiple passes with different orders of partitions and choose the
//    permutation with lowest path-cost overall
//  - some partition crossings do occur; detect and retry path creation
//  - for both above, the path should not be stored direclty in grid but be stored in lookup
//    structure
//
// Refactoring:
//  - vector-conversions from Vec2 to Vec3 and back between different stages of context graph
//    creation
//  - redundant storage of the same information in different places (currently for convenience)
//  - many iterations (which could be united?)


public partial class DungeonGenerator : MonoBehaviour
{
    
    [SerializeField]
    NavMeshSurface navmesh;
    
    // coord-systems:
    // - cell-coordinates (without consideration of cellsize)
    // - grid-coordinates (with consideration of cellsize)
    // - room-coordinates (used for scanning the geometry of a room, local to room itself)
    public struct Cell
    {
        public CellType type;
        public int roomIdx;
        public List<DoorMarker> doorMarkers;
        public List<Path> paths;
        public int pathPartitionIdx;
        public Vector3Int doorCellOfDoorDock;
        public bool doorDockConnected;
        public HashSet<Vector3Int> neighborsInPaths;
    }

    public struct Path
    {
        // will be ordered from start to end, i guess
        public List<Vector2Int> cells;
        public int partitionIdx;
        public float cost;
        public int startRoomIdx;
        public int endRoomIdx;
    }

    public enum NeighborPattern
    {
        None,               // none of them
        AboveBelow,         // neighbor above and below
        LeftBelow,          // neighbor below and left
        AllSides,           // neighbor on all sides
        BelowBothSides,     // neighbor below and to the sides
        OneSide             // without rotation applied, this is the cell below
    }

    public struct CellPathData
    {
        // use flags
        public Direction incomingDirections;
        public bool doorDock;
        public NeighborPattern type;
        public float rotationInDeg;
    }

    public enum Direction
    {
        None    = 0x0000,
        Up      = 0x0001,
        Down    = 0x0010,
        Left    = 0x0100,
        Right   = 0x1000
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

        public bool InBounds(int index)
        {
            return index >= StartIdxInclusive && index < EndIdxExclusive;
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

    private struct PartitionDimensionRange
    {
        public Vector2Int lowBound;
        public Vector2Int highBound;
    }

    [Header("Grid specification")]

    //[Tooltip("Dimensions of Grid in cells")]
    //[SerializeField]

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
    int MaxPlacementTriesPerRoom = 100;

    [Tooltip("Maximum of tries to place all rooms")]
    [SerializeField]
    int MaxTotalPlacementTries = 100;

    [SerializeField]
    bool AllowRoomRotation = true;

    [Tooltip("Fixed seed to use for random room placement")]
    [SerializeField]
    long RandomSeed;

    [Tooltip("Use externally generated seed for room placement")]
    [SerializeField]
    bool UseExternalEntropy;

    [SerializeField] 
    float BruteForcePartitionSizeModifierX = 1.0f;
    
    [SerializeField] 
    float BruteForcePartitionSizeModifierZ = 1.0f;
    
    [SerializeField] 
    float BruteForcePartitionSizeModifierLastPartition = 1.5f;

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

    [Header("CorridorPlacement")]
    [SerializeField]
    GameObject StraigtCorridor;

    [SerializeField]
    GameObject LeftTurnCorridor;

    [SerializeField]
    GameObject TCrossCorridor;

    [SerializeField]
    GameObject CrossCorridor;

    [Header("Dungeon Generation")]
    [SerializeField]
    int MaxDungeonTries = 20;

    [SerializeField]
    bool CreateDungeonOnStart = true;

    [Header("Minimap Creation")]
    [SerializeField]
    Minimap Minimap;

    [SerializeField] 
    bool ShowGridNeighbors = false;

    [SerializeField] 
    Vector2Int GridCoordsForNeighbor = Vector2Int.zero;

    // grid coordinates will start at local (0,0,0) and extend in positive x and
    // y coordinates
    DungeonGrid<Cell> _grid;

    // the read-in rooms of the roomfinder
    List<Room> _roomTemplates = new List<Room>();


    // the instantiated gameobjects, which should be removed on cleanup
    // this maps the instantiation-idx to the actual room-gameobject
    private Dictionary<int, Room> _instantiatedRooms = new Dictionary<int, Room>();

    List<GameObject> _instantiatedCorridors = new List<GameObject>();

    List<RoomTemplateCandiate> _roomTemplateIdxsToInstantiate = new List<RoomTemplateCandiate>();

    List<Delaunay2D> _delaunays;

    List<Partition> _partitions = new List<Partition>();

    List<List<Vector3Int>> _doorPartitions = new List<List<Vector3Int>>();

    List<List<Prim.Edge>> _msts = new List<List<Prim.Edge>>();

    List<List<Path>> _partitionedPaths = new List<List<Path>>();

    private Dictionary<int, List<int>> _irrelevantRoomAssignment = new Dictionary<int, List<int>>();
    
    private List<Vector2Int> _partitionDims = new List<Vector2Int>();

    private List<PartitionDimensionRange> _partitionDimensionRanges = new List<PartitionDimensionRange>();

    private Vector3Int _gridDimensions;

    CellPathData[,] _cellPathData;

    GameObject _playerInstance;

    bool _generationSuccessfull = false;

    // TODO: find better place for this
    HashSet<Vector2Int> _triedCells = new HashSet<Vector2Int>();

    #region Getters for Data

    public List<List<Vector3Int>> DoorPartitions { get => _doorPartitions; }
    public List<List<Path>> PartitionedPaths { get => _partitionedPaths; }
    public CellPathData[,] CellPathDatas { get => _cellPathData; }
    public bool FinishedGenerating { get => _generationSuccessfull; }
    public DungeonGrid<Cell> Grid { get => _grid; }
    public Dictionary<int, Room> InstantiatedRooms { get => _instantiatedRooms; }

    #endregion

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

    public readonly static Vector3Int[] GridDirections = new Vector3Int[]
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
        return GridDirections[((int)dir)];
    }

    [SerializeField]
    GameObject playerPrefab;


    public void InstantiatePlayer()
    {
        var rooms = _instantiatedRooms.Where(room => room.Value.GetFirstStoryMarker().IndexInStory == 0);
        var spawnPoint = FindObjectOfType<SpawnPoint>();
        var startRoom = rooms.First().Value;
        if (spawnPoint != null)
        {
            _playerInstance = Instantiate(playerPrefab, spawnPoint.transform.position, spawnPoint.transform.rotation);
        }
        else
        {
            _playerInstance = Instantiate(playerPrefab, startRoom.GameObject.transform.position + startRoom.GameObject.transform.rotation * new Vector3(3, 3, 3) , this.transform.rotation);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (CreateDungeonOnStart)
        {
            GenerateDungeon();
            navmesh.BuildNavMesh();
        }
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
            var position = new Vector2(cell.x, cell.z);
            vertices.Add(new Vertex<int>(position, _grid[cell].roomIdx));
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
            var doorCell = GlobalToCellIdx(marker.transform.position, CellSize);

            var position = new Vector2(doorCell.x, doorCell.z);
            vertices.Add(new Vertex<Vector3Int>(position, doorCell));
        }

        return Delaunay2D.Triangulate(vertices);
    }

    public List<Partition> PartitionRooms(Room[] rooms)
    {
        if (rooms.Length == 0)
        {
            Debug.LogError("Room array is empty");
            return null;
        }

        // partition rooms
        List<Partition> partitions = new List<Partition>();
        int i = 1;

        bool prevRoomStoryRelevant = IsMarkerRelevantForStory(rooms[0].GetFirstStoryMarker());

        bool firstRoomStoryRelevant = IsMarkerRelevantForStory(rooms[0].GetFirstStoryMarker());
        if (!firstRoomStoryRelevant)
        {
            for (; i < rooms.Length; i++) // find first, that is relevant for story
            {
                Room room = rooms[i];
                if (IsMarkerRelevantForStory(room.GetFirstStoryMarker()))
                {
                    partitions.Add(new Partition(0, i, false));
                    break;
                }
            }
        }

        // continuation of room partitioning
        for (; i < rooms.Length; i++)
        {
            Room room = rooms[i];
            // check for barriers
            // check, if room has only doors in front of barrier -> unite partitions
            if (room.HasBarrier())
            {
                var doorsBehindBarrier = GetDoorTransformsOnSideOfBarrier(room, false);
                if (doorsBehindBarrier.Any()) // split up in partitions only, if doors on other side
                {
                    partitions.Add(new Partition(partitions[partitions.Count - 1].EndIdxExclusive, i));
                }
            }
        }

        // close of last partition
        partitions.Add(new Partition(partitions[partitions.Count - 1].EndIdxExclusive, i));
        
        return partitions;
    }


    // TODO: either make this deterministic or store this information somewhere
    public Dictionary<int, List<int>> AssignNonRelevantRoomsToPartition(List<Partition> partitions)
    {
        // -> randomly put doors of non-story-relevant rooms in partitions (in the same per room obviously)
        var nonRelevantRooms = new Dictionary<int, List<int>>();
        if (!partitions[0].IsRelevantForStory)
        {
            for (int j = 0; j < partitions[0].EndIdxExclusive; j++)
            {
                int partitionIdx = Mathf.RoundToInt(Random.Range(1f, partitions.Count - 1));
                if (!nonRelevantRooms.ContainsKey(partitionIdx))
                {
                    nonRelevantRooms.Add(partitionIdx, new List<int>());
                }
                nonRelevantRooms[partitionIdx].Add(j);
            }
        }

        return nonRelevantRooms;
    }

    public bool FindPaths()
    {
        // sort list of rooms based on story-index
        // -> store rooms with no index at the beginning

        // create door-dock-cells (the cell which is supposed to connect to a door)
        foreach (var room in _instantiatedRooms)
        {
            var doorCells = GetDoorCellsOfPlacedRoom(room.Value, true);
            foreach (var cell in doorCells)
            {
                room.Value.AddDoorCell(cell);
            }

            var dockCells = GetDoorDockCells(doorCells);
            foreach (var dockCell in dockCells)
            {
                // temporary, for testing
                _grid[dockCell].type = CellType.DoorDock;
                //associate the dockcell with the room
                _grid[dockCell].roomIdx = _grid[doorCells[0]].roomIdx;
            }
        }

        // partition doors-cells
        List<List<Vector3Int>> doorPartitions = new List<List<Vector3Int>>();
        int firstRelevantPartition = _partitions[0].IsRelevantForStory ? 0 : 1;

        // init
        for (int j = firstRelevantPartition; j < _partitions.Count; j++)
        {
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
                    var dockCells = GetDoorDockCells(room.DoorCells);
                    foreach (var cell in dockCells)
                    {
                        if (!doorPartitions[currentDoorPartition].Contains(cell))
                        {
                            doorPartitions[currentDoorPartition].Add(cell);
                        }
                    }
                }
                else
                {
                    var doorCellsAfterBarrier = GetDoorCellsOnSideOfBarrier(room, beforeBarrier: false);
                    var doorDockCells = GetDoorDockCells(doorCellsAfterBarrier);
                    foreach (var cell in doorDockCells)
                    {
                        if (!doorPartitions[currentDoorPartition].Contains(cell))
                        {
                            doorPartitions[currentDoorPartition].Add(cell);
                        }
                    }
                    // the barrier room is part of the next partition, but the doors before the barrier should be part of 
                    // the partition before
                    if (doorCellsAfterBarrier.Any())
                    {
                        var doorCellsBeforeBarrier = GetDoorCellsOnSideOfBarrier(room, beforeBarrier: true);
                        var doorDockCellsBefore = GetDoorDockCells((doorCellsBeforeBarrier));
                        foreach (var cell in doorDockCellsBefore)
                        {
                            if (!doorPartitions[currentDoorPartition-1].Contains(cell))
                            {
                                doorPartitions[currentDoorPartition-1].Add(cell);
                            }
                        }
                    }
                    else
                    {
                        var doorCellsBeforeBarrier = GetDoorCellsOnSideOfBarrier(room, beforeBarrier: true);
                        var doorDockCellsBefore = GetDoorDockCells(doorCellsBeforeBarrier);
                        foreach (var cell in doorDockCellsBefore)
                        {
                            if (!doorPartitions[currentDoorPartition].Contains(cell))
                            {
                                doorPartitions[currentDoorPartition].Add(cell);
                            }
                        }
                    }
                }
            }

            if (_irrelevantRoomAssignment.ContainsKey(j))
            {
                foreach (var roomIdx in _irrelevantRoomAssignment[j])
                {
                    var room = _instantiatedRooms[roomIdx];
                    doorPartitions[currentDoorPartition].AddRange(GetDoorDockCells(room.DoorCells));
                }
            }
            currentDoorPartition++;
        }

        _doorPartitions = doorPartitions;

        // create delaunay between doors of one partition
        foreach (var partition in _doorPartitions)
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
                _msts.Add(new List<Prim.Edge> { new Prim.Edge(del.Edges[0].U, del.Edges[0].V) });
            }
        }

        // TODO: keep track of multiple direction changes right after another and increase weight
        // to avoid squiggly paths
        DungeonPathfinder2D astar = new DungeonPathfinder2D(new Vector2Int(_gridDimensions.x, _gridDimensions.z));

        // index of door-partitions

        // currently produces pretty long paths:
        // TODO: (idea)
        // this should try out all different orders of paths
        // and compare the total cost of all of them and use the
        // permutation, which is cheapest
        // for this to work, the pathidx (and the path) should not be directly stored
        // in the cell but in a hashset based lookup structure, which allows quick checking,
        // if a cell is already contained in another path
        //
        // TODO: other idea:
        // could also order edges of _mst by length and start with the longest
        // in order to be able to find the most direct path

        // create pseudo-grid
        // TODO: this won't cut it, astar is not random enough to do this...
        // Should do the whole partitioning thing, before the rooms are actually instantiated..
        bool pathIsValid = false;

        var pseudoGrid = _grid.DeepClone(); // pretty fast, it's basically just an array with small structs
        // TODO: could randomize the order, in which the edges of the msts are traversed, but this 
        // complicates the tracking partitions...

        int pathPartitionIdx = 0;
        foreach (var mst in _msts)
        {

            var doorPartition = _doorPartitions[pathPartitionIdx];
            _partitionedPaths.Add(new List<Path>());
            foreach (var edge in mst)
            {
                var startCellDoorf = edge.U.Position;
                var startCellDoor = new Vector2Int(Mathf.RoundToInt(startCellDoorf.x), Mathf.RoundToInt(startCellDoorf.y));
                var startCellDoor3Int = new Vector3Int(Mathf.RoundToInt(startCellDoorf.x), 0, Mathf.RoundToInt(startCellDoorf.y));
                var endCellDoorf = edge.V.Position;
                var endCellDoor = new Vector2Int(Mathf.RoundToInt(endCellDoorf.x), Mathf.RoundToInt(endCellDoorf.y));
                var endCellDoor3Int = new Vector3Int(Mathf.RoundToInt(endCellDoorf.x), 0, Mathf.RoundToInt(endCellDoorf.y));

                var path = astar.FindPath(startCellDoor, endCellDoor, (DungeonPathfinder2D.Node a, DungeonPathfinder2D.Node b) => {
                    var pathCost = new DungeonPathfinder2D.PathCost();
                    try
                    {
                        var bPosVec3 = new Vector3Int(b.Position.x, 0, b.Position.y);
                        pathCost.cost = Vector2Int.Distance(b.Position, endCellDoor);    //heuristic

                        if (pseudoGrid[bPosVec3].type == CellType.Room || pseudoGrid[bPosVec3].type == CellType.Door)
                        {
                            // don't go there
                            pathCost.cost += 100000;
                        }
                        else if (pseudoGrid[bPosVec3].type == CellType.Free ||
                                 pseudoGrid[bPosVec3].type == CellType.Bufferzone)
                        {
                            pathCost.cost += 5;
                        }
                        else if (pseudoGrid[bPosVec3].type == CellType.Hallway &&
                                 pseudoGrid[bPosVec3].pathPartitionIdx == pathPartitionIdx)
                        {
                            pathCost.cost += 1;
                        }
                        // don't cross paths, which are already part of other partition
                        else if (pseudoGrid[bPosVec3].type == CellType.Hallway &&
                                 pseudoGrid[bPosVec3].pathPartitionIdx != pathPartitionIdx ||
                                 pseudoGrid[bPosVec3].type == CellType.DoorDock &&
                                 !doorPartition.Contains(bPosVec3)) // doorDock is not in current partition
                        {
                            // don't go there
                            pathCost.cost += 100000;
                        }

                        pathCost.traversable = true;
                    }
                    catch (IndexOutOfRangeException ex)
                    {
                        pathCost.cost += 1000000;
                        //pathCost.traversable = false;
                    }

                    return pathCost;
                });

                if (null == path)
                {
                    Debug.LogError("Path could not be created");
                    return false;
                }
                else
                {
                    float pathCost = path.Item2;
                    pathIsValid = pathCost < 100000;
                    if (!pathIsValid)
                    {
                        Debug.LogError("Path contained crossing of partitions, redoing the whole operation");
                        return false;
                    }

                    Path newPath = new Path();
                    newPath.partitionIdx = pathPartitionIdx;
                    newPath.cells = path.Item1;
                    newPath.cost = pathCost;

                    int startIdx = _grid[startCellDoor3Int].roomIdx;
                    int endIdx = _grid[endCellDoor3Int].roomIdx;

                    newPath.startRoomIdx = startIdx;
                    newPath.endRoomIdx = endIdx;

                    _partitionedPaths[pathPartitionIdx].Add(newPath);

                    foreach (var cell in path.Item1)
                    {
                        var cellIdx = new Vector3Int(cell.x, 0, cell.y);
                        pseudoGrid[cellIdx].pathPartitionIdx = pathPartitionIdx;
                        if (pseudoGrid[cellIdx].paths == null)
                        {
                            pseudoGrid[cellIdx].paths = new List<Path>();
                        }

                        pseudoGrid[cellIdx].paths.Add(newPath);

                        if (pseudoGrid[cellIdx].type != CellType.DoorDock) // don't convert door-dock to hallway
                        {
                            pseudoGrid[cellIdx].type = CellType.Hallway;
                        }
                        else
                        {
                            pseudoGrid[cellIdx].doorDockConnected = true;
                        }
                    }

                }
            }
            
            // check, that all door docks are connected
            // TODO: should investigate, why this happens sometimes
            foreach (var door in doorPartition)
            {
                if (!pseudoGrid[door].doorDockConnected)
                {
                    Debug.LogError("Could not connect all door docks!");
                    return false;
                }
            }

            pathPartitionIdx++;
        }

        foreach (var partition in _partitionedPaths)
        {
            foreach (var path in partition)
            {
                // set neighbors in path
                for (int j = 0; j < path.cells.Count; j++)
                {
                    var currentCell = path.cells[j];
                    if (_grid[currentCell.x, 0, currentCell.y].neighborsInPaths == null)
                    {
                        _grid[currentCell.x, 0, currentCell.y].neighborsInPaths = new HashSet<Vector3Int>();
                    }
                    if (j > 0)
                    {
                        var previousCell = path.cells[j - 1];
                        _grid[currentCell.x, 0, currentCell.y].neighborsInPaths
                            .Add(new Vector3Int(previousCell.x, 0, previousCell.y));
                        _grid[previousCell.x, 0, previousCell.y].neighborsInPaths
                            .Add(new Vector3Int(currentCell.x, 0, currentCell.y));
                    }

                    if (j < path.cells.Count - 1)
                    {
                        var nextCell = path.cells[j + 1];
                        if (_grid[nextCell.x, 0, nextCell.y].neighborsInPaths == null)
                        {
                            _grid[nextCell.x, 0, nextCell.y].neighborsInPaths = new HashSet<Vector3Int>();
                        }
                        _grid[currentCell.x, 0, currentCell.y].neighborsInPaths
                            .Add(new Vector3Int(nextCell.x, 0, nextCell.y));
                        _grid[nextCell.x, 0, nextCell.y].neighborsInPaths
                            .Add(new Vector3Int(currentCell.x, 0, currentCell.y));
                    }
                }
            }
        }

        return true;
    }

    List<Vector3> GetDoorTransformsOnSideOfBarrier(Room room, bool beforeBarrier)
    {
        var barrierMarker = room.GetFirstStoryMarker();
        var barrierDir = barrierMarker.GetBarrierDirection();
        var barrierLoc = barrierMarker.transform.position;

        var doorMarkers = room.GameObject.GetComponentsInChildren<DoorMarker>();
        List<Vector3> doorCellsOnSide = new List<Vector3>();
        foreach (var marker in doorMarkers)
        {
            var markerPos = marker.transform.position;
            var diff = markerPos - barrierLoc;
            var dot = Vector3.Dot(barrierDir, diff);
            if (dot > 0 && !beforeBarrier)
            {
                doorCellsOnSide.Add(markerPos);
            } 
            else if (dot < 0 && beforeBarrier)
            {
                doorCellsOnSide.Add(markerPos);
            }
        }
        return doorCellsOnSide;
    }

    // This requires cells of room to be initialized
    List<Vector3Int> GetDoorCellsOnSideOfBarrier(Room room, bool beforeBarrier)
    {
        var barrierMarker = room.GetFirstStoryMarker();
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

    #region corridor placement

    // dir must be an adjacent cell
    public static Direction GetDirectionRelativeToCell(Vector2Int relativeTo, Vector2Int other)
    {
        if (relativeTo.x < other.x)
        {
            return Direction.Right;
        }
        if (relativeTo.x > other.x)
        {
            return Direction.Left;
        }
        if (relativeTo.y < other.y)
        {
            return Direction.Up;
        }
        if (relativeTo.y > other.y)
        {
            return Direction.Down;
        }
        // same?
        return Direction.None;
    }

    private Direction InvertDirection(Direction dir)
    {
        if (dir.HasFlag(Direction.Right))
        {
            return Direction.Left;
        }
        if (dir.HasFlag(Direction.Left))
        {
            return Direction.Right;
        }
        if (dir.HasFlag(Direction.Up))
        {
            return Direction.Down;
        }
        if (dir.HasFlag(Direction.Down))
        {
            return Direction.Up;
        }
        return Direction.None;
    }

    public void PlaceCorridors()
    {
        _cellPathData = new CellPathData[_gridDimensions.x, _gridDimensions.z];
        Vector2Int previousCell;
        foreach (var pathsOfPartition in _partitionedPaths)
        {
            foreach (var path in pathsOfPartition)
            {
                // start door dock
                var startDoorDock = path.cells.First();
                var door = _grid[startDoorDock.x, 0, startDoorDock.y].doorCellOfDoorDock;
                var door2Int = new Vector2Int(door.x, door.z);

                _cellPathData[startDoorDock.x, startDoorDock.y].incomingDirections |= GetDirectionRelativeToCell(startDoorDock, door2Int);
                _cellPathData[startDoorDock.x, startDoorDock.y].doorDock = true;

                previousCell = startDoorDock;
                for (int i = 1; i < path.cells.Count; i++)
                {
                    var cell = path.cells[i];

                    // where do we come from, where is the previous cell located relative to current cell?
                    var direction = GetDirectionRelativeToCell(cell, previousCell); 
                    _cellPathData[cell.x, cell.y].incomingDirections |= direction;

                    // set outgoing direction of previous cell (incoming direction of this cell is the inverse of the outgoing
                    // of the last one)
                    _cellPathData[previousCell.x, previousCell.y].incomingDirections |= InvertDirection(direction);

                    previousCell = cell;
                }

                // end door dock, get relative door position
                var endDoorDock = path.cells.Last();
                door = _grid[endDoorDock.x, 0, endDoorDock.y].doorCellOfDoorDock;
                door2Int = new Vector2Int(door.x, door.z);

                _cellPathData[endDoorDock.x, endDoorDock.y].incomingDirections |= GetDirectionRelativeToCell(endDoorDock, door2Int);
                _cellPathData[endDoorDock.x, endDoorDock.y].doorDock = true;
            }
        }

        // convert incoming directions to corridor type
        // TODO: directly place module at location
        for (int x = 0; x < _gridDimensions.x; x++)
        {
            for (int z = 0; z < _gridDimensions.z; z++)
            {
                switch (_cellPathData[x,z].incomingDirections)
                {
                    case Direction.None:
                        break;
                    case Direction.Up | Direction.Down:
                        _cellPathData[x, z].type = NeighborPattern.AboveBelow;
                        _cellPathData[x, z].rotationInDeg = 0.0f;
                        break;
                    case Direction.Left | Direction.Right:
                        _cellPathData[x, z].type = NeighborPattern.AboveBelow;
                        _cellPathData[x, z].rotationInDeg = 90.0f;
                        break;
                    case Direction.Down | Direction.Left | Direction.Right:
                        _cellPathData[x, z].type = NeighborPattern.BelowBothSides;
                        _cellPathData[x, z].rotationInDeg = 0.0f;
                        break;
                    case Direction.Down | Direction.Left | Direction.Up:
                        _cellPathData[x, z].type = NeighborPattern.BelowBothSides;
                        _cellPathData[x, z].rotationInDeg = 90.0f;
                        break;
                    case Direction.Up | Direction.Left | Direction.Right:
                        _cellPathData[x, z].type = NeighborPattern.BelowBothSides;
                        _cellPathData[x, z].rotationInDeg = 180.0f;
                        break;
                    case Direction.Up | Direction.Right | Direction.Down:
                        _cellPathData[x, z].type = NeighborPattern.BelowBothSides;
                        _cellPathData[x, z].rotationInDeg = 270.0f;
                        break;
                    case Direction.Down | Direction.Left:
                        _cellPathData[x, z].type = NeighborPattern.LeftBelow;
                        _cellPathData[x, z].rotationInDeg = 0.0f;
                        break;
                    case Direction.Up | Direction.Left:
                        _cellPathData[x, z].type = NeighborPattern.LeftBelow;
                        _cellPathData[x, z].rotationInDeg = 90.0f;
                        break;
                    case Direction.Up | Direction.Right:
                        _cellPathData[x, z].type = NeighborPattern.LeftBelow;
                        _cellPathData[x, z].rotationInDeg = 180.0f;
                        break;
                    case Direction.Down | Direction.Right:
                        _cellPathData[x, z].type = NeighborPattern.LeftBelow;
                        _cellPathData[x, z].rotationInDeg = 270.0f;
                        break;
                    case Direction.Down | Direction.Up | Direction.Right | Direction.Left:
                        _cellPathData[x, z].type = NeighborPattern.AllSides;
                        _cellPathData[x, z].rotationInDeg = 0.0f;
                        break;
                    default:
                        break;
                }
                var cell = _cellPathData[x, z];
                InstantiateCorridorTile(new Vector2Int(x,z), cell.type, cell.rotationInDeg);
            }
        }
    }

    public static void DirectionsToNeighborPattern(Direction direction, out NeighborPattern type, out float rotationInDeg)
    {
        type = NeighborPattern.None;
        rotationInDeg = 0.0f;
        switch (direction)
        {
            case Direction.None:
                break;
            case Direction.Up | Direction.Down:
                 type = NeighborPattern.AboveBelow;
                 rotationInDeg = 0.0f;
                break;
            case Direction.Left | Direction.Right:
                 type = NeighborPattern.AboveBelow;
                 rotationInDeg = 90.0f;
                break;
            case Direction.Down | Direction.Left | Direction.Right:
                 type = NeighborPattern.BelowBothSides;
                 rotationInDeg = 0.0f;
                break;
            case Direction.Down | Direction.Left | Direction.Up:
                 type = NeighborPattern.BelowBothSides;
                 rotationInDeg = 90.0f;
                break;
            case Direction.Up | Direction.Left | Direction.Right:
                 type = NeighborPattern.BelowBothSides;
                 rotationInDeg = 180.0f;
                break;
            case Direction.Up | Direction.Right | Direction.Down:
                 type = NeighborPattern.BelowBothSides;
                 rotationInDeg = 270.0f;
                break;
            case Direction.Down | Direction.Left:
                 type = NeighborPattern.LeftBelow;
                 rotationInDeg = 0.0f;
                break;
            case Direction.Up | Direction.Left:
                 type = NeighborPattern.LeftBelow;
                 rotationInDeg = 90.0f;
                break;
            case Direction.Up | Direction.Right:
                 type = NeighborPattern.LeftBelow;
                 rotationInDeg = 180.0f;
                break;
            case Direction.Down | Direction.Right:
                 type = NeighborPattern.LeftBelow;
                 rotationInDeg = 270.0f;
                break;
            case Direction.Down | Direction.Up | Direction.Right | Direction.Left:
                 type = NeighborPattern.AllSides;
                 rotationInDeg = 0.0f;
                break;
            case Direction.Down:
                type = NeighborPattern.OneSide;
                break;
            case Direction.Left:
                type = NeighborPattern.OneSide;
                rotationInDeg = 90.0f;
                break;
            case Direction.Up:
                type = NeighborPattern.OneSide;
                rotationInDeg = 180.0f;
                break;
            case Direction.Right:
                type = NeighborPattern.OneSide;
                rotationInDeg = 270.0f;
                break;
            default:
                break;
        }
    }

    private void InstantiateCorridorTile(Vector2Int cell, NeighborPattern type, float rotationInDeg)
    {
        var placementCell = new Vector3Int(cell.x, 0, cell.y);
        var rotationOffset = GetRotationRelatedCellOffset(rotationInDeg);

        var location = (placementCell + rotationOffset) * CellSize;

        GameObject corridorTemplate = null;
        switch (type)
        {
            case NeighborPattern.None:
                return;
            case NeighborPattern.AboveBelow:
                corridorTemplate = StraigtCorridor;
                break;
            case NeighborPattern.LeftBelow:
                corridorTemplate = LeftTurnCorridor;
                break;
            case NeighborPattern.AllSides:
                corridorTemplate = CrossCorridor;
                break;
            case NeighborPattern.BelowBothSides:
                corridorTemplate = TCrossCorridor;
                break;
        }

        var go = Instantiate(
            corridorTemplate,
            new Vector3(location.x, 0, location.z),
            Quaternion.Euler(new Vector3(0, rotationInDeg, 0)),
            this.transform
            );
        _instantiatedCorridors.Add(go);
        if (_grid[placementCell].type != CellType.DoorDock)
        {
            _grid[placementCell].type = CellType.Hallway;
        }
    }

    #endregion

    #region setup
    public void Setup()
    {
        Cleanup();
        ReadRooms();
    }

    public void CreateGrid(Vector3Int gridDimensions)
    {
        _grid = new DungeonGrid<Cell>(gridDimensions, Vector3Int.zero);

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

        for (int x = 0; x < _grid.Size.x; x++)
        {
            for (int z = 0; z < _grid.Size.z; z++)
            {
                _grid[x, 0, z].roomIdx = -1;
            }
        }

        _triedCells = new HashSet<Vector2Int>();
    }


    public void ReadRooms()
    {
        _instantiatedRooms = new Dictionary<int, Room>();
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

    public void CreateMinimap()
    {
        if (Minimap == null)
        {
            Debug.Log("No minimap");
            return;
        }
        Minimap.CreateMinimap(_grid, _gridDimensions, _playerInstance, CellSize, _cellPathData, this);
    }
    #endregion

    #region cleanup
    public void Cleanup(bool cleanupRoomTemplates = true)
    {
        if (cleanupRoomTemplates)
        {
            _roomTemplates.Clear();
        }
        _grid = null;
        DestroyRooms();
        DestroyCorridors();
        _delaunays = new List<Delaunay2D>();
        _partitions = new List<Partition>();
        _doorPartitions = new List<List<Vector3Int>>();
        _msts = new List<List<Prim.Edge>>();
        _partitionedPaths = new List<List<Path>>();
        _irrelevantRoomAssignment = new Dictionary<int, List<int>>();
        _partitionDims = new List<Vector2Int>();
        _partitionDimensionRanges = new List<PartitionDimensionRange>();
        
        DestroyPlayer();
        if (Minimap != null)
        {
            Minimap.Cleanup();
        }
    }

    private void DestroyPlayer()
    {
        if (null != _playerInstance)
        {
            if (Application.isEditor)
            {
                DestroyImmediate(_playerInstance);
            } 
            else
            {
                Destroy(_playerInstance);
            }
        }
    }

    private void DestroyCorridors()
    {
        if (null != _instantiatedCorridors)
        {
            foreach (var corridor in _instantiatedCorridors)
            {
                if (Application.isEditor)
                {
                    DestroyImmediate(corridor);
                } 
                else
                {
                    Destroy(corridor);
                }
            }
        }
        _instantiatedCorridors = new List<GameObject>();
    }

    private void DestroyRooms()
    {
        if (null != _instantiatedRooms)
        {
            foreach (var room in _instantiatedRooms)
            {
                if (Application.isEditor)
                {
                    DestroyImmediate(room.Value.GameObject);
                } 
                else
                {
                    Destroy(room.Value.GameObject);
                }
            }
        }
        _instantiatedRooms = new Dictionary<int, Room>();
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
        if (UseExternalEntropy)
        {
            long seed = GetExternalSeed();
            Random.InitState((int)seed);
            Debug.Log("Using seed " + seed + " for placement");
        }
        else 
        {
            Random.InitState((int)RandomSeed);
        }
        
        _partitions = PartitionRooms(_roomTemplates.ToArray());
        _irrelevantRoomAssignment = AssignNonRelevantRoomsToPartition(_partitions);
        
        // print partitions
        for (int i = 0; i < _partitions.Count; i++)
        {
            Debug.LogWarning("Printing partition: " + i);
            for (int idx = _partitions[i].StartIdxInclusive; idx < _partitions[i].EndIdxExclusive; idx++)
            {
                Debug.LogWarning("room: " + _roomTemplates[idx].GameObject.name);
            }

            if (_irrelevantRoomAssignment.TryGetValue(i, out var roomList))
            {
                foreach (var idx in roomList)
                {
                    Debug.LogWarning("room: " + _roomTemplates[idx].GameObject.name + " (assigned irrelevant room)");
                }
            }
        }
        
        int count = 1;
        while (!PlaceRooms(_partitions) && count <= MaxTotalPlacementTries) count++;
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

    class Rect
    {
        Vector2Int lowerLeftCorner;
        Vector2Int upperRightCorner;
        List<Vector2Int> buffer;
        List<Vector2Int> cells;
    }

    private bool PlaceRoomMultipleTries(int roomIdx, Vector2Int rangeLow, Vector2Int rangeHigh)
    {
        bool success = false;
        int iterationCount = 0;
        var room = _roomTemplates[roomIdx];
        Vector2Int cell;
        do
        {
            // get random position
            int x = Mathf.FloorToInt(Random.Range(rangeLow.x, rangeHigh.x));
            int y = Mathf.FloorToInt(Random.Range(rangeLow.y, rangeHigh.y));

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
                success = PlaceRoom(roomIdx, cell, rotation);
            }
            iterationCount++;
            
            if (!success && iterationCount > MaxPlacementTriesPerRoom)
            {
                Debug.LogError("Could not place room: " + room.GameObject.name + ", to many tries");
                return false;
            }
            // revise condition
        } while (!success /*&& _triedCells.Count < _grid.Size.x * _grid.Size.z * 0.8f*/ && iterationCount <= MaxPlacementTriesPerRoom);

        return success;
    }
    
    // select random cell within gird-size with border = 2 cells (to ensure,
    // that doors always can be connected on each side
    // ensure, that rooms are not directly placed besides each other (marked bufferzone in
    // between)
    // ensure, that room is completely placed within borders -> use buffer zone

    // TODO: do partitioning of rooms before placement and create sectors of grid, 
    // in which most of the rooms of the same partition are placed, to create more stringent
    // path creation
    
    // TODO: create partitions of grid
    // How to determine partition dimensions and locations?
    // this basically is the packing problem
    // 
    // Need to "rectangularize" the room-footprints (account for buffer zone)
    // place rooms (from left to right, bottom to top), placing all rectangles of one room at a time
    // -> dimensions of partition based on heuristic, which should take in to account the dimensions of all rooms in the partition
    // -> scan from left to right to find suitable place for putting a room
    // then place the partitions themselves on grid using same algorithm
    
    // how to rectangularize?
    // get occupied cells
    // scan from left to right -> break rectangle, if we meet free cell
    
    // or just brute force it?
    // TODO: place room with barrier at lowest possible position in partition (with door before barrier at the bottom
    public bool PlaceRooms(List<Partition> partitions)
    {
        _partitionDimensionRanges = new List<PartitionDimensionRange>();
        
        Vector2Int accumTotalGridDim = Vector2Int.zero;
        _partitionDims = new List<Vector2Int>();
        
        for (int n = 0; n < partitions.Count; n++)
        {
            var partition = partitions[n];
            if (!partition.IsRelevantForStory) continue;

            Vector2 accumPartitionDim = Vector2.zero;
            // calculate partition dimensions
            // just put each partition on top of the other
            for (int j = partition.StartIdxInclusive; j < partition.EndIdxExclusive; j++)
            {
                // get mesh-extents
                var room = _roomTemplates[j];
                var actualExtents = room.MeshExtents() * 2;

                accumPartitionDim.x += (actualExtents.x / CellSize) * BruteForcePartitionSizeModifierX;  
                accumPartitionDim.y += (actualExtents.z / CellSize) * BruteForcePartitionSizeModifierZ; // not a typo
            }
            
            if (n == partitions.Count - 1)
            {
                accumPartitionDim *= BruteForcePartitionSizeModifierLastPartition;
            }

            var accumInt = Vector2Int.CeilToInt(accumPartitionDim);
            _partitionDims.Add(accumInt);
            accumTotalGridDim.x = Mathf.Max(accumInt.x, accumTotalGridDim.x);
            accumTotalGridDim.y += accumInt.y;
        }

        _gridDimensions = new Vector3Int(accumTotalGridDim.x + OuterBufferZone, 1, accumTotalGridDim.y + OuterBufferZone);
        CreateGrid(_gridDimensions);
        UnmarkTemplatesForInstantiation();

        int i;
        int roomsSkipped = 0;
        bool success = true;
        var partitionDimEnumerator = _partitionDims.GetEnumerator();
        partitionDimEnumerator.MoveNext();
        int lastDimZ = OuterBufferZone;
        
        // TODO: add tries per partition
        for (int n = 0; n < partitions.Count; n++)
        {
            var partition = partitions[n];
            if (!partition.IsRelevantForStory)
            {
                // add this, so the visualized partition range matches the actual instantiated partition
                var dummyRange = new PartitionDimensionRange();
                dummyRange.lowBound = Vector2Int.zero;
                dummyRange.highBound = Vector2Int.zero;
                _partitionDimensionRanges.Add(dummyRange);
                continue;
            }
            
            var dim = partitionDimEnumerator.Current;

            Vector2Int rangeLow = new Vector2Int(OuterBufferZone, lastDimZ + 1);
            Vector2Int rangeHigh = new Vector2Int(accumTotalGridDim.x, lastDimZ + dim.y);

            // for debugging / visualization purposes
            PartitionDimensionRange range = new PartitionDimensionRange();
            range.lowBound = rangeLow;
            range.highBound = rangeHigh;
            _partitionDimensionRanges.Add(range);
            
            // TODO: find room with barrier in it, if there is one (will probably be the first one)
            
            for (i = partition.StartIdxInclusive; i < partition.EndIdxExclusive; i++)
            {
                var room = _roomTemplates[i];
                bool placedRoom = false;
                if (_roomTemplates[i].HasBarrier()) // force position an rotation
                {
                    // get cell 
                    Vector2Int cell = new Vector2Int(rangeLow.x, rangeLow.y);
                    Vector3 rotation = Vector3.zero;
                    var doorMarkers = room.GetDoorMarkers();
                    var doorBeforeBarrier = doorMarkers.Where(marker => !marker.BeforeBarrier()).First();
                    if (doorBeforeBarrier != null)
                    {
                        var meshCenter= room.GetMeshCenter();
                        var doorPosition = doorBeforeBarrier.transform.position;
                        
                        if (doorPosition.x > meshCenter.x && doorPosition.z > meshCenter.z) // flip 180 deg
                        {
                            rotation.y = 180;
                        }
                        else if (doorPosition.x > meshCenter.x && doorPosition.z < meshCenter.z) // flip 90 deg
                        {
                            rotation.y = 90;
                        }
                        else if (doorPosition.x < meshCenter.x && doorPosition.z < meshCenter.z) // don't flip 
                        {
                            ;
                        }
                        else if (doorPosition.x < meshCenter.x && doorPosition.z > meshCenter.z) // flip 270 deg
                        {
                            rotation.y = 270;
                        }
                        for (int offset = 0; offset < dim.x; offset++)
                        {
                            if (PlaceRoom(i, cell + new Vector2Int(offset, 0), rotation))
                            {
                                placedRoom = true;
                                break;
                            }
                        }
                    }
                }
                if (placedRoom)
                {
                    success = true;
                }
                else
                {
                    success &= PlaceRoomMultipleTries(i, rangeLow, rangeHigh);
                }
                if (!success) break;
            }

            if (success)
            {
                if (_irrelevantRoomAssignment.TryGetValue(n, out var rooms))
                {
                    foreach (var roomIdx in rooms)
                    {
                        success &= PlaceRoomMultipleTries(roomIdx, rangeLow, rangeHigh);
                        if (!success) break;
                    }
                }
            }
            
            if (!success)
            {
                Debug.LogError(
                        "Could not place all rooms, tried cells: " + 
                        _triedCells.Count.ToString() + 
                        ", skipped rooms: " + 
                        roomsSkipped
                        );
                //CreateGrid(Vector3Int.one * OuterBufferZone);
                UnmarkTemplatesForInstantiation();
                return false;
            }

            lastDimZ += dim.y; // not a typo
            partitionDimEnumerator.MoveNext();
        }

        InstantiateRooms();
        return true;
    }

    private bool PlaceRoom(int roomIdx, Vector2Int cell, Vector3 rotation) // cell: Zellenkoordinaten
    { 
        var room = _roomTemplates[roomIdx];
        var occupiedCells = CalculateOccupiedCells(room, cell, rotation);

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
                    foreach (var direction in GridDirections)
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
            Vector3Int offset = GetRotationRelatedCellOffset(placementCandidate.Rotation.y);
            offset = offset * CellSize;

            var go = Instantiate(
                room.GameObject,
                new Vector3(location.x, 0, location.y) + offset,
                Quaternion.Euler(placementCandidate.Rotation),
                this.transform
                );

            int instantiationIdx = _instantiatedRooms.Count;
            go.GetComponent<RoomMarker>().InstantiationIndex = instantiationIdx;

            Room instantiatedRoom = new Room(go, instantiationIdx);
            
            // map instantiated room to templateIdx (the idx used by the partitioning)
            _instantiatedRooms.Add(placementCandidate.TemplateIdx, instantiatedRoom);

            foreach (var occupiedCell in placementCandidate.OccupiedCells)
            {
                // TODO: I think this is really brittle and needs to be improved
                _grid[occupiedCell].roomIdx = placementCandidate.TemplateIdx;
                _instantiatedRooms[placementCandidate.TemplateIdx].AssociatedCells.Add(occupiedCell);
            }
        }
        //_instantiatedRooms.Sort(new RoomComparer());
    }

    public void GenerateDungeon()
    {
        bool success = false;
        int currentTries = 0;
        ReadRooms();
        while (!success && currentTries < MaxDungeonTries)
        {
            Cleanup(cleanupRoomTemplates: false);

            if (!SudoPlaceRooms())
            {
                currentTries++;
                continue;
            }
            if (!FindPaths())
            {
                currentTries++;
                continue;
            }

            PlaceCorridors();
            InstantiatePlayer();
            _generationSuccessfull = true;
            
            if (Minimap == null)
            {
                Debug.LogError("Minimap object is null");
            } 
            else
            {
                Minimap.CreateMinimap(_grid, _gridDimensions, _playerInstance, CellSize, _cellPathData, this);
            }
            success = true;
        }
        if (currentTries >= MaxDungeonTries)
        {
            Debug.LogError("Could not generate dungeon, maybe the grid should be expanded");
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
    List<Vector3Int> CalculateOccupiedCells
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

    public List<Vector3Int> GetAssociatedCellsOfRoom(int roomIdx)
    {
        if (roomIdx > _instantiatedRooms.Count)
        {
            Debug.Log("roomIdx is bigger than room count");
            return new List<Vector3Int>();
        }

        return _instantiatedRooms[roomIdx].AssociatedCells;
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
                foreach (var dir in GridDirections)
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

    // TODO: implement this, when concept of room replacement is validated
    private List<Vector3Int> GetDoorCellsOfRoomTemplates(Room room, Vector2Int placementCell, Vector3 rotationOfRoom, bool overwriteCellType = false)
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
            // TODO: modify this transformation to account for placement cell and rotation
            var scaledPos = (roomPosition + roomRotation * markerLocalPos) / CellSize;
            var gridCell = Vector3Int.FloorToInt(scaledPos);

            // the doorCell must be an occupied room cell or another door
            if (_grid[gridCell].type != CellType.Room &&
                _grid[gridCell].type != CellType.Door)
            {
                // if it is not, select nearest room-cell
                foreach (var dir in GridDirections)
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


    public List<Vector3Int> GetDoorDockCellsRawCoords(List<Vector3Int> doorCellCoords)
    {
        List<Vector3Int> doorDockCells = new List<Vector3Int>();
        foreach (var coord in doorCellCoords)
        {
            var coords = GetDoorDockCells(new List<Vector3Int> {GlobalToCellIdx(coord)});
            foreach (var doorCellCoord in coords)
            {
                doorDockCells.Add(Vector3Int.RoundToInt(CellIdxToGlobal(doorCellCoord)));
            }
        }

        return doorDockCells;
    }

    private List<Vector3Int> GetDoorDockCells(List<Vector3Int> doorCells)
    {
        List<Vector3Int> doorDockCells = new List<Vector3Int>();
        foreach (var doorCell in doorCells)
        {
            var doorMarkers = _grid[doorCell].doorMarkers;
            if (doorMarkers == null)
            {
                return null;
            }
            
            foreach (var marker in doorMarkers)
            {
                var markerDirection = marker.gameObject.transform.forward;
                var doorDockCell = doorCell - Vector3Int.RoundToInt(markerDirection);
                doorDockCells.Add(doorDockCell);

                _grid[doorDockCell].doorCellOfDoorDock = doorCell;
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

    public static Vector3Int GetRotationRelatedCellOffset(float rotation)
    {
        int rot = Mathf.RoundToInt(rotation);
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

    public static Vector3Int GlobalToCellIdx(Vector3 globalCoord, int CellSize)
    {
        var downScaled = globalCoord / CellSize;
        //return Vector3Int.RoundToInt(downScaled);
        return Vector3Int.FloorToInt(downScaled);
    }

    // using own cell size
    public Vector3Int GlobalToCellIdx(Vector3 globalCoord)
    {
        var downScaled = globalCoord / CellSize;
        //return Vector3Int.RoundToInt(downScaled);
        return Vector3Int.FloorToInt(downScaled);
    }

    // calculate the center of the cell
    public Vector3 CellIdxToGlobal(Vector3Int cell)
    {
        // this should account for transform of parent 
        var coord = cell * CellSize + new Vector3Int(1,0,1) * CellSize / 2;
        return coord; //?
    }
    #endregion

    #region Visualization

    private void DrawCellMarking(Vector3 cellLocation, Color color)
    {
        var lowerCorner = cellLocation * CellSize;
        var upperCorner = (cellLocation + new Vector3(1, 0, 1)) * CellSize;
        Debug.DrawLine(lowerCorner, upperCorner, color);
    }
    
    private void DrawCellMarking(Cell cell, Vector3 cellLocation)
    {
        if (CellType.Room == cell.type)
        {
            DrawCellMarking(cellLocation, Color.red);
        } 
        else if (CellType.Bufferzone == cell.type)
        {
            DrawCellMarking(cellLocation, Color.blue);
        }
        else if (CellType.Door == cell.type)
        {
            DrawCellMarking(cellLocation, Color.cyan);
        }
        else if (CellType.Hallway == cell.type)
        {
            DrawCellMarking(cellLocation, Color.green);
        }
        else if (CellType.DoorDock == cell.type)
        {
            DrawCellMarking(cellLocation, Color.magenta);
        }
    }

    private void OnDrawGizmos()
    {
        if (null != _grid && ShowGridNeighbors)
        {
            if (GridCoordsForNeighbor.x < _gridDimensions.x && GridCoordsForNeighbor.y < _gridDimensions.z)
            {
                DrawCellMarking(new Vector3(GridCoordsForNeighbor.x, 0, GridCoordsForNeighbor.y), Color.magenta);
                var neighbors = _grid[GridCoordsForNeighbor.x, 0, GridCoordsForNeighbor.y].neighborsInPaths;
                if (neighbors != null)
                {
                    foreach (var cell in neighbors)
                    {
                        DrawCellMarking(cell, Color.blue);
                    }
                }
            }
        }
        
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


#if UNITY_EDITOR_WIN
        if (null != _partitions && PartitionIdxToShow < _partitions.Count && ShowPartition)
        {
            var partition = _partitions[PartitionIdxToShow];
            for (int i = partition.StartIdxInclusive; i < partition.EndIdxExclusive; i++)
            {
                var room = _instantiatedRooms[i];
                Handles.DrawWireDisc(room.GetMeshCenter(), room.GameObject.transform.up, 10, 10);
            }
            
            if (_irrelevantRoomAssignment.TryGetValue(PartitionIdxToShow, out var roomList))
            {
                foreach (var idx in roomList)
                {
                    var room = _instantiatedRooms[idx];
                    Handles.DrawWireDisc(room.GetMeshCenter(), room.GameObject.transform.up, 10, 10);
                }
            }
        }
#endif
        
        if (null != _partitionDimensionRanges && PartitionIdxToShow < _partitionDimensionRanges.Count && ShowPartition)
        {
            var range = _partitionDimensionRanges[PartitionIdxToShow];
            // draw rectangle
            
            // bottom line
            Debug.DrawLine(new Vector3(range.lowBound.x * CellSize, 0, range.lowBound.y * CellSize), new Vector3(range.highBound.x * CellSize, 0, range.lowBound.y * CellSize), Color.magenta); 
            // left side 
            Debug.DrawLine(new Vector3(range.lowBound.x * CellSize, 0, range.lowBound.y * CellSize), new Vector3(range.lowBound.x * CellSize, 0, range.highBound.y * CellSize), Color.magenta); 
            // right side 
            Debug.DrawLine(new Vector3(range.highBound.x * CellSize, 0, range.lowBound.y * CellSize), new Vector3(range.highBound.x * CellSize, 0, range.highBound.y * CellSize), Color.magenta); 
            // top line
            Debug.DrawLine(new Vector3(range.lowBound.x * CellSize, 0, range.highBound.y * CellSize), new Vector3(range.highBound.x * CellSize, 0, range.highBound.y * CellSize), Color.magenta); 
        }

        if (null != _doorPartitions && DoorPartitionIdxToShow < _doorPartitions.Count && ShowDoorPartition)
        {
#if UNITY_EDITOR_WIN
            var partition = _doorPartitions[DoorPartitionIdxToShow];
            foreach (var cell in partition)
            {
                var trans = cell * CellSize + new Vector3Int(1,0,1) * CellSize / 2;
                Handles.DrawSolidDisc(trans, Vector3.up, 2);
            }
#endif

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
                        Color.cyan
                        );
                }
            }

            // only show paths for current partition
            if (null != _partitionedPaths && ShowPaths)
            {
                var pathsForPartition = _partitionedPaths[DoorPartitionIdxToShow];
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
