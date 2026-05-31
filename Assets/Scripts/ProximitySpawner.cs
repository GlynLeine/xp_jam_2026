using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.Serialization;

public class ProximitySpawner : MonoBehaviour
{
    public GameObject[] enemyPrefabs; // Array to hold different enemy types
    public Transform[] spawnPoints;
    public Transform player;
    public float spawnRadius = 10f;
    public float spawnAmount = 5f;

    private float nextSpawnTime;
    private float enemyCount;
    private bool spawnerConsumed = false;

    void Update()
    {
        if (Vector3.Distance(player.position, transform.position) <= spawnRadius && enemyCount <= spawnAmount && spawnerConsumed == false)
        {
            SpawnEnemy();
            enemyCount ++;

        }
        else
        {
            spawnerConsumed = true;
        }

    }

    void SpawnEnemy()
    {
        int spawnIndex = Random.Range(0, spawnPoints.Length);
        int enemyIndex = Random.Range(0, enemyPrefabs.Length);
        Instantiate(enemyPrefabs[enemyIndex], spawnPoints[spawnIndex].position, spawnPoints[spawnIndex].rotation);
    }
}