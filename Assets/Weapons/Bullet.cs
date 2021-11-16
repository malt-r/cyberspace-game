using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField]
    private float velocity;
    [SerializeField]
    private float damage;

    public void Ignite(Vector3 direction)
    {
        var rigidBody = GetComponent<Rigidbody>();
        rigidBody.velocity =direction.normalized*velocity;
        Destroy(gameObject,500f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer != LayerMask.NameToLayer("Combat")){ return; }
        
        var target = other.GetComponent<CombatParticipant>();
        target.TakeDamage(this.damage);
    }
}
