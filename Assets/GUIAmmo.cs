using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Assets.Weapons;
using UnityEngine;
using UnityEngine.UI;

public class GUIAmmo : MonoBehaviour
{
    private Text text;
    [SerializeField]
    private WeaponControl weaponControl;
    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if(weaponControl.CurrentWeapon.Type == WeaponType.LASER)
        {
            handleLaser();
        }
        
    }

    private void handleLaser()
    {
        var laser = (Laser) weaponControl.CurrentWeapon;
        if (laser.overHeated)
        {
            text.text = "###";
            return;
        }
        var prefix = laser.condition < 15 ? "‼ " : "";
        text.text = prefix+"¤" +laser.condition.ToString(CultureInfo.InvariantCulture);
        
    }
}
