using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using UnityEngine;
using UnityEngine.Android;
using static DungeonGenerator;

// Questionmark
public class Minimap : MonoBehaviour
{
    [SerializeField] private bool SetAllInactive;
    
    [Header("Minimap Tileprefabs")]
    [SerializeField]
    GameObject TilePrefabStandard;

    [SerializeField]
    GameObject TilePrefabLTurn;

    [SerializeField]
    GameObject TilePrefabStraight;

    [SerializeField]
    GameObject TilePrefabTCross;

    [SerializeField]
    GameObject TilePrefabCrossing;

    [SerializeField]
    GameObject TilePrefabRoomLowerLeftCorner;

    [SerializeField]
    GameObject TilePrefabRoomUShape;

    [SerializeField]
    GameObject TilePrefabRoomOneSide;

    [SerializeField] 
    GameObject TilePrefabQuestionMark;

    [Header("Options")] [SerializeField] bool enableWayfinding;

    GameObject _toFollow;
    DungeonGrid<Cell> _dungeonGrid;
    CellPathData[,] _cellPathData;

    // this needs to be enhanced.. or converted to array with the same dimensions as the grid..
    GameObject[,] _instantiatedTiles;
    int _cellSize;

    int _prevRoomIdx = -1;
    Vector3Int _prevCellIdx = Vector3Int.zero;
    private int _prevStoryMarkerIdx = -1;

    HashSet<int> _seenRooms;
    private HashSet<Vector3Int> _seenCells;
    private Dictionary<Vector3Int, GameObject> _markedWithQuestionMark;

    DungeonGenerator _generator;

    private bool _initialized;
    private bool _extendedVariation;

    private Wayfinder _wayfinder;
    private LineRenderer _lineRenderer;
    private StoryManager _storyManager;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_initialized)
        {
            GetCurrentLocation(out var roomIdx, out var cellIdx);
            UpdateVisibility(roomIdx, cellIdx);
            
            // TODO: this should not be depentent on the minimap
            if (_prevCellIdx != cellIdx && _dungeonGrid[cellIdx].type == CellType.Door)
            {
                var marker = _dungeonGrid[cellIdx].doorMarkers.First();
                EventManager.TriggerEvent("Level/PassDoorMarker", marker);
            }
            
            if (enableWayfinding)
            {
                int storyMarkerTargetIdx = _storyManager.CurrentStoryMarker.IndexInStory;
                if (_prevStoryMarkerIdx != storyMarkerTargetIdx ||
                    cellIdx != _prevCellIdx)
                {
                    UpdateWayToTarget(storyMarkerTargetIdx);
                }
                _prevStoryMarkerIdx = storyMarkerTargetIdx;
            }
            else if (_lineRenderer.positionCount > 0)
            {
                _lineRenderer.positionCount = 0;
            }
            _prevRoomIdx = roomIdx;
            _prevCellIdx = cellIdx;
        }
    }
    
    private void UpdateWayToTarget(int storyMarkerTargetIdx)
    {
        if (_wayfinder == null) return;
        if (_lineRenderer == null) return;
        var path = _wayfinder.FindWay(storyMarkerTargetIdx);
        
        if (path.cells.Count > 0)
        {
            _lineRenderer.positionCount = path.cells.Count;
            Vector3 pos;
            for (int i = 0; i < path.cells.Count; i++)
            {
                // TODO: offset to minimap
                pos = new Vector3(path.cells[i].x, 0, path.cells[i].y) + new Vector3(1,1,1) / 2;
                pos += transform.position;
                _lineRenderer.SetPosition(i, pos);
            }
        }
    }

    private void GetCurrentLocation(out int roomIdx, out Vector3Int cellIdx)
    {
        cellIdx = Vector3Int.zero;
        roomIdx = -1;
        if (_toFollow == null) return;
        if (_generator == null) return;

        // check, which parts of the minimap to uncover based on player location
        var pos = _toFollow.transform.position;
        cellIdx = GlobalToCellIdx(pos, _cellSize);

        roomIdx = _dungeonGrid[cellIdx].roomIdx;
    }

    public void UpdateVisibility(int roomIdx, Vector3Int cellIdx)
    {
        if (roomIdx == -1 || _dungeonGrid[cellIdx].type == CellType.DoorDock)
        {
            if (_instantiatedTiles[cellIdx.x, cellIdx.z] != null)
            {
                // mark not-uncovered cells with questionmark
                //_instantiatedTiles[cellIdx.x, cellIdx.z].gameObject.SetActive(true);
                UncoverCell(cellIdx);
                
            }
        }
        else if (_dungeonGrid[cellIdx].type != CellType.DoorDock)
        {
            if (!_seenRooms.Contains(roomIdx))
            {
                // mark not-uncovered cells with questionmark -> door docks?
                UncoverRoom(roomIdx);
                
            }
        }

        if (_prevRoomIdx != roomIdx)
        {
            Debug.LogWarning("player is now in room with idx: " + roomIdx);
        }
    }

    private void MarkCellWithQuestionMark(Vector3Int cell)
    {
        if (_markedWithQuestionMark.ContainsKey(cell)) return;
        
        var placementPosition = this.transform.position + cell + Vector3.up;
        var questionMarkTile = (Instantiate(TilePrefabQuestionMark, placementPosition, Quaternion.identity, this.transform));
        _markedWithQuestionMark.Add(cell, questionMarkTile);
    }

    private void UncoverCell(Vector3Int cell)
    {
        _instantiatedTiles[cell.x, cell.z].gameObject.SetActive(true);
        _seenCells.Add(cell);
        
        if (_markedWithQuestionMark.TryGetValue(cell, out var questionMarkTile))
        {
            _markedWithQuestionMark.Remove(cell);
            Destroy(questionMarkTile);
        }

        var neighborsInPaths = _dungeonGrid[cell].neighborsInPaths;
        if (neighborsInPaths != null)
        {
            foreach (var neighbor in neighborsInPaths)
            {
                if (!_seenCells.Contains(neighbor))
                {
                    MarkCellWithQuestionMark(neighbor);
                }
            }
        }
        
        if (_dungeonGrid[cell].type == CellType.DoorDock)
        {
            var doorCell = _dungeonGrid[cell].doorCellOfDoorDock;
            if (!_seenCells.Contains(doorCell))
            {
                MarkCellWithQuestionMark(doorCell);
            }
        }
    }

    private void UncoverRoom(int roomIdx)
    {
        // activate all tiles associated with room
        var cells = _generator.GetAssociatedCellsOfRoom(roomIdx);
        foreach (var cell in cells)
        {
            // _instantiatedTiles[cell.x, cell.z].gameObject.SetActive(true);
            UncoverCell(cell);

            if (_dungeonGrid[cell].type == CellType.Door)
            {
                var docks = _generator.GetDoorDockCells(new List<Vector3Int>{cell});
                foreach (var dock in docks)
                {
                    if (!_seenCells.Contains(dock))
                    {
                        MarkCellWithQuestionMark(dock);
                    }
                }
            }
        }
        _seenRooms.Add(roomIdx);
    }

    public void Cleanup()
    {
        if (_instantiatedTiles != null)
        {
            foreach (var tile in _instantiatedTiles)
            {
                if (Application.isEditor)
                {
                    DestroyImmediate(tile);
                } 
                else
                {
                    Destroy(tile);
                }
            }
        }
        _seenRooms = new HashSet<int>();
    }

    #region minimap generation
    public void CreateMinimap(DungeonGrid<Cell> dungeonGrid, Vector3Int GridDimensions, GameObject toFollow,
        int CellSize, CellPathData[,] cellPathData, DungeonGenerator generator, bool extendedVariation)
    {
        Cleanup();

        _instantiatedTiles = new GameObject[GridDimensions.x, GridDimensions.z];
        _toFollow = toFollow;
        _dungeonGrid = dungeonGrid;
        _cellPathData = cellPathData;
        _cellSize = CellSize;
        _generator = generator;
        _extendedVariation = extendedVariation;
        _seenCells = new HashSet<Vector3Int>();
        _markedWithQuestionMark = new Dictionary<Vector3Int, GameObject>();

        // implement variation of minimap
        SetAllInactive = !extendedVariation;
        enableWayfinding = extendedVariation;

        if (extendedVariation)
        {
            Debug.Log("Creating extended minimap");
        }
        else
        {
            Debug.Log("Creating basic minimap");
        }
        

        for (int x = 0; x < GridDimensions.x; x++)
        {
            for (int z = 0; z < GridDimensions.z; z++)
            {
                var cellType = dungeonGrid[x, 0, z].type;
                switch (cellType)
                {
                    case CellType.Room:
                    case CellType.Door:
                        PlaceMinimapTileRoom(new Vector3Int(x, 0, z));
                        break;
                    case CellType.Hallway:
                    case CellType.DoorDock:
                        PlaceMinimapTileHallway(new Vector3Int(x, 0, z));
                        break;
                    case CellType.Bufferzone:
                    case CellType.Free:
                        break;
                }
            }
        }

        var follower = this.GetComponent<MinimapPlayerFollower>();

        follower.FollowDeltaScaling = 1 / (float)CellSize;
        follower.ToFollow = _toFollow;
        
        _wayfinder = GetComponent<Wayfinder>();
        if (_wayfinder == null)
        {
            Console.WriteLine("Did not find Wayfinder");
            return;
        }
        else
        {
            _wayfinder.Init();
        }

        _lineRenderer = GetComponent<LineRenderer>();
        _storyManager = FindObjectOfType<StoryManager>();

        if (_lineRenderer == null)
        {
            Console.WriteLine("Did not find LineRenderer");
            return;
        }

        if (_storyManager == null)
        {
            Console.WriteLine("Did not find StoryManager");
            return;
        }

        _initialized = true;
    }

    private void PlaceMinimapTileRoom(Vector3Int cell)
    {
        // we know, that this is a room..
        // check, if straight neighbors are buffer zone or free

        Vector2Int cell2Int = new Vector2Int(cell.x, cell.z);

        // get the direction, in which lay the free neighbor-cells
        Direction freeNeighbors = Direction.None;
        for (int i = 0; i < 4; i++)
        {
            Vector3Int direction = GridDirections[i];
            Vector3Int neighborCell = cell + direction;

            Direction dirRelToCell = GetDirectionRelativeToCell(cell2Int, new Vector2Int(neighborCell.x, neighborCell.z));

            if (_dungeonGrid[neighborCell].type != CellType.Room &&
                _dungeonGrid[neighborCell].type != CellType.Door &&
                _dungeonGrid[neighborCell].type != CellType.DoorDock 
                )
            {
                freeNeighbors |= dirRelToCell;
            }
        }

        // neighbor pattern indicates location of free cells -> needed to use correct
        // tile prefab with correct border
        DirectionsToNeighborPattern(freeNeighbors, out NeighborPattern pattern, out float rotation);

        GameObject prefabToInstance = TilePrefabStandard;
        switch (pattern)
        {
            case NeighborPattern.None: // no free neighbors, use completely filled tile
                break;
            case NeighborPattern.AboveBelow: // use tile with borders on top an bottom (flip straight corridor tile by 90 deg)
                prefabToInstance = TilePrefabStraight;
                rotation += 90.0f;
                break;
            case NeighborPattern.LeftBelow: 
                prefabToInstance = TilePrefabRoomLowerLeftCorner;
                break;
            case NeighborPattern.BelowBothSides:
                prefabToInstance = TilePrefabRoomUShape;
                break;
            case NeighborPattern.OneSide:
                prefabToInstance = TilePrefabRoomOneSide;
                break;
            case NeighborPattern.AllSides: // this should not be a thing
                break;
        }

        var rotationOffset = GetRotationRelatedCellOffset(rotation);

        var placementPosition = this.transform.position + cell + rotationOffset;
        var go = (Instantiate(prefabToInstance, placementPosition, Quaternion.Euler(0, rotation, 0), this.transform));
        if (SetAllInactive)
        {
            go.SetActive(false);
        }
        _instantiatedTiles[cell.x, cell.z] = go;
    }

    // TODO: setup association between 'real' cell in level and the cell in the minimap
    // TODO: Create real minimap-class, which holds this data (and a reference to the grid?)
    public void PlaceMinimapTileHallway(Vector3Int cell)
    {
        var rotation = _cellPathData[cell.x, cell.z].rotationInDeg;
        var rotationOffset = GetRotationRelatedCellOffset(rotation);
        GameObject prefabToInstance = TilePrefabStandard;
        var type = _cellPathData[cell.x, cell.z].type;
        switch (type)
        {
            case NeighborPattern.None:
                break;
            case NeighborPattern.AboveBelow:
                prefabToInstance = TilePrefabStraight;
                break;
            case NeighborPattern.LeftBelow:
                prefabToInstance = TilePrefabLTurn;
                break;
            case NeighborPattern.AllSides:
                prefabToInstance = TilePrefabCrossing;
                break;
            case NeighborPattern.BelowBothSides:
                prefabToInstance = TilePrefabTCross;
                break;
        }

        var placementPosition = this.transform.position + cell + rotationOffset;
        var go = (Instantiate(prefabToInstance, placementPosition, Quaternion.Euler(0, rotation, 0), this.transform));
        if (SetAllInactive)
        {
            go.SetActive(false);
        }
        _instantiatedTiles[cell.x, cell.z] = go;
    }


    #endregion

    private void OnDrawGizmos()
    {
    }
}
