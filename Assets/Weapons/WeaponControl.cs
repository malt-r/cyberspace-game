using StarterAssets;
using Assets.Weapons;
using UnityEngine;

public class WeaponControl : MonoBehaviour
{
    public Weapon CurrentWeapon { get; set; }
    public Transform WeaponHolder;
    [SerializeField]
    private int weaponIndex;
    public bool Use { get; set; }

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
    private void Update()
    {
        CurrentWeapon = WeaponHolder.GetChild(weaponIndex).GetComponent<Weapon>();
    }

    public void UseWeapon()
    {
        CurrentWeapon.Use();
    }


}
