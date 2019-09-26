using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScanController : MonoBehaviour
{
    private bool _touchpadButtonPressed = false;

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (MiraController.TriggerButtonPressed)
        {
            Debug.Log("Trigger Button Pressed");
        }
        if (MiraController.StartButtonPressed)
        {
            Debug.Log("Start Button Pressed");
        }
        if (MiraController.TouchpadButtonPressed)
        {
            Debug.Log("Touchpad Button Pressed");
        }
        if (MiraController.UpButtonPressed)
        {
            Debug.Log("Up Button Pressed");
        }
        if (MiraController.DownButtonPressed)
        {
            Debug.Log("Down Button Pressed");
        }
        if (MiraController.RightButtonPressed)
        {
            Debug.Log("Right Button Pressed");
        }
        if (MiraController.LeftButtonPressed)
        {
            Debug.Log("Left Button Pressed");
        }
    }
}
