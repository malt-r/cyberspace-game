using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponControl : MonoBehaviour
{
    public Weapon currentWeapon;
    private StarterAssetsInputs input;
    public float Damage { get { return currentWeapon.damage; } }
    // Start is called before the first frame update
    void Start()
    {
        input = GetComponent<StarterAssetsInputs>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!currentWeapon) { return; }
        currentWeapon.shoot = input.shoot;
    }


}
