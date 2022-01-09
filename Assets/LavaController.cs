using UnityEngine;

public class LavaController : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        other.GetComponent<CombatParticipant>().TakeDamage(float.MaxValue, byLava : true);
    }

    private void OnTriggerStay(Collider other)
    {
        other.GetComponent<CombatParticipant>().TakeDamage(float.MaxValue, byLava : true);
    }

    public void ShowLava()
    {
        
    }

    public void HideLava()
    {
        
    }
}
