using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Weapons;
using TMPro;
using StarterAssets;
using Statistics;
using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

// TODO: store stats about game-flow
// - player movement
// - number of deaths (with position)
// - time for level completion
// - time for minigame completion
// - number of found collectibles
namespace Statistics
{
    [Serializable]
    public struct GameStats
    {
        public LevelStats[] stats;
    }
    
    [Serializable]
    public struct LevelStats
    {
        public string levelName;
        public Vector3[] deaths;
        public float timeForLevelInS;
        public int foundCollectibles;
        public int totalCollectibles;
        public MinigameSolveDataPoint[] minigameData;
        public MinimapType minimapType;
        public MovementDataPoint[] movementData;
    }

    [Serializable]
    public struct MovementDataPoint
    {
        public Vector3 position;
        public string timestamp;
        public MovementDataPointType type;
    }

    [Serializable]
    public struct MinigameSolveDataPoint
    {
        public string minigameName;
        public float time;
    }

    [Serializable]
    public enum MinimapType
    {
        None, 
        Basic,
        Extended
    }

    [Serializable]
    public enum MovementDataPointType
    {
        None,
        Death,
    }
}


public class GameManager : MonoBehaviour
{
    // scene state
    private DungeonGenerator _generator;
    private GameObject _instantiatedPlayer;
    private Vector3Int _lastPassedRespawnPoint;
    private Vector3 _lastPassedDoorMarkerPos;
    private Minimap _minimap;
    private int _totalCollectibleCount;
    private int _collectedCount;
    private CollectibleGuiController _collectibleGuiController;
    
    // statistics
    private Dictionary<int, Statistics.LevelStats> _levelStats;
    private List<Statistics.MovementDataPoint> _movementDataPoints;
    private List<Vector3> _deaths;
    private List<MinigameSolveDataPoint> _minigameSolves;
    private DateTime _levelStartTime;
    private DateTime _minigameStartTime;
    private string _currentMinigameName;
    
    private bool _initializedScene = false;
    
    [SerializeField]
    GameObject playerPrefab;

    [SerializeField] 
    private string startSceneName;

    [SerializeField] 
    private string[] gameLevels;

    [Header("Statistics")] [SerializeField]
    private float movementRecordInterval = 1.0f;

    private int _gameLevelIdx = 0;

    [SerializeField] 
    private bool ExtendedMinimapInFirstLevel;

    [SerializeField] 
    public bool ShowCollectiblesTimed;

    private bool _extendedMinimap;
    private bool _activateScanner;
    
    // Start is called before the first frame update
    void Start()
    {
        _levelStats = new Dictionary<int, Statistics.LevelStats>();
        
        EventManager.StartListening("Combat/PlayerDied", HandlePlayerDeath);
        EventManager.StartListening("Level/PassDoorMarker", HandlePassDoorMarker);
        EventManager.StartListening("Collectible/Collect", HandleCollectible);
        EventManager.StartListening("Level/EndLevel", HandleEndLevel);
        
        // minigame
        EventManager.StartListening(MinigameInteractor.evt_StartMinigame, HandleStartMinigame);
        EventManager.StartListening(MinigameInteractor.evt_FinishMinigame, HandleFinishMinigame);
        
        SceneManager.LoadScene(startSceneName);
    }

    private void HandleFinishMinigame(object arg0)
    {
        var status = arg0 as MinigameStatus;
        string name = status.Name;
        if (!name.Equals(_currentMinigameName))
        {
            Debug.Log("wtf");
        }
        else
        {
            MinigameSolveDataPoint point = new MinigameSolveDataPoint();
            point.time = diffInS(DateTime.Now, _minigameStartTime);
            point.minigameName = name;
            _minigameSolves.Add(point);
        }
    }

    private void HandleStartMinigame(object arg0)
    {
        var status = arg0 as MinigameStatus;
        _currentMinigameName = status.Name;
        _minigameStartTime = DateTime.Now;
    }

    private void HandleEndLevel(object arg0)
    {
        Debug.Log("HandleEndLevel");
        FinalizeLevelStats();
        DumpStats();
    }

    private void HandleCollectible(object arg0)
    {
        Debug.Log("Collected collectible");
        _collectedCount++;
        _collectibleGuiController.UpdateGui(_collectedCount, _totalCollectibleCount, timed: ShowCollectiblesTimed);
    }

    // store door dock as last passed respawn point
    private void HandlePassDoorMarker(object data)
    {
        var marker = data as DoorMarker;
        _lastPassedDoorMarkerPos = marker.transform.position;
        Debug.Log("Passed door marker");
        _lastPassedRespawnPoint = _generator.GetDoorDockCellsRawCoords(new List<Vector3Int>() {Vector3Int.RoundToInt(marker.transform.position)}).First();
        if (_lastPassedRespawnPoint == null)
        {
            _lastPassedRespawnPoint = _generator.GetDoorDockCellsRawCoords(new List<Vector3Int>() {Vector3Int.CeilToInt(marker.transform.position)}).First();
            if (_lastPassedRespawnPoint == null)
            {
                _lastPassedRespawnPoint = _generator.GetDoorDockCellsRawCoords(new List<Vector3Int>() {Vector3Int.FloorToInt(marker.transform.position)}).First();
                if (_lastPassedRespawnPoint == null)
                {
                    Debug.LogError("Invalid door marker position, can't find corresponding door dock");
                }
            }
        }
    }
    
    // respawn player at last passed respawn point
    private void HandlePlayerDeath(object data)
    {
        var actorObject = data as GameObject;
        if (!actorObject.CompareTag("Player"))
        {
            Debug.LogError("HandlePlayerDeath was called but tag is not 'player'");
        }
        else 
        {
            RecordDeath();
            
            Debug.Log("Reviving Player");
            var combatant = actorObject.GetComponent<CombatParticipant>();
            combatant.Revive();
            actorObject.transform.position = _lastPassedRespawnPoint;
            _instantiatedPlayer.GetComponent<FirstPersonController>().ForceLookAt(_lastPassedDoorMarkerPos);
            actorObject.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!_initializedScene)
        {
            _initializedScene = InitializeScene();
        }
    }

    bool InitializeScene()
    {
        _generator = FindObjectOfType<DungeonGenerator>();
        if (_generator != null && _generator.FinishedGenerating) // generator will start generating on scene loading
        {
            if (!InstantiatePlayer())
            {
                Debug.LogError("Could not instantiate player");
                return false;
            }
            else 
            {
                // create minimap
                _minimap = _generator.CreateMinimap(_instantiatedPlayer, _extendedMinimap);
                SetScannerActive(_activateScanner);
            }

            _totalCollectibleCount = _generator.PlacedCollectibles;
            _collectibleGuiController = FindObjectOfType<CollectibleGuiController>();
            _collectibleGuiController.UpdateGui(0, _totalCollectibleCount, false);
            
            InitializeStats();
            
            return true;
        }

        return false;
    }


    private void InitFirstLevel()
    {
        _extendedMinimap = ExtendedMinimapInFirstLevel;
        
        // TODO: setting the scanner active should really only be done, if the player finds the scanner
        _activateScanner = false;
    }

    private void ResetSceneState()
    {
        _generator = null;
        _instantiatedPlayer = null;
        _minimap = null;
        _lastPassedRespawnPoint = Vector3Int.zero;
        _initializedScene = false;
        
        // this effectively alternates between extended and basic minimap for each scene initialization
        _extendedMinimap = !_extendedMinimap;
    }
    
    private void LoadScene(int levelIdx)
    {
        SceneManager.LoadScene(gameLevels[levelIdx]);
        ResetSceneState();
        if (levelIdx == 0)
        {
            InitFirstLevel();
        }
        
    }

    private void SetScannerActive(bool value)
    {
        if (_instantiatedPlayer == null)
        {
            Debug.LogError("No player instance");
            return;
        }

        var weaponControl = _instantiatedPlayer.GetComponent<WeaponControl>();
        if (weaponControl == null)
        {
            Debug.LogError("Could not find weaponcontrol in playerinstance");
        }
        else
        {
            weaponControl.enabled = value;
        }

        var camera = _instantiatedPlayer.GetComponentInChildren<Camera>(true);
        if (camera == null)
        {
            Debug.LogError("Could not find camera in children of player instance");
        }
        else
        {
            var camGameObj = camera.gameObject;
            var scanner = camGameObj.GetComponentInChildren<Scanner>(true);
            if (scanner == null)
            {
                Debug.LogError("Could not find Scanner");
            }
            else
            {
                scanner.gameObject.SetActive(value);
            }
        }
    }

    public void LoadNextGameLevel()
    {
        if (_gameLevelIdx < gameLevels.Length)
        {
            LoadScene(_gameLevelIdx);
            _gameLevelIdx++;
        }
        else
        {
            Debug.LogError("Index is already at the end of gameLevel array");
        }
    }

    private bool InstantiatePlayer()
    {
        var spawnPoint = FindObjectOfType<SpawnPoint>();
        if (spawnPoint == null)
        {
            Debug.LogError("Could not find SpawnPoint in Scene");
        }
        else 
        {
            _instantiatedPlayer = Instantiate(playerPrefab, spawnPoint.transform.position, spawnPoint.transform.rotation);
            return true;
        }
        return false;
    }
    
    #region Statistics

    void FinalizeLevelStats()
    {
        // stop movement recording
        CancelInvoke("RecordMovementStat");
        
        // finalize game stats
        if (_levelStats.TryGetValue(_gameLevelIdx, out var stats))
        {
            stats.levelName = gameLevels[_gameLevelIdx - 1];
            stats.foundCollectibles = _collectedCount;
            stats.totalCollectibles = _totalCollectibleCount;
            stats.deaths = _deaths.ToArray();
            stats.minigameData = _minigameSolves.ToArray();
            stats.movementData = _movementDataPoints.ToArray();
            
            stats.timeForLevelInS = diffInS(DateTime.Now, _levelStartTime);
            
            stats.minimapType = _extendedMinimap ? MinimapType.Extended : MinimapType.Basic;
            _levelStats[_gameLevelIdx] = stats;
        }
    }

    float diffInS(DateTime first, DateTime second)
    {
        var diff = first - second;
        var ms = diff.TotalMilliseconds;
        return (float)ms / (1000.0f);
    }
    
    void InitializeStats()
    {
        // create new entry in game stats
        _levelStats.Add(_gameLevelIdx, new LevelStats());
        
        _deaths = new List<Vector3>();
        _minigameSolves = new List<MinigameSolveDataPoint>();
        _movementDataPoints = new List<MovementDataPoint>();
        _levelStartTime = DateTime.Now;
        
        InvokeRepeating("RecordMovementStat", 0.0f, movementRecordInterval);
    }
    
    private void RecordMovementStat()
    {
        RecordMovementStat(MovementDataPointType.None);
    }

    private void RecordMovementStat(MovementDataPointType type)
    {
        MovementDataPoint point = new MovementDataPoint();
        point.position = _instantiatedPlayer.transform.position;
        point.timestamp = DateTime.Now.ToString("O");
        point.type = type;
        
        _movementDataPoints.Add(point);
    }

    private void RecordDeath()
    {
        RecordMovementStat(MovementDataPointType.Death);
        _deaths.Add(_instantiatedPlayer.transform.position);
    }

    public void DumpStats()
    {
        var data = _levelStats.Values.ToArray();
        // wrap in serializable array wrapper
        GameStats stats = new GameStats();
        stats.stats = data;
        
        string dataAsJson = JsonUtility.ToJson(stats);
        string path = Application.persistentDataPath + "/LevelData.json";
        
        Debug.Log($"Dumping stats to {path}");
        System.IO.File.WriteAllText(path, dataAsJson); 
    }
    #endregion
}