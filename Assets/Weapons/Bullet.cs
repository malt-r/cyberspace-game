using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField]
    private float velocity;
    [SerializeField]
    private float damage;

    private int ownerId;

    public void Ignite(int ownerId)
    {
        this.ownerId = ownerId;
        var rigidBody = GetComponent<Rigidbody>();
        rigidBody.velocity =transform.forward.normalized*velocity;
        Destroy(gameObject,500f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer != LayerMask.NameToLayer("Combat")){ return; }

        if (other.gameObject.GetInstanceID().Equals(this.ownerId))
        { return; }
        var target = other.GetComponent<CombatParticipant>();
        target.TakeDamage(this.damage);
        gameObject.GetInstanceID();
    }
}
