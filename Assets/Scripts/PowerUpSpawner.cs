using UnityEngine;

public class PowerUpSpawner : MonoBehaviour
{
    public GameObject powerUpPrefab;
    public Transform[] spawnPoints;
    public float spawnInterval = 10f;

    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnPowerUp();
        }
    }

    void SpawnPowerUp()
    {
        if (powerUpPrefab == null || spawnPoints.Length == 0) return;

        int randomIndex = Random.Range(0, spawnPoints.Length);
        Transform selectedPoint = spawnPoints[randomIndex];

        Instantiate(powerUpPrefab, selectedPoint.position, Quaternion.identity);
    }
}