using System.Collections;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    [SerializeField]
    private float velocity;
    [SerializeField]
    private float damage;

    [SerializeField] 
    private float igniteTime;
    [SerializeField]
    private int maxTargets;

    [SerializeField] 
    private float attackRadius;
    [SerializeField] 
    private LayerMask attackLayers;

    [SerializeField] 
    private Transform logicBombPrefab;

    private GameObject owner;
    
    public void Ignite(GameObject owner)
    {
        this.owner = owner;
        StartCoroutine(igniteBomb());
    }

    private void explode()
    {
        Instantiate(logicBombPrefab, transform.position, Quaternion.identity);
        Collider[] hitColliders = new Collider[maxTargets];
        int foundColliders = Physics.OverlapSphereNonAlloc(transform.position, attackRadius, hitColliders,attackLayers);
        for (int i = 0; i < foundColliders; i++)
        {
            if (owner.tag.Equals("Player"))
            {
                var enemy = hitColliders[i].GetComponent<CombatParticipant>();
                if(enemy) { 
                    enemy.TakeDamage(damage, true);
                }
            }
            else
            {
                var enemyIsPlayer = hitColliders[i].tag.Equals("Player");
                if (!enemyIsPlayer) { continue; }

                var enemy = hitColliders[i].GetComponent<CombatParticipant>();
                enemy.TakeDamage(damage, true);
                
            }
            Debug.Log($" Bomb damages , {hitColliders[i].name} {damage} points");
            
            
        }


        Destroy(gameObject);
    }

    IEnumerator igniteBomb()
    {
        yield return new WaitForSeconds(igniteTime);
        explode();
    }

}