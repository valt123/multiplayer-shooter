using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ClientHandle : MonoBehaviour
{
    public static void Welcome(Packet _packet)
    {
        string _msg = _packet.ReadString();
        int _myId = _packet.ReadInt();

        Debug.Log($"Message from server: {_msg}");
        Client.instance.myId = _myId;
        ClientSend.WelcomeReceived();

        Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
    }

    public static void SpawnPlayer(Packet _packet)
    {
        int _id = _packet.ReadInt();
        string _username = _packet.ReadString();
        Vector3 _position = _packet.ReadVector3();
        Quaternion _rotation = _packet.ReadQuaternion();
        int _playerKills = _packet.ReadInt();
        int _playerDeaths = _packet.ReadInt();

        GameManager.instance.SpawnPlayer(_id, _username, _position, _rotation, _playerKills, _playerDeaths);
    }

    public static void PlayerPosition(Packet _packet)
    {
        int _id = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();

        GameManager.players[_id].transform.position = _position;
    }

    public static void PlayerRotation(Packet _packet)
    {
        int _id = _packet.ReadInt();
        Quaternion _rotation = _packet.ReadQuaternion();
        Quaternion _cameraRotation = _packet.ReadQuaternion();

        GameManager.players[_id].transform.rotation = _rotation;
        GameManager.players[_id].cameraTransform.rotation = _cameraRotation;
    }

    public static void PlayerDisconnect(Packet _packet)
    {
        int _id = _packet.ReadInt();

        Destroy(GameManager.players[_id].gameObject);
        GameManager.players.Remove(_id);
    }

    public static void PlayerHealth(Packet _packet)
    {
        int _id = _packet.ReadInt();
        float _health = _packet.ReadFloat();

        GameManager.players[_id].SetHealth(_health);
    }

    public static void PlayerRespawned(Packet _packet)
    {
        int _id = _packet.ReadInt();

        GameManager.players[_id].Respawn();
    }

    public static void PlayerShootReceived(Packet _packet)
    {
        int _id = _packet.ReadInt();
        Vector3 _target = _packet.ReadVector3();

        GameManager.players[_id].ShootReceived(_target);
    }

    public static void PlayerIsReloading(Packet _packet)
    {
        int _id = _packet.ReadInt();

        GameManager.players[_id].PlayerReloading();
    }

    public static void PlayerAmmoCapacity(Packet _packet)
    {
        int _id = _packet.ReadInt();
        int _ammoCapacity = _packet.ReadInt();

        GameManager.players[_id].AmmoCapacity(_ammoCapacity);
    }

    public static void PlayerKills(Packet _packet)
    {
        var _killerId = _packet.ReadInt();
        var _kills = _packet.ReadInt();

        var _killedId = _packet.ReadInt();
        var _deaths = _packet.ReadInt();

        var _killer = GameManager.players[_killerId];
        var _killed = GameManager.players[_killedId];

        _killer.kills = _kills;
        _killed.deaths = _deaths;


        var killer = _killer.IsLocalPlayer() ? $"<color=orange>{_killer.username}</color>" : $"<color=red>{_killer.username}</color>";
        var killed = _killed.IsLocalPlayer() ? $"<color=orange>{_killed.username}</color>" : $"<color=red>{_killed.username}</color>";

        UIManager.KillFeed(killer, killed);

        Debug.Log($"{_killer.username} killed {_killed.username} ");
    }
}
