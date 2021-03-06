﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>();

    public GameObject localPlayerPrefab;
    public GameObject playerPrefab;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }

        if(instance != this)
        {
            Debug.Log("Instance already exists");
            Destroy(gameObject);
        }
    }

    public void SpawnPlayer(int _id, string _username, Vector3 _position, Quaternion _rotation, int _kills, int _deaths)
    {
        GameObject _player;
        if (_id == Client.instance.myId)
        {
            _player = Instantiate(localPlayerPrefab, _position, _rotation);
        }
        else
        {
            _player = Instantiate(playerPrefab, _position, _rotation);
        }

        _player.GetComponent<PlayerManager>().Initialize(_id, _username, _kills, _deaths);
        players.Add(_id, _player.GetComponent<PlayerManager>());
    }
}
