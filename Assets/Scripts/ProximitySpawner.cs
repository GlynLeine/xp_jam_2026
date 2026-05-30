using UnityEngine;
using UnityEngine.Serialization;

public class ProximitySpawner : MonoBehaviour
{
    public GameObject[] enemyPrefabs; // Array to hold different enemy types
    public Transform[] spawnPoints;
    public Transform player;
    public float spawnDelay = 3f;
    public float spawnRadius = 10f;

    private float nextSpawnTime;

    void Update()
    {
        if (Time.time > nextSpawnTime && Vector3.Distance(player.position, transform.position) <= spawnRadius)
        {
            SpawnEnemy();
            nextSpawnTime = Time.time + spawnDelay;
        }
    }

    void SpawnEnemy()
    {
        int spawnIndex = Random.Range(0, spawnPoints.Length);
        int enemyIndex = Random.Range(0, enemyPrefabs.Length);
        Instantiate(enemyPrefabs[enemyIndex], spawnPoints[spawnIndex].position, spawnPoints[spawnIndex].rotation);
    }
}