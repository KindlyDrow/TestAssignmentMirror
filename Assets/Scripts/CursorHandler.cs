using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorHandler : MonoBehaviour
{
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftAlt))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        } else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

}
