using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int id;
    public string username;

    public CharacterController controller;
    public Collider playerCollider;
    public Transform shootOrigin;
    public Transform facing;

    #region Health variables
    [Header("Health Variables")]
    public float health;
    public float maxHealth = 100f;
    public bool isDead = false;

    public bool isRegen = false;
    public float regenWaitTime = 2f;
    public float regenSpeed = 0.1f;
    private IEnumerator regenCoroutine;

    //Currently not implemented 
    public float respawnTime = 1f;
    #endregion

    #region Score variables
    [Header("Score variables")]
    public int kills = 0;
    public int deaths = 0;
    #endregion

    #region Movement variables
    [Header("Movement variables")]
    public float walkSpeed = 5f;

    public float sprintSpeed = 10f;
    public float minSprintSpeed = 4f;

    public float gravity = -9.81f;
    public float jumpSpeed = 10f;
    public float minJumpSpeed = 5f;

    private bool[] inputs;
    private float yVelocity = 0;
    public Vector3 velocity;
    private Vector3 aerialDirection;

    #region Stamina variables
    [Header("Stamina variables")]
    public bool isRunning;
    public float stamina = 100f;
    public float maxStamina = 100f;

    public float staminaSprintDecrease = 10f;
    public float staminaJumpDecrease = 20f;
    public float staminaRegen = 40f;
    #endregion

    #endregion

    #region Weapon variables
    [Header("Weapon variables")]
    public float meleeDamage = 100f;
    public float meleeCooldown = 1f;
    private float nextMelee;

    public TommyGun tommyGun;
    public bool isReloading = false;
    #endregion

    private void Start()
    {
        gravity *= Time.fixedDeltaTime * Time.fixedDeltaTime;
        walkSpeed *= Time.fixedDeltaTime;
        sprintSpeed *= Time.fixedDeltaTime;
        minSprintSpeed *= Time.fixedDeltaTime;
        jumpSpeed *= Time.fixedDeltaTime;
        minJumpSpeed *= Time.fixedDeltaTime;

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

        Stamina();

        if (health > maxHealth)
        {
            health = maxHealth;
        }
        else if (health != maxHealth && !isRegen)
        {
            regenCoroutine = Regen();
            StartCoroutine(regenCoroutine);
        }

        Vector2 _inputDirection = InputDirection();

        Move(_inputDirection);
    }

    private Vector2 InputDirection()
    {
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

        if (inputs[(int)InputKeys.w] && inputs[(int)InputKeys.shift])
        {
            isRunning = true;
        }
        else
        {
            isRunning = false;
        }

        return _inputDirection;
    }

    #region Movement

    private void Stamina()
    {
        if (isRunning)
        {
            stamina -= staminaSprintDecrease * Time.fixedDeltaTime;

            if (stamina < 0)
            {
                stamina = 0;
            }
        }
        else
        {
            stamina += staminaRegen * Time.fixedDeltaTime;
            
            if (stamina > maxStamina)
            {
                stamina = maxStamina;
            }
        }
    }

    private void Move(Vector2 _inputDirection)
    {
        float _moveSpeed;
        Vector3 _moveDirection;

        if (controller.isGrounded)
        {
            yVelocity = 0f;

            if (isRunning)
            {
                _moveSpeed = Mathf.Lerp(minSprintSpeed, sprintSpeed, stamina / maxStamina);
            }
            else
            {
                _moveSpeed = walkSpeed;
            }

            if (inputs[(int)InputKeys.space])
            {
                stamina -= staminaJumpDecrease;
                if (stamina < 0)
                {
                    stamina = 0;
                }
                yVelocity = Mathf.Lerp(minJumpSpeed, jumpSpeed, stamina / maxStamina);
                
                AerialDirection(_inputDirection, _moveSpeed);
            }
            _moveDirection = transform.right * _inputDirection.x + transform.forward * _inputDirection.y;
            _moveDirection = Vector3.ClampMagnitude(_moveDirection, 1f) * _moveSpeed;
        }
        else
        {
            _moveDirection = aerialDirection;
        }

        yVelocity += gravity;
        _moveDirection.y = yVelocity;

        controller.Move(_moveDirection);

        velocity = controller.velocity;

        ServerSend.PlayerPosition(this);
        ServerSend.PlayerRotation(this);
    }

    private void AerialDirection(Vector2 _inputDirection, float _moveSpeed)
    {
        aerialDirection = transform.right * _inputDirection.x + transform.forward * _inputDirection.y;
        aerialDirection = Vector3.ClampMagnitude(aerialDirection, 1f) * _moveSpeed;
    }

    public void SetInput(bool[] _inputs, Quaternion _rotation, Quaternion _cameraRotation)
    {
        inputs = _inputs;
        transform.rotation = _rotation;
        facing.rotation = _cameraRotation;
    }
    #endregion

    #region Weapons
    public void Melee(Vector3 _meleeDirection)
    {
        if (nextMelee <= Time.time && !isDead)
        {
            nextMelee = Time.time + meleeCooldown;

            bool hitPlayer = false;

            if (Physics.Raycast(shootOrigin.position, _meleeDirection, out RaycastHit _hit, 3f))
            {
                if (_hit.collider.CompareTag("Player"))
                {
                    _hit.collider.GetComponent<Player>().TakeDamage(meleeDamage, this.id);
                    hitPlayer = true;
                }
                Debug.DrawRay(shootOrigin.position, shootOrigin.position + (_meleeDirection.normalized * 1f), Color.green);
            }

            ServerSend.PlayerMeleed(Server.clients[this.id].player, hitPlayer);
        }
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
        return isDead || isReloading || isRunning ? false : true;
    }
    #endregion

    #region Health
    public void TakeDamage(float _damage, int _damageSourceId)
    {
        if (health <= 0)
        {
            return;
        }

        health -= _damage;

        if (isRegen)
        {
            StopCoroutine(regenCoroutine);
            isRegen = !isRegen;
        }

        ServerSend.PlayerHealth(this);

        if (health <= 0f)
        {
            Die(_damageSourceId);
        }
    }

    public void Die(int _damageSourceId)
    {
        health = 0f;
        controller.enabled = false;
        playerCollider.enabled = false;

        isDead = true;

        deaths += 1;

        velocity = Vector3.zero;
        aerialDirection = Vector3.zero;

        if (_damageSourceId != this.id)
        {
            var _killer = Server.clients[_damageSourceId].player;
            _killer.kills += 1;

            ServerSend.PlayerKills(_killer, this);
        }
    }

    public void Respawn()
    {
        health = maxHealth;

        transform.position = SpawnManager.SpawnLocation();
        ServerSend.PlayerPosition(this);

        controller.enabled = true;
        playerCollider.enabled = true;

        isDead = false;
        tommyGun.ammoCapacity = tommyGun.maxAmmoCapacity;

        ServerSend.PlayerAmmoCapacity(this, tommyGun.ammoCapacity, tommyGun.maxAmmoCapacity);
        ServerSend.PlayerRespawned(this);
    }

    IEnumerator Regen()
    {
        isRegen = true;
        yield return new WaitForSeconds(regenWaitTime); //Wait for delay

        while (health < maxHealth)
        { //Start the regen cycle
            health += 1; //Increase health by 1
            ServerSend.PlayerHealth(this);
            yield return new WaitForSeconds(regenSpeed); //Wait for regen speed
        }
        isRegen = false; //Set regenning to false
    }
    #endregion
}
