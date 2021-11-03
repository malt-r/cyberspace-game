using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class StoryMarker : MonoBehaviour
{
    [Header("Red is the barrier")]
    [Header("green is direction, ")]
    [Header("in which barrier will be opened")]

    [Space(40)]

    [Tooltip("The index of the story marker in the progression of the story, 0 is the start")]
    [SerializeField]
    public int IndexInStory = 0;

    [Tooltip("The index-field of this marker will be ignored by the generator and the room will appear 'somewhere' in the level")]
    [SerializeField]
    public bool RelevantForStory = true;

    public bool AccomplishedMarker { get; set; }

    [SerializeField]
    public bool IsBarrier = false;

    [Tooltip("This is used purely for visualization purposes during development to simplify the placement of doors or stuff..")]
    [SerializeField]
    int BarrierVisualizationLength = 10;

    [SerializeField]
    bool IsKey = false;

    [SerializeField]
    StoryMarker DependantBarrierMarker;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Vector3 GetBarrierDirection()
    {
        return transform.forward;
    }

    private void OnDrawGizmosSelected()
    {
        var _style = new GUIStyle();
        _style.normal.textColor = Color.black;
        _style.fontSize = 20;
        _style.fontStyle = FontStyle.Bold;

        // write index
        Handles.Label(transform.position, "idx: " + this.IndexInStory.ToString(), _style);

        // draw barrier visualization
        if (IsBarrier)
        {
            // draw thicc line to indicate direction of door marker
            var p1 = transform.position + transform.right * -BarrierVisualizationLength;
            var p2 = transform.position + transform.right * BarrierVisualizationLength;
            var thickness = 8;
            Handles.DrawBezier(p1,p2,p1,p2, Color.red, null, thickness);

            p1 = transform.position;
            p2 = transform.position + transform.forward * (int)(BarrierVisualizationLength * 0.7); 
            Handles.DrawBezier(p1,p2,p1,p2, Color.green, null, thickness);
        }
    }
}
