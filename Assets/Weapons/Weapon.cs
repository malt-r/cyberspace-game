using Assets.Weapons;

public interface Weapon
{
    WeaponType Type { get; }
    float Damage { get; }
    float UseCooldown { get; }
    void Use();

}
