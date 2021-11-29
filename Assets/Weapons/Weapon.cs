using Assets.Weapons;
using UnityEngine;

public interface Weapon
{
    WeaponType Type { get; }
    float Damage { get; }
    float AttackSpeed { get; }
    void Use();
    Transform Owner { get; }

    float UseCost { get; }
}
