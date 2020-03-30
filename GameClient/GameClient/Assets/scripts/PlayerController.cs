using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Transform camTransform;

    public void Update()
    {
        if (Input.GetKey(KeyCode.Mouse0) && Cursor.lockState == CursorLockMode.Locked)
        {
            ClientSend.PlayerShoot(camTransform.forward);
        }

        if (Input.GetKeyDown(KeyCode.R) && GetComponent<PlayerManager>().ammoCapacity < 50)
        {
            ClientSend.PlayerReload();
            UIManager.AmmoCapacity("Reloading");
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            UIManager.Scoreboard(true);
        }

        else if (Input.GetKeyUp(KeyCode.Tab))
        {
            UIManager.Scoreboard(false);
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