using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantBehaviour : MonoBehaviour
{
    private PlantSpawner spawner;

    public void SetSpawner(PlantSpawner s)
    {
        spawner = s;
    }

    private void OnDestroy()
    {
        spawner?.NotifyPlantEaten();
    }
}
