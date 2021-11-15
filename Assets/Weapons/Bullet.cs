using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField]
    private float velocity;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
    }

    public void Ignite(Vector3 direction)
    {
        var rigidBody = GetComponent<Rigidbody>();
        rigidBody.velocity =direction.normalized*velocity;
        Destroy(gameObject,500f);
    }
}
