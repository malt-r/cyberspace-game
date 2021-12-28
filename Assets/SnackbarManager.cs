using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SnackbarManager : MonoBehaviour
{
  private static SoundManager _instance;
  public static SoundManager Instance => _instance;

  
  [SerializeField] private TMP_Text title;
  [SerializeField] private TMP_Text text;

  [SerializeField] private bool hasMessage;

  [SerializeField] private float lifeTime;
  [SerializeField] private float currentTime;

  public const string spriteString = @"<sprite=""{0}"" name=""{1}"">";

  [SerializeField]
  private GameObject snackbarUI;
  void Start()
  {
    CheckForSnackbarUI();
    if(snackbarUI){
      title.text = "";
      text.text = "";
    }
  }
  private void Update()
  {
    CheckForSnackbarUI();
    if (hasMessage)
    {
      currentTime += Time.deltaTime;
      if (currentTime > lifeTime)
      {
        currentTime = 0;
        hideText();
      }
    }
  }

  private void CheckForSnackbarUI()
  {
    if (snackbarUI == null)
    {
      snackbarUI = GameObject.Find("SnackbarUI");
      if (snackbarUI == null)
      {
        return;
      }
      title = snackbarUI.transform.GetChild(0).GetComponent<TMP_Text>();
      text = snackbarUI.transform.GetChild(1).GetComponent<TMP_Text>();
    }
  }

  private void hideText()
  {
    title.text = "";
    text.text = "";
    hasMessage = false;
    currentTime = 0;
  }

  public enum SnackbarMessageType
  {
    Information
  }
  public void DisplayMessage(string message, float time = 15f, SnackbarMessageType type =SnackbarMessageType.Information)
  {
    if (title != null)
    { title.text = getDisplayStringOfEnum(type);}
    if (text != null) {text.text = message;}
    hasMessage = true;
    lifeTime = time;
  }
  
  public void HideMessage()
  {
    hideText();
  }

  private string getDisplayStringOfEnum(SnackbarMessageType type)
  {
    var iconString = GetSpriteString("GUI/INFORMATION");
    var defaultString = iconString + " Information";
 
    if (type == SnackbarMessageType.Information)
    {
      return defaultString;
    }
    
    Debug.LogError("No String representation found for enum");
    return defaultString;
  }

  public string GetSpriteString(string iconIdentifier)
  {
    var baseString = @"<sprite=""{0}"" name=""{1}"">";
    var splitString = iconIdentifier.Split('/');
    if (splitString.Length != 2)
    {
      Debug.LogError($"Wrong Format on iconIdentifier {iconIdentifier}");
      return string.Format(baseString, "GUI", "INFORMATION");
    }

    var iconGroup = splitString[0];
    var iconName = splitString[1];


    switch (iconIdentifier)
    {
      case "ICONS/MOUSE":
      case "ICONS/SHIFT":
      case "ICONS/A":
      case "ICONS/SPACE":
      case "ICONS/CTRL":
      case "ICONS/D":
      case "ICONS/E":
      case "ICONS/G":
      case "ICONS/H":
      case "ICONS/MOUSE_WHEEL":
      case "ICONS/MOUSE_RIGHTCLICK":
      case "ICONS/Q":
      case "ICONS/R":
      case "ICONS/S":
      case "ICONS/W":
      case "ICONS/MOUSE_LEFTCLICK":
      case "GUI/INFORMATION":
        return string.Format(baseString, iconGroup, iconName);
      default:
        Debug.LogError($"Unknwon icon identifier {iconIdentifier}");
        return string.Format(baseString, iconGroup, iconName);
    }
  }
}
