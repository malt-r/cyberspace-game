using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using StarterAssets;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.HID;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private bool skipTutorial;
    [SerializeField] private bool disableMinimapOnStart;
    [SerializeField] private bool messageOnFirstCollectible;
    
    private FirstPersonController _playerController;
    private SnackbarManager _snackBar;
    private GameObject _minimapUIPanel;
    private bool _initialized = false;
    private bool _currentStageFiredMessage = false;
    private bool _currentStageFinished = false;

    private bool _foundCollectibleBefore = false;

    private Vector2 _prevLook;
    private Vector2 _prevMove;

    private const string msg_learnSee = "Drücke #icon{ICONS/MOUSE_LEFTCLICK} zum Sehen";
    private const string msg_learnLook = "Bewege #icon{ICONS/MOUSE}, um dich umzusehen";
    private const string msg_learnWalk = "Nutze #icon{ICONS/W}#icon{ICONS/A}#icon{ICONS/S}#icon{ICONS/D}, um dich zu bewegen";
    private const string msg_learnSprint = "Nutze die linke Umschalt-Taste (#icon{ICONS/SHIFT}), um zu sprinten";
    private const string msg_learnJump = "Nutze die Leertaste (#icon{ICONS/SPACE}), um zu springen";
    private const string msg_learnScan = "Der Scanner erlaubt das Aufsaugen von Items, feuere ihn mit der linken Maustaste";
    private const string msg_infoInteract = "Drücke #icon{ICONS/E} um zu interagieren";
    private const string msg_minimap = "Die Minimap rechts oben hilft bei der Orientierung";
    private const string msg_collectible = "Du hast ein Collectible gefunden, finde sie alle!";

    private bool _readyForNextStage = true;

    public void SignalReadyForNextStage()
    {
        Debug.Log("Ready for next tutorial stage");
        _readyForNextStage = true;
    }

    // stages for linear tutorial at beginning
    public enum TutorialStage
    {
        init = 0, 
        learnSee = 1,
        learnLook = 2,
        learnWalk = 3,
        free = 4
    }

    private HashSet<TutorialStage> shownMessages = new HashSet<TutorialStage>();

    private TutorialStage _currentTutorialStage = TutorialStage.init;
    private StarterAssetsInputs _input;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!_initialized)
        {
            _playerController = FindObjectOfType<FirstPersonController>();
            if (_playerController != null)
            {
                _input = FindObjectOfType<StarterAssetsInputs>();
                if (_input != null)
                {
                    Debug.Log("Found input");
                }
                if (!skipTutorial)
                {
                    _playerController.canSee = false;
                    _playerController.canMove = false;
                    _playerController.canSprint = false;
                    _playerController.canLookAround = false;
                    _playerController.canJump = false;
                }
                else
                {
                    _currentTutorialStage = TutorialStage.free;
                }
                _snackBar = FindObjectOfType<SnackbarManager>();

                _minimapUIPanel = GameObject.Find("MinimapUIPanel");
                if (_minimapUIPanel != null && disableMinimapOnStart)
                {
                    _minimapUIPanel.SetActive(false);
                }
                
                _initialized = true;
                Debug.Log("TutorialManager Initialized");

                EventManager.StartListening("Minigame/GetJump", HandleLearnJump);
                EventManager.StartListening("Minigame/GetSprint", HandleLearnSprint);
                EventManager.StartListening("tut_leave", HandleLeaveTutorial);
                
                EventManager.StartListening(MinigameInteractor.evt_EnterCollider, HandleInteractPrompt);
                EventManager.StartListening(MinigameInteractor.evt_StartMinigame, HidePopup);
                
                EventManager.StartListening("Collectible/Collect", HandleCollectible);
                EventManager.StartListening("Combat/PlayerDied", HandlePlayerDeath);
            }
        }

        if (_initialized)
        {
            ImplementFirstTutorial();
        }
    }

    private void HandlePlayerDeath(object arg0)
    {
        DisplayPopup("Du bist gestorben", 3.0f);
    }

    private void HandleCollectible(object arg0)
    {
        if (!_foundCollectibleBefore && messageOnFirstCollectible)
        {
            DisplayPopup(msg_collectible, 7.0f);
        }
        _foundCollectibleBefore = true;
    }

    private void HandleLeaveTutorial(object arg0)
    {
        _minimapUIPanel.SetActive(true);
        DisplayPopup(msg_minimap);
    }


    void ImplementFirstTutorial()
    {
        switch (_currentTutorialStage) // do stages with statemachine pattern? refactor
        {
            case TutorialStage.init:
                if (_readyForNextStage)
                {
                    _readyForNextStage = false;
                    _currentTutorialStage = TutorialStage.learnSee;
                    _currentStageFiredMessage = false;
                    EventManager.TriggerEvent("tut_init", new StoryEventData().SetEventName("tut_init").SetSender(this));
                }
                break;
            case TutorialStage.learnSee:
                if (!_currentStageFiredMessage && _readyForNextStage)
                {
                    DisplayPopup(msg_learnSee);
                    _currentStageFiredMessage = true;
                }
                
                // left mouse button
                if (Input.GetMouseButtonDown(0) && _readyForNextStage) // TODO: fire event
                {
                    _snackBar.HideMessage();
                    _playerController.canSee = true;
                    _currentTutorialStage = TutorialStage.learnLook;
                    EventManager.TriggerEvent("tut_see", new StoryEventData().SetEventName("tut_see").SetSender(this));
                    _currentStageFiredMessage = false;
                    _prevLook = _input.look;
                    _readyForNextStage = false;
                }
                break;
            case TutorialStage.learnLook:
                if (!_currentStageFiredMessage && _readyForNextStage)
                {
                    DisplayPopup(msg_learnLook);
                    _currentStageFiredMessage = true;
                }
                
                if (_prevLook != _input.look && _readyForNextStage) // TODO: fire event
                {
                    _snackBar.HideMessage();
                    _playerController.canLookAround = true;
                    _currentTutorialStage = TutorialStage.learnWalk;
                    EventManager.TriggerEvent("tut_lookAround", new StoryEventData().SetEventName("tut_lookAround").SetSender(this));
                    _currentStageFiredMessage = false;
                    _readyForNextStage = false;
                    _prevMove = _input.move;
                }
                _prevLook = _input.look;
                break;
            case TutorialStage.learnWalk:
                if (!_currentStageFiredMessage && _readyForNextStage)
                {
                    DisplayPopup(msg_learnWalk);
                    _currentStageFiredMessage = true;
                }
                
                if (_prevMove != _input.move && _readyForNextStage) // TODO: fire event
                {
                    _snackBar.HideMessage();
                    _playerController.canMove = true;
                    _currentTutorialStage = TutorialStage.free;
                    EventManager.TriggerEvent("tut_walk", new StoryEventData().SetEventName("tut_walk").SetSender(this));
                    _currentStageFiredMessage = false;
                }

                _prevMove = _input.move;
                break;
            case TutorialStage.free:
                if (!_currentStageFiredMessage)
                {
                    var trigger = GetComponent<StoryTrigger>();
                    trigger.Activate();
                    _snackBar.HideMessage();
                    _currentStageFiredMessage = true;
                }
                break;
            default:
                break;
        }
    }

    void SwitchToStage(TutorialStage stage)
    {
        _currentTutorialStage = stage;
    }

    void HandleLearnJump(object arg)
    {
        _playerController.canJump = true;
        DisplayPopup(msg_learnJump, 7);
    }
    
    private void HandleLearnSprint(object arg0)
    {
        _playerController.canSprint = true;
        DisplayPopup(msg_learnSprint, 7);
    }
    
    private void HandleInteractPrompt(object arg0)
    {
        DisplayPopup(msg_infoInteract);
    }

    void DisplayPopup(string message, float timeInS = 20)
    {
        if (_snackBar == null)
        {
            Debug.Log(message);
        }
        else
        {
            var parsed = ParseSnackBarString(message);
            _snackBar.DisplayMessage(parsed, timeInS);
        }
    }

    void HidePopup(object arg0)
    {
        _snackBar.HideMessage();
    }

    string ParseSnackBarString(string message)
    {
        var matches = Regex.Matches(message, "#icon{([^}]+)}");

        foreach (Match match in matches)
        {
            var groups = match.Groups;
            message = message.Replace(match.Value, _snackBar.GetSpriteString(groups[1].Value));
        }

        return message;
    }
}
