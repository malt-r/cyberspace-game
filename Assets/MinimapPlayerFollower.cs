using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapPlayerFollower : MonoBehaviour
{
    [SerializeField]
    Camera FollowerCamera;

    [SerializeField]
    public GameObject ToFollow;

    [SerializeField]
    Vector3 FollowTargetOrigin;

    [SerializeField]
    public float FollowDeltaScaling;

    [SerializeField]
    GameObject playerMarker;

    [SerializeField]
    float playerMarkerScale = 1.0f;

    [SerializeField]
    float cameraProjectionSize = 5.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (null == ToFollow)
        {
            return;
        }

        if (null == FollowerCamera)
        {
            return;
        }

        playerMarker.transform.localScale = Vector3.one * playerMarkerScale;

        FollowerCamera.orthographicSize = cameraProjectionSize;

        // Calc delta of target from origin
        var delta = ToFollow.transform.position - FollowTargetOrigin ;

        // scale 
        delta *= FollowDeltaScaling;

        // apply delta to camera
        FollowerCamera.transform.localPosition = new Vector3(delta.x, FollowerCamera.transform.position.y, delta.z);

        if (null == playerMarker)
        {
            return;
        }

        // apply delta to marker
        playerMarker.transform.localPosition = new Vector3(delta.x, playerMarker.transform.localPosition.y, delta.z);

        // apply rotation to marker
        playerMarker.transform.rotation = ToFollow.transform.rotation;
    }
}
