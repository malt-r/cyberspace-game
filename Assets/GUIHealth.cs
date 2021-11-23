using System.Collections;
using System.Collections.Generic;
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

    // Update is called once per frame
    void Update()
    {
        text.text = playerStats.CurrentHealth.ToString(CultureInfo.InvariantCulture)+"♥🔫";
    }
}
