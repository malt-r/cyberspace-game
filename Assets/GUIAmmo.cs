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
        if (weaponControl.isActiveAndEnabled)
        {
            var laser = (Scanner) weaponControl.CurrentWeapon;
            if (laser.overHeated)
            {
                text.text = "###";
                return;
            }
            
            bool critical = laser.condition < 15;
            var prefix = critical ? "‼" : "";
            text.color = critical ? Color.red : Color.white;
            
            var laserCondition = laser.condition.ToString("000", CultureInfo.InvariantCulture);
            text.text = prefix + "¤" + laserCondition;
        }
    }
}
