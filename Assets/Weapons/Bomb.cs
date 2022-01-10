using System.Collections;
using System.Linq;
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
        foreach (var collider in hitColliders.ToArray())
        {
            if (owner.tag.Equals("Player"))
            {
                if (collider != null)
                {
                    var enemy = collider.GetComponent<CombatParticipant>();
                    if (enemy != null) { 
                        enemy.TakeDamage(damage, true);
                    }
                }
            }
            else
            {
                if (collider != null)
                {
                    var enemyIsPlayer = collider.tag.Equals("Player");
                    if (!enemyIsPlayer) { continue; }

                    var enemy = collider.GetComponent<CombatParticipant>();
                    if (enemy != null)
                    {
                        if (enemy.OnlyPlayerCanDamageMe)
                        {
                            if (owner.CompareTag("Player"))
                            {
                                enemy.TakeDamage(damage, true);
                            }
                        }
                        else
                        {
                            enemy.TakeDamage(damage, true);
                        }
                    }
                }
            }
            
            if (collider != null)
            {
                Debug.Log($" Bomb damages , {collider.name} {damage} points");
            }
        }

        Destroy(gameObject);
    }

    IEnumerator igniteBomb()
    {
        yield return new WaitForSeconds(igniteTime);
        explode();
    }

}