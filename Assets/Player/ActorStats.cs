using UnityEngine;

public class ActorStats : MonoBehaviour
{

	public float maxHealth = 100;
	[SerializeField]
	public float currentHealth { get; protected set; }   
	public float damage = 10;

	public event System.Action OnHealthReachedZero;

    public void Awake()
    {
		currentHealth = maxHealth;
    }  
    public void TakeDamage(float damage)
	{
		currentHealth -= damage;
		Debug.Log(transform.name + " takes " + damage + " damage.");

		if (currentHealth > 0) { return; }
		if (OnHealthReachedZero != null) { OnHealthReachedZero(); }
		gameObject.SetActive(false);
	}

	public void Heal(int amount)
	{
		currentHealth += amount;
		currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
	}



}
