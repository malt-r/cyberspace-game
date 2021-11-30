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
        CurrentWeapon = WeaponHolder.GetChild(weaponIndex).GetComponent<BaseWeapon>();
        CurrentWeapon.InitWeapon(Owner, Camera, Firepoint);
    }

    public void UseWeapon()
    {
        CurrentWeapon.Use();
    }

    public void SwitchWeapon(int indexDelta)
    {
        var scanner = CurrentWeapon as Scanner;
        //TODO: Switching passiert hier
        //Weil Monster auch switchen eventuell
        scanner.SwitchMode(indexDelta);
    }

    public void AddWeapon(BaseWeapon weapon)
    {
        var scanner = CurrentWeapon as Scanner;
        scanner.AddSkill(weapon);
    }

}
