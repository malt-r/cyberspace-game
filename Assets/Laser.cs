using UnityEngine;

public class Laser : MonoBehaviour
{
    public Transform firepoint;
    public float range = 500;
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

    // Update is called once per frame
    void LateUpdate()
    {
        
    }
}