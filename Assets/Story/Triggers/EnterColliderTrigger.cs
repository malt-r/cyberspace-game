using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnterColliderTrigger : VoiceTrigger
{
    private Collider _collider;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void OnEnable()
    {
        _collider = GetComponent<Collider>();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Activate();
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
