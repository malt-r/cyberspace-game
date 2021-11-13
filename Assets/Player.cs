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
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!input) { return; } 
        if (!input.shoot) { return; }
        if (!weaponControl) { return; }
        
        weaponControl.UseWeapon();
    }
}
