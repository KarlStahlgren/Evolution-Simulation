using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{   
    public static WorldManager Instance; // Singleton instance

    [Header("World Area")]
    public Vector2 areaMin = new Vector2(-10f, -10f);
    public Vector2 areaMax = new Vector2(10f, 10f);

    [Header("Statistics")]

    [SerializeField] private float updateInterval = 1f; // How often to update the stats
     [SerializeField] private int maxSamples = 100; // Max number of samples to keep for stats
    [SerializeField] private int plantCount = 0; // Number of plants in the world
    [SerializeField] private List<AnimalBehaviour> animals = new(); // List of animals in the world for stats
    public List<float> avgSpeedList = new();
    public List<float> avgVisionList = new();
    public List<float> avgStrengthList = new();
    public List<float> avgDefenseList = new();
    public List<float> avgAggressionList = new();
    public List<float> avgTimidityList = new(); 
    public List<float> animalCountList = new();
    public List<float> plantCountList = new();
    private void Awake()
    {
        // Make this accessible from anywhere
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple WorldManagers in scene!");
            Destroy(gameObject);
        }

        //Speed up time
        Time.timeScale = 10f; // Speed up time for testing purposes
        StartCoroutine(StatsRoutine());
    }

    void Start()
    {
        SetupCamera();
    }

    void SetupCamera()
    {
        Camera cam = Camera.main;
        cam.orthographic = true;

        float height = areaMax.y - areaMin.y;
        //float width = areaMax.x - areaMin.x;
        cam.orthographicSize = height / 2f;

        cam.transform.position = new Vector3(
            (areaMin.x + areaMax.x) / 2f,
            (areaMin.y + areaMax.y) / 2f,
            -10f // keep some Z offset for 2D
        );
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector2 center = (areaMin + areaMax) / 2f;
        Vector2 size = areaMax - areaMin;
        Gizmos.DrawWireCube(center, size);
    }

    IEnumerator StatsRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(updateInterval); // Adjust interval as needed
            UpdateStats();
        }
    }

    void UpdateStats()
    {
        animals.RemoveAll(a => a == null);

        float totalSpeed = 0, totalVision = 0, totalStrength = 0, totalDefense = 0; 

        foreach (var a in animals)
        {
            totalSpeed += a.speed;
            totalVision += a.visionRange;
            totalStrength += a.strength;
            totalDefense += a.defense;
        }

        int count = animals.Count;
        avgSpeedList.Add(count > 0 ? totalSpeed / count : 0);
        avgVisionList.Add(count > 0 ? totalVision / count : 0);
        avgStrengthList.Add(count > 0 ? totalStrength / count : 0);
        avgDefenseList.Add(count > 0 ? totalDefense / count : 0);
        avgAggressionList.Add(count > 0 ? totalSpeed / count : 0); // Assuming aggression is similar to speed for this example
        avgTimidityList.Add(count > 0 ? totalVision / count : 0); // Assuming timidity is similar to vision for this example
        animalCountList.Add(count);
        plantCountList.Add(plantCount);

        // After adding values:
        TrimList(avgSpeedList);
        TrimList(avgVisionList);
        TrimList(avgStrengthList);
        TrimList(avgDefenseList);
        TrimList(avgAggressionList);
        TrimList(avgTimidityList);
        TrimList(animalCountList);
        TrimList(plantCountList);
    }
    public void RegisterAnimal(AnimalBehaviour animal)
    {
        animals.Add(animal);
    }
     public void UnregisterAnimal(AnimalBehaviour animal)
    {
        animals.Remove(animal);
    }


    public void RegisterPlant()
    {
        plantCount++;
    }

    public void UnregisterPlant()
    {
        plantCount--;
    }
    

    void TrimList(List<float> list)
    {
        if (list.Count > maxSamples)
            list.RemoveAt(0);
    }
}
