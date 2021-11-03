using UnityEngine;

public class Weapon : MonoBehaviour
{
    public Transform firepoint;
    public float range = 500;
    public bool shoot = false;
    private LineRenderer lr;


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
        lr = GetComponent<LineRenderer>();
    }

    void UpdateRoute()
    {
        if (!shoot){
            if (lr.enabled) { lr.enabled = false; }
            return; 
        }
        lr.enabled = true; 
        lr.SetPosition(0, firepoint.position);
        RaycastHit hit;
        if (Physics.Raycast(firepoint.position, firepoint.forward, out hit))
        {
            if (hit.collider)
            {
                lr.SetPosition(1, hit.point);
            }
        }
        else
        {

            //lr.SetPosition(1, transform.position + (transform.forward * range));
            lr.SetPosition(1, firepoint.forward * 5000);
        }
    }

}