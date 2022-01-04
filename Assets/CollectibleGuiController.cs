using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class CollectibleGuiController : MonoBehaviour
{
    [SerializeField] private float ShowTimeInS = 5.0f;
    
    private GameObject _collectibleGUIAnimation;
    private GameObject _collectibleGUIStat;
    private TMP_Text _collectibleGUIText;
    
    private bool wasShownTimed;
    private bool initialized = false;

    private float lifeTime;
    private float currentTime;

    private int _currentCollected;
    private int _total;

    // Start is called before the first frame update
    void Start()
    {
        _collectibleGUIAnimation = GameObject.Find("CollectibleAnimation");
        _collectibleGUIStat = GameObject.Find("CollectibleStat");
        _collectibleGUIText = _collectibleGUIStat.GetComponent<TMP_Text>();
        
        _collectibleGUIAnimation.SetActive(false);
        _collectibleGUIStat.SetActive(false);
        
        UpdateText();
        initialized = true;
    }

    private void UpdateText()
    {
        _collectibleGUIText.text = $"{_currentCollected} / {_total}";
    }

    // Update is called once per frame
    void Update()
    {
        if (wasShownTimed)
        { 
            currentTime += Time.deltaTime;
            if (currentTime > lifeTime)
            {
                currentTime = 0;
                HideGui();
            }
        }
    }

    public void UpdateGui(int currentCollected, int total, bool showGui = true, bool timed = true)
    {
        _currentCollected = currentCollected;
        _total = total;
        
        if (initialized)
        {
            UpdateText();
            if (showGui)
            {
                ShowGui(timed);
            }
        }
    }

    public void ShowGui(bool timed = true)
    {
        if (timed) wasShownTimed = true;
        
        currentTime = 0;
        _collectibleGUIAnimation.SetActive(true);
        _collectibleGUIStat.SetActive(true);
        lifeTime = ShowTimeInS;
    }

    public void HideGui()
    {
        _collectibleGUIAnimation.SetActive(false);
        _collectibleGUIStat.SetActive(false);
        wasShownTimed = false;
    }
}
