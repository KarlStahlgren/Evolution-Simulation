using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EggBehaviour : MonoBehaviour
{
    public float hatchTime = 3f;
    private float timer;

    public GameObject animalPrefab;

    // Store mutated stats directly
    public float speed, visionRange, energyToReproduce, strength, defense, aggression, timidity, startEnergy;

    void Start()
    {
        timer = hatchTime;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            Hatch();
        }
    }

    void Hatch()
    {
        GameObject child = Instantiate(animalPrefab, transform.position, Quaternion.identity);
        AnimalBehaviour childBehaviour = child.GetComponent<AnimalBehaviour>();

        // Apply stored values
        childBehaviour.speed = speed;
        childBehaviour.visionRange = visionRange;
        childBehaviour.energyToReproduce = energyToReproduce;
        childBehaviour.strength = strength;
        childBehaviour.defense = defense;
        childBehaviour.aggression = aggression;
        childBehaviour.timidity = timidity;
        childBehaviour.currentEnergy = startEnergy;

        Destroy(gameObject);
    }
}
