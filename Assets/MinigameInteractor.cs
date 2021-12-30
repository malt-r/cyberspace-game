
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
                // Tim hoerst du mich?
            }
        }
        
        if (playerInput.currentActionMap.name == "Minigame")
        {
            if (input.escape)
            {
                disableMinigame();
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
