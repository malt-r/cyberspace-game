using System.Globalization;
using Unity.VisualScripting;
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
        var health = playerStats.CurrentHealth;
        bool critical = health < 15;
        var suffix = critical ? "‼" : "";
        text.color = critical ? Color.red : Color.white;
        text.text = "♥" + playerStats.CurrentHealth.ToString("000",CultureInfo.InvariantCulture) + suffix;
    }
}
