using UnityEngine;
using System;
using System.Diagnostics.Tracing;
using UnityEngine.EventSystems;

public class ActorStats : MonoBehaviour
{
	public float maxHealth = 100;
	[SerializeField]
	private float currentHealth;
	public event Action OnHealthReachedZero;
	public float CurrentHealth { get { return currentHealth; } }

	public bool onlyBombsCanDamageMe;
    public void Awake()
    {
		currentHealth = maxHealth;
    }

    public virtual void TakeDamage(float damage, bool bomb = false, bool byEnemy = false, bool byLava = false)
    {
	    if (onlyBombsCanDamageMe && !bomb)
	    {
		    Debug.Log("I can only be killed by bombs");
		    return;
	    }
		currentHealth -= damage;
		Debug.Log(transform.name + " takes " + damage + " damage.");

		if (currentHealth > 0) { return; }
        OnHealthReachedZero?.Invoke(); 
		gameObject.SetActive(false);
		if (gameObject.CompareTag("Player"))
		{
			if (byEnemy)
			{
				EventManager.TriggerEvent("Death/ByEnemy", new StoryEventData().SetEventName("Death/ByEnemy"));
			}
			if (bomb)
			{
				EventManager.TriggerEvent("Death/ByBomb", new StoryEventData().SetEventName("Death/ByBomb"));
			}
			if (byLava)
			{
				EventManager.TriggerEvent("Death/ByLava", new StoryEventData().SetEventName("Death/ByLava"));
			}
			EventManager.TriggerEvent("Combat/PlayerDied", this.gameObject);
		}
		else
		{
			EventManager.TriggerEvent("Combat/EnemyDied", this.gameObject);
			SoundManager.PlaySound(Sound.EnemyDie);
		}
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
