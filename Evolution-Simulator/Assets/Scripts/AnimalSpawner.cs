using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalSpawner : MonoBehaviour
{
    [SerializeField] private GameObject animalPrefab;
    [SerializeField] private int numberOfAnimalsToSpawn = 20;

    private void Start()
    {
        SpawnAnimals();
    }

    void SpawnAnimals()
    {
        for (int i = 0; i < numberOfAnimalsToSpawn; i++)
        {
            Vector2 pos = GetRandomPositionInWorld();
            float angle = Random.Range(0f, 360f);
            Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

            Instantiate(animalPrefab, pos, rotation);
        }
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