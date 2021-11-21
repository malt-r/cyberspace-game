using Assets.Weapons;
using UnityEngine;

public interface Weapon
{
    WeaponType Type { get; }
    float Damage { get; }
    float UseCooldown { get; }
    void Use();
    Transform Owner { get; }

}
