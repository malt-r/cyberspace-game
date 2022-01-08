using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BossEnemy : Enemy 
{
    [SerializeField] protected int maxConcurrentMobs =3;
    [SerializeField] List<Enemy> mobList;
    [SerializeField] float timeSinceLastMonsterSpawn;
    [SerializeField] float spawnDelay=15f;
    
    [SerializeField] List<ShieldGenerator> shields;
    
    MonsterSpawner spawner;

    [SerializeField]
    private Collider spawnArea;
    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        mobList = new List<Enemy>();
        spawner = GetComponent <MonsterSpawner>();
        weaponControl = GetComponent<WeaponControl>();
        foreach (var shield in shields)
        {
            shield.OnShieldDestroy += handleShieldDestroyed;
        }
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();
        spawnMobs();
    }


    void spawnMobs()
    {
        if(mobList.Count == maxConcurrentMobs) { return; }
        
        timeSinceLastMonsterSpawn += Time.deltaTime;
        if (timeSinceLastMonsterSpawn > spawnDelay && mobList.Count < maxConcurrentMobs)
        {
            timeSinceLastMonsterSpawn = 0;
            var monster = spawner.SpawnRandomMonster(spawnArea.bounds);
            mobList.Add(monster);
            monster.GetComponent<ActorStats>().OnHealthReachedZero += () => deleteMobFromList(monster);

        }
    }
    protected override void updateAppearance(){
        //Dont scale boss
    }

    void deleteMobFromList(Enemy mob)
    {
        mobList.Remove(mob);
        //TODO MR Voiceline on death of mobs? 
    }
    
    void handleShieldDestroyed()
    {
        //TODO MR Voiceline shield destroyes

        var shieldThatAreActive = shields.Count(shield => shield.shieldActive);
        
        switch (shieldThatAreActive)
        {
            case 3:
                Debug.Log("Den Schild brauche ich nicht, ich hab noch mehr als genug.");
                break;
            case 2:
                Debug.Log("Du hälst dich für ganz schlau oder?");
                break;
            case 1:
                Debug.Log("Einer reicht auch");
                break;
            case 0:
                Debug.Log("Wie konnte das passiere, ich habe keinen Schutz mehr... Ich meine ich besiege dich auch so.");
                weaponControl.SwitchWeapon(1);
                break;
        }
    }
}
