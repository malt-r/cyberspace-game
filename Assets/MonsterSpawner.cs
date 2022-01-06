using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    public List<GameObject> spawnList;

    public float deltaTime;

    public float spawnTime;
    // Update is called once per frame
    void Update()
    {
        deltaTime += Time.deltaTime;

        if (deltaTime > spawnTime)
        {
            deltaTime = 0;
            var index = Random.Range(0, spawnList.Count-1);
            var randX = Random.Range(-15, 15);
            var randz = Random.Range(-15, 15);
            var spawnPos = new Vector3(randX, 0, randz);
            Instantiate(spawnList[index],spawnPos, Quaternion.identity);
        }
    }
}
