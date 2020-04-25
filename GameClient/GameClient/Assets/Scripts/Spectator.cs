using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spectator : MonoBehaviour
{
    private float sensitivity;
    public float clampAngle = 85f;

    private float verticalRotation;
    private float horizontalRotation;

    void Start()
    {
        verticalRotation = transform.localEulerAngles.x;
        horizontalRotation = transform.localEulerAngles.y;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        UIManager.PauseMenu();
    }

    private void Update()
    {
        sensitivity = PlayerPrefs.GetFloat("Sensitivity", 100f);
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Look();
            Move();
        }
    }

    private void Move()
    {
        var moveSides = Input.GetAxis("Horizontal") * 5 * Time.deltaTime;
        var moveForward = Input.GetAxis("Vertical") * 5 * Time.deltaTime;
        var moveUp = Input.GetKey(KeyCode.E);
        var moveDown = Input.GetKey(KeyCode.Q);

        float upDown;

        if (moveUp)
        {
            upDown = 1 * 3 * Time.deltaTime;
        }
        else if (moveDown)
        {
            upDown = -1 * 3 * Time.deltaTime;
        }
        else
        {
            upDown = 0;
        }

        transform.position += (transform.right * moveSides + transform.forward * moveForward + transform.up * upDown) * 5;
    }

    private void Look()
    {
        float _mouseVertical = -Input.GetAxis("Mouse Y");
        float _mouseHorizontal = Input.GetAxis("Mouse X");

        verticalRotation += _mouseVertical * sensitivity * Time.deltaTime;
        horizontalRotation += _mouseHorizontal * sensitivity * Time.deltaTime;

        verticalRotation = Mathf.Clamp(verticalRotation, -clampAngle, clampAngle);

        transform.localRotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0f);
    }
}
