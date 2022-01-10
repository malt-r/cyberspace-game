using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Weapons;
using StarterAssets;
using Statistics;
using UnityEngine;
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
        public ulong sessionId;
        public LevelStats[] stats;
    }
    
    [Serializable]
    public struct LevelStats
    {
        public string levelName;
        public long generatorSeed;
        public Vector3[] deaths;
        public float timeForLevelInS;
        public Vector3[] foundCollectibles;
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
    //private int _collectedCount;
    private List<Vector3> _collectedCollectibles;
    private long _generatorSeed;
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

    private ulong sessionID = 0;
    
    [SerializeField]
    GameObject playerPrefab;

    [SerializeField] 
    private string startSceneName;

    [SerializeField] 
    private string stasticsSceneName;

    [SerializeField] 
    private string endSceneName;

    [SerializeField] 
    private string EndBossSceneName;

    [SerializeField] 
    private string[] gameLevels;

    [SerializeField] 
    private bool startWithBossScene = false;

    [Header("Statistics")] [SerializeField]
    private float movementRecordInterval = 1.0f;

    private int _gameLevelIdx = 0;

    [SerializeField] 
    private bool ExtendedMinimapInFirstLevel;

    [SerializeField] 
    public bool ShowCollectiblesTimed;

    private bool _extendedMinimap;
    private bool _activateScanner;
    private bool _displayStats;

    private string _dumpPath;
    private bool _displayEndScreen;
    private bool _initializedEndBossScene;
    private bool _inBossScene;

    public bool ExtendedMinimap => _extendedMinimap;

    // Start is called before the first frame update
    void Start()
    {
        // why does this not persist?
        _levelStats = new Dictionary<int, Statistics.LevelStats>();

        sessionID = GenerateSessionID(10);
        
        EventManager.StartListening("Combat/PlayerDied", HandlePlayerDeath);
        EventManager.StartListening("Level/PassDoorMarker", HandlePassDoorMarker);
        EventManager.StartListening("Collectible/Collect", HandleCollectible);
        EventManager.StartListening("Level/EndLevel", HandleEndLevel);
        
        // minigame
        EventManager.StartListening(MinigameInteractor.evt_StartMinigame, HandleStartMinigame);
        EventManager.StartListening(MinigameInteractor.evt_FinishMinigame, HandleFinishMinigame);
        
        SceneManager.LoadScene(startSceneName);

        if (startWithBossScene)
        {
            _gameLevelIdx = gameLevels.Length + 1;
        }
    }

    private ulong GenerateSessionID(int numDigits)
    {
        ulong buffer = 0;
        for (int i = 0; i < numDigits; i++)
        {
            ulong tmp = 0;
            while (tmp == 0)
            {
                tmp = (ulong) RNG.GetRand() % 10;
            }
            buffer += tmp * (ulong)Mathf.RoundToInt(Mathf.Pow(10, i));
        }

        return buffer;
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
        if (!_inBossScene)
        {
            Debug.Log("HandleEndLevel");
            FinalizeLevelStats();
            
            SceneManager.LoadScene(stasticsSceneName);

            _displayStats = true;
            DisplayStatsInStatsScene();
        }
        else
        {
            LoadNextGameLevel();
        }
    }

    private void HandleCollectible(object arg0)
    {
        Debug.Log("Collected collectible");

        var data = arg0 as StoryEventData;
        var payload = data.Payload as Collectible;
        
        _collectedCollectibles.Add(payload.transform.position);
        
        _collectibleGuiController.UpdateGui(_collectedCollectibles.Count, _totalCollectibleCount, timed: ShowCollectiblesTimed);
    }

    // store door dock as last passed respawn point
    private void HandlePassDoorMarker(object data)
    {
        // data contains the global position of the passed door dock
        if (data is Vector3Int)
        {
            _lastPassedRespawnPoint = (Vector3Int) data;
            var dockCellIdx = _generator.GlobalToCellIdx(_lastPassedRespawnPoint);
            var doorCellIdx = _generator.GetDoorCellPositionOfDoorDock(dockCellIdx);
            _lastPassedDoorMarkerPos = _generator.CellIdxToGlobal(doorCellIdx);
        }
        Debug.Log("Passed door marker");
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
            if (!_inBossScene)
            {
                RecordDeath();
                RevivePlayer(actorObject, _lastPassedRespawnPoint, _lastPassedDoorMarkerPos);
            }
            else
            {
                var respawnTransform = GameObject.FindObjectOfType<SpawnPoint>().transform;
                var lookDirection = respawnTransform.position + respawnTransform.forward * 1;
                RevivePlayer(actorObject, respawnTransform.position, lookDirection);
            }
            
        }
    }

    private void RevivePlayer(GameObject actorObject, Vector3 respawnPoint, Vector3 lookDirection)
    {
        Debug.Log("Reviving Player");
        var combatant = actorObject.GetComponent<CombatParticipant>();
        combatant.Revive();
        actorObject.transform.position = respawnPoint;
        _instantiatedPlayer.GetComponent<FirstPersonController>().ForceLookAt(lookDirection);
        actorObject.SetActive(true);
        
        var scanner =actorObject.GetComponent<WeaponControl>().CurrentWeapon as Scanner;
        if (scanner != null)
        {
            scanner.Reset();
        }
    }
    

    // Update is called once per frame
    void Update()
    {
        if (!_initializedScene && !_inBossScene)
        {
            _initializedScene = InitializeScene();
        }

        if (_displayStats)
        {
            DisplayStatsInStatsScene();
        }

        if (_displayEndScreen)
        {
            DisplayEndScreen();
        }

        if (_inBossScene && !_initializedEndBossScene)
        {
            _initializedEndBossScene = InitializeEndBossScene();
        }
    }

    private bool InitializeEndBossScene()
    {
        _instantiatedPlayer = GameObject.FindObjectOfType<FirstPersonController>().gameObject;
        
        // TODO: deactivate minimap gui
        var minimapPanel = GameObject.Find("GUI").transform.Find("MinimapUIPanel").gameObject;
        if (minimapPanel != null)
        {
            minimapPanel.SetActive(false);
        }
        
        // TODO: activate scanner
        var scanner = GameObject.FindObjectOfType<Scanner>();
        if (scanner == null)
        {
            return false;
        }
        //scanner.gameObject.SetActive(true);
        
        // TODO: Deactivate scanner weapon control
        SetScannerControlActive(false);
        
        // TODO: bestow the weapon modes upon the player
        var powerUpsToApplyAtStart = GameObject.Find("ScannerModesInit").gameObject.GetComponentsInChildren<PowerUpWeapon>();

        foreach (var powerUp in powerUpsToApplyAtStart)
        {
            var weapon = powerUp.weapon;
            scanner.AddSkill(weapon);
            Destroy(powerUp.gameObject);
        }
        
        DeactivateAllEnemies();

        return true;
    }
    
    public static void DeactivateAllEnemies()
    {
        var enemies = FindObjectsOfType<Enemy>();
        foreach (var enemy in enemies)
        {
            enemy.SetForceIdle(true);
        }
    }

    public static void ActivateAllEnemies()
    {
        var enemies = FindObjectsOfType<Enemy>();
        foreach (var enemy in enemies)
        {
            enemy.SetForceIdle(false);
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
                //SetScannerActive(_activateScanner);
            }

            InitializeStats();
            
            _totalCollectibleCount = _generator.PlacedCollectibles;
            _collectibleGuiController = FindObjectOfType<CollectibleGuiController>();
            _collectibleGuiController.UpdateGui(0, _totalCollectibleCount, false);
            
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

    public void SetScannerControlActive(bool value)
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

        //var camera = _instantiatedPlayer.GetComponentInChildren<Camera>(true);
        //if (camera == null)
        //{
        //    Debug.LogError("Could not find camera in children of player instance");
        //}
        //else
        //{
        //    var camGameObj = camera.gameObject;
        //    var scanner = camGameObj.GetComponentInChildren<Scanner>(true);
        //    if (scanner == null)
        //    {
        //        Debug.LogError("Could not find Scanner");
        //    }
        //    else
        //    {
        //        scanner.gameObject.SetActive(value);
        //    }
        //}
    }

    public void LoadNextGameLevel()
    {
        if (_gameLevelIdx < gameLevels.Length && !_inBossScene)
        {
            LoadScene(_gameLevelIdx);
            _gameLevelIdx++;
            _inBossScene = false;
        }
        else
        {
            if (!_initializedEndBossScene && !_inBossScene)
            {
                // finish stats
                _dumpPath = DumpStats();
                
                // load boss scene
                SceneManager.LoadScene(EndBossSceneName);
                _inBossScene = true;
            }
            else
            {
                SceneManager.LoadScene(endSceneName);
                _displayEndScreen = true;
                DisplayEndScreen();
                _inBossScene = false;
            }
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
            stats.foundCollectibles = _collectedCollectibles.ToArray();
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
        _collectedCollectibles = new List<Vector3>();
        _totalCollectibleCount = 0;
        
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
    
    private void DisplayEndScreen()
    {
        var endScreen = GameObject.FindObjectOfType<EndScreen>();
        if (endScreen != null)
        {
            endScreen.SetPath(_dumpPath);
            _displayEndScreen = false;
        }
    }

    private void DisplayStatsInStatsScene()
    {
        var statsMenu = GameObject.FindObjectOfType<StatsMenu>();
        if (statsMenu != null)
        {
            var stats = _levelStats[_gameLevelIdx];
            statsMenu.SetDeaths(stats.deaths.Length);
            statsMenu.SetTime(stats.timeForLevelInS);
            statsMenu.SetCollectibleCount(stats.foundCollectibles.Length, stats.totalCollectibles);
            statsMenu.SetSessionId(sessionID);
            statsMenu.SetMinimapType(stats.minimapType);
            _displayStats = false;
        }
    }

    public string DumpStats()
    {
        var data = _levelStats.Values.ToArray();
        // wrap in serializable array wrapper
        GameStats stats = new GameStats();
        stats.sessionId = sessionID;
        stats.stats = data;
        
        string dataAsJson = JsonUtility.ToJson(stats);
        string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        if (Directory.Exists(folderPath))
        {
            string path = folderPath + "/LevelData.json";
            Debug.Log($"Dumping stats to {path}");
            File.WriteAllText(path, dataAsJson);
            return path;
        }
        else
        {
            string path = Application.persistentDataPath + "/LevelData.json";
            Debug.Log($"Dumping stats to {path}");
            File.WriteAllText(path, dataAsJson);
            return path;
        }
        
    }
    
    #endregion
}