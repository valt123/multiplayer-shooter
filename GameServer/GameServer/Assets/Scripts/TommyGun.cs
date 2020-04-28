using UnityEngine;
using System.Collections;

public class TommyGun : Weapon
{
    public TommyGun(Player _player)
    {
        shooter = _player;
        fireRate = 20f;
        ammoCapacity = 50;
        maxAmmoCapacity = 50;
        reloadSpeed = 2.5f;
        damage = 15f;
        maxRecoil = 0.1f;
        recoilIncrease = 0.01f;
        weaponRange = 50f;
    }
}