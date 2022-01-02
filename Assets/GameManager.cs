using System.Collections.Generic;
using System.Linq;
using Assets.Weapons;
using UnityEngine;
using UnityEngine.SceneManagement;

// TODO: store stats about game-flow
public class GameManager : MonoBehaviour
{
    // scene state
    private DungeonGenerator _generator;
    private GameObject _instantiatedPlayer;
    private Vector3Int _lastPassedRespawnPoint;
    private Minimap _minimap;
    
    private bool _initializedScene = false;
    
    [SerializeField]
    GameObject playerPrefab;

    [SerializeField] 
    private string startSceneName;

    [SerializeField] 
    private string[] gameLevels;

    private int _gameLevelIdx = 0;

    [SerializeField] 
    private bool ExtendedMinimapInFirstLevel;

    private bool _extendedMinimap;
    private bool _activateScanner;
    
    // Start is called before the first frame update
    void Start()
    {
        EventManager.StartListening("Combat/PlayerDied", HandlePlayerDeath);
        EventManager.StartListening("Level/PassDoorMarker", HandlePassDoorMarker);
        EventManager.StartListening("Collectible/Collect", HandleCollectible);
        
        SceneManager.LoadScene(startSceneName);
    }

    private void HandleCollectible(object arg0)
    {
        Debug.Log("Collected collectible");
    }

    // store door dock as last passed respawn point
    private void HandlePassDoorMarker(object data)
    {
        var marker = data as DoorMarker;
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
            Debug.Log("Reviving Player");
            var combatant = actorObject.GetComponent<CombatParticipant>();
            combatant.Revive();
            actorObject.transform.position = _lastPassedRespawnPoint;
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
}