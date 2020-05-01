using UnityEngine;
using System.Collections;

public class Weapon
{
    public Player shooter;

    private float lastFired = 0f;
    private float nextTimeToFire = 0f;
    private float verticalRecoil = 0f;
    public float maxRecoil;
    public float recoilIncrease;

    public float fireRate = 20f;
    public float damage = 15f;
    public float weaponRange = 50f;

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
                //_shootDirection = Recoil(_shootDirection);
                FireWeapon(_shootDirection);
            }
            else if(ammoCapacity == 0)
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
            Debug.DrawLine(shooter.shootOrigin.position, shooter.shootOrigin.position + (_shootDirection * weaponRange), Color.green, 5f);
        }
        else
        {
            Vector3 _target = shooter.shootOrigin.position + (_shootDirection * 50f);
            Debug.DrawRay(shooter.shootOrigin.position, _target, Color.red);
            ServerSend.PlayerShootReceived(shooter, _target, false);
        }

        ammoCapacity -= 1;
        ServerSend.PlayerAmmoCapacity(shooter, ammoCapacity, maxAmmoCapacity);
        lastFired = Time.time;
    }
}