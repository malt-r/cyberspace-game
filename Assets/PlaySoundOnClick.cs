using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySoundOnClick : MonoBehaviour
{
   public void PlayGUIClickSound()
   {
      SoundManager.PlaySound(Sound.GUICLick);
   }
}
