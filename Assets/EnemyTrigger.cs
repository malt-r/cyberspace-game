using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTrigger : StoryTrigger
{
    [SerializeField] 
    private GameObject[] enemiesToWatch;

    private int _totalEnemies;
    private int _deadEnemies;
    
    
    // Start is called before the first frame update
    void Start()
    {
        _totalEnemies = enemiesToWatch.Length;
        foreach (var enemy in enemiesToWatch)
        {
            enemy.GetComponent<ActorStats>().OnHealthReachedZero += () =>
            {
                _deadEnemies++;
                if (_deadEnemies >= _totalEnemies)
                {
                    Activate(null);
                }
            };
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
