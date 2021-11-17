using System.Collections;
using System.Collections.Generic;
using Assets.Weapons;
using Unity.VisualScripting;
using UnityEngine;

public class BulletWeapon : BaseWeapon
{
    [SerializeField]
    private Transform bulletPrefab;

    [SerializeField] private Transform firePoint;
    private float deltaTime;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        deltaTime += Time.deltaTime;
    }

    public override void Use()
    {
        if (!(deltaTime > useCooldown)) return;
        deltaTime = 0f;
        var bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(transform.root.forward,Vector3.up)).GetComponentInChildren<Bullet>();
        
        bullet.Ignite(transform.root.forward, transform.root.gameObject.GetInstanceID());

    }
}
