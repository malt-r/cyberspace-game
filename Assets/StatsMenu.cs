using System;
using Statistics;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class StatsMenu : MonoBehaviour
{
    [SerializeField] private TMP_Text MinimapTypeField;
    [SerializeField] private TMP_Text CollectibleField;
    [SerializeField] private TMP_Text TimeField;
    [SerializeField] private TMP_Text DeathField;
    
    [SerializeField] private TMP_Text SessionIDField;

    [SerializeField] private Button ContinueButton;

    private ulong _sessionId;
    private bool _canContinue = false;
    
    
    // Start is called before the first frame update
    void Start()
    {
        ContinueButton.enabled = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetMinimapType(MinimapType type)
    {
        MinimapTypeField.text = type == MinimapType.Basic ? "Basis" : "Erweitert";
    }

    public void SetCollectibleCount(int collected, int total)
    {
        CollectibleField.text = $"{collected} / {total}";
    }

    public void SetDeaths(int deathCount)
    {
        DeathField.text = $"{deathCount}";
    }

    public void SetTime(float timeInS)
    {
        TimeField.text = $"{timeInS.ToString("F")} s";
    }

    public void SetSessionId(ulong id)
    {
        _sessionId = id;
        SessionIDField.text = $"{id}";
    }

    public void OnCopyButton()
    {
        GUIUtility.systemCopyBuffer = _sessionId.ToString();
    }

    public void OnFilledButton()
    {
        _canContinue = true;
        ContinueButton.enabled = true;
    }

    public void OnCotinueButton()
    {
        Debug.Log("Awesome Logic");
        GameObject.FindObjectOfType<GameManager>().LoadNextGameLevel();
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
