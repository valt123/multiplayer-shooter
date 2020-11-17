using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Transform camTransform;
    public PlayerManager player;

    private void Start()
    {
        player = GetComponent<PlayerManager>();
    }

    private void Update()
    {
        // Shoot on left mouse click
        if (Input.GetKey(KeyCode.Mouse0) && Cursor.lockState == CursorLockMode.Locked)
        {
            ClientSend.PlayerShoot(camTransform.forward);
        }

        // Melee when E is pressed
        if (Input.GetKeyDown(KeyCode.E))
        {
            ClientSend.PlayerMelee(camTransform.forward);
        }

        // Reload on pressing R
        if (Input.GetKeyDown(KeyCode.R) && player.ammoCapacity < player.maxAmmoCapacity)
        {
            ClientSend.PlayerReload();
            UIManager.AmmoCapacity("Reloading");
        }

        // Show scoreboard when pressing tab
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            UIManager.Scoreboard(true);
        }
        // Hide scoreboard when letting go tab
        else if (Input.GetKeyUp(KeyCode.Tab))
        {
            UIManager.Scoreboard(false);
        }

        // Spawn when F is pressed if dead
        if (player.isDead && Input.GetKeyDown(KeyCode.F))
        {
            ClientSend.PlayerRespawn();
        }

        // Suicide when P is pressed if alive
        if (!player.isDead && Input.GetKeyDown(KeyCode.P))
        {
            ClientSend.PlayerSuicide();
        }
    }

    private void FixedUpdate()
    {
        SendInputToServer();
    }

    private void SendInputToServer()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            bool[] _inputs = new bool[]
            {
                Input.GetKey(KeyCode.W),
                Input.GetKey(KeyCode.S),
                Input.GetKey(KeyCode.A),
                Input.GetKey(KeyCode.D),
                Input.GetKey(KeyCode.Space),
                Input.GetKey(KeyCode.LeftShift)
            };

            ClientSend.PlayerMovement(_inputs);
        }
        else
        {
            bool[] _inputs = new bool[]
            {
                false,
                false,
                false,
                false,
                false,
                false
            };

            ClientSend.PlayerMovement(_inputs);
        }
    }
}