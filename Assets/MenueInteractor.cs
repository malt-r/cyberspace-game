using UnityEngine;
using UnityEngine.InputSystem;
using JetBrains.Annotations;
using StarterAssets;

public class MenueInteractor : MonoBehaviour
{
    [NotNull] 
    private StarterAssetsInputs input;
    
    private PlayerInput playerInput;
    
    [NotNull]
    public GameObject menue;

    // Start is called before the first frame update
    void Start()
    {
        input = GetComponent<StarterAssetsInputs>();
        playerInput = GetComponent<PlayerInput>();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerInput.currentActionMap.name == "Player")
        {
            if (input.escape)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                input.escape = false;
                playerInput.SwitchCurrentActionMap("Menue");
                menue.SetActive(true);
            }
        }else if (playerInput.currentActionMap.name == "Menue")
        {
            if (input.escape)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                playerInput.SwitchCurrentActionMap("Player");
                menue.SetActive(false);
                input.escape = false;

            }
        }


    }
}