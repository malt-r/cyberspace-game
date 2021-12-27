using TMPro;
using UnityEngine;

public class SnackbarManager : MonoBehaviour
{
  private static SoundManager _instance;
  public static SoundManager Instance => _instance;

  
  [SerializeField] private TMP_Text title;
  [SerializeField] private TMP_Text text;

  [SerializeField] private bool hasMessage;

  [SerializeField] private float lifeTime;
  [SerializeField] private float currentTime;

  void Start()
  {
    title.text = "";
    text.text = "";
  }
  private void Update()
  {
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
  public void DisplayMessage(string message, SnackbarMessageType type =SnackbarMessageType.Information)
  {
    if (title != null)
    { title.text = getDisplayStringOfEnum(type);}
    if (text != null) {text.text = message;}
    hasMessage = true;
  }
  
  public void HideMessage()
  {
    hideText();
  }

  private string getDisplayStringOfEnum(SnackbarMessageType type)
  {
    var defaultString = @"<sprite=""GUI"" name=""INFO""> Information";
    if (type == SnackbarMessageType.Information)
    {
      return defaultString;
    }
    
    Debug.LogError("No String representation found for enum");
    return defaultString;
  }
}
