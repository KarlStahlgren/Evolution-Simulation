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

    // === Configuration ===
    public float energyLossPerSecond = 1f;
    public float energyGainFromFood = 20f;

    // === Current State ===
    [Space]
    [Header("Current State")]
    [SerializeField] private float currentEnergy;
    private Vector3 moveDirection;
    private GameObject targetFood;

    void Start()
    {
        currentEnergy = maxEnergy / 3f; // Start with some energy
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

        // Try to find food if not already targeting
        if (targetFood == null)
        {
            LookForFood();
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

    void Move()
    {
        if (targetFood != null)
        {
            moveDirection = (targetFood.transform.position - transform.position).normalized;
        }

        transform.position += moveDirection * speed * Time.deltaTime;

        // Optional: Change direction randomly if wandering
        if (targetFood == null && Random.value < 0.001f)
        {
            PickNewDirection();
        }
    }

    void LookForFood()
    {
        GameObject[] foods = GameObject.FindGameObjectsWithTag("Food");
        float closestDistance = visionRange;
        GameObject closest = null;

        foreach (GameObject food in foods)
        {
            float dist = Vector3.Distance(transform.position, food.transform.position);
            if (dist < closestDistance)
            {
                closest = food;
                closestDistance = dist;
            }
        }

        targetFood = closest;
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
        currentEnergy -= energyToReproduce / 2f;

        GameObject child = Instantiate(gameObject, transform.position + Random.insideUnitSphere * 1f, Quaternion.identity);
        AnimalBehaviour childBehaviour = child.GetComponent<AnimalBehaviour>();

        // Slight mutation
        childBehaviour.speed = Mathf.Clamp(speed + Random.Range(-0.2f, 0.2f), 0.1f, 10f);
        childBehaviour.visionRange = Mathf.Clamp(visionRange + Random.Range(-1f, 1f), 1f, 20f);
        childBehaviour.energyToReproduce = Mathf.Clamp(energyToReproduce + Random.Range(-5f, 5f), 10f, 100f);
        childBehaviour.maxEnergy = Mathf.Clamp(maxEnergy + Random.Range(-10f, 10f), 10f, 200f);
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
