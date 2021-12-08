using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BlueRaja;
using static DungeonGenerator;

public class Helper : MonoBehaviour
{
    [SerializeField]
    int roomInstantiationIdx = 0;

    [SerializeField]
    int targetIdx = 8;

    internal void FindWay()
    {
        FindWay(roomInstantiationIdx, targetIdx);
    }

    // TODO:
    // - use simple node-structure for roomgraph (could also be created in Dictionary to List of int (Index into instantiated Rooms)
    // - implement needed interfaces for usage of roomgraph with astar-algo
    //   - implement heuristic
    //   - implement neighbor -> should probably just be kids of node
    //   - implement cost -> just return already calculated path cost, if there is one, or return invalid value

    public class RoomGraph : WeightedGraph<int>
    {
        #region data
        Dictionary<int, List<Tuple<int, float>>> _roomGraph = new Dictionary<int, List<Tuple<int, float>>>();
        #endregion data

        public float Cost(int a, int b)
        {
            var paths = _roomGraph[a].Where(path => path.Item1 == b).OrderBy(path => path.Item2).ToList();
            if (!paths.Any())
            {
                return float.MaxValue;
            } 
            else
            {
                return paths.First().Item2;
            }
        }

        public IEnumerable<int> Neighbors(int id)
        {
            return _roomGraph[id].Select(p => p.Item1);
        }

        public RoomGraph(Dictionary<int, List<Tuple<int, float>>> data)
        {
            _roomGraph = data;
        }
    }

    // A* needs only a WeightedGraph and a location type L, and does *not*
    // have to be a grid. However, in the example code I am using a grid.
    public interface WeightedGraph<L>
    {
        float Cost(L a, L b);
        IEnumerable<L> Neighbors(L id);
    }

    public class AStarSearch
    {
        // TODO: modify
        public Dictionary<int, int> cameFrom
            = new Dictionary<int, int>();
        // TODO: modify
        public Dictionary<int, float> costSoFar
            = new Dictionary<int, float>();

        // Note: a generic version of A* would abstract over Location and
        // also Heuristic
        // TODO: generify
        static public double Heuristic(int a, int b)
        {
            //return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
            return 1.0;
        }

        public AStarSearch(WeightedGraph<int> graph, int start, int goal)
        {
            var frontier = new SimplePriorityQueue<int>();
            frontier.Enqueue(start, 0);

            cameFrom[start] = start;
            costSoFar[start] = 0;

            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();

                if (current.Equals(goal))
                {
                    break;
                }

                foreach (var next in graph.Neighbors(current))
                {
                    float newCost = (float)costSoFar[current]
                        + graph.Cost(current, next);
                    if (!costSoFar.ContainsKey(next)
                        || newCost < costSoFar[next])
                    {
                        costSoFar[next] = newCost;
                        float priority = (float)newCost + (float)Heuristic(next, goal);
                        frontier.Enqueue(next, priority);
                        cameFrom[next] = current;
                    }
                }
            }
        }
    }

    RoomGraph _roomGraph;

    DungeonGenerator _gen;
    Player _player;
    bool _gotDataFromGen = false;
    private List<List<DungeonGenerator.Path>> _paths;
    private DungeonGenerator.CellPathData[,] _cellPathData;
    private DungeonGrid<DungeonGenerator.Cell> _grid;
    private List<DungeonGenerator.Room> _rooms;


    private Dictionary<int, List<DungeonGenerator.Room>> _orderedRoomsByStoryIdx = new Dictionary<int, List<DungeonGenerator.Room>>();
    private AStarSearch _astar;
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
            _paths = _gen.PartitionedPaths;
            _cellPathData = _gen.CellPathDatas;
            _grid = _gen.Grid;
            _rooms = _gen.InstantiatedRooms;
            _player = FindObjectOfType<Player>();
            _gotDataFromGen = true;

            OrderRooms();
            CreateRoomGraph();
        }
        else if (!_gen.FinishedGenerating)
        {
            Debug.Log("Generator is not finished");
            return;
        }
    }

    // this removes the information about the room partitions!
    // but is this really relevant? the player should not be able to reach a story index greater than the one
    // in the current partition...
    private void CreateRoomGraph()
    {
        // dictionary about neighbors of rooms and the associated path cost
        Dictionary<int, List<Tuple<int, float>>> roomGraphData = new Dictionary<int, List<Tuple<int, float>>>();

        // construct a vertex for each room
        int idx = 0;
        foreach (var room in _rooms)
        {
            //_roomVertices.Add(new Graphs.Vertex<int>(room.InstantiationIdx));
            //_roomInstantiationIdxToGraphIdx.Add(room.InstantiationIdx, idx);
            //idx++;
            roomGraphData.Add(room.InstantiationIdx, new List<Tuple<int, float>>());
        }

        foreach (var partition in _paths)
        {
            foreach (var path in partition)
            {
                var startIdx = path.startRoomIdx;
                var endIdx = path.endRoomIdx;
                //// not good, should use simple node structure..
                //_roomGraph.Add(new Graphs.Edge(startRoomVertex, endRoomVertex));

                roomGraphData[startIdx].Add(new Tuple<int, float>(endIdx, path.cost));
                roomGraphData[endIdx].Add(new Tuple<int, float>(startIdx, path.cost));
            }
        }

        _roomGraph = new RoomGraph(roomGraphData);
    }

    private void OrderRooms()
    {
        foreach(var room in _rooms)
        {
            foreach (var marker in room.GetStoryMarkers())
            {
                if (_orderedRoomsByStoryIdx.TryGetValue(marker.IndexInStory, out var roomList))
                {
                    if (marker.RelevantForStory)
                    {
                        roomList.Add(room);
                    }
                } 
                else
                {
                    _orderedRoomsByStoryIdx.Add(marker.IndexInStory, new List<DungeonGenerator.Room>() { room });
                }
            }
        }
    }

    public void FindWay(int currentRoomInstantiationIdx, int targetStoryIdx)
    {
        // TODO: start timer to update the direction, in which to point, when cell of player changes
        // Visuals:
        // - arrow pointing in direction of next tiles (or direclty to the story marker, if player is in same room)
        //   - how to handle zigzagging:
        //      - maybe calculate mean value of next tiles 
        //      - calculate next 'non-zigzagging' direction change of cells -> could 'connect' the centres of the cells 
        //        and detect in the 'kinks' (direction-changes) in the line formed by this -> point arrow at the 'kink' point
        // - draw "line in the sand" on the ground..

        var pos = _player.transform.position;
        var cellIdx = _gen.GlobalToCellIdx(pos);

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
            var typeA = _grid[aPosVec3].type;
            var typeB = _grid[bPosVec3].type;
            if (a.Position == test || b.Position == test)
            {
                bool bBreak = true;
            }


            pathCost.traversable = true;
            // TODO: prevent from walking directly through walls
            if (_grid[bPosVec3].type == CellType.Bufferzone ||
                _grid[bPosVec3].type == CellType.Free)
            {
                // don't go there
                pathCost.cost += 100000;
                pathCost.traversable = false;
            }
            else if (_grid[aPosVec3].type == CellType.Room &&
                     _grid[bPosVec3].type == CellType.Hallway)
            {
                // don't go through walls
                pathCost.cost += 100000;
                pathCost.traversable = false;
            }
            else if (_grid[bPosVec3].type == CellType.Room &&
                     _grid[aPosVec3].type == CellType.Hallway)
            {
                // don't go through walls
                pathCost.cost += 100000;
                pathCost.traversable = false;
            }
            else if (_grid[bPosVec3].type == CellType.Door && // door is in room
                     _grid[aPosVec3].type == CellType.Hallway)
            {
                // don't go through walls
                pathCost.cost += 100000;
                pathCost.traversable = false;
            }
            else if (_grid[aPosVec3].type == CellType.Door && // door is in room
                     _grid[bPosVec3].type == CellType.Hallway)
            {
                // don't go through walls
                pathCost.cost += 100000;
                pathCost.traversable = false;
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
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDrawGizmosSelected()
    {
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
