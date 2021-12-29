using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BlueRaja;
using static DungeonGenerator;

public class Wayfinder : MonoBehaviour
{
    [SerializeField]
    int roomInstantiationIdx = 0;

    [SerializeField]
    int targetIdx = 8;

    [SerializeField] 
    bool overrideTargetIdx = false;

    internal void FindWay()
    {
        FindWay(targetIdx);
    }

    DungeonGenerator _gen;
    Player _player;
    bool _gotDataFromGen = false;
    private DungeonGrid<DungeonGenerator.Cell> _grid;
    private Dictionary<int, DungeonGenerator.Room> _rooms;

    private Dictionary<int, List<DungeonGenerator.Room>> _orderedRoomsByStoryIdx = new Dictionary<int, List<DungeonGenerator.Room>>();
    private Path _foundPath;

    // Start is called before the first frame update
    void Start()
    {
        _gen = FindObjectOfType<DungeonGenerator>();
    }

    public void Init()
    {
        if (!_gotDataFromGen && _gen.FinishedGenerating)
        {
            _grid = _gen.Grid;
            _rooms = _gen.InstantiatedRooms;
            _player = FindObjectOfType<Player>();
            _gotDataFromGen = true;

            OrderRooms();
            //CreateRoomGraph();
        }
        else if (!_gen.FinishedGenerating)
        {
            Debug.Log("Generator is not finished");
        }
    }

    private void OrderRooms()
    {
        foreach(var room in _rooms)
        {
            foreach (var marker in room.Value.GetStoryMarkers())
            {
                if (_orderedRoomsByStoryIdx.TryGetValue(marker.IndexInStory, out var roomList))
                {
                    if (marker.RelevantForStory)
                    {
                        roomList.Add(room.Value);
                    }
                } 
                else
                {
                    _orderedRoomsByStoryIdx.Add(marker.IndexInStory, new List<DungeonGenerator.Room>() { room.Value });
                }
            }
        }
    }

    public Path FindWay(int targetStoryIdx)
    {
        if (overrideTargetIdx)
        {
            targetStoryIdx = targetIdx;
        }
        // TODO: start timer to update the direction, in which to point, when cell of player changes
        // Visuals:
        // - arrow pointing in direction of next tiles (or direclty to the story marker, if player is in same room)
        //   - how to handle zigzagging:
        //      - maybe calculate mean value of next tiles 
        //      - calculate next 'non-zigzagging' direction change of cells -> could 'connect' the centres of the cells 
        //        and detect in the 'kinks' (direction-changes) in the line formed by this -> point arrow at the 'kink' point
        // - draw "line in the sand" on the ground..

        var pos = _player.transform.position;

        // just use existing grid and crank the cost for the free tiles way up
        DungeonPathfinder2D pathFinder = new DungeonPathfinder2D(new Vector2Int(_grid.Size.x, _grid.Size.z));

        var startCell = _gen.GlobalToCellIdx(pos);
        var startCellVec2 = new Vector2Int(startCell.x, startCell.z);

        var firstRoomWithTargetIdx = _orderedRoomsByStoryIdx[targetStoryIdx].First();
        var storyMarkerPosition = firstRoomWithTargetIdx.GetStoryMarkers().Where(marker => marker.IndexInStory == targetStoryIdx).First().gameObject.transform.position;

        var endCell = _gen.GlobalToCellIdx(storyMarkerPosition);
        var endCellVec2 = new Vector2Int(endCell.x, endCell.z);

        var path = pathFinder.FindPath(startCellVec2, endCellVec2, (DungeonPathfinder2D.Node a, DungeonPathfinder2D.Node b) => {
            var pathCost = new DungeonPathfinder2D.PathCost();

            var bPosVec3 = new Vector3Int(b.Position.x, 0, b.Position.y);
            var aPosVec3 = new Vector3Int(a.Position.x, 0, a.Position.y);
            pathCost.cost = 1;

            var test = new Vector2Int(21, 14);
            if (a.Position == test || b.Position == test)
            {
                bool bBreak = true;
            }

            pathCost.traversable = true;

            var aCellType = _grid[aPosVec3].type;
            var bCellType = _grid[bPosVec3].type;

            // TODO: prevent from walking directly through walls (from corridor to corridor)
            if (bCellType == CellType.Bufferzone ||
                bCellType == CellType.Free)
            {
                // don't go there
                pathCost.cost += 100000;
                pathCost.traversable = false;
            }
            else if (aCellType == CellType.Room &&
                     bCellType == CellType.Hallway)
            {
                // don't go through walls
                pathCost.cost += 100000;
                pathCost.traversable = false;
            }
            else if (bCellType == CellType.Room &&
                     aCellType == CellType.Hallway)
            {
                // don't go through walls
                pathCost.cost += 100000;
                pathCost.traversable = false;
            }
            else if (bCellType == CellType.Door && // door is in room
                     aCellType == CellType.Hallway)
            {
                // don't go through walls
                pathCost.cost += 100000;
                pathCost.traversable = false;
            }
            else if (aCellType == CellType.Door && // door is in room
                     bCellType == CellType.Hallway)
            {
                // don't go through walls
                pathCost.cost += 100000;
                pathCost.traversable = false;
            }
            else if (aCellType == CellType.Hallway &&
                     bCellType == CellType.Hallway || 
                     aCellType == CellType.DoorDock && 
                     bCellType == CellType.Hallway ||
                     aCellType == CellType.Hallway && 
                     bCellType == CellType.DoorDock)
            {
                if (_grid[aPosVec3].neighborsInPaths == null)
                {
                    return pathCost;
                }
                var neighborsA = _grid[aPosVec3].neighborsInPaths;
                bool areNeighbors = neighborsA.Contains(bPosVec3);
                if (!areNeighbors)
                {
                    // don't go through walls
                    pathCost.cost += 100000;
                    pathCost.traversable = false;
                }
            }
            return pathCost;
        });


        Path newPath = new Path();
        newPath.cells = path.Item1;
        newPath.cost = path.Item2;

        int startIdx = _grid[startCell].roomIdx;
        int endIdx = _grid[endCell].roomIdx;

        newPath.startRoomIdx = startIdx;
        newPath.endRoomIdx = endIdx;
        _foundPath = newPath;
        return _foundPath;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDrawGizmosSelected()
    {
        if (_foundPath.cells == null) return;
        
        // just set it..
        int CellSize = 5;
        if (_foundPath.cells.Count > 0)
        {
            var path = _foundPath;
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
