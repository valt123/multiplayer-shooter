using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int id;
    public string username;
    public CharacterController controller;
    public Transform shootOrigin;
    public float gravity = -9.81f;
    public float moveSpeed = 5f;
    public float sprintSpeed = 10f;
    public float jumpSpeed = 10f;
    public float health;
    public float maxHealth = 100f;

    private float fireRate = 20f;
    private float lastFired = 0f;
    private float nextTimeToFire = 0f;
    private float verticalRecoil = 0f;
    private int ammoCapacity = 50;
    private bool reloading = false;
    private float damage = 15f;
    private float reloadSpeed = 3f;

    private bool[] inputs;
    private float yVelocity = 0;

    private void Start()
    {
        gravity *= Time.fixedDeltaTime * Time.fixedDeltaTime;
        moveSpeed *= Time.fixedDeltaTime;
        sprintSpeed *= Time.fixedDeltaTime;
        jumpSpeed *= Time.fixedDeltaTime;
    }

    public void Initialize(int _id, string _username)
    {
        id = _id;
        username = _username;
        health = maxHealth;

        inputs = new bool[6];
    }

    public void FixedUpdate()
    {
        if (health <= 0f)
        {
            return;
        }

        Vector2 _inputDirection = Vector2.zero;

        if (inputs[0])
        {
            _inputDirection.y += 1;
        }
        if (inputs[1])
        {
            _inputDirection.y -= 1;
        }
        if (inputs[2])
        {
            _inputDirection.x -= 1;
        }
        if (inputs[3])
        {
            _inputDirection.x += 1;
        }
        
        Move(_inputDirection);
    }

    private void Move(Vector2 _inputDirection)
    {
        if (controller.isGrounded)
        {
            yVelocity = 0f;
            if (inputs[4])
            {
                yVelocity = jumpSpeed;
            }
        }
        yVelocity += gravity;

        Vector3 _moveDirection = transform.right * _inputDirection.x + transform.forward * _inputDirection.y;

        float _moveSpeed = inputs[5] ? sprintSpeed : moveSpeed;

         _moveDirection = Vector3.ClampMagnitude(_moveDirection, 1f) * _moveSpeed;

        _moveDirection.y = yVelocity;
        controller.Move(_moveDirection);

        ServerSend.PlayerPosition(this);
        ServerSend.PlayerRotation(this);
    }

    public void SetInput(bool[] _inputs, Quaternion _rotation)
    {
        inputs = _inputs;
        transform.rotation = _rotation;
    }

    public void Shoot(Vector3 _shootDirection)
    {
        if (Time.time >= nextTimeToFire)
        {
            if (!reloading)
            {
                nextTimeToFire = Time.time + 1 / fireRate;
                _shootDirection = Recoil(_shootDirection);

                if (ammoCapacity > 0)
                {
                    FireWeapon(_shootDirection);
                }
            }
        }
    }

    private Vector3 Recoil(Vector3 _shootDirection)
    {
        if (Time.time - lastFired <= 0.5f)
        {
            if (verticalRecoil <= 0.2f)
            {
                verticalRecoil += 0.01f;
            }
            
        }
        else
        {
            verticalRecoil = 0f;
        }

        float randX = Random.Range(-.03f, .03f);
        float randY = Random.Range(0f, verticalRecoil);
        return _shootDirection += new Vector3(randX, randY, 0);
    }

    private void FireWeapon(Vector3 _shootDirection)
    {
        if (Physics.Raycast(shootOrigin.position, _shootDirection, out RaycastHit _hit, 50f))
        {
            if (_hit.collider.CompareTag("Player"))
            {
                _hit.collider.GetComponent<Player>().TakeDamage(damage);
            }

            ServerSend.PlayerShootReceived(this, _hit.point);
        }
        else
        {
            Vector3 _target = shootOrigin.position + (_shootDirection.normalized * 50f);
            ServerSend.PlayerShootReceived(this, _target);
        }

        ammoCapacity -= 1;
        ServerSend.PlayerAmmoCapacity(this, ammoCapacity);
        lastFired = Time.time;
    }
    
    public void TakeDamage(float _damage)
    {
        if (health <= 0)
        {
            return;
        }

        health -= _damage;

        if (health <= 0f)
        {
            health = 0f;
            controller.enabled = false;
            transform.position = SpawnManager.SpawnLocation();
            ServerSend.PlayerPosition(this);
            StartCoroutine(Respawn());
        }

        ServerSend.PlayerHealth(this);
    }

    public void Reload()
    {
        if (!reloading)
        {
            reloading = true;
            ServerSend.PlayerIsReloading(this);
            StartCoroutine(ReloadWeapon());
        }
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(2f);

        health = maxHealth;
        controller.enabled = true;
        ServerSend.PlayerRespawned(this);
    }

    private IEnumerator ReloadWeapon()
    {
        yield return new WaitForSeconds(reloadSpeed);

        ammoCapacity = 50;
        ServerSend.PlayerAmmoCapacity(this, ammoCapacity);
        reloading = false;
    }
}
