using Assets.Weapons;
using JetBrains.Annotations;
using StarterAssets;
using UnityEngine;

public class Player : MonoBehaviour
{
    [NotNull] 
    private StarterAssetsInputs input;

    [NotNull] 
    private WeaponControl weaponControl;
    // Start is called before the first frame update
    void Start()
    {
        input = GetComponent<StarterAssetsInputs>();
        weaponControl = GetComponent<WeaponControl>();
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
       
    }

    // Update is called once per frame
    void Update()
    {
        if (input.weaponSwitch.y != 0)
        {
            weaponControl.SwitchWeapon(Mathf.FloorToInt((input.weaponSwitch.y)));
            input.weaponSwitch.y = 0;
        }

        if (!input) { return; } 
        if (!input.shoot) { return; }

        input.shoot = false;
        if (!weaponControl) { return; }

        input.shoot = true;
        if (weaponControl.isActiveAndEnabled)
        {
            weaponControl.UseWeapon();
        }
    }

    public void AddWeapon(BaseWeapon weapon)
    {
        weaponControl.AddWeapon(weapon);
    }
}
