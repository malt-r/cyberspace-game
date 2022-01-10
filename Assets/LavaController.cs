using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class LavaController : MonoBehaviour
{
    [SerializeField]
    private GameObject lava;

    private List<CombatParticipant> combatParticipants;
    private List<BaseItem> items;
    public bool LavaIsActive;


    private bool active;
    [SerializeField]
    private float deltatime;
    [SerializeField]
    private float damageIntervall =1;
    [SerializeField]
    private float damageFactor = 10;

    public Material RoomMaterial;
    
    private void Start()
    {
        combatParticipants = new List<CombatParticipant>();
        items = new List<BaseItem>();
        //lava.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        registerCombatParticipant(other);
        registerItems(other);
    }

    
    private void OnTriggerStay(Collider other)
    {
        registerCombatParticipant(other);
        registerItems(other);
    }

    void registerCombatParticipant(Collider other)
    {
        var cp = other.GetComponent<CombatParticipant>();
        if (!cp) { return;}
        if(combatParticipants.Contains(cp)){return;}
        combatParticipants.Add(cp);
        cp.GetComponent<ActorStats>().OnHealthReachedZero += () =>
        {
            combatParticipants.Remove(cp);
        };
    }

    void registerItems(Collider other)
    {
        var item = other.GetComponent<BaseItem>();
        if (!item) { return;}
        if(items.Contains(item)){return;}
        items.Add(item);

    }
    
    private void OnTriggerExit(Collider other)
    {
        unregisterCombatParticipants(other);
        unregisterItems(other);

    }

    private void unregisterItems(Collider other)
    {
        var item = other.GetComponent<BaseItem>();
        if (!item) { return;}
        items.Remove(item);
    }

    private void unregisterCombatParticipants(Collider other)
    {
        var cp = other.GetComponent<CombatParticipant>();
        if (!cp) { return;}
        combatParticipants.Remove(cp);
    }


    private void Update()
    {
        deltatime += Time.deltaTime;
        if (deltatime < damageIntervall) return;
        
        deltatime = 0;
        foreach (var cp in combatParticipants.ToList())
        {
            handleDamage(cp);
        }
            
        foreach (var item in items.ToList())
        {
            handleItem(item);
        }
    }

    void handleDamage(CombatParticipant other)
    {
       // if (!LavaIsActive) { return; }
        other.TakeDamage(damageFactor, byLava : true);
    }

    void handleItem(BaseItem item)
    {
        items.Remove(item);
        Destroy(item.gameObject);
    }
    public void ShowLava()
    {
        lava.SetActive(true);
    }

    public void HideLava()
    {
        combatParticipants.Clear();
        lava.SetActive(false);
    }
}
