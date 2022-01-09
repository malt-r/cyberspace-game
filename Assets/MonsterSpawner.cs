using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    public List<GameObject> spawnList;

    public Enemy SpawnRandomMonster(Bounds bounds)
    {
        var index = Random.Range(0, spawnList.Count-1);
        var randX = Random.Range(bounds.min.x, bounds.max.x);
        var randz = Random.Range(bounds.min.z, bounds.max.z);
        var randomOffset = new Vector3(randX, 0, randz);

        var spawnPos = transform.position + randomOffset;
        return Instantiate(spawnList[index],spawnPos, Quaternion.identity).GetComponent<Enemy>();
    }
}
