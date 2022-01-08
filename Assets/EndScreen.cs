using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EndScreen : MonoBehaviour
{
    [SerializeField] private TMP_Text pathText;
    
    private string _path;
    
    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetPath(string path)
    {
        //_path = path;
        //pathText.text = _path;
    }

    public void PathCopy()
    {
        //GUIUtility.systemCopyBuffer = _path;
    }

    public void OnFinish()
    {
        Application.Quit();
    }
}
