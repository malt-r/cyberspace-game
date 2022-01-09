using System.Collections;
using Assets.Weapons;
using UnityEngine;

public class WeaponControl : MonoBehaviour
{
    public BaseWeapon CurrentWeapon { get; set; }
    public Transform WeaponHolder;
    [SerializeField]
    private int weaponIndex;
    public bool Use { get; set; }

    public Transform Camera;
    public Transform Owner;
    public Transform Firepoint;

    public float Damage => CurrentWeapon.Damage;

    // Start is called before the first frame update
    private void Awake()
    {
        Debug.Log("Awake");
        if (CurrentWeapon == null)
        {
            Debug.LogWarning("Weapon null");
        }
       
    }

    // Update is called once per frame
    private void Start()
    {
        CurrentWeapon = getWeaponByIndex(weaponIndex);
        CurrentWeapon.InitWeapon(Owner, Camera, Firepoint);
    }

    private BaseWeapon getWeaponByIndex(int newWeaponIndex)
    {
        return WeaponHolder.GetChild(newWeaponIndex).GetComponent<BaseWeapon>();
    }

    public void UseWeapon()
    {
        if (CurrentWeapon == null) return;
        
        CurrentWeapon.Use();
        
    }
    
    
   

    public void SwitchWeapon(int indexDelta)
    {
        if (CurrentWeapon == null) return;
        
        var scanner = CurrentWeapon as Scanner;
        if(scanner){
        scanner.SwitchMode(indexDelta);
        }
        //Is monster
        else
        {
            weaponIndex += indexDelta;
            CurrentWeapon =getWeaponByIndex(weaponIndex);
            CurrentWeapon.InitWeapon(Owner, Camera, Firepoint);
        }
    }

    public void AddWeapon(BaseWeapon weapon)
    {
        var scanner = CurrentWeapon as Scanner;
        scanner.AddSkill(weapon);
    }

}
