using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AnimalBehaviour : MonoBehaviour   
{
    // === Animal Stats ===
    public float speed;
    public float visionRange;
    public float energyToReproduce;
    public float maxEnergy;//Not modifiable, set by parent
    // === Fight Stats ===
    public float strength = 1f;
    public float defense = 1f;
    public float aggression = 0.1f; // Chance to chase other animal
    public float timidity = 0.1f; // Chance to flee when encountering another animal

    // === Configuration ===
    [SerializeField] private float energyLossPerSecond;
    public float energyGainFromFood = 20f;

    // === Current State ===
    [Space]
    [Header("Current State")]
    public float currentEnergy;
    private Vector3 moveDirection;
    [SerializeField] private GameObject targetFood;
    [SerializeField] private GameObject closestAnimal; 

    // Food and Animal detection
    [SerializeField] private CircleCollider2D visionTrigger;
    [SerializeField] private List<GameObject> nearbyFoods = new List<GameObject>();
    [SerializeField] private List<GameObject> nearbyAnimals = new List<GameObject>();

    // Movement
    private enum MovementMode { Wander, ChaseFood, ChaseAnimal, Flee } // Different modes of movement
    [SerializeField] private MovementMode currentMode = MovementMode.Wander;

    public bool isDead = false;//To avoid double dying in fights

    // === Reproduction ===
    public GameObject eggPrefab; 

    void Start()
    {   
        if (currentEnergy == 0f)
        {
            currentEnergy = maxEnergy / 3f; // Start with some energy if not already set by parent
        }

        energyLossPerSecond = (1f + MathF.Pow(speed,2) + visionRange + strength + defense) * 0.1f; // Energy loss based on stats

        visionTrigger.radius = visionRange; // Set the vision trigger radius for food and animals

        PickNewDirection();

        WorldManager.Instance?.RegisterAnimal(this); // Register the animal in the ecosystem manager
    }

    void Update()
    {   

        // Energy loss over time
        currentEnergy -= energyLossPerSecond * Time.deltaTime;

        if (currentEnergy <= 0)
        {
            Die();
            return;
        }

        // Move
        Move();

        // Try to eat food
        TryEatFood();

        //Try to fight
        TryFightAnimal();

        // Try to reproduce
        if (currentEnergy >= energyToReproduce)
        {
            Reproduce();
        }

        WrapAround(); // Wrap around the world
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Food"))
        {
            nearbyFoods.Add(other.gameObject);
            UpdateClosestFood();
        }

        else if (other.CompareTag("Animal"))
        {   
            nearbyAnimals.Add(other.gameObject);
            UpdateClosestAnimal();
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Food"))
        {   
            nearbyFoods.Remove(other.gameObject);
            UpdateClosestFood();
        }
        else if (other.CompareTag("Animal"))
        {
            nearbyAnimals.Remove(other.gameObject);
            UpdateClosestAnimal();
        }
    }
    void UpdateClosestFood()
    {   
        CleanNullEntries(nearbyFoods); // Clean up null entries in the list
        float closestDistance = Mathf.Infinity;
        GameObject closest = null;

        foreach (var food in nearbyFoods)
        {
            //if(food == null) continue; // Skip if food is destroyed
            float dist = Vector3.Distance(transform.position, food.transform.position);
            if (dist < closestDistance)
            {
                closest = food;
                closestDistance = dist;
            }
        }

        targetFood = closest;
        // Only pursue food if not in a more important state
        if (currentMode == MovementMode.Wander && targetFood != null)
        {
            currentMode = MovementMode.ChaseFood;
        }
    }
    void UpdateClosestAnimal()
    {
        CleanNullEntries(nearbyAnimals); // Clean up null entries in the list
        float closestDistance = Mathf.Infinity;
        GameObject closest = null;

        foreach (var animal in nearbyAnimals)
        {
            //if(animal == null) continue; // Skip if food is destroyed
            float dist = Vector3.Distance(transform.position, animal.transform.position);
            if (dist < closestDistance)
            {
                closest = animal;
                closestDistance = dist;
            }
        }
        closestAnimal = closest;

        //Fight or flight logic
        if (closestAnimal !=null)
        {
            float encounterChanse = UnityEngine.Random.value; //max 1
            if (encounterChanse < aggression)
            {
                currentMode = MovementMode.ChaseAnimal;
            }
            else if(encounterChanse < timidity)
            {
                currentMode = MovementMode.Flee;
            }else
            {
                currentMode = MovementMode.Wander;
            }
        }
    }

    void Move()
    {
        // Exit conditions: If targets are gone, revert to wandering
        if (currentMode == MovementMode.ChaseFood && targetFood == null)
            currentMode = MovementMode.Wander;

        if ((currentMode == MovementMode.Flee || currentMode == MovementMode.ChaseAnimal) && closestAnimal == null)
            currentMode = MovementMode.Wander;

        if (currentMode == MovementMode.Wander && targetFood != null)
            currentMode = MovementMode.ChaseFood;
            
        switch (currentMode)
        {
            case MovementMode.Flee:
                if (closestAnimal != null)
                    moveDirection = (transform.position - closestAnimal.transform.position).normalized;
                break;

            case MovementMode.ChaseAnimal:
                if (closestAnimal != null)
                    moveDirection = (closestAnimal.transform.position - transform.position).normalized;
                break;

            case MovementMode.ChaseFood:
                if (targetFood != null)
                    moveDirection = (targetFood.transform.position - transform.position).normalized;
                break;

            case MovementMode.Wander:
                if (UnityEngine.Random.value < 0.001f)
                    PickNewDirection();
                break;
        }

        transform.position += moveDirection * speed * Time.deltaTime;
    }

    void TryEatFood()
    {
        if (targetFood != null && Vector3.Distance(transform.position, targetFood.transform.position) < 1f)
        {
            currentEnergy = Mathf.Min(currentEnergy + energyGainFromFood, maxEnergy);
            nearbyFoods.Remove(targetFood); // Remove eaten food from the list
            Destroy(targetFood);
            UpdateClosestFood(); // Update the closest food after eating, could be bug since targetfood is destroyed
        }
    }

    void TryFightAnimal()
    {
        if (currentMode == MovementMode.ChaseAnimal && closestAnimal != null)
        {
            float distance = Vector3.Distance(transform.position, closestAnimal.transform.position);

            //If target in range, decide a winner of the fight
            if (distance < 1f) //threshold
            { 
                if (isDead || closestAnimal == null) return; 
                AnimalBehaviour other = closestAnimal.GetComponent<AnimalBehaviour>();

                if (other == null || other.isDead) return; //Avoid double fighting

                float myPower = strength - other.defense;
                float otherPower = other.strength - defense;

                if (myPower > otherPower)
                {
                    currentEnergy +=  other.currentEnergy * 0.5f; // Gain energy from defeated animal
                    other.Die();
                }
                else
                {
                    other.currentEnergy += currentEnergy * 0.5f; // Gain energy from defeated animal
                    Die();
                }

                UpdateClosestAnimal(); // Update the closest animal after fighting
            }
        }
    }

    void PickNewDirection()
    {
        moveDirection = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)).normalized;
    }

    void Reproduce()
    {
        currentEnergy -= energyToReproduce / 2f; // Consume energy for reproduction

        GameObject child = Instantiate(eggPrefab, transform.position, Quaternion.identity);
        EggBehaviour eggBehaviour = child.GetComponent<EggBehaviour>();
        eggBehaviour.startEnergy = energyToReproduce / 2f; // Start with some energy

        // Slight mutation, cannot be negative
        eggBehaviour.speed = Mathf.Max(speed + UnityEngine.Random.Range(-1f, 1f), 0.1f); 
        eggBehaviour.visionRange = Mathf.Max(visionRange + UnityEngine.Random.Range(-1f, 1f), 0.1f);
        eggBehaviour.energyToReproduce = Mathf.Max(energyToReproduce + UnityEngine.Random.Range(-5f, 5f), 0.1f);
        eggBehaviour.strength = Mathf.Max(strength + UnityEngine.Random.Range(-1f, 1f), 0.1f);
        eggBehaviour.defense = Mathf.Max(defense + UnityEngine.Random.Range(-1f, 1f), 0.1f);
        eggBehaviour.aggression = Mathf.Clamp(aggression + UnityEngine.Random.Range(-0.1f, 0.1f), 0f, 1f);
        eggBehaviour.timidity = Mathf.Clamp(timidity + UnityEngine.Random.Range(-0.1f, 0.1f), 0f, 1f);
    }

    void Die()
    {
        if (isDead) return;//Avoid double death in fights
        isDead = true;
        WorldManager.Instance?.UnregisterAnimal(this); // Unregister the animal in the ecosystem manager

        Destroy(gameObject);
    }

    void WrapAround()
    {
        Vector3 pos = transform.position;
        Vector2 min = WorldManager.Instance.areaMin;
        Vector2 max = WorldManager.Instance.areaMax;

        if (pos.x < min.x) pos.x = max.x;
        else if (pos.x > max.x) pos.x = min.x;

        if (pos.y < min.y) pos.y = max.y;
        else if (pos.y > max.y) pos.y = min.y;

        transform.position = pos;
    }

    // Helper function
    void CleanNullEntries<T>(List<T> list) where T : UnityEngine.Object
    {
        list.RemoveAll(item => item == null);
    }
}
