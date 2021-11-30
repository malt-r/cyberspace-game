using System.Globalization;
using Assets.Weapons;
using UnityEngine;
using UnityEngine.UI;

public class GUIAmmo : MonoBehaviour
{
    private Text text;
    [SerializeField]
    private WeaponControl weaponControl;

    void Start()
    {
        text = GetComponent<Text>();
    }
    
    void LateUpdate()
    { 
        updateProperties();
    }

    private void updateProperties()
    {
        var laser = (Scanner) weaponControl.CurrentWeapon;
        if (laser.overHeated)
        {
            text.text = "###";
            return;
        }
        var prefix = laser.condition < 15 ? "‼" : "";
        var laserCondition = Mathf.Floor(laser.condition).ToString(CultureInfo.InvariantCulture);
        text.text = prefix + "¤" + laserCondition;
        
    }
}
