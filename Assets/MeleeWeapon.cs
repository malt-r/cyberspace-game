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
    public float AttackRadius => attackRadius;
    
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
        var self = Owner.GetComponent<CombatParticipant>();
        var hasEnemies = enemyInRange(out var enemies, self);
        foreach (var enemy in enemies)
        {
            
            StartCoroutine(DelayForDamage(enemy, self));
            
        }

        if (hasEnemies)
        {
            PlayUseSound();
        }
    }
    private IEnumerator DelayForDamage(CombatParticipant enemy, CombatParticipant self){
        yield return new WaitForSeconds(0.633333333333333333333333333f);
        enemy.TakeDamage(self);
        
    }
    private bool enemyInRange(out List<CombatParticipant> enemies, CombatParticipant owner)
    {
        enemies = new List<CombatParticipant>();
        Collider[] hitColliders = new Collider[maxEnemies];
        int foundColliders = Physics.OverlapSphereNonAlloc(this.transform.position, attackRadius, hitColliders,lm);
        for (int i = 0; i < foundColliders; i++)
        {
           
            var enemy = hitColliders[i].GetComponent<CombatParticipant>();
            
            if (enemy == null || Owner ==  null || enemy == owner) continue;
            enemies.Add(enemy);

            Debug.Log($" Attack , {hitColliders[i].name}");
        }

        return enemies.Count > 0;
    }

    public override bool CanAttack()
    {
        var self = Owner.GetComponent<CombatParticipant>();
        return deltaTime > AttackSpeed && enemyInRange(out var dummy ,self);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.11f, 0.85f);
        Gizmos.DrawWireSphere(this.transform.position,attackRadius);
    }
}
