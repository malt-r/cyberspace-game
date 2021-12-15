using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenue : MonoBehaviour
{
   [SerializeField] private string playScene;
   
   public void PlayGame()
   {
      SoundManager.Instance.SetBackgroundMusic("Game");
      SceneManager.LoadScene(playScene);
   }

   public void QuitGame()
   {
      Application.Quit();
   }
}
