using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PurveyorOfDeath : MonoBehaviour
{
    private BoxCollider _collider;
    
    // Start is called before the first frame update
    void Start()
    {
        _collider = GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        var combatant = other.gameObject.GetComponent<CombatParticipant>();
        if (combatant != null)
        {
            combatant.TakeDamage(float.MaxValue);
        }
    }
}
