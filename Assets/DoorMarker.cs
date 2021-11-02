using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DoorMarker : MonoBehaviour
{
    public Vector3 GetDirection()
    {
        return this.transform.forward;
    }

    // TODO: associate with room? This would need some component to attach to each
    // room...

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmosSelected()
    {
        // draw thicc line to indicate direction of door marker
        var p1 = this.transform.position;
        var p2 = this.transform.position + this.transform.forward * 3;
        var thickness = 8;
        Handles.DrawBezier(p1,p2,p1,p2, Color.blue, null, thickness);
    }
}
