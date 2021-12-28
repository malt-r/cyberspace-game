using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(CombatParticipant))]
public class Enemy : MonoBehaviour
{
    public float Speed = 1f;
    public float minFollowDistance = 5f;
    public float maxFollowDistance = 10f;
    //Used for debugging
    [SerializeField]
    private bool isFollowing = false;
    public float distance;
    private ActorStats stats;
    private PlayerDetector playerDetector;
    private CombatParticipant combat;
    private WeaponControl weaponControl;
    public ParticleSystem ParticleSystem;
    [SerializeField]
    private Transform model;

    [SerializeField]
    private float rotationSpeed = 0.5f;

    public bool inverse = true;

    [SerializeField]
    private List<Transform> dropableItems;

    [Tooltip("Forces the enemy to be idle and not attack")]
    [SerializeField] 
    private bool ForceIdle;

    private NavMeshAgent navMeshAgent;
    // Start is called before the first frame update
    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        stats = GetComponent<ActorStats>();
        playerDetector = GetComponentInChildren<PlayerDetector>();
        combat = GetComponent<CombatParticipant>();
        weaponControl = GetComponent<WeaponControl>();
        ParticleSystem.Stop();
        stats.OnHealthReachedZero += dropItems;
    }

    private void dropItems()
    {
        foreach(var item in dropableItems)
        {
            Instantiate(item,transform.position,Quaternion.identity);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!ForceIdle)
        {
            SearchAndFollowPlayer();
        }
        HandleCombat(ForceIdle);
        model.transform.Rotate(0, 0, rotationSpeed);
    }

    private void HandleCombat(bool forceIdle = false)
    {
        handleHealth();
        if (!weaponControl) { return; }
        if (playerDetector.ReachablePlayer == null) { return; }
        var player = playerDetector.ReachablePlayer;

        var ownTransform = transform;
        var ownPosition = ownTransform.position;
        var playerPosition = player.transform.position;
        var targetVec = playerPosition - ownPosition;
        var angle = Vector3.Angle(targetVec, ownTransform.forward);

        
        var hitSomething = Physics.Raycast(ownPosition, targetVec, out var hit);
        Debug.DrawRay(ownPosition, targetVec, Color.green);
        
        if (!hitSomething) { return; }
        if (!hit.collider.tag.Equals("Player")) { return;}

        distance = Vector3.Distance(ownPosition, hit.transform.position);

        if (angle > 15 || angle < -15) { return;}
        if (!(distance < 20.0F)) return;
        
        if (!forceIdle)
        {
            weaponControl.UseWeapon();
        }
    }

    private void handleHealth()
    {
        float deadRatio = (stats.CurrentHealth + 1) / stats.maxHealth;
        deadRatio = Mathf.Clamp(deadRatio, 0.5f, 1f);
        if (inverse)
        {
            deadRatio = 2 / (deadRatio + 1);
            deadRatio = Mathf.Clamp(deadRatio, 1f, 2);
        }
        var newScale = new Vector3(deadRatio, deadRatio,  deadRatio);
        this.transform.localScale = newScale;
    }

    private void SearchAndFollowPlayer()
    {
        if (playerDetector.ReachablePlayer == null) {
            ParticleSystem.Stop();
            return; 
        }
        var player = playerDetector.ReachablePlayer;
        ParticleSystem.Play();
        distance = Vector3.Distance(transform.position, player.position);

        isFollowing = false;
        if (distance > maxFollowDistance) { return; }
        var playerPosition = player.position;
        transform.LookAt(playerPosition);
        if (distance < minFollowDistance) { return; }
        isFollowing = true;
        navMeshAgent.SetDestination(playerPosition);
    }
}
