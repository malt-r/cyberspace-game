
using System.Collections;
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
        }else if (playerInput.currentActionMap.name == "Minigame")
        {
            if (input.escape)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                playerInput.SwitchCurrentActionMap("Player");
                minigame.SetActive(false);
                wireTask.StopMinigame();
            }
        }


    }
}
