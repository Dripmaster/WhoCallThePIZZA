using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputSystem : MonoBehaviour
{
    public static InputSystem instance;


    bool[] keyDownValues;
    bool[] keyUpValues;
    bool[] keyValues;
    int Length;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
        }


        Length = Enum.GetValues(typeof(InputKeys)).Length;
        keyValues = new bool[Length];
        keyDownValues = new bool[Length];
        keyUpValues = new bool[Length];
    }

    void Update()
    {
        for (int i = 0; i < Length; i++)
        {
            keyValues[i] = false;
            keyUpValues[i] = false;
            keyDownValues[i] = false;
        }

        if (Input.GetKey(KeyCode.W))
            keyValues[(int)InputKeys.Move_up] = true;
        if (Input.GetKey(KeyCode.A))
            keyValues[(int)InputKeys.Move_left] = true;
        if (Input.GetKey(KeyCode.S))
            keyValues[(int)InputKeys.Move_down] = true;
        if (Input.GetKey(KeyCode.D))
            keyValues[(int)InputKeys.Move_right] = true;
        if (Input.GetKey(KeyCode.Space))
            keyValues[(int)InputKeys.DashBtn] = true;
        if (Input.GetKey(KeyCode.E))
            keyValues[(int)InputKeys.SkillBtn] = true;
        if (Input.GetKey(KeyCode.R))
            keyValues[(int)InputKeys.UltmateBtn] = true;
        if (Input.GetMouseButton(0))
            keyValues[(int)InputKeys.MB_L_click] = true;
        if (Input.GetMouseButton(1))
            keyValues[(int)InputKeys.MB_R_click] = true;

        if (Input.GetKeyDown(KeyCode.W))
            keyDownValues[(int)InputKeys.Move_up] = true;
        if (Input.GetKeyDown(KeyCode.A))
            keyDownValues[(int)InputKeys.Move_left] = true;
        if (Input.GetKeyDown(KeyCode.S))
            keyDownValues[(int)InputKeys.Move_down] = true;
        if (Input.GetKeyDown(KeyCode.D))
            keyDownValues[(int)InputKeys.Move_right] = true;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            keyDownValues[(int)InputKeys.DashBtn] = true;
        }
        if (Input.GetKeyDown(KeyCode.E))
            keyDownValues[(int)InputKeys.SkillBtn] = true;
        if (Input.GetKeyDown(KeyCode.R))
            keyDownValues[(int)InputKeys.UltmateBtn] = true;
        if (Input.GetMouseButtonDown(0))
            keyDownValues[(int)InputKeys.MB_L_click] = true;
        if (Input.GetMouseButtonDown(1))
            keyDownValues[(int)InputKeys.MB_R_click] = true;

        if (Input.GetKeyUp(KeyCode.W))
            keyUpValues[(int)InputKeys.Move_up] = true;
        if (Input.GetKeyUp(KeyCode.A))
            keyUpValues[(int)InputKeys.Move_left] = true;
        if (Input.GetKeyUp(KeyCode.S))
            keyUpValues[(int)InputKeys.Move_down] = true;
        if (Input.GetKeyUp(KeyCode.D))
            keyUpValues[(int)InputKeys.Move_right] = true;
        if (Input.GetKeyUp(KeyCode.Space))
            keyUpValues[(int)InputKeys.DashBtn] = true;
        if (Input.GetKeyUp(KeyCode.E))
            keyUpValues[(int)InputKeys.SkillBtn] = true;
        if (Input.GetKeyUp(KeyCode.R))
            keyUpValues[(int)InputKeys.UltmateBtn] = true;
        if (Input.GetMouseButtonUp(0))
            keyUpValues[(int)InputKeys.MB_L_click] = true;
        if (Input.GetMouseButtonUp(1))
            keyUpValues[(int)InputKeys.MB_R_click] = true;
    }

    public bool getKeyDown(InputKeys input)
    {
        return keyDownValues[(int)input];
    }
    public bool getKeyUp(InputKeys input)
    {
        return keyUpValues[(int)input];
    }
    public bool getKey(InputKeys input)
    {
        return keyValues[(int)input];
    }

}
