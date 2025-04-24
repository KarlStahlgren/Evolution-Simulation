using System.Collections;
using UnityEngine;

public class PlantSpawner : MonoBehaviour
{
    [SerializeField] private GameObject plantPrefab;
    [SerializeField] private float plantsPerSecond = 10f; // Number of plants to spawn per second
    [SerializeField] private int maxPlants = 100;
    [SerializeField] private int startAmount = 100; //
    [SerializeField] private int currentPlantCount;

    private void Start()
    {
        SpawnInitialPlants();
        StartCoroutine(SpawnRoutine());
    }
    private void SpawnInitialPlants()
    {
        for (int i = 0; i < startAmount && currentPlantCount < maxPlants; i++)
        {
            SpawnPlant();
        }
    }
    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            if (currentPlantCount < maxPlants)
            {
                SpawnPlants();
            }
            yield return new WaitForSeconds(1); //Spawn 1 times per second
        }
    }
    void SpawnPlants()
    {
        for (int i = 0; i < plantsPerSecond; i++)
        {
            if (currentPlantCount >= maxPlants) break; // Stop spawning if maxPlants is reached
            WorldManager.Instance?.RegisterPlant();
            SpawnPlant();
        }
    }
    void SpawnPlant()
    {
        Vector2 pos = GetRandomPositionInWorld();
        GameObject plant = Instantiate(plantPrefab, pos, Quaternion.identity);
        currentPlantCount++;

        // Optional: hook into a "destroyed" event to decrement the count
        plant.GetComponent<PlantBehaviour>()?.SetSpawner(this);
    }

    public void NotifyPlantEaten()
    {
        currentPlantCount--;
        WorldManager.Instance?.UnregisterPlant();
    }

    Vector2 GetRandomPositionInWorld()
    {
        Vector2 min = WorldManager.Instance.areaMin;
        Vector2 max = WorldManager.Instance.areaMax;
        return new Vector2(
            Random.Range(min.x, max.x),
            Random.Range(min.y, max.y)
        );
    }
}

