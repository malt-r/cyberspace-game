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
      currentTime += Time.fixedDeltaTime;
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

  public void DisplayMessage(string message)
  {
    if (title != null) {title.text = "Info";}
    if (text != null) {text.text = message;}
    hasMessage = true;
  }
  
  public void HideMessage()
  {
    hideText();
  }
}
