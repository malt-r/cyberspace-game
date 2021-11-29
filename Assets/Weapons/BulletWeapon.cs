using Assets.Weapons;
using UnityEngine;

public class BulletWeapon : BaseWeapon
{
    [SerializeField]
    private Transform bulletPrefab;
    
    private float deltaTime;

    void Update()
    {
        deltaTime += Time.deltaTime;
    }

    public override void Use()
    {
        if (!(deltaTime > atackSpeed)) return;
        deltaTime = 0f;
        var bullet = Instantiate(bulletPrefab, Firepoint.position, Quaternion.LookRotation(Camera.forward,Vector3.up)).GetComponentInChildren<Bullet>();
        
        bullet.Ignite(Owner.gameObject.GetInstanceID());

    }
}
