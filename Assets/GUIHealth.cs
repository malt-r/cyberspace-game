using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class GUIHealth : MonoBehaviour
{
    private Text text;
    [SerializeField]
    private ActorStats playerStats;
    void Start()
    {
        text = GetComponent<Text>();
    }
    
    void LateUpdate()
    {
        text.text = "♥" + playerStats.CurrentHealth.ToString(CultureInfo.InvariantCulture);
    }
}
