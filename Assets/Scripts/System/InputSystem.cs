using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class InputSystem : MonoBehaviour { 
    private static InputSystem instance;
    public static InputSystem Instance
    {

        get

        {

            if (instance == null)

            {

                var obj = FindObjectOfType<InputSystem>();

                if (obj != null)
                {

                    instance = obj;

                }
                else
                {

                    var newSingleton = new GameObject("InputSystem Class").AddComponent<InputSystem>();

                    instance = newSingleton;
                }

            }

            return instance;

        }

        private set

        {

            instance = value;

        }

    }

    bool[] keyDownValues;
    bool[] keyUpValues;
    bool[] keyValues;
    int Length;
    public void init()
    {
        Length = Enum.GetValues(typeof(InputKeys)).Length;
        keyValues = new bool[Length];
        keyDownValues = new bool[Length];
        keyUpValues = new bool[Length];
    }
    void Awake()
    {
        var objs = FindObjectsOfType<InputSystem>();

        if (objs.Length != 1)
        {
            Destroy(gameObject);
            return;
        }
        init();
        DontDestroyOnLoad(gameObject);
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
        if (Input.GetKey(KeyCode.Tab))
            keyValues[(int)InputKeys.WeaponSwapBtn] = true;
        if (Input.GetKey(KeyCode.I))
            keyValues[(int)InputKeys.InfoBtn] = true;
        if (Input.GetKey(KeyCode.Q))
            keyValues[(int)InputKeys.TakeBtn] = true;

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
        if (Input.GetKeyDown(KeyCode.Tab))
            keyDownValues[(int)InputKeys.WeaponSwapBtn] = true;
        if (Input.GetKeyDown(KeyCode.I))
            keyDownValues[(int)InputKeys.InfoBtn] = true;
        if (Input.GetKeyDown(KeyCode.Q))
            keyDownValues[(int)InputKeys.TakeBtn] = true;

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
        if (Input.GetKeyUp(KeyCode.Tab))
            keyUpValues[(int)InputKeys.WeaponSwapBtn] = true;
        if (Input.GetKeyUp(KeyCode.I))
            keyUpValues[(int)InputKeys.InfoBtn] = true;
        if (Input.GetKeyUp(KeyCode.Q))
            keyUpValues[(int)InputKeys.TakeBtn] = true;
    }

    public bool getKeyDown(InputKeys input)
    {
        if (keyDownValues == null)
            init();
        return keyDownValues[(int)input];
    }
    public bool getKeyUp(InputKeys input)
    {
        if (keyUpValues == null)
            init();
        return keyUpValues[(int)input];
    }
    public bool getKey(InputKeys input)
    {
        if (keyValues == null)
            init();
        return keyValues[(int)input];
    }

}
