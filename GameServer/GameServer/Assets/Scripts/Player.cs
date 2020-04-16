using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int id;
    public string username;

    public CharacterController controller;
    public Transform shootOrigin;
    public Transform facing;

    public float health;
    public float maxHealth = 100f;

    public int kills = 0;
    public int deaths = 0;

    public float respawnTime = 1f;
    private Vector3 aerialDirection;

    public bool isRegen = false;
    public float regenWaitTime = 2f;
    public float regenSpeed = 0.1f;

    private IEnumerator coroutine;

    #region Movement variables
    public float gravity = -9.81f;
    public float moveSpeed = 5f;
    public float sprintSpeed = 10f;
    public float jumpSpeed = 10f;
    private bool[] inputs;
    private float yVelocity = 0;
    public Vector3 velocity;
    #endregion

    #region Weapon variables
    public float fireRate = 20f;
    public int ammoCapacity = 50;
    public int maxAmmoCapacity = 50;
    public float damage = 15f;
    public float reloadSpeed = 3f;
    private float lastFired = 0f;
    private float nextTimeToFire = 0f;
    private float verticalRecoil = 0f;
    private bool isReloading = false;
    private bool isDead = false;
    #endregion

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

        if (health > maxHealth)
        {
            health = maxHealth;
        }
        else if (health != maxHealth && !isRegen)
        {
            coroutine = Regen();
            StartCoroutine(coroutine);
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

    #region Movement
    private void Move(Vector2 _inputDirection)
    {
        if (controller.isGrounded)
        {
            yVelocity = 0f;
            if (inputs[4])
            {
                yVelocity = jumpSpeed;
                aerialDirection = transform.right * _inputDirection.x + transform.forward * _inputDirection.y;

                float _moveSpeed = inputs[5] && inputs[0] ? sprintSpeed : moveSpeed;
                aerialDirection = Vector3.ClampMagnitude(aerialDirection, 1f) * _moveSpeed;
            }
        }
        yVelocity += gravity;

        Vector3 _moveDirection;
        if (controller.isGrounded)
        {
            _moveDirection = transform.right * _inputDirection.x + transform.forward * _inputDirection.y;

            float _moveSpeed = inputs[5] && inputs[0] ? sprintSpeed : moveSpeed;
            _moveDirection = Vector3.ClampMagnitude(_moveDirection, 1f) * _moveSpeed;
        }
        else
        {
            _moveDirection = aerialDirection;
        }

        _moveDirection.y = yVelocity;
        controller.Move(_moveDirection);
        velocity = controller.velocity;

        ServerSend.PlayerPosition(this);
        ServerSend.PlayerRotation(this);
    }

    public void SetInput(bool[] _inputs, Quaternion _rotation, Quaternion _cameraRotation)
    {
        inputs = _inputs;
        transform.rotation = _rotation;
        facing.rotation = _cameraRotation;
    }
    #endregion

    #region Shooting
    public void Shoot(Vector3 _shootDirection)
    {
        if (Time.time >= nextTimeToFire)
        {
            if ( CanShoot() )
            {
                if (ammoCapacity > 0)
                {
                    nextTimeToFire = Time.time + 1 / fireRate;
                    _shootDirection = Recoil(_shootDirection);
                    FireWeapon(_shootDirection);
                }
                else if(ammoCapacity == 0)
                {
                    Reload();
                }
            }
        }
    }

    private Vector3 Recoil(Vector3 _shootDirection)
    {
        if (Time.time - lastFired <= 0.3f)
        {
            if (verticalRecoil <= 0.1f)
            {
                verticalRecoil += 0.01f;
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
        if (Physics.Raycast(shootOrigin.position, _shootDirection, out RaycastHit _hit, 50f))
        {
            if (_hit.collider.CompareTag("Player"))
            {
                _hit.collider.GetComponent<Player>().TakeDamage(damage, this.id);
                ServerSend.PlayerShootReceived(this, _hit.point, true);
            }
            else
            {
                ServerSend.PlayerShootReceived(this, _hit.point, false);
            }
            Debug.DrawRay(shootOrigin.position, shootOrigin.position + (_shootDirection.normalized * 50f), Color.green);
        }
        else
        {
            Vector3 _target = shootOrigin.position + (_shootDirection.normalized * 50f);
            Debug.DrawRay(shootOrigin.position, _target, Color.red);
            ServerSend.PlayerShootReceived(this, _target, false);
        }

        ammoCapacity -= 1;
        ServerSend.PlayerAmmoCapacity(this, ammoCapacity, maxAmmoCapacity);
        lastFired = Time.time;
    }

    public void Reload()
    {
        if (!isReloading && !isDead)
        {
            isReloading = true;
            ServerSend.PlayerIsReloading(this);
            StartCoroutine(ReloadWeapon());
        }
    }

    public bool CanShoot()
    {
        return isDead || isReloading ? false : true;
    }

    private IEnumerator ReloadWeapon()
    {
        yield return new WaitForSeconds(reloadSpeed);

        ammoCapacity = maxAmmoCapacity;
        ServerSend.PlayerAmmoCapacity(this, ammoCapacity, maxAmmoCapacity);
        isReloading = false;
    }
    #endregion

    public void TakeDamage(float _damage, int _damageSourceId)
    {
        if (health <= 0)
        {
            return;
        }

        health -= _damage;

        if (isRegen)
        {
            StopCoroutine(coroutine);
            isRegen = !isRegen;
        }

        ServerSend.PlayerHealth(this);

        if (health <= 0f)
        {
            health = 0f;
            controller.enabled = false;

            transform.position = SpawnManager.SpawnLocation();
            ServerSend.PlayerPosition(this);

            isDead = true;

            this.deaths += 1;
            if (_damageSourceId != this.id)
            {
                var _killer = Server.clients[_damageSourceId].player;
                _killer.kills += 1;

                ServerSend.PlayerKills(_killer, this);
            }
        }
    }

    public void Respawn()
    {
        health = maxHealth;
        controller.enabled = true;
        isDead = false;
        ammoCapacity = maxAmmoCapacity;

        ServerSend.PlayerAmmoCapacity(this, ammoCapacity, maxAmmoCapacity);
        ServerSend.PlayerRespawned(this);
    }

    IEnumerator Regen()
    {
        isRegen = true; //Set regenning to true
        yield return new WaitForSeconds(regenWaitTime); //Wait for delay

        while (health< maxHealth)
        { //Start the regen cycle
            health += 1; //Increase health by 1
            ServerSend.PlayerHealth(this);
            yield return new WaitForSeconds(regenSpeed); //Wait for regen speed
        }
        isRegen = false; //Set regenning to false
    }
}
