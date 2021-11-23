using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static DungeonGenerator;

public class Minimap : MonoBehaviour
{
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

    GameObject _toFollow;
    DungeonGrid<Cell> _dungeonGrid;
    CellPathData[,] _cellPathData;
    List<GameObject> _instantiatedTiles;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // check, which parts of the minimap to uncover based on player location
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
    }

    #region minimap generation
    public void CreateMinimap(DungeonGrid<Cell> dungeonGrid, Vector3Int GridDimensions, GameObject toFollow, int CellSize, CellPathData[,] cellPathData)
    {
        Cleanup();
        _instantiatedTiles = new List<GameObject>();
        _toFollow = toFollow;
        _dungeonGrid = dungeonGrid;
        _cellPathData = cellPathData;

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
        _instantiatedTiles.Add(Instantiate(prefabToInstance, placementPosition, Quaternion.Euler(0, rotation, 0), this.transform));
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
        _instantiatedTiles.Add(Instantiate(prefabToInstance, placementPosition, Quaternion.Euler(0, rotation, 0), this.transform));
    }
    #endregion

}
