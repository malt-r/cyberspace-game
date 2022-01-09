using System;
using System.Collections.Generic;
using System.Linq;
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

    [SerializeField] private float bossTransitionLenght = 27.5f;
    [SerializeField] private float shieldActivationDelay = 18.0f;
    [SerializeField] private float shieldActivationDistanceInS = 2.5f;
    
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
    private const string msg_learnScan = "Der Scanner kann Items aufsaugen, nutze hierfür #icon{ICONS/MOUSE_LEFTCLICK}";
    private const string msg_learnLaser = "Du hast den Laser-Modus gefunden. Nutze #icon{ICONS/MOUSE_WHEEL} um Modi zu wechseln.";
    private const string msg_learnLaser2 = "Der Laser verbraucht Energie. Die aktuelle Energie wird rechts unten angezeigt.";
    private const string msg_learnBomb  = "Du hast die Bombe eingesammelt. Sie richtet sehr viel Schaden gegen einen besonderen Gegner an.";
    private const string msg_pickupHealth = "Du hast Gesundheit aufgesammelt. Deine Gesundheit wird links unten angezeigt.";
    
    
    private const string msg_infoInteract = "Drücke #icon{ICONS/E} um zu interagieren";
    private const string msg_minimap = "Die Minimap rechts oben hilft bei der Orientierung";
    private const string msg_collectible = "Du hast ein Collectible gefunden, finde sie alle!";

    private bool pickupUpHealth;
    private bool pickedUpLaser;
    private bool pickedUpBomb;

    public const string evt_learnHealth = "Story/Health";
    public const string evt_learnLaser = "Story/Laser";
    public const string evt_learnBomb = "Story/Bomb";

    public const string evt_startBossIntroVoiceLine = "Boss/StartIntroVoiceLine";

    private bool _readyForNextStage = true;

    [SerializeField]
    private float DefaultTutorialPopupLength = 5.0f;

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
        free = 4,
        bossIntro = 5,
        bossTransition = 6,
        bossFight = 7
    }

    private HashSet<TutorialStage> shownMessages = new HashSet<TutorialStage>();

    private TutorialStage _currentTutorialStage = TutorialStage.init;
    private StarterAssetsInputs _input;
    private bool _bossFight;

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
                EventManager.StartListening("Story/GotScanner", HandleGetScanner);
                
                EventManager.StartListening(Absorber.evt_pickupBomb, HandlePickupBomb);
                EventManager.StartListening(Absorber.evt_pickupHealth, HandlePickupHealth);
                EventManager.StartListening(Absorber.evt_pickupLaser, HandleGetLaser);
                
                EventManager.StartListening("Boss/TriggerIntro", HandleBossIntro);
                EventManager.StartListening("Boss/EnterBossLevel", HandleBossEnterLevel);
            }
        }

        if (_initialized)
        {
            if (_bossFight)
            {
                ImplementBossFight();
            }
            else
            {
                ImplementFirstTutorial();
            }
        }
    }

    private void HandleBossEnterLevel(object arg0)
    {
        var enemies = FindObjectsOfType<Enemy>();
        foreach (var enemy in enemies)
        {
            enemy.SetForceIdle(true);
        }
    }

    private void HandleBossIntro(object arg0)
    {
        _playerController.canMove = false;
        _playerController.canJump = false;
        _playerController.canSprint = false;
        _currentTutorialStage = TutorialStage.bossIntro;
        EventManager.TriggerEvent(evt_startBossIntroVoiceLine, new StoryEventData().SetEventName(evt_startBossIntroVoiceLine).SetSender(this));
        _currentStageFiredMessage = false;
        _bossFight = true;
        _readyForNextStage = false;
    }

    void TransitionToNextStage()
    {
        _readyForNextStage = true;
    }

    void ActivateNextShield()
    {
        var shieldGens = GameObject.FindObjectsOfType<ShieldGenerator>();
        var inactiveCount = shieldGens.Count(gen => !gen.wasActivated);
        if (inactiveCount == 0)
        {
            // activate shield of boss
            var boss = GameObject.FindObjectOfType<BossEnemy>();
            boss.ActivateShieldGameObject();
            
            CancelInvoke("ActivateNextShield");
        }
        else
        {
            var notActive = shieldGens.First(gen => !gen.wasActivated);
            notActive.PlayStartupAnimation();
        }
    }
    
    void ImplementBossFight()
    {
        switch (_currentTutorialStage) // do stages with statemachine pattern? refactor
        {
            case TutorialStage.bossIntro:
                if (_readyForNextStage)
                {
                    _readyForNextStage = false;
                    _currentTutorialStage = TutorialStage.bossTransition;
                    _currentStageFiredMessage = false;
                }
                break;
            case TutorialStage.bossTransition:
                if (!_currentStageFiredMessage)
                {
                    FindObjectOfType<BossMusicManager>().newSoundtrack(BossMusicManager.TrackType.transition);
                    
                    // TODO: trigger Animation of Boss enemy
                    FindObjectOfType<BossEnemy>().transform.GetComponent<Animator>().SetTrigger("Rise");
                    
                    Invoke("TransitionToNextStage", bossTransitionLenght);
                    InvokeRepeating("ActivateNextShield", shieldActivationDelay, shieldActivationDistanceInS);
                    _currentStageFiredMessage = true;
                    _readyForNextStage = false;
                }
                
                if (_readyForNextStage) // TODO: fire event
                {
                    _currentTutorialStage = TutorialStage.bossFight;
                    _currentStageFiredMessage = false;
                    _readyForNextStage = false;
                }
                break;
            case TutorialStage.bossFight:
                if (!_currentStageFiredMessage)
                {
                    _playerController.canMove = true;
                    _playerController.canJump = true;
                    _playerController.canSprint = true;
                    
                    StoryManager.UpdateStoryUI("Besiege den Computer");
                    _currentStageFiredMessage = true;
                    _readyForNextStage = false;
                    
                    
                    var enemies = FindObjectsOfType<Enemy>();
                    foreach (var enemy in enemies)
                    {
                        enemy.SetForceIdle(false);
                    }
                    
                }
                break;
        }
    }
    

    private void HandlePickupHealth(object arg0)
    {
        if (!pickupUpHealth)
        {
            EventManager.TriggerEvent(evt_learnHealth, new StoryEventData().SetEventName(evt_learnHealth));
            DisplayPopup(msg_pickupHealth, DefaultTutorialPopupLength);
            pickupUpHealth = true;
        }
    }

    private void HandlePickupBomb(object arg0)
    {
        if (!pickedUpBomb)
        {
            EventManager.TriggerEvent(evt_learnBomb, new StoryEventData().SetEventName(evt_learnBomb));
            DisplayPopup(msg_learnBomb, DefaultTutorialPopupLength);
            pickedUpBomb = true;
        }
    }

    private void HandleGetLaser(object arg0)
    {
        if (!pickedUpLaser)
        {
            pickedUpLaser = true;
            EventManager.TriggerEvent(evt_learnLaser, new StoryEventData().SetEventName(evt_learnLaser));
            DisplayPopup(msg_learnLaser, DefaultTutorialPopupLength);
            Invoke("ContinueLaser", DefaultTutorialPopupLength + 0.5f);
        }
    }

    private void ContinueLaser()
    {
        DisplayPopup(msg_learnLaser2, DefaultTutorialPopupLength);
    }

    private void HandleGetScanner(object arg0)
    {
        DisplayPopup(msg_learnScan);
    }

    private void HandlePlayerDeath(object arg0)
    {
        DisplayPopup("Du bist gestorben", 3.0f);
    }

    private void HandleCollectible(object arg0)
    {
        if (!_foundCollectibleBefore && messageOnFirstCollectible)
        {
            DisplayPopup(msg_collectible, DefaultTutorialPopupLength);
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
        DisplayPopup(msg_learnJump, DefaultTutorialPopupLength);
    }
    
    private void HandleLearnSprint(object arg0)
    {
        _playerController.canSprint = true;
        DisplayPopup(msg_learnSprint, DefaultTutorialPopupLength);
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
