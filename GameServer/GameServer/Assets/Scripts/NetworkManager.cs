using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;

    public GameObject playerPrefab;

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
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        Server.Start(50, 26950);

    }

    private void OnApplicationQuit()
    {
        Server.Stop();
    }

    public Player InstantiatePlayer()
    {
        return Instantiate(playerPrefab, randomSpawn(), Quaternion.identity).GetComponent<Player>();
    }

    public static Vector3 randomSpawn()
    {
        float x = Random.Range(5, 45);
        float y = 10;
        float z = Random.Range(4, 46);
        return new Vector3(x, y, z);
    }
}
