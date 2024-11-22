using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PsCloseWindows : MonoBehaviour
{
    public enum buttons { X, O, triangle, square,RB,Lb, any }
    public buttons myButton;
    public Button buttonToPress;
    public bool isPressing=false;

    // Update is called once per frame
    void Update()
    {
        if (buttonToPress != null)
        {
           if (!isPressing)
            {
                switch (myButton)
                {
                    case buttons.X:
                        if (Gamepad.current.buttonSouth.wasPressedThisFrame)
                        {
                            buttonToPress.onClick.Invoke();
                            Debug.LogError("XPressed");
                        }
                        break;
                    case buttons.O:
                        if (Gamepad.current.buttonEast.wasPressedThisFrame)
                        {
                            buttonToPress.onClick.Invoke();
                        }
                        break;
                    case buttons.triangle:
                        if (Gamepad.current.buttonNorth.wasPressedThisFrame)
                        {
                            buttonToPress.onClick.Invoke();
                        }
                        break;
                    case buttons.square:
                        if (Gamepad.current.buttonWest.wasPressedThisFrame)
                        {
                            buttonToPress.onClick.Invoke();
                        }
                        break;
                    case buttons.RB:
                        if (Gamepad.current.rightShoulder.wasPressedThisFrame)
                            buttonToPress.onClick.Invoke();
                        break;
                    case buttons.Lb:
                        if (Gamepad.current.leftShoulder.wasPressedThisFrame)
                            buttonToPress.onClick.Invoke();
                        break;
                    case buttons.any:
                        if (Gamepad.current.buttonWest.wasPressedThisFrame || Gamepad.current.buttonNorth.wasPressedThisFrame || Gamepad.current.buttonEast.wasPressedThisFrame || Gamepad.current.buttonSouth.wasPressedThisFrame)
                        {

                            buttonToPress.onClick.Invoke();
                        }
                        break;
                }
            }
            else
            {
                switch (myButton)
                {
                    case buttons.X:
                        if (Gamepad.current.buttonSouth.isPressed)
                        {
                            buttonToPress.onClick.Invoke();
                            Debug.LogError("XPressed");
                        }
                        break;
                    case buttons.O:
                        if (Gamepad.current.buttonEast.isPressed)
                        {
                            buttonToPress.onClick.Invoke();
                        }
                        break;
                    case buttons.triangle:
                        if (Gamepad.current.buttonNorth.isPressed)
                        {
                            buttonToPress.onClick.Invoke();
                        }
                        break;
                    case buttons.square:
                        if (Gamepad.current.buttonWest.isPressed)
                        {
                            buttonToPress.onClick.Invoke();
                        }
                        break;
                    case buttons.RB:
                        if (Gamepad.current.rightShoulder.isPressed)
                            buttonToPress.onClick.Invoke();
                        break;
                    case buttons.Lb:
                        if (Gamepad.current.leftShoulder.isPressed)
                            buttonToPress.onClick.Invoke();
                        break;
                    case buttons.any:
                        if (Gamepad.current.buttonWest.isPressed || Gamepad.current.buttonNorth.isPressed || Gamepad.current.buttonEast.isPressed || Gamepad.current.buttonSouth.isPressed)
                        {

                            buttonToPress.onClick.Invoke();
                        }
                        break;
                }
            }
        }
        else
        {
            Debug.Log(this.gameObject+" not Working");
        }
    }
}
