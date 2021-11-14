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
        transform.Rotate(0, rotationSpeed, 0);

    }

    private void HandleCombat()
    {
        handleHealth();
        if (!weaponControl) { return; }
        if (playerDetector.DetectedPlayer == null)
        {
            return;
        }
        var player = playerDetector.DetectedPlayer;
        if (Vector3.Distance(this.transform.position, player.transform.position) < 5.0F)
        {
            weaponControl.UseWeapon();
        }
    }

    private void handleHealth()
    {
        float deadRatio = (stats.CurrentHealth + 1) / stats.maxHealth;
        deadRatio = Mathf.Clamp(deadRatio, 0.5f, 1);
        if (inverse)
        {
            deadRatio = 2 / (deadRatio + 1);
            deadRatio = Mathf.Clamp(deadRatio, 1, 1.5f);
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

        var finalPos = player.position;
        finalPos.y += 2;
        var smooth = Vector3.zero;
        transform.position = Vector3.SmoothDamp(transform.position, finalPos, ref smooth, Speed*Time.deltaTime);
    }
}
