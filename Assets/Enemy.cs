using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(CombatParticipant))]
public class Enemy : MonoBehaviour
{
    [SerializeField] private float minFollowDistance = 5f;
    [SerializeField] private float maxFollowDistance = 10f;
    [SerializeField] private float attackAngle = 15f;
    [SerializeField] private float attackRange = 20f;
    //Used for debugging
    [SerializeField] private bool isFollowing = false;
    private ActorStats stats;
    [SerializeField] private PlayerDetector playerDetector;
    private WeaponControl weaponControl;
    [SerializeField] private ParticleSystem aggroParticleSystem;
    
    [SerializeField] private Transform model;
    [SerializeField] private float rotationSpeed = 0.5f;
    [SerializeField] private bool inverse = true;

    [SerializeField] private List<Transform> dropableItems;

    [Tooltip("Forces the enemy to be idle and not attack")]
    [SerializeField] private bool ForceIdle;

    private NavMeshAgent navMeshAgent;
    
    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        stats = GetComponent<ActorStats>();
        weaponControl = GetComponent<WeaponControl>();
        aggroParticleSystem.Stop();
        stats.OnHealthReachedZero += dropItems;
    }

    private void dropItems()
    {
        foreach(var item in dropableItems)
        {
            Instantiate(item,transform.position,Quaternion.identity);
        }
    }
    
    void Update()
    {
        if (!ForceIdle)
        {
            SearchAndFollowPlayer();
            HandleCombat();
        }
        
        updateAppearance();
    }

    private void HandleCombat()
    {
        
        if (!weaponControl) { return; }
        if (playerDetector.ReachablePlayer == null) { return; }
        var player = playerDetector.ReachablePlayer;

        var ownTransform = transform;
        var ownPosition = ownTransform.position;
        var playerPosition = player.transform.position+ Vector3.up/2;
        var targetVec = playerPosition - ownPosition;
        var angle = Vector3.Angle(targetVec, ownTransform.forward);

        
        var hitSomething = Physics.Raycast(ownPosition, targetVec, out var hit);
        //Debug.DrawRay(ownPosition, targetVec, Color.green);
        
        if (!hitSomething) { return; }
        if (!hit.collider.tag.Equals("Player")) { return;}

        if (angle > attackAngle || angle < -attackAngle) { return;}

        var enemyWantsToAttack = playerDetector.Distance < attackRange;
        if (!enemyWantsToAttack) { return; }

        var canAttack = weaponControl.CurrentWeapon.CanAttack();
        if(canAttack){ weaponControl.UseWeapon(); }
        }

    private void updateAppearance()
    {
        model.transform.Rotate(0, 0, rotationSpeed);
        var deadRatio = (stats.CurrentHealth + 1) / stats.maxHealth;
        deadRatio = Mathf.Clamp(deadRatio, 0.5f, 1f);
        if (inverse)
        {
            deadRatio = 2 / (deadRatio + 1);
            deadRatio = Mathf.Clamp(deadRatio, 1f, 2);
        }
        var newScale = new Vector3(deadRatio, deadRatio,  deadRatio);
        transform.localScale = newScale;
    }

    private void SearchAndFollowPlayer()
    {
        if (playerDetector.ReachablePlayer == null) {
            aggroParticleSystem.Stop();
            return; 
        }
        var player = playerDetector.ReachablePlayer;
        aggroParticleSystem.Play();

        isFollowing = false;
        
        var distanceToPlayer = playerDetector.Distance;
        
        if (distanceToPlayer > maxFollowDistance) { return; }
        var playerPosition = player.position;
        transform.LookAt(player);
        if (distanceToPlayer < minFollowDistance) { return; }
        isFollowing = true;
        navMeshAgent.SetDestination(playerPosition);
    }
}
