using UnityEngine;

public class LineRendererControl : MonoBehaviour
{
    public Transform Boss;
    private LineRenderer lineRender;
    public Transform startPoint;
    void Start()
    {
        lineRender = GetComponent<LineRenderer>();
        Boss = GameObject.Find("Boss").transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (!Boss)
        {
            Boss = GameObject.Find("Boss").transform;
        }

        
        lineRender.SetPosition(0,startPoint.position);
        lineRender.SetPosition(1,Boss.position);

    }
}
