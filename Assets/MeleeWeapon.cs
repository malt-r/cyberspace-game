using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Weapons;
using UnityEngine;

public class MeleeWeapon : BaseWeapon
{
    private float deltaTime;
    [SerializeField]
    private int maxEnemies = 1;

    [SerializeField] 
    private float attackRadius = 5f;
    // Start is called before the first frame update

    public  LayerMask lm;
    void Start()
    {
        Type = WeaponType.MELEE;
    }

    // Update is called once per frame
    void Update()
    {
        deltaTime += Time.deltaTime;
    }


    public override void Use()
    {
        if (!CanAttack()) return;
        deltaTime = 0f;
        //TODO: Work with layers to get enemies more performant
        Collider[] hitColliders = new Collider[maxEnemies];
        int foundColliders = Physics.OverlapSphereNonAlloc(this.transform.position, attackRadius, hitColliders,lm);
        for (int i = 0; i < foundColliders; i++)
        {
           
            var enemy = hitColliders[i].GetComponent<CombatParticipant>();
            var self = Owner.GetComponent<CombatParticipant>();
            if (enemy == null || Owner ==  null || enemy == self) continue;
            enemy.TakeDamage(self);
            PlayUseSound();
            Debug.Log($" Attack , {hitColliders[i].name}");
        }
    }

    public override bool CanAttack() { return deltaTime > AttackSpeed; }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.11f, 0.85f);
        Gizmos.DrawWireSphere(this.transform.position,attackRadius);
    }
}
