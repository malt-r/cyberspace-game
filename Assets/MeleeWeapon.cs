using System.Collections;
using System.Collections.Generic;
using Assets.Weapons;
using UnityEngine;

public class MeleeWeapon : MonoBehaviour, Weapon
{
    public WeaponType Type { get; private set; }
    [SerializeField]
    private float damage =10f;
    public float Damage => damage;
    
    [SerializeField]
    private float useCoolDown =2f;
    public float UseCooldown { get; }

    // Start is called before the first frame update
    void Start()
    {
        Type = WeaponType.MELEE;
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void Use()
    {
        Debug.Log("Melee is used");
    }
}
