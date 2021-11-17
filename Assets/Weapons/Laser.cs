using System.Collections;
using Assets.Weapons;
using UnityEngine;


public class Laser : BaseWeapon
{
    public Transform firepoint;
    public float range = 500;
    public WeaponType Type { get; private set; }

    public bool shooted = false;
    private LineRenderer lineRenderer;
    private CombatParticipant owner;

    private ParticleSystem overheatParticleSystem;

    public float condition = 100f;
    public float maxCondition = 100f;
    public bool overHeated = false;

    public float heatRate = 25f;
    public float cooldownRate = 10f;
    public float cooldownTime = 5;

    private float deltaTime;
    
    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        owner = transform.parent.parent.GetComponent<CombatParticipant>();
        overheatParticleSystem = GetComponentInChildren<ParticleSystem>();
        overheatParticleSystem.Stop();
        Type = WeaponType.LASER;
        deltaTime = useCooldown+1;
    }
    
    void UpdateRoute()
    {
        if (!shooted || overHeated){
            lineRenderer.enabled = false;
            return; 
        }

        shooted = false;
        lineRenderer.enabled = true; 
        lineRenderer.SetPosition(0, firepoint.position);
        if (Physics.Raycast(firepoint.position, firepoint.forward, out var hit))
        {
            lineRenderer.SetPosition(1, hit.point);
            
            var enemy = hit.collider.gameObject.GetComponent<CombatParticipant>();
            if (!enemy) { return; }
            var parent = transform.root.GetComponent<CombatParticipant>();
            
            if (!(deltaTime > useCooldown)) return;
            deltaTime = 0f;
            enemy.TakeDamage(parent);
        }
        else
        {
            lineRenderer.SetPosition(1, firepoint.forward * range);
        }
        
        
        
    }
    public override void Use()
    {
        shooted = true;
    }

    public void LateUpdate()
    {
        deltaTime += Time.deltaTime;
        UpdateRoute();
        DrawParticleSystem();
        handleHeatingAndCooling();
    }
    
    private void handleHeatingAndCooling()
    {
        if (shooted && !overHeated)
        {
            condition -= Time.deltaTime*heatRate;
            if (condition < 0)
            {
                condition = 0;
                overHeated = true;
                StartCoroutine(StartWeaponCooldown());
            }
        }
        else if(!shooted && condition<maxCondition && !overHeated)
        {
            condition += Time.deltaTime*cooldownRate;
            if (condition > maxCondition)
            {
                condition = maxCondition;
            }
        }
       
               
    }
    

    private void DrawParticleSystem()
    {
        if (overHeated && overheatParticleSystem.isStopped)
        {
            overheatParticleSystem.Play();
        }
        else if(!overHeated &&  overheatParticleSystem.isPlaying)
        {
            overheatParticleSystem.Stop();
        }
    }

    IEnumerator StartWeaponCooldown()
    {
        yield return new WaitForSeconds(cooldownTime); 
        overHeated = false;
    }
}