using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(CombatParticipant))]
public class Enemy : MonoBehaviour
{
    [SerializeField] protected float minFollowDistance = 5f;
    [SerializeField] protected float maxFollowDistance = 10f;
    [SerializeField] protected float attackAngle = 15f;
    [SerializeField] protected float attackRange = 20f;
    //Used for debugging
    [SerializeField] protected bool isFollowing = false;
    private ActorStats stats;
    [SerializeField] protected PlayerDetector playerDetector;
    [SerializeField] protected WeaponControl weaponControl;
    [SerializeField] protected ParticleSystem aggroParticleSystem;
    
    [SerializeField] protected Transform model;
    [SerializeField] protected float rotationSpeed = 0.5f;
    [SerializeField] protected bool inverse = true;

    [SerializeField] protected List<Transform> dropableItems;

    [Tooltip("Forces the enemy to be idle and not attack")]
    
    
    [SerializeField] protected bool ForceIdle;

    protected NavMeshAgent navMeshAgent;

    protected AnimationController animator;
    
    protected void Start()
    {
        playerDetector = GetComponentInChildren<PlayerDetector>();
        aggroParticleSystem = transform.Find("Particle System").GetComponent<ParticleSystem>();
        if( !model ){ model = transform.Find("Model");}
        navMeshAgent = GetComponent<NavMeshAgent>();
        stats = GetComponent<ActorStats>();
        weaponControl = GetComponent<WeaponControl>();
        aggroParticleSystem.Stop();
        stats.OnHealthReachedZero += dropItems;
        animator = GetComponentInChildren<AnimationController>();
    }

    private void dropItems()
    {
        foreach(var item in dropableItems)
        {
            Instantiate(item,transform.position,Quaternion.identity);
        }
    }

    public void SetForceIdle(bool value)
    {
        ForceIdle = value;
    }
    
    protected void Update()
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
        
        if (!hitSomething) { return; }
        if (!hit.collider.tag.Equals("Player")) { return;}
        if (angle > attackAngle || angle < -attackAngle) { return;}

        var enemyWantsToAttack = playerDetector.Distance < attackRange;
        if (!enemyWantsToAttack) { return; }

        var canAttack = weaponControl.CurrentWeapon.CanAttack();
        if (canAttack)
        {

            TriggerAnimation("Attack");
            weaponControl.UseWeapon();
        }
        
    }
    

    protected virtual void updateAppearance()
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
            ChangeAnimationState("Idle");
            return; 
        }
        
        var player = playerDetector.ReachablePlayer;
        aggroParticleSystem.Play();
        isFollowing = false;
        var distanceToPlayer = playerDetector.Distance;

        if (distanceToPlayer > maxFollowDistance)
        {
            ChangeAnimationState("Idle");
            return;
        }
        
        transform.LookAt(player);
        
        if (distanceToPlayer < minFollowDistance)
        {
            ChangeAnimationState("Idle");
            return;
        }

        ChangeAnimationState("Walk");
        isFollowing = true;
        navMeshAgent.SetDestination(player.position);
    }

    protected void TriggerAnimation(string triggerName)
    {
        if (!animator) { return; }
        animator.Trigger(triggerName);
    }

    protected void ChangeAnimationState(string state)
    {
        if (!animator) return;
        animator.ChangeAnimationState(state);
    }
}
