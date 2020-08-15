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
        mainCamera = Camera.main;
        Weapon = GetComponentInChildren<WeaponBase>();
        Weapon.player = this;
        initData();
    }
    new void OnEnable()
    {
        base.OnEnable();
        moveDir = Vector2.zero;
        dashFrameCount = 0;
        setState((int)PlayerState.idle);
    }
    void initData() {//현재는 임시 데이터
        _animator.SetInteger("WeaponNumber", (int)Weapon.weaponType);
        status.setStat(STAT.hp, 50);
        status.setStat(STAT.AtkPoint, 5);
        status.setStat(STAT.moveSpeed, moveSpeed);
        status.setStat(STAT.CriticalDamage, 2);
        status.setStat(STAT.CriticalPoint, 99);
        status.init();
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
    public bool MoveInput() {
        SetViewPoint();

        moveDir = new Vector2(0, 0);
        if (InputSystem.Instance.getKey(InputKeys.Move_left))
        {
            moveDir.x += -1;
        }
        if (InputSystem.Instance.getKey(InputKeys.Move_right))
        {
            moveDir.x += 1;
        }
        if (InputSystem.Instance.getKey(InputKeys.Move_up))
        {
            moveDir.y += 1;
        }
        if (InputSystem.Instance.getKey(InputKeys.Move_down))
        {
            moveDir.y += -1;
        }
        if (moveDir != Vector2.zero)
        {
            if (dashInput())
            {
                setState((int)PlayerState.dash);
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
        //!TODO : 
        if (status.getStat(STAT.hp) <= 0)
        {
            setState((int)PlayerState.dead);
            Weapon.SetDead();
        }
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

        if (Weapon.CanDash())
        {
            return InputSystem.Instance.getKeyDown(InputKeys.DashBtn);
        }
        else
        {
            return false;
        }
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
    public float getAttackDamage() {
        float damage = status.getCurrentStat(STAT.AtkPoint);
        return damage;
    }
    public PlayerState getState()
    {
        return (PlayerState)objectState;
    }

    public void SetComboCount(int c) {
        _animator.SetInteger("ComboCount",c);
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
                setState((int)PlayerState.move);
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
                setState((int)PlayerState.idle);
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
                    setState((int)PlayerState.move);
                    Weapon.SetMove();
                }
                else {
                    setState((int)PlayerState.idle);
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
                        if(!dashInput())
                        setState((int)PlayerState.move);
                    }
                    break;
                case MoveWhileAttack.Move_Cancel_Attack:
                    if (MoveInput())
                    {
                        if (!dashInput())
                        {
                            setState((int)PlayerState.move);
                            Weapon.SetMove();
                        }
                    }
                    break;
                case MoveWhileAttack.Cannot_Move:
                    {
                        MoveInput();
                    }
                    break;
                default:
                    break;
            }
            yield return null;
        } while (!newState);
    }
    IEnumerator skill()
    {
        do
        {
            SetViewPoint();
            switch (Weapon.currentMoveCondition)
            {
                case MoveWhileAttack.Move_Attack:
                    if (MoveInput())
                    {
                        if (!dashInput())
                            setState((int)PlayerState.move);
                    }
                    break;
                case MoveWhileAttack.Move_Cancel_Attack:
                    if (MoveInput())
                    {
                        if (!dashInput())
                        {
                            setState((int)PlayerState.move);
                            Weapon.SetMove();
                        }
                    }
                    break;
                case MoveWhileAttack.Cannot_Move:
                    {
                        MoveInput();
                    }
                    break;
                default:
                    break;
            }
            yield return null;
        } while (!newState);
    }
    IEnumerator CC()
    {
        do
        {
            Weapon.SetCC();
            yield return null;
        } while (!newState);
        Weapon.SetIdle();
    }
    public void SetPosition(Vector2 movePos)
    {
        //TODO : fixedUpdate에서 이동하도록 해야함
        transform.position = movePos;

        //_rigidbody2D.MovePosition(movePos);
    }
    public void AddPosition(Vector2 movePos)
    {
        //TODO : fixedUpdate에서 이동하도록 해야함
        transform.position += (Vector3)movePos;

        //_rigidbody2D.MovePosition(movePos);
    }
}
