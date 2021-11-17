using System;
using UnityEngine;

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
    // Start is called before the first frame update
    void Start()
    {
        stats = GetComponent<ActorStats>();
        playerDetector = GetComponentInChildren<PlayerDetector>();
        combat = GetComponent<CombatParticipant>();
        weaponControl = GetComponent<WeaponControl>();
        ParticleSystem.Stop();
    }

    // Update is called once per frame
    void Update()
    {
        HandleCombat();
        SearchAndFollowPlayer();
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

        Debug.Log(angle);
        if (angle > 15) { return;}
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
        var newScale = new Vector3(100f * deadRatio, 100f * deadRatio, 100f * deadRatio);

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
        
        if (distance < minFollowDistance) { return; }
        isFollowing = true;

        var playerPosition = player.position;
        var finalPos = playerPosition;
        finalPos.y += 2;
        var smooth = Vector3.zero;
        transform.position = Vector3.SmoothDamp(transform.position, finalPos, ref smooth, Speed*Time.deltaTime);
        transform.LookAt(playerPosition);
    }
}
