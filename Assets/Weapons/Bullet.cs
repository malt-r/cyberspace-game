using Unity.VisualScripting;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField]
    private float velocity;
    [SerializeField]
    public float damage;

    private int ownerId;

    public void Ignite(int ownerId)
    {
        this.ownerId = ownerId;
        var rigidBody = GetComponent<Rigidbody>();
        rigidBody.velocity =transform.forward.normalized*velocity;
        Destroy(gameObject,5);
    }

    private void OnTriggerEnter(Collider other)
    {
       handleCollision(other);
    }

    private void handleCollision(Collider other)
    {
        
        
        var otherIsWall = other.gameObject.layer == LayerMask.NameToLayer("Default");
        if (otherIsWall)
        {
           // Destroy(gameObject);
            return;
        }
        
        var otherIsInCombatLayer = other.gameObject.layer == LayerMask.NameToLayer("Combat");
        if (!otherIsInCombatLayer){ return; }

        var otherIsBulletOwner = other.gameObject.GetInstanceID().Equals(this.ownerId);
        if (otherIsBulletOwner) { return; }
        
        var target = other.GetComponent<CombatParticipant>();
        if(!target) { return; }
        if (!target.tag.Equals("Player")) { return;}
        target.TakeDamage(this.damage);
        
        gameObject.GetInstanceID();
    }
}
