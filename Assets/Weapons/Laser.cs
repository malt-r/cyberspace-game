using Assets.Weapons;
using UnityEngine;


public class Laser : BaseWeapon
{
    
    public float range = 500;
    public bool shooted = false;
    public bool overHeated = false;
    private LineRenderer lineRenderer;
    public float deltaTime;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        deltaTime = atackSpeed+1;
        Type = WeaponType.LASER;

    }
    
    public void UpdateRoute()
    {
        if (!shooted || overHeated){
            lineRenderer.enabled = false;
            audioSource.volume = 0;
            return; 
        }
        if(!audioSource.isPlaying){
            PlayUseSound();
        }
        shooted = false;
        lineRenderer.enabled = true; 
        lineRenderer.SetPosition(0, Firepoint.position);
        if (Physics.Raycast(Camera.position, Camera.forward, out var hit))
        {
            
            lineRenderer.SetPosition(1, hit.point);
            if(hit.distance>range){
                lineRenderer.SetPosition(1,Camera.transform.position + Camera.forward * range);
                return;
            }   
            var enemy = hit.collider.gameObject.GetComponent<CombatParticipant>();
            if (!enemy) { return; }
            var parent = Owner.GetComponent<CombatParticipant>();
            
            if (!(deltaTime > atackSpeed)) return;
            deltaTime = 0f;
            enemy.TakeDamage(parent);
        }
        else
        {
            lineRenderer.SetPosition(1,Camera.transform.position + Camera.forward * range);
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
    }
}