using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class PlayerFSM : FSMbase
{
    public float moveSpeed;
    public float dashSpeed;
    public int DashFrameCount;
    Vector2 moveDir;
    public int dashFrameCount;
    WeaponBase Weapon;
    Camera mainCamera;

    new void Awake()
    {
        base.Awake();
        
        setStateType(typeof(PlayerState));
        initData();
        mainCamera = Camera.main;
        Weapon = GetComponentInChildren<WeaponBase>();
        Weapon.player = this;
    }
    new void OnEnable()
    {
        base.OnEnable();
        moveDir = Vector2.zero;
        dashFrameCount = 0;
        setState((int)PlayerState.idle, Weapon.idleAnimType);
    }
    void initData() {//현재는 임시 데이터

        maxHP = 100;
        currentHP = maxHP;
        //moveSpeed = 1;
    }
    public void SetViewPoint() {


        if (!Weapon.CanRotateView)
            return;
       Vector2 viewDir = (mainCamera.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized;

        ViewDirection = (Mathf.RoundToInt((Mathf.Atan2(viewDir.y, viewDir.x) / Mathf.PI * 180f - 180) * -1) + 24) / 45;
        Weapon.WeaponViewDirection = (Mathf.Atan2(viewDir.y, viewDir.x) / Mathf.PI * 180f - 180);
        Weapon.ViewDirection = ViewDirection;

        switch (ViewDirection)
        {
            case 0:
            case 1:
            case 7:
                _sr.flipX = true;
                break;
            case 2:
            case 6:
            case 3:
            case 4:
            case 5:
                _sr.flipX = false;
                break;
        }/*
        if (viewDir.x < 0)
        {
            //transform.localScale = new Vector2(-1, 1);
            //Weapon.transform.localScale = new Vector2(-1, 1);
            _sr.flipX = true;
        }
        else
        {
            //transform.localScale = new Vector2(1, 1);
            //Weapon.transform.localScale = new Vector2(1, 1);
            _sr.flipX = false;
        }*/
    }
    bool MoveInput() {
        SetViewPoint();

        moveDir = new Vector2(0, 0);
        if (InputSystem.instance.getKey(InputKeys.Move_left))
        {
            moveDir.x += -1;
        }
        if (InputSystem.instance.getKey(InputKeys.Move_right))
        {
            moveDir.x += 1;
        }
        if (InputSystem.instance.getKey(InputKeys.Move_up))
        {
            moveDir.y += 1;
        }
        if (InputSystem.instance.getKey(InputKeys.Move_down))
        {
            moveDir.y += -1;
        }
        if (moveDir != Vector2.zero)
        {
            if (dashInput())
            {
                setState((int)PlayerState.dash, Weapon.dashAnimType);
                Weapon.SetDash();
                return true;
            }
            else
            {
                return true;
            }
        }
        else
        {
            return false;
        }
    }
    public override void TakeAttack(float dmg)
    {//!TODO : 대쉬중인지 + 무기가 대쉬중일때 안맞는 무기인지 확인할 것

    }
    public bool doMove(Vector2 moveDir) {
        moveDir.Normalize();
        if (moveDir != Vector2.zero)
        {
            _rigidbody2D.MovePosition((Vector2)transform.position + moveDir * moveSpeed * Time.deltaTime);
            return true;
        }
        return false;
    }
    bool dashInput()
    {//!TODO : dash가능한 상태인지 확인할 것

        return InputSystem.instance.getKeyDown(InputKeys.DashBtn);
    }
    bool doDash(Vector2 moveDir)
    {
        moveDir.Normalize();
        dashFrameCount++;
        if (dashFrameCount >= DashFrameCount)
            return false;
        if (moveDir != Vector2.zero)
        {
            _rigidbody2D.MovePosition((Vector2)transform.position + moveDir * dashSpeed * Time.deltaTime);
            return true;
        }
        return false;
    }
    void MouseInput() {
        Weapon.MouseInput();
    }



    private void FixedUpdate()
    {
        if (moveDir != Vector2.zero)
        {
            if (objectState == (int)PlayerState.move)
            {
                doMove(moveDir);
            }
            else if (objectState == (int)PlayerState.dash)
            {
                doDash(moveDir);
            }
        }
    }
    IEnumerator idle()
    {
        do
        {
            if (MoveInput())
            {
                setState((int)PlayerState.move, Weapon.moveAnimType);
                Weapon.SetMove();
            }

            yield return null;

        } while (!newState);
    }
    IEnumerator move()
    {
        do
        {
            if (!MoveInput()) {
                setState((int)PlayerState.idle, Weapon.idleAnimType);
                Weapon.SetIdle();
            }
            yield return null;
        } while (!newState);
    }
    IEnumerator dash()
    {
        dashFrameCount = 0;
        do
        {
            if (dashFrameCount>=DashFrameCount)
            {
                if (MoveInput())
                {
                    setState((int)PlayerState.move, Weapon.moveAnimType);
                    Weapon.SetMove();
                }
                else {
                    setState((int)PlayerState.idle, Weapon.idleAnimType);
                    Weapon.SetIdle();
                }
            }
            yield return null;
        } while (!newState);
    }
    IEnumerator attack()
    {
        do
        {
            SetViewPoint();
            switch (Weapon.currentMoveCondition)
            {
                case MoveWhileAttack.Move_Attack:
                    if (MoveInput())
                    {
                        setState((int)PlayerState.move, Weapon.moveAnimType);
                    }
                    break;
                case MoveWhileAttack.Move_Cancel_Attack:
                    if (MoveInput())
                    {
                        setState((int)PlayerState.move, Weapon.moveAnimType);
                        Weapon.SetMove();
                    }
                    break;
                case MoveWhileAttack.Cannot_Move:
                    break;
                default:
                    break;
            }
            yield return null;
        } while (!newState);
    }
}
