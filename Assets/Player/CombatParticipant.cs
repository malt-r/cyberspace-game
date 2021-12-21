using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ActorStats))]
public class CombatParticipant : MonoBehaviour
{
    private ActorStats stats;
    private WeaponControl weaponControl;
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
        stats.TakeDamage(enemy.weaponControl.Damage);
    }

    public void TakeDamage(float amount)
    {
        stats.TakeDamage(amount);
    }

}
