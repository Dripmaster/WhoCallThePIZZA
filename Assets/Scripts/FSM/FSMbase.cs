﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**
* @brief FSM을 가지는 오브젝트들의 부모 클래스.
* @details 상태와 방향에 맞는 애니메이션 재생을 담당하며, 상태 변화에 따라 코루틴을 실행한다. *FSMmain 클래스를 컴포넌트로 연결금지!
* @author Dripmaster, 한글닉이최고
* @date 2020-05-17
* @version 0.0.1
*
*/
public abstract class FSMbase : IHitable
{
    protected Animator _animator;
    protected SpriteRenderer _sr;
    protected StepFoward stepFoward;
    public int objectState;
    Type stateType;
    protected bool newState = false;
    int viewDirection;
    private float animSpeed;
    protected Rigidbody2D _rigidbody2D;
    Collider2D[] _colliders;

    protected float knockBackVelocity;
    protected float knockBackDistance;
    protected Vector2 knockDir;
    protected Vector2 viewDir;
    protected int hittedNextState;
    public  bool animEnd;

    protected ZSystem zSystem;
    protected bool isDead;

    public ZSystem GetZSystem()
    {
        return zSystem;
    }

    protected float AnimSpeed {
        get {
            return animSpeed;
        }
        set {
            animSpeed = value;
            if (_animator != null)
            {
                _animator.SetFloat("SpeedParam", animSpeed);
            }
        }
    }
    public int ViewDirection {
        get {
            return viewDirection;
        }
        set {
            if (viewDirection != value % 8)
            {
                viewDirection = value % 8;
                if (_animator != null)
                {
                   // _animator.SetFloat("BlendParam", (ViewDirection *0.1f));
                }
            }
        }
    }
    protected void Awake()
    {
        _animator = GetComponent<Animator>();
        var rigids = GetComponentsInChildren<Rigidbody2D>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _sr = GetComponentInChildren<SpriteRenderer>();
        status = new StatusBase();
        _colliders = GetComponentsInChildren<CircleCollider2D>();
        stepFoward = GetComponent<StepFoward>();
    }
    protected void Update()
    {
        status.UpdateBuff();
    }
    public virtual void setZ(float z)
    {
        zSystem.Z = z;
    }
    protected void OnEnable()
    {
        zSystem = GetComponent<ZSystem>();
        ViewDirection = 6;
        setState(0);
        AnimSpeed = 1;
        isDead = false;
        StartCoroutine(FSMmain());
    }
    protected void setStateType(Type t) {
        stateType = t;
    }
    public void setState(int state) {
        objectState = state;
        newState = true;
        _animator.SetInteger("State",objectState);
    }
    public void setState(int state,int combo)
    {
        objectState = state;
        newState = true;
        _animator.SetInteger("State", objectState);
        _animator.SetInteger("ComboCount", combo);
    }
    public void SetComboCount(int c)
    {
        _animator.SetInteger("ComboCount", c);
    }
    public void SetAnimEnd()
    {
        animEnd = true;
    }
    IEnumerator FSMmain() {
        while (true)
        {
            newState = false;
            animEnd = false;
            yield return StartCoroutine(Enum.GetName(stateType, objectState));
        }
    }
    IEnumerator idle() {
        do
        {
            yield return null;
        } while (!newState);
    }
    IEnumerator move()
    {
        do
        {
            yield return null;
        } while (!newState);
    }
    
    IEnumerator dead()
    {
        
        foreach (var item in _colliders)
        {
            item.enabled = false;
        }
        DropItem();
        do
        {
            yield return null;
        } while (!newState);
    }
   // public abstract void TakeKnockBack(float distance, float velocity, Vector2 knockBackDir);


    public abstract void CCfree();
    public abstract void KnockBackEnd();

    public bool CCreamin()
    {
        if (status.IsBuff(BUFF.Electrified))//CC상태 쭉 추가(속박 등 나중엔 배열로)
        {
            return true;
        }
        if (status.IsBuff(BUFF.Stuned))//CC상태 쭉 추가(속박 등 나중엔 배열로)
        {
            return true;
        }
        return false;
    }
    public void IgnoreEnemyPlayerCollison(bool value)//플레이어와 적의 충돌 무시
    {//도약, 돌진, 넉백 시 호출
        Physics2D.IgnoreLayerCollision(14, 9, value);
        Physics2D.IgnoreLayerCollision(15, 10, value);
    }
    public Collider2D getCollider()
    {
        return _colliders[0];
    }
    public Collider2D getChildCollider()
    {
        return _colliders[1];
    }
    public Collider2D getTerrainCollider()
    {
        return _colliders[2];
    }
    
    public bool getAnimEnd(float targetTime = 0.99f)
    {
        if (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= targetTime)
        {
            return true;
        }
        return false;
    }
    public float getAnimProgress()
    {
        return _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }
    public virtual void DropItem() { }

    public abstract void moveFoward(StepForwardValues sfv);
    public override abstract void TakeAttack(float dmg, bool cancelAttack);

    public delegate void StepFowardCallBack(bool v);
}
