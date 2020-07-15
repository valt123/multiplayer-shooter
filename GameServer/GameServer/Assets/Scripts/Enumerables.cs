/// <summary>Sent from server to client.</summary>
public enum ServerPackets
{
    welcome = 1,
    spawnPlayer,
    playerPosition,
    playerRotation,
    playerDisconnected,
    playerHealth,
    playerRespawned,
    playerShootReceived,
    playerAmmoCapacity,
    playerIsReloading,
    playerKills,
    playerMeleed,
    playerTakeDamage
}

/// <summary>Sent from client to server.</summary>
public enum ClientPackets
{
    welcomeReceived = 1,
    playerMovement,
    playerShoot,
    playerReload,
    playerSuicide,
    playerRespawn,
    playerMelee
}

public enum InputKeys 
{ 
    w, 
    s, 
    a, 
    d, 
    space, 
    shift 
}