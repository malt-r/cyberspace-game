using Assets.Weapons;
using UnityEngine;

public class BulletWeapon : BaseWeapon
{
    [SerializeField]
    private Transform bulletPrefab;
    
    [SerializeField] private Transform firePoint;
    private float deltaTime;

    void Update()
    {
        deltaTime += Time.deltaTime;
    }

    public override void Use()
    {
        if (!(deltaTime > useCooldown)) return;
        deltaTime = 0f;
        var bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(Owner.forward,Vector3.up)).GetComponentInChildren<Bullet>();
        
        bullet.Ignite(Owner.gameObject.GetInstanceID());

    }
}
