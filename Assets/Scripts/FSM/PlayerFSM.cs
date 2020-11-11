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
    Vector2 forcedDir;

    int dashFrameCount;
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
    public void refreshWeapon(int weaponType)
    {
        _animator.SetInteger("WeaponNumber", weaponType);
    }
    void initData()
    {//현재는 임시 데이터
        _animator.SetInteger("WeaponNumber", (int)Weapon.weaponType);
        status.setStat(STAT.hp, 50);
        status.setStat(STAT.AtkPoint, 5);
        status.setStat(STAT.moveSpeed, moveSpeed);
        status.setStat(STAT.CriticalDamage, 2);
        status.setStat(STAT.CriticalPoint, 5);
        status.init();
    }
    public void SetViewPoint() {

        if (Weapon == null)
            return;
        if (!Weapon.CanRotateView)
            return;

        Quaternion r = Quaternion.FromToRotation(Vector2.right, viewDir);
        ViewDirection = (int)r.eulerAngles.z / 45;
        Weapon.WeaponViewDirection = (Mathf.Atan2(viewDir.y, viewDir.x) / Mathf.PI * 180f);
        Weapon.ViewDirection = ViewDirection;
        Weapon.MouseViewDegree = r.eulerAngles.z;
        switch (ViewDirection)
        {
            case 0:
            case 1:
            case 7:
            case 6:
                _sr.flipX = false;
                
                break;
            case 2:
            case 3:
            case 4:
            case 5:
                float w = 180-Weapon.WeaponViewDirection;
                if (w >= 270)
                {
                    w -= 360;
                }
                Weapon.WeaponViewDirection = -w;
                _sr.flipX = true;
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
   
    public bool doMove(Vector2 moveDir) {
        moveDir.Normalize();
        if (moveDir != Vector2.zero)
        {
            _rigidbody2D.MovePosition((Vector2)transform.position + moveDir * status.getCurrentStat(STAT.moveSpeed)*Weapon.weakedSpeed * Time.deltaTime);
            
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

    private void FixedUpdate()
    {
        viewDir = (mainCamera.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized;
        if (stepFoward.IsProgress())
        {
            return;
        }
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
        if(forcedDir != Vector2.zero)
        {
                _rigidbody2D.MovePosition((Vector2)transform.position + forcedDir * Time.deltaTime);
            forcedDir = Vector2.zero;
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
                if(InventorySystem.MyInstance.IsEquipAcc(0))
                {
                    /*풀차지*/
                    status.AddBuff(new BatteryCharged(3, this));
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
    IEnumerator hitted()
    {
        do
        {
            if (animEnd && knockBackDistance <= 0 && !CCreamin())
            {
                CCfree();
            }
            yield return null;
        } while (!newState);
    }
    public override void TakeAttack(float dmg, bool cancelAttack = false)
    {//!TODO : 대쉬중인지 + 무기가 대쉬중일때 안맞는 무기인지 확인할 것
        //!TODO : 
        status.ChangeStat(STAT.hp, -dmg);
        if (status.getCurrentStat(STAT.hp) <= 0)
        {
            setState((int)PlayerState.dead);
            Weapon.SetDead();
        }
        else
        {
            if (cancelAttack)
            {
                setState((int)PlayerState.hitted);
                Weapon.SetHitted();
                _animator.SetTrigger("OneShot");
            }
        }
    }
    public override void TakeCC(int CCnum)
    {//TODO : 하던거 캔슬하게(어차피 캔슬 되지만 추가작업 필요 할 수 있음)
        setState((int)PlayerState.hitted,CCnum);
    }
    public override void CCfree()
    {
        if (MoveInput())
        {
            setState((int)PlayerState.move);
            Weapon.SetMove();
        }
        else
        {
            setState((int)PlayerState.idle);
            Weapon.SetIdle();
        }
    }
    
    public override void TakeKnockBack(float force, Vector2 knockBackDir)
    {

        IgnoreEnemyPlayerCollison(true);
        knockDir = knockBackDir.normalized * force;
    }
    public override void KnockBackEnd()
    {
        knockDir = Vector2.zero;
        knockBackDistance = 0;
        knockBackVelocity = 0;
        IgnoreEnemyPlayerCollison(false);
        
        if (MoveInput())
        {
            setState((int)PlayerState.move);
            Weapon.SetMove();
        }
        else
        {
            setState((int)PlayerState.idle);
            Weapon.SetIdle();
        }
    }
    public void AddPosition(Vector2 movePos)
    {
        forcedDir = movePos;
    }
    public override void moveFoward(StepForwardValues sfv)
    {
        IgnoreEnemyPlayerCollison(true);
        stepFoward.SetStep(sfv, viewDir, IgnoreEnemyPlayerCollison, false);
    }

    
}
