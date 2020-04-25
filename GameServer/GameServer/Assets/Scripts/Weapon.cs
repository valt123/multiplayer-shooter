﻿using UnityEngine;
using System.Collections;

public class Weapon
{
    private float lastFired = 0f;
    private float nextTimeToFire = 0f;
    private float verticalRecoil = 0f;

    public float fireRate;
    public float damage;
    public Player shooter;
    public float weaponRange;
    public float maxRecoil;
    public float recoilIncrease;

    public int ammoCapacity;
    public int maxAmmoCapacity;
    public float reloadSpeed;

    public void Shoot(Vector3 _shootDirection)
    {
        if (Time.time >= nextTimeToFire)
        {
            if (ammoCapacity > 0 && shooter.CanShoot())
            {
                nextTimeToFire = Time.time + 1 / fireRate;
                _shootDirection = Recoil(_shootDirection);
                FireWeapon(_shootDirection);
            }
            else
            {
                shooter.Reload();
            }
        }
    }

    private Vector3 Recoil(Vector3 _shootDirection)
    {
        if (Time.time - lastFired <= 0.3f)
        {
            if (verticalRecoil <= maxRecoil)
            {
                verticalRecoil += recoilIncrease;
            }
        }
        else
        {
            verticalRecoil = 0f;
        }

        float randX = 0;
        float randY = Random.Range(0, verticalRecoil);
        return _shootDirection += new Vector3(randX, randY, 0);
    }

    private void FireWeapon(Vector3 _shootDirection)
    {
        if (Physics.Raycast(shooter.shootOrigin.position, _shootDirection, out RaycastHit _hit, weaponRange))
        {
            if (_hit.collider.CompareTag("Player"))
            {
                _hit.collider.GetComponent<Player>().TakeDamage(damage, shooter.id);
                ServerSend.PlayerShootReceived(shooter, _hit.point, true);
            }
            else
            {
                ServerSend.PlayerShootReceived(shooter, _hit.point, false);
            }
            Debug.DrawRay(shooter.shootOrigin.position, shooter.shootOrigin.position + (_shootDirection.normalized * weaponRange), Color.green);
        }
        else
        {
            Vector3 _target = shooter.shootOrigin.position + (_shootDirection.normalized * 50f);
            Debug.DrawRay(shooter.shootOrigin.position, _target, Color.red);
            ServerSend.PlayerShootReceived(shooter, _target, false);
        }

        ammoCapacity -= 1;
        ServerSend.PlayerAmmoCapacity(shooter, ammoCapacity, maxAmmoCapacity);
        lastFired = Time.time;
    }
}