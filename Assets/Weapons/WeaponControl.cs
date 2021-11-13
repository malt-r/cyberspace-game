using StarterAssets;
using Assets.Weapons;
using UnityEngine;

public class WeaponControl : MonoBehaviour
{
    public Weapon CurrentWeapon { get; set; }
    public Transform WeaponHolder;

    public bool Use { get; set; }

    public float Damage => CurrentWeapon.Damage;

    // Start is called before the first frame update
    private void Awake()
    {
        Debug.Log("Awake");
        CurrentWeapon = WeaponHolder.GetChild(0).GetComponent<Weapon>();
        if (CurrentWeapon == null)
        {
            Debug.LogWarning("Weapon null");
        }
    }

    // Update is called once per frame
    private void Update()
    {
        //CurrentWeapon.using = Use;
    }

    public void UseWeapon()
    {
        CurrentWeapon.Use();
    }


}
