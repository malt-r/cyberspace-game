using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(ActorStats))]
public class CombatParticipant : MonoBehaviour
{
    private ActorStats stats;
    private WeaponControl weaponControl;
    
    
	[SerializeField] 
	private bool onlyPlayerCanDamageMe = false;

    public bool OnlyPlayerCanDamageMe => onlyPlayerCanDamageMe;
    
    // Start is called before the first frame update
    void Start()
    {
        stats = GetComponent<ActorStats>();
        weaponControl = GetComponent<WeaponControl>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Attack(CombatParticipant enemy)
    {
        Debug.Log("Attacking");
        enemy.TakeDamage(this);
    }

    public void TakeDamage(CombatParticipant enemy)
    {
        Debug.Log("Getting Damage");

        if (onlyPlayerCanDamageMe)
        {
            if (!enemy.CompareTag("Player"))
            {
                return;
            }
        }
        
        stats.TakeDamage(enemy.weaponControl.Damage, byEnemy: true);
    }

    public void TakeDamage(float amount, bool bomb = false, bool byEnemy = false, bool byLava = false)
    {
        stats.TakeDamage(amount,bomb, byEnemy, byLava);
    }

    public void Revive()
    {
        stats.HealToMaxHealth();
    }
}
