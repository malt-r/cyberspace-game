using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ActorStats))]
public class CombatParticipant : MonoBehaviour
{
    private ActorStats myStats;
    // Start is called before the first frame update
    void Start()
    {
        myStats.GetComponent<ActorStats>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Attack(ActorStats enemy)
    {
        enemy.TakeDamage(myStats.damage);
    }
}
