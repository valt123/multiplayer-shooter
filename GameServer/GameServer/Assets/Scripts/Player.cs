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
    public bool isDead = false;

    public int kills = 0;
    public int deaths = 0;

    public TommyGun tommyGun;
    public bool isReloading = false;

    public float respawnTime = 1f;
    private Vector3 aerialDirection;

    public bool isRegen = false;
    public float regenWaitTime = 2f;
    public float regenSpeed = 0.1f;

    private IEnumerator coroutine;

    #region Movement variables
    public float gravity = -9.81f;
    public float walkSpeed = 5f;
    public float sprintSpeed = 10f;
    public float jumpSpeed = 10f;
    private bool[] inputs;
    private float yVelocity = 0;
    public Vector3 velocity;
    enum InputKeys { w, s, a, d, space, shift };
    #endregion

    #region Melee variables
    public float meleeDamage = 100f;
    public float meleeCooldown = 1f;
    private float nextMelee;
    #endregion

    private void Start()
    {
        gravity *= Time.fixedDeltaTime * Time.fixedDeltaTime;
        walkSpeed *= Time.fixedDeltaTime;
        sprintSpeed *= Time.fixedDeltaTime;
        jumpSpeed *= Time.fixedDeltaTime;

        tommyGun = new TommyGun(this);
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

        if (inputs[(int)InputKeys.w])
        {
            _inputDirection.y += 1;
        }
        if (inputs[(int)InputKeys.s])
        {
            _inputDirection.y -= 1;
        }
        if (inputs[(int)InputKeys.a])
        {
            _inputDirection.x -= 1;
        }
        if (inputs[(int)InputKeys.d])
        {
            _inputDirection.x += 1;
        }
        
        Move(_inputDirection);
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

    private IEnumerator ReloadWeapon()
    {
        yield return new WaitForSeconds(tommyGun.reloadSpeed);

        tommyGun.ammoCapacity = tommyGun.maxAmmoCapacity;
        ServerSend.PlayerAmmoCapacity(this, tommyGun.ammoCapacity, tommyGun.maxAmmoCapacity);
        isReloading = false;
    }

    public bool CanShoot()
    {
        return isDead || isReloading ? false : true;
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

                float _moveSpeed = inputs[5] && inputs[0] ? sprintSpeed : walkSpeed;
                aerialDirection = Vector3.ClampMagnitude(aerialDirection, 1f) * _moveSpeed;
            }
        }
        yVelocity += gravity;

        Vector3 _moveDirection;
        if (controller.isGrounded)
        {
            _moveDirection = transform.right * _inputDirection.x + transform.forward * _inputDirection.y;

            float _moveSpeed = inputs[(int)InputKeys.shift] && inputs[(int)InputKeys.w] ? sprintSpeed : walkSpeed;
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

    #region Melee
    public void Melee(Vector3 _meleeDirection)
    {
        if (nextMelee <= Time.time && !isDead)
        {
            nextMelee = Time.time + meleeCooldown;
            ServerSend.PlayerMeleed(Server.clients[this.id].player);

            if (Physics.Raycast(shootOrigin.position, _meleeDirection, out RaycastHit _hit, 3f))
            {
                if (_hit.collider.CompareTag("Player"))
                {
                    _hit.collider.GetComponent<Player>().TakeDamage(meleeDamage, this.id);
                }
                Debug.DrawRay(shootOrigin.position, shootOrigin.position + (_meleeDirection.normalized * 1f), Color.green);
            }
        }
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

        transform.position = SpawnManager.SpawnLocation();
        ServerSend.PlayerPosition(this);

        controller.enabled = true;
        isDead = false;
        tommyGun.ammoCapacity = tommyGun.maxAmmoCapacity;

        ServerSend.PlayerAmmoCapacity(this, tommyGun.ammoCapacity, tommyGun.maxAmmoCapacity);
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
