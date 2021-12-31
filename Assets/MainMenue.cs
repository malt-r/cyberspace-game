using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenue : MonoBehaviour
{
   [SerializeField] private string playScene;

   private void Start()
   {
      SoundManager.Instance.SetBackgroundMusic("Menue");
   }

   public void PlayGame()
   {
      SoundManager.Instance.SetBackgroundMusic("Game");
      FindObjectOfType<GameManager>().LoadScene(playScene);
      //SceneManager.LoadScene(playScene);
   }

   public void QuitGame()
   {
      Application.Quit();
   }
}
