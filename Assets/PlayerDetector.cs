using UnityEngine;

public class PlayerDetector : MonoBehaviour
{
    [SerializeField] private string targetTag = "Player";
    
    [SerializeField] private Transform detectedPlayer;
    [SerializeField] private Transform reachablePlayer;
    public Transform ReachablePlayer => reachablePlayer;

    [SerializeField] private float distance;
    public float Distance  => distance;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(targetTag))
        {
            detectedPlayer = other.gameObject.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag(targetTag))
        {
            detectedPlayer = null;
        }
    }

    private void Update()
    {
        reachablePlayer = null;
        if (!detectedPlayer) { return; }
        
        var ownTransform = transform;
        var ownPosition = ownTransform.position;

        var canSeePlayer = checkIfPlayerIsVisible(ownPosition, out var playerPosition);
        if (canSeePlayer)
        {
            distance = Vector3.Distance(ownPosition, playerPosition);
            reachablePlayer = detectedPlayer;
        }
    }

    bool checkIfPlayerIsVisible(Vector3 ownPosition, out Vector3 playerPosition)
    {
        
        playerPosition = detectedPlayer.transform.position + Vector3.up * 1.5f;
        var targetVec = playerPosition - ownPosition;
        
        var hitSomething = Physics.Raycast(ownPosition, targetVec, out var hit);
        
        Debug.DrawRay(ownPosition, targetVec, Color.red);

        
        if (!hitSomething) { return false; }
        if (!hit.collider.tag.Equals("Player")) { return false; }

        return true;
    }
}
