using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    public List<GameObject> spawnList;
    

    public Enemy SpawnMonster(Bounds bounds, int? monsterIndex)
    {
        var index = monsterIndex ?? Random.Range(0, spawnList.Count-1);
        if (index < spawnList.Count - 1)
        {
            index = Random.Range(0, spawnList.Count-1);
        }
        
        if (index > spawnList.Count)
        {
            return null;
        }
       
        var randX = Random.Range(bounds.min.x, bounds.max.x);
        var randz = Random.Range(bounds.min.z, bounds.max.z);
        var randomOffset = new Vector3(randX, 0, randz);

        var spawnPos = randomOffset;
        return Instantiate(spawnList[index],spawnPos, Quaternion.identity).GetComponent<Enemy>();
    }
    
}
