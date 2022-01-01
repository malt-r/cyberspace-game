using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenue : MonoBehaviour
{
   private void Start()
   {
      SoundManager.Instance.SetBackgroundMusic("Menue");
   }

   public void PlayGame()
   {
      SoundManager.Instance.SetBackgroundMusic("Game");
      FindObjectOfType<GameManager>().LoadNextGameLevel();
   }

   public void QuitGame()
   {
      Application.Quit();
   }
}
