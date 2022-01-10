using Assets.Weapons;
using UnityEngine;

public class BulletWeapon : BaseWeapon
{
    [SerializeField]
    private Transform bulletPrefab;
    
    private float deltaTime;

    void Awake()
    {
        Type = WeaponType.BULLET;
    }
    void Update()
    {
        deltaTime += Time.deltaTime;
    }

    public override void Use()
    {
        if (!CanAttack()) return;
        PlayUseSound();
        deltaTime = 0f;
        var bullet = Instantiate(bulletPrefab, Firepoint.position+Firepoint.forward*bulletPrefab.transform.localScale.z, Quaternion.LookRotation(Camera.forward,Vector3.up)).GetComponentInChildren<Bullet>();
        bullet.damage = damage;
        bullet.Ignite(Owner.gameObject.GetInstanceID());

    }

    public override bool CanAttack() { return deltaTime > atackSpeed; }
}
