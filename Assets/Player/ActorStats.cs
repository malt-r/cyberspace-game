using UnityEngine;
using System;

public class ActorStats : MonoBehaviour
{

	public float maxHealth = 100;
	[SerializeField]
	private float currentHealth;
	public event Action OnHealthReachedZero;
	public float CurrentHealth { get { return currentHealth; } }

    public void Awake()
    {
		currentHealth = maxHealth;
    }  
    public void TakeDamage(float damage)
	{
		currentHealth -= damage;
		Debug.Log(transform.name + " takes " + damage + " damage.");

		if (currentHealth > 0) { return; }
        OnHealthReachedZero?.Invoke(); 
		gameObject.SetActive(false);
		EventManager.TriggerEvent("Combat/ActorDied", this.gameObject);
	}

	public void Heal(float amount)
	{
		currentHealth += amount;
		currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
	}

	public void HealToMaxHealth()
	{
		currentHealth = maxHealth;
	}
}
