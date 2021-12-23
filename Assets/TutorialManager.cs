using System;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    private FirstPersonController _playerController;
    private bool _initialized = false;
    private bool _currentStageFiredMessage = false;
    private bool _currentStageFinished = false;

    private Vector2 _prevLook;

    private const string msg_learnSee = "Dücke die Leertaste zum sehen";
    private const string msg_learnLook = "Bewege die Maus, um dich umzusehen";
    private const string msg_learnWalk = "Nutze WASD um dich zu bewegen";
    private const string msg_learnSprint = "Nutze die linke Umschalt-Taste, um zu sprinten";
    private const string msg_learnJump = "Nutze die Leertaste, um zu springen";
    private const string msg_learnScan = "Der Scanner erlaubt das Aufsaugen von Items, feuere ihn mit der linken Maustaste";

    private bool _readyForNextStage = true;

    public void SignalReadyForNextStage()
    {
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

    private TutorialStage _currentTutorialStage = TutorialStage.learnSee;
    private StarterAssetsInputs _input;

    public TutorialStage CurrentTutorialStage => _currentTutorialStage;

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
                    Console.WriteLine("Found input");
                }
                _playerController.canSee = false;
                _playerController.canMove = false;
                _playerController.canSprint = false;
                _playerController.canLookAround = false;
                _initialized = true;
                Console.WriteLine("TutorialManager Initialized");
            }
        }

        if (_initialized)
        {
            ImplementFirstTutorial();
        }
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
                }
                break;
            case TutorialStage.learnSee:
                if (!_currentStageFiredMessage)
                {
                    DisplayPopup(msg_learnSee);
                    Console.WriteLine(msg_learnSee);
                    _currentStageFiredMessage = true;
                }
                
                if (Input.GetKeyDown(KeyCode.Space) && _readyForNextStage) // TODO: fire event
                {
                    _playerController.canSee = true;
                    _currentTutorialStage = TutorialStage.learnLook;
                    _currentStageFiredMessage = false;
                    _prevLook = _input.look;
                }
                break;
            case TutorialStage.learnLook:
                if (!_currentStageFiredMessage)
                {
                    DisplayPopup(msg_learnLook);
                    Console.WriteLine(msg_learnLook);
                    _currentStageFiredMessage = true;
                }
                
                if (_prevLook != _input.look && _readyForNextStage) // TODO: fire event
                {
                    _playerController.canLookAround = true;
                    _currentTutorialStage = TutorialStage.learnWalk;
                    _currentStageFiredMessage = false;
                }
                _prevLook = _input.look;
                break;
            case TutorialStage.learnWalk:
                if (!_currentStageFiredMessage)
                {
                    DisplayPopup(msg_learnWalk);
                    Console.WriteLine(msg_learnWalk);
                    _currentStageFiredMessage = true;
                }
                
                if (Input.GetKeyDown(KeyCode.Space) && _readyForNextStage) // TODO: fire event
                {
                    _playerController.canMove = true;
                    _currentTutorialStage = TutorialStage.free;
                    _currentStageFiredMessage = false;
                }
                break;
            case TutorialStage.free:
                break;
            default:
                break;
        }
    }

    void SwitchToStage(TutorialStage stage)
    {
        _currentTutorialStage = stage;
    }

    // DUMMY
    void DisplayPopup(string message)
    {
        
    }
}
