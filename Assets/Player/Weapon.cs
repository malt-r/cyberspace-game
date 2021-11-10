using System;
using System.Collections;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public Transform firepoint;
    public float range = 500;
    public float damage = 1;
    public bool shoot = false;
    private LineRenderer lineRenderer;
    private CombatParticipant owner;
    public bool hit2 = false;

    public float condition = 100f;
    public float maxCondition = 100f;
    public bool overHeated = false;

    public float heatRate = 25f;
    public float cooldownRate = 10f;
    public float cooldownTime = 5;


    private void OnEnable()
    {
        Application.onBeforeRender += UpdateRoute;
    }

    private void OnDisable()
    {
        Application.onBeforeRender -= UpdateRoute;
    }

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        owner = transform.parent.GetComponent<CombatParticipant>();
    }

    void UpdateRoute()
    {
        Heat();
        if (!shoot || overHeated){
            if (lineRenderer.enabled) { lineRenderer.enabled = false; }
            return; 
        }
        lineRenderer.enabled = true; 
        lineRenderer.SetPosition(0, firepoint.position);
        RaycastHit hit;
        hit2 = false;
        
        if (Physics.Raycast(firepoint.position, firepoint.forward, out hit))
        {
            
            if (hit.collider)
            {
                hit2 = true;
                lineRenderer.SetPosition(1, hit.point);
                var enemy = hit.collider.gameObject.GetComponent<CombatParticipant>();
                if (!enemy) { return; }
                var parent = transform.parent.parent.GetComponent<CombatParticipant>();
                Debug.Log("Laser Firing");
                enemy.TakeDamage(parent);  
               
            }
        }
        else
        {

            lineRenderer.SetPosition(1, firepoint.forward * range);
        }
    }

    private void Heat()
    {
        if (shoot && !overHeated)
        {
            condition -= Time.deltaTime*heatRate;
            if (condition < 0)
            {
                condition = 0;
                overHeated = true;
                StartCoroutine(StartWeaponCooldown());
            }
        }
        else if(!shoot && condition<maxCondition && !overHeated)
        {
            condition += Time.deltaTime*cooldownRate;
            if (condition > maxCondition)
            {
                condition = maxCondition;
            }
        }
       
               
    }

    IEnumerator StartWeaponCooldown()
    {
        yield return new WaitForSeconds(cooldownTime); 
        overHeated = false;
    }
}