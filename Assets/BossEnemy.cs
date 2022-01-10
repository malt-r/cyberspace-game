using System.Collections.Generic;
using System.Linq;
using Assets.Weapons;
using UnityEngine;
using UnityEngine.AI;

public class BossEnemy : Enemy 
{
    [SerializeField] protected int maxConcurrentMobs =3;
    [SerializeField] protected int numberOfMobsToSpawnAtTheSameTime =2;
    [SerializeField] List<Enemy> mobList;
    [SerializeField] float timeSinceLastMonsterSpawn;
    [SerializeField] float spawnDelay=15f;
    
    [SerializeField] List<ShieldGenerator> shields;

    [SerializeField] private AudioClip shieldActivationSound;
    
    MonsterSpawner spawner;
    [SerializeField]
    private LavaController lavaController;
    [SerializeField]
    private GameObject ownShieldGameObject;

    [SerializeField]
    private Collider spawnArea;

    [SerializeField]  private float lavaInterfall = 15f;
    private float timeSinceLastLavaSpawn;

    [SerializeField]
    private bool inIntro = true;

    private bool _registeredInitialMobSpawning;

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        Debug.Log("Start Boss");
        mobList = new List<Enemy>();
        spawner = GetComponent<MonsterSpawner>();
        weaponControl = GetComponent<WeaponControl>();
        foreach (var shield in shields)
        {
            shield.OnShieldDestroy += handleShieldDestroyed;
        }

        GetComponent<BossActorStats>().OnHealthReachedZero += cleanUpRoom;

        if (ownShieldGameObject != null)
        {
            ownShieldGameObject.SetActive(false);
        }
        
        GetComponent<ActorStats>().OnHealthReachedZero += () => throwDeathEvent();

        EventManager.StartListening("Boss/Ready", OnBossReady);
    }

    private void OnBossReady(object arg0)
    {
        inIntro = false;
        _registeredInitialMobSpawning = true;
    }

    public void ActivateShieldGameObject()
    {
        ownShieldGameObject.SetActive(true);
        GetComponent<AudioSource>().PlayOneShot(shieldActivationSound);
    }

    void cleanUpRoom()
    {
        foreach (var mob in mobList)
        {
            mob.GetComponent<CombatParticipant>().TakeDamage(float.MaxValue);
        }
    }
    // Update is called once per frame
    new void Update()
    {
        if (inIntro) { return; }
        base.Update();

        if (!ForceIdle)
        {
            for (int i = 0; i < numberOfMobsToSpawnAtTheSameTime; i++)
            {
                spawnMobs();
            }

            if (_registeredInitialMobSpawning && !lavaController.LavaIsActive)
            {
                _registeredInitialMobSpawning = false;
                spawnInitialMobs();
            }
        }
    }

    void spawnMobs()
    {
        if(mobList.Count == maxConcurrentMobs) { return; }

        timeSinceLastMonsterSpawn += Time.deltaTime;
        if (timeSinceLastMonsterSpawn > spawnDelay 
            && mobList.Count < maxConcurrentMobs 
            && !lavaController.LavaIsActive)
        {
            spawnMob(null);
        }
    }
    
    public void spawnInitialMobs()
    {
        spawnMob(0);
        spawnMob(1);
        spawnMob(2);
    }


    void spawnMob(int? index)
    {
        timeSinceLastMonsterSpawn = 0;
        var monster = spawner.SpawnMonster(spawnArea.bounds, index);
        if (monster != null)
        {
            mobList.Add(monster);
            monster.GetComponent<ActorStats>().OnHealthReachedZero += () => deleteMobFromList(monster);
        }
    }
    protected override void updateAppearance(){
        //Dont scale boss
    }

    void throwDeathEvent()
    {
        EventManager.TriggerEvent("Boss/Death", new StoryEventData().SetEventName("Boss/Death"));
    }

    void deleteMobFromList(Enemy mob)
    {
        mobList.Remove(mob);
    }
    
    void handleShieldDestroyed()
    {
        var shieldThatAreActive = shields.Count(shield => shield.shieldActive);
        
        switch (shieldThatAreActive)
        {
            case 3:
                EventManager.TriggerEvent("boss/3shields-left",new StoryEventData().SetEventName("boss/3shields-left"));
                break;
            case 2:
                EventManager.TriggerEvent("boss/2shields-left",new StoryEventData().SetEventName("boss/2shields-left"));
                break;
            case 1:
                EventManager.TriggerEvent("boss/1shields-left",new StoryEventData().SetEventName("boss/1shields-left"));
                break;
            case 0:
                EventManager.TriggerEvent("boss/0shields-left",new StoryEventData().SetEventName("boss/0shields-left"));
                weaponControl.SwitchWeapon(1);
                break;
        }
    }
}
