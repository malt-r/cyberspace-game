using UnityEngine;

public class PlayerDetector : MonoBehaviour
{
    [SerializeField]
    private Transform detectedPlayer;
    public string TargetTag = "Player";

    [SerializeField] private float range = 20f;
    [SerializeField]
    private Transform reachablePlayer;
    public Transform ReachablePlayer { get => reachablePlayer; private set => reachablePlayer = value; }

    [SerializeField]
    private float distance;
    public float Distance  => distance;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(TargetTag))
        {
            detectedPlayer = other.gameObject.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag(TargetTag))
        {
            detectedPlayer = null;
        }
    }

    private void Update()
    {
        if (!detectedPlayer)
        {
            reachablePlayer = null;
            return;
        }
        var ownTransform = transform;
        var ownPosition = ownTransform.position;
        var playerPosition = detectedPlayer.transform.position;
        var targetVec = playerPosition - ownPosition;


        var hitSomething = Physics.Raycast(ownPosition, targetVec, out var hit);
        Debug.DrawRay(ownPosition, targetVec, Color.green);

        reachablePlayer = null;
        if (!hitSomething) { return; }
        if (!hit.collider.tag.Equals("Player")) { return;}

        distance = Vector3.Distance(ownPosition, hit.transform.position);
            
        if (distance > range) { return; }

        reachablePlayer = detectedPlayer;
    }
}
