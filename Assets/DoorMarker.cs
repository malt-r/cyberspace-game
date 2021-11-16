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

    bool BeforeBarrier()
    {
        bool beforeBarrier = true;
        var roomMarker = gameObject.GetComponentInParent<RoomMarker>();
        var marker = roomMarker.gameObject.GetComponentInChildren<StoryMarker>();

        if (marker != null && marker.IsBarrier)
        {
            var barrierDir = marker.GetBarrierDirection();
            var barrierLoc = marker.transform.position;
            List<Vector3Int> doorCellsOnSide = new List<Vector3Int>();
            
            var diff = this.transform.position - barrierLoc;
            var dot = Vector3.Dot(barrierDir, diff);
            if (dot > 0 && beforeBarrier)
            {
                return true;
            } 
            else if (dot > 0 && !beforeBarrier)
            {
                return true;
            }
        }
        return false;
    }

#if UNITY_EDITOR_WIN
    private void OnDrawGizmosSelected()
    {
        // draw thicc line to indicate direction of door marker
        var p1 = this.transform.position;
        var p2 = this.transform.position + this.transform.forward * 2;
        var thickness = 8;
        Handles.DrawBezier(p1,p2,p1,p2, Color.blue, null, thickness);

        p2 = this.transform.position + this.transform.up * 3;
        if (BeforeBarrier())
        {
            Handles.DrawBezier(p1,p2,p1,p2, Color.green, null, thickness);
        } 
        else
        {
            Handles.DrawBezier(p1,p2,p1,p2, Color.red, null, thickness);
        }
    }
#endif
}
