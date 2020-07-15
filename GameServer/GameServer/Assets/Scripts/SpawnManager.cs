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

        else if (instance != this)
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
        var spawnPoint = instance.FindSafeSpawn();

        return spawnPoint;
    }

    Vector3 FindSafeSpawn()
    {
        List<Vector3> safeSpawns = new List<Vector3>();

        foreach(var spawnPoint in spawnPoints)
        {
            bool isPlayerNearSpawn = false;

            Collider[] hitColliders = Physics.OverlapSphere(spawnPoint.transform.position, 15f);

            foreach (var collider in hitColliders)
            {
                if (collider.CompareTag("Player"))
                {
                    isPlayerNearSpawn = true;
                }
            }

            if (!isPlayerNearSpawn)
            {
                safeSpawns.Add(spawnPoint.transform.position);
            }
        }

        if (safeSpawns.Count == 0)
        {
            return spawnPoints[Random.Range(0, spawnPoints.Length)].transform.position;
        }
        else
        {
           return safeSpawns.ToArray()[Random.Range(0, safeSpawns.Count)];
        }
        
    }
}
