
using System;
using System.Collections;
using System.Transactions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using JetBrains.Annotations;
using StarterAssets;

public class MinigameInteractor : MonoBehaviour
{
    [NotNull] 
    private StarterAssetsInputs input;
    
    private PlayerInput playerInput;
    
    [NotNull]
    public GameObject minigame;
    private WireTask wireTask;

    private MinigameStatus currentMgStatus;

    public const string evt_EnterCollider = "Minigame/EnterCollider";
    public const string evt_StartMinigame = "Minigame/StartMinigame";

    // Start is called before the first frame update
    void Start()
    {
        input = GetComponent<StarterAssetsInputs>();
        playerInput = GetComponent<PlayerInput>();
        wireTask = minigame.GetComponentInChildren<WireTask>();
    }

    // Update is called once per frame
    void Update()
    {
        if (currentMgStatus != null && !currentMgStatus.isDone)
        {
            if (wireTask.IsTaskCompleted)
            {
                currentMgStatus.SetDone();
                disableMinigame();
            }
        }
        
        if (playerInput.currentActionMap.name == "Minigame")
        {
            if (input.escape)
            {
                disableMinigame();
                input.escape = false;
            }
        }
    }

    private void disableMinigame()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        playerInput.SwitchCurrentActionMap("Player");
        minigame.SetActive(false);
        wireTask.ResetMinigame();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("MiniGame"))
        {
            var status = other.gameObject.GetComponent<MinigameStatus>();
            if (!status.isDone)
            {
                EventManager.TriggerEvent(evt_EnterCollider, null);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("MiniGame"))
        {
            currentMgStatus = other.gameObject.GetComponent<MinigameStatus>();
            if (currentMgStatus.isDone) { return; }

            if (playerInput.currentActionMap.name == "Player")
            {
                if (input.interact)
                {
                    EventManager.TriggerEvent(evt_StartMinigame, null);
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                    input.interact = false;
                    playerInput.SwitchCurrentActionMap("Minigame");
                    minigame.SetActive(true);
                    wireTask.StartMinigame();
                }
            }
        }
    }
}
