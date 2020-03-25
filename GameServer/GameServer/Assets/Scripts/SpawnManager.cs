using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager instance;

    public static GameObject[] spawnPoints;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        if (instance != this)
        {
            Debug.Log("Instance already exists");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        spawnPoints = GameObject.FindGameObjectsWithTag("Spawn");
    }

    public static Vector3 SpawnLocation()
    {
        int spawnPoint = Random.Range(0, spawnPoints.Length);

        return spawnPoints[spawnPoint].transform.position;
    }
}
