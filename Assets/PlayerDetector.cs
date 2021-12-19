using UnityEngine;

public class PlayerDetector : MonoBehaviour
{
    [SerializeField]
    private Transform detectedPlayer;
    public Transform DetectedPlayer { get => detectedPlayer; private set => detectedPlayer = value; }
    public string TargetTag = "Player";

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
            DetectedPlayer = null;
        }
    }
}
