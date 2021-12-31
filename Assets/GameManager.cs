using System.Collections;
using System.Collections.Generic;
using System.Linq;
using StarterAssets;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private DungeonGenerator generator;
    private Vector3Int _lastPassedRespawnPoint;
    private bool _foundGenerator = false;

    [SerializeField] 
    private string startSceneName;

    // Start is called before the first frame update
    void Start()
    {
        EventManager.StartListening("Combat/PlayerDied", HandlePlayerDeath);
        EventManager.StartListening("Level/PassDoorMarker", HandlePassDoorMarker);
        
        SceneManager.LoadScene(startSceneName);
    }

    private void HandlePassDoorMarker(object data)
    {
        var marker = data as DoorMarker;
        Debug.Log("Passed door marker");
        _lastPassedRespawnPoint = generator.GetDoorDockCellsRawCoords(new List<Vector3Int>() {Vector3Int.RoundToInt(marker.transform.position)}).First();
        if (_lastPassedRespawnPoint == null)
        {
            _lastPassedRespawnPoint = generator.GetDoorDockCellsRawCoords(new List<Vector3Int>() {Vector3Int.CeilToInt(marker.transform.position)}).First();
            if (_lastPassedRespawnPoint == null)
            {
                _lastPassedRespawnPoint = generator.GetDoorDockCellsRawCoords(new List<Vector3Int>() {Vector3Int.FloorToInt(marker.transform.position)}).First();
                if (_lastPassedRespawnPoint == null)
                {
                    Debug.LogError("Invalid door marker position, can't find corresponding door dock");
                }
            }
        }
    }
    
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
        if (!_foundGenerator)
        {
            generator = FindObjectOfType<DungeonGenerator>();
            if (generator != null && generator.FinishedGenerating)
            {
                _foundGenerator = true;
            }
        }
    }

    public void LoadScene(string name)
    {
        SceneManager.LoadScene(name);
        generator = null;
        _foundGenerator = false;
    }
}
