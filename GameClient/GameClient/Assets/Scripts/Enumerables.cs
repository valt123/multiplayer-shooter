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
    playerMeleed
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