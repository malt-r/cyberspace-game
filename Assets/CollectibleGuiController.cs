using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CollectibleGuiController : MonoBehaviour
{
    [SerializeField] private float ShowTimeInS = 5.0f;
    
    private GameObject _collectibleGUIAnimation;
    private GameObject _collectibleGUIStat;
    private TMP_Text _collectibleGUIText;
    
    private bool wasShownTimed;

    private float lifeTime;
    private float currentTime;

    // Start is called before the first frame update
    void Start()
    {
        _collectibleGUIAnimation = GameObject.Find("CollectibleAnimation");
        _collectibleGUIStat = GameObject.Find("CollectibleStat");
        _collectibleGUIText = _collectibleGUIStat.GetComponent<TMP_Text>();
        
        _collectibleGUIAnimation.SetActive(false);
        _collectibleGUIStat.SetActive(false);
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

    public void UpdateGui(int currentCollected, int total, bool timed = true)
    {
        _collectibleGUIText.text = $"{currentCollected} / {total}";
        ShowGui(timed);
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
