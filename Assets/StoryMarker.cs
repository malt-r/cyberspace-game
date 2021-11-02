using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class StoryMarker : MonoBehaviour
{
    [Tooltip("The index of the story marker in the progression of the story, 0 is the start")]
    [SerializeField]
    public int IndexInStory = 0;

    public bool AccomplishedMarker { get; set; }

    [SerializeField]
    public bool IsBarrier = false;

    [SerializeField]
    Collider BarrierCollider;

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

    private void OnDrawGizmosSelected()
    {
        var _style = new GUIStyle();
        _style.normal.textColor = Color.black;
        _style.fontSize = 20;
        _style.fontStyle = FontStyle.Bold;

        // write index
        Handles.Label(this.transform.position, "idx: " + this.IndexInStory.ToString(), _style);
    }
}
