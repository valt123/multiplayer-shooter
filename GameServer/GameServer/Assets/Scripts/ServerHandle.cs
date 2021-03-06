﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerHandle
{
    public static void WelcomeReceived(int _fromClient, Packet _packet)
    {
        int _clientIdCheck = _packet.ReadInt();
        string _username = _packet.ReadString();

        if (_username == "")
        {
            _username = Client.PickRandomName();
        }

        Debug.Log($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {_username} ({_fromClient})");
        if (_fromClient != _clientIdCheck)
        {
            Debug.Log($"Player \"{_username}\" (ID: {_fromClient} has assumed the wrong client ID ({_clientIdCheck})! ");
        }

        Server.clients[_fromClient].SendIntoGame(_username);
    }

    public static void PlayerMovement(int _fromClient, Packet _packet)
    {
        bool[] _inputs = new bool[_packet.ReadInt()];
        for (int i = 0; i < _inputs.Length; i++)
        {
            _inputs[i] = _packet.ReadBool();
        }
        Quaternion _rotation = _packet.ReadQuaternion();
        Quaternion _cameraRotation = _packet.ReadQuaternion();

        Server.clients[_fromClient].player.SetInput(_inputs, _rotation, _cameraRotation);
    }

    public static void PlayerMelee(int _fromClient, Packet _packet)
    {
        Vector3 _meleeDirection = _packet.ReadVector3();
        Server.clients[_fromClient].player.Melee(_meleeDirection);
    }

    public static void PlayerShoot(int _fromClient, Packet _packet)
    {
        Vector3 _shootDirection = _packet.ReadVector3();

        Server.clients[_fromClient].player.tommyGun.Shoot(_shootDirection);
    }

    public static void PlayerReload(int _fromClient, Packet _packet)
    {
        Server.clients[_fromClient].player.Reload();
    }

    public static void PlayerSuicide(int _fromClient, Packet _packet)
    {
        var _player = Server.clients[_fromClient].player;
        _player.TakeDamage(_player.maxHealth, _fromClient);
    }

    public static void PlayerRespawn(int _fromClient, Packet _packet)
    {
        var _player = Server.clients[_fromClient].player;
        _player.Respawn();
    }
}
