using System.Collections;
using System.Collections.Generic;
using Assets.Weapons;
using UnityEngine;


public class BombThrower : BaseWeapon
{
    public float range = 500;
    public WeaponType Type { get; private set; }

    public bool shooted = false;
    public float condition = 100f;
    public float maxCondition = 100f;
    public bool overHeated = false;

    public float heatRate = 25f;
    public float cooldownRate = 10f;
    public float cooldownTime = 5;

    private float deltaTime;

    
    void Awake()
    {
        Type = WeaponType.BOMBTHROWER;
        deltaTime = atackSpeed+1;
    }

    public override void Use()
    {
        shooted = true;
    }

    public void LateUpdate()
    {

      
    }
    
    IEnumerator StartWeaponCooldown()
    {
        shooted = false;
        yield return new WaitForSeconds(cooldownTime); 
        overHeated = false;
    }
}