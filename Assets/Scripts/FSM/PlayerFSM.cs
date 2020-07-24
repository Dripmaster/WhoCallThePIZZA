using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFSM : FSMbase
{
    public float moveSpeed;
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
        setState((int)PlayerState.idle, Weapon.idleAnimType);
    }
    void initData() {//현재는 임시 데이터

        maxHP = 100;
        currentHP = maxHP;
        //moveSpeed = 1;
    }
    void SetViewPoint() {

        Vector2 viewDir = (mainCamera.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized;

        ViewDirection = (Mathf.RoundToInt((Mathf.Atan2(viewDir.y, viewDir.x) / Mathf.PI * 180f - 180) * -1) + 24) / 45;
        Weapon.ViewDirection = ViewDirection;
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
        }
    }
    bool MoveInput() {
        SetViewPoint();
        Vector2 moveDir = new Vector2(0, 0);

        if (Input.GetKey(KeyCode.A))
        {
            moveDir.x += -1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            moveDir.x += 1;
        }
        if (Input.GetKey(KeyCode.W))
        {
            moveDir.y += 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            moveDir.y += -1;
        }
        moveDir.Normalize();
        if (moveDir != Vector2.zero)
        {
            _rigidbody2D.MovePosition((Vector2)transform.position + moveDir * moveSpeed*Time.deltaTime);
            return true;
        }
        return false;
    }
    public override void TakeAttack(float dmg)
    {

    }
    void MouseInput() {
        Weapon.MouseInput();
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
