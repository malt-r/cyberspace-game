using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Weapons;
using UnityEngine;

public class MeleeWeapon : BaseWeapon
{
    public WeaponType Type { get; private set; }
    
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
        deltaTime = atackSpeed+1;
    }

    // Update is called once per frame
    void Update()
    {
        deltaTime += Time.deltaTime;
    }


    public override void Use()
    {
        if (!(deltaTime > atackSpeed)) return;
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
            Debug.Log($" Attack , {hitColliders[i].name}");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.11f, 0.85f);
        Gizmos.DrawWireSphere(this.transform.position,attackRadius);
    }
}
