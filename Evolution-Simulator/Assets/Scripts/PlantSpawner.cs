using System.Collections;
using UnityEngine;

public class PlantSpawner : MonoBehaviour
{
    [SerializeField] private GameObject plantPrefab;
    [SerializeField] private float spawnInterval = 1f;
    [SerializeField] private int maxPlants = 100;

    [SerializeField] private int currentPlantCount;

    private void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            if (currentPlantCount < maxPlants)
            {
                SpawnPlant();
            }
            yield return new WaitForSeconds(spawnInterval);
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

