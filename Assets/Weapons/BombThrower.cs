using Assets.Weapons;
using UnityEngine;

public class BombThrower : BaseWeapon
{
    [SerializeField]
    private Transform bombPrefab;

    [SerializeField] 
    private float throwingForceMultiplier;
    private float deltaTime;

    void Awake()
    {
        Type = WeaponType.BOMBTHROWER;
        
        // can attack on spawn
        deltaTime = atackSpeed + 1;
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
        var forward = Camera.forward;
        var bomb = Instantiate(bombPrefab, Firepoint.position, Quaternion.LookRotation(forward,Vector3.up)).GetComponentInChildren<Bomb>();
        bomb.GetComponent<Rigidbody>().AddForce(forward*throwingForceMultiplier,ForceMode.Impulse);
        bomb.Ignite(this.Owner.gameObject);

    }

    public override bool CanAttack()
    {
        return deltaTime > atackSpeed;
    }
}