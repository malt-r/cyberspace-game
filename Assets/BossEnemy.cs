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
    }

    public void ActivateShieldGameObject()
    {
        ownShieldGameObject.SetActive(true);
        GetComponent<AudioSource>().PlayOneShot(shieldActivationSound);
    }

    void cleanUpRoom()
    {
        //TODO: MR change music
        hideLava();
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
            spawnMobs();
            handleLava();
        }
    }

    private void handleLava()
    {
        timeSinceLastLavaSpawn += Time.deltaTime;

        if (timeSinceLastLavaSpawn < lavaInterfall) { return; }
        timeSinceLastLavaSpawn = 0;
            
        if(lavaController.LavaIsActive){
            lavaController.HideLava();
        }
        else
        {
            lavaController.ShowLava();
        }
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

    void showLava()
    {
        lavaController.ShowLava();
    }

    void hideLava()
    {
        lavaController.HideLava();
    }
    
}
