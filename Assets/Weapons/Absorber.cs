using Assets.Weapons;
using UnityEngine;


public class Absorber : BaseWeapon
{
    public float range = 500;
    public bool shooted = false;
    public bool overHeated = false;
    private LineRenderer lineRenderer;
    public float deltaTime;
    
    void Awake()
    {
        Type = WeaponType.ABSORBER;
        lineRenderer = GetComponent<LineRenderer>();
        deltaTime = atackSpeed+1;
    }
    public void UpdateRoute()
    {
        if (!shooted || overHeated){
            lineRenderer.enabled = false;
            audioSource.volume = 0;
            return; 
        }

        shooted = false;
        lineRenderer.enabled = true; 
        if(!audioSource.isPlaying){
           PlayUseSound();
        }
        lineRenderer.SetPosition(0, Firepoint.position);
        if (Physics.Raycast(Camera.position, Camera.forward, out var hit))
        {
            lineRenderer.SetPosition(1, hit.point);
            
            var item = hit.collider.GetComponent<BaseItem>();

            if (item != null)
            {
                handleItem(item);
                return;
            }
        }
        else
        {
            lineRenderer.SetPosition(1,Camera.forward * range);
        }
    }

    private void handleItem(BaseItem item)
    {
        var smooth = Vector3.zero;
        var Speed = 0.2f;
        item.transform.position = Vector3.SmoothDamp( transform.position,item.transform.position, ref smooth, Speed*Time.deltaTime);
        
        var distance = Vector3.Distance(transform.position, item.transform.position);
        if(distance <2){
            item.Visit(Owner);
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