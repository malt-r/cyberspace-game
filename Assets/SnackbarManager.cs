using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SnackbarManager : MonoBehaviour
{
  private static SoundManager _instance;
  public static SoundManager Instance => _instance;

  
  [SerializeField] private TMP_Text title;
  [SerializeField] private TMP_Text text;
  [SerializeField] private Image backgroundImage;

  [SerializeField] private bool hasMessage;

  [SerializeField] private float lifeTime;
  [SerializeField] private float currentTime;

  public const string spriteString = @"<sprite=""{0}"" name=""{1}"">";

  [SerializeField]
  private GameObject snackbarUI;
  void Start()
  {
    snackbarUI = GameObject.Find("SnackbarUI");
    ResetSnackbarUI();
    if(snackbarUI){
      title.text = "";
      text.text = "";
      backgroundImage.transform.gameObject.SetActive(false);
    }
  }
  private void Update()
  {
    ResetSnackbarUI();
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

  private void ResetSnackbarUI()
  {
    if (snackbarUI == null)
    {
      snackbarUI = GameObject.Find("SnackbarUI");
      if (snackbarUI == null)
      {
        return;
      }
      title = GameObject.Find("SnackbarUITitle").GetComponent<TMP_Text>();
      text = GameObject.Find("SnackbarUIMessage").GetComponent<TMP_Text>();
      backgroundImage = GameObject.Find("SnackbarUIBackground").GetComponent<Image>();
      hideText();
    }
  }

  private void hideText()
  {
    title.text = "";
    text.text = "";
    hasMessage = false;
    currentTime = 0;
    backgroundImage.transform.gameObject.SetActive(false);
  }

  public enum SnackbarMessageType
  {
    Information
  }
  public void DisplayMessage(string message, float time = 15f, SnackbarMessageType type =SnackbarMessageType.Information)
  {
    if (backgroundImage != null) { backgroundImage.transform.gameObject.SetActive(true); }
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
        return string.Format(baseString, "GUI", "INFORMATION");
    }
  }
}
