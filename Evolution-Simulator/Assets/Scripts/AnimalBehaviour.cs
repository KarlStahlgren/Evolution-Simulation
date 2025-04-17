using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalBehaviour : MonoBehaviour
{
    // === Animal Stats (modifiable via evolution) ===
    public float speed;
    public float visionRange;
    public float energyToReproduce;
    public float maxEnergy;
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
    [SerializeField] private float currentEnergy;
    private Vector3 moveDirection;
    private GameObject targetFood;
    private GameObject closestAnimal; 

    [SerializeField] private CircleCollider2D visionTrigger;
    private List<GameObject> nearbyFoods = new List<GameObject>();
    private List<GameObject> nearbyAnimals = new List<GameObject>();

    private enum MovementMode { Wander, ChaseFood, ChaseAnimal, Flee } // Different modes of movement
    [SerializeField] private MovementMode currentMode = MovementMode.Wander;

    void Start()
    {   
        if (currentEnergy == 0f)
        {
            currentEnergy = maxEnergy / 3f; // Start with some energy if not already set by parent
        }

        energyLossPerSecond = 1f + speed * 0.2f + visionRange * 0.1f; // Energy loss based on speed and vision range

        visionTrigger.radius = visionRange; // Set the vision trigger radius for food and animals

        PickNewDirection();
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

        // Try to reproduce
        if (currentEnergy >= energyToReproduce)
        {
            Reproduce();
        }

        WrapAround(); // Wrap around the world
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Trigger Entered: " + other.gameObject.name);
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
        float closestDistance = Mathf.Infinity;
        GameObject closest = null;

        foreach (var food in nearbyFoods)
        {
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
        float closestDistance = Mathf.Infinity;
        GameObject closest = null;

        foreach (var animal in nearbyAnimals)
        {

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
            float encounterChanse = Random.value; //max 1
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
                if (Random.value < 0.001f)
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
            Destroy(targetFood);
            targetFood = null;
        }
    }

    void PickNewDirection()
    {
        moveDirection = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
    }

    void Reproduce()
    {
        currentEnergy -= energyToReproduce / 2f; // Consume energy for reproduction

        GameObject child = Instantiate(gameObject, transform.position + Random.insideUnitSphere * 1f, Quaternion.identity);
        AnimalBehaviour childBehaviour = child.GetComponent<AnimalBehaviour>();
        childBehaviour.currentEnergy = energyToReproduce / 2f; // Start with some energy

        // Slight mutation
        childBehaviour.speed = speed + Random.Range(-0.2f, 0.2f);
        childBehaviour.visionRange = visionRange + Random.Range(-1f, 1f);
        childBehaviour.energyToReproduce = energyToReproduce + Random.Range(-5f, 5f);
        //childBehaviour.maxEnergy = maxEnergy + Random.Range(-10f, 10f);
    }

    void Die()
    {
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
}
