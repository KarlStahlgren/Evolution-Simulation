using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{   
    public static WorldManager Instance; // Singleton instance

    [Header("World Area")]
    public Vector2 areaMin = new Vector2(-10f, -10f);
    public Vector2 areaMax = new Vector2(10f, 10f);

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

}
