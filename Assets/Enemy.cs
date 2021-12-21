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
        SearchAndFollowPlayer();
        HandleCombat();
        model.transform.Rotate(0, 0, rotationSpeed);

    }

    private void HandleCombat()
    {
        handleHealth();
        if (!weaponControl) { return; }
        if (playerDetector.DetectedPlayer == null)
        { return; }
        var player = playerDetector.DetectedPlayer;
        
        var targetVec = player.transform.position - transform.position;
        var angle = Vector3.Angle(targetVec, transform.forward);


        if (angle > 15 || angle < -15) { return;}
        if (Vector3.Distance(transform.position, player.transform.position) < 20.0F)
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
        if (playerDetector.DetectedPlayer == null) {
            ParticleSystem.Stop();
            return; 
        }
        var player = playerDetector.DetectedPlayer;
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
