using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RoomMarker : MonoBehaviour
{
    [SerializeField]
    public int InstantiationIndex = -1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

#if UNITY_EDITOR_WIN
    private void OnDrawGizmosSelected()
    {
        if (InstantiationIndex != -1)
        {
            var _style = new GUIStyle();
            _style.normal.textColor = Color.white;
            _style.fontSize = 20;
            _style.fontStyle = FontStyle.Bold;

            // write index
            Handles.Label(transform.position, "inst. idx: " + this.InstantiationIndex.ToString(), _style);
        }
    }
#endif
}
