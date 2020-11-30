using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MapObject : IHitable
{
    SpriteRenderer _sr;
    protected HitableState hitableState;
    bool activeTake;
    public Hitableinfo myHitableInfo;
    Material tmpMat;
    Collider2D _collider2D;
    bool chainAble;

    void Awake()
    {
        
    }
    private void OnEnable()
    {
        _sr = GetComponent<SpriteRenderer>();
        _collider2D = GetComponent<Collider2D>();
        hitableState = HitableState.idle;
        activeTake = false;
        _sr.sprite = myHitableInfo.DefaultSprite;
        tmpMat = _sr.material;
        showEffect = false;
        status = new StatusBase();
        initData();
        setBuffImmune();
    }
    protected virtual void setBuffImmune()
    {
        foreach (BUFF key in Enum.GetValues(typeof(BUFF)))
        {
            status.BuffImmune[key] = 1;
        }
    }

    public override void initData()
    {
        chainAble = myHitableInfo.ChainAble;
        showEffect = myHitableInfo.ShowEffect;
        status.setStat(STAT.hp, myHitableInfo.MaxHp);
        status.init();
    }
    protected void Update()
    {
        status.UpdateBuff();
        if (activeTake)
        {
            if (InputSystem.Instance.getKeyDown(InputKeys.TakeBtn))
                DoTake();
        }
    }

    public abstract void OnPlayerEnter();
    public abstract void OnPlayerExit();
    public abstract void OnOtherEnter(Collider2D collision);
    public abstract void OnOtherExit(Collider2D collision);
    public virtual void OnHit(float dmg) {
    }

    protected void getDamage(float dmg)
    {
        status.ChangeStat(STAT.hp, -dmg);
        if (status.getCurrentStat(STAT.hp) <= 0)
        {
            DoDest();
        }
    }
    public virtual void DoTake() {
        _collider2D.enabled = false;
        ActivateTake(false);
    }
    public virtual void DoDest()
    {
        _collider2D.enabled = false;
        gameObject.SetActive(false);
    }


    void HandleEnter(Collider2D collision)
    {
        if (hitableState == HitableState.idle)
        {
            if (collision.GetComponent<PlayerFSM>() == null)
            {
                OnOtherEnter(collision);
            }
        }
        if (collision.GetComponent<PlayerFSM>() != null)
        {
            OnPlayerEnter();
            ActivateTake(true);
        }
        hitableState = HitableState.collision;
    }
    void HandleExit(Collider2D collision)
    {
        if (activeTake && collision.GetComponent<PlayerFSM>() != null)
        {
            OnPlayerExit();
            ActivateTake(false);
        }
        if (collision.GetComponent<PlayerFSM>() == null)
        {
            OnOtherExit(collision);
        }
        hitableState = HitableState.idle;
    }
    #region CallHandleFunction
    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandleEnter(collision);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleEnter(collision.collider);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        HandleExit(collision);
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        HandleExit(collision.collider);
    }
    #endregion

    public void ActivateTake(bool value)//사용 가능한 Object일 시 아웃라인 등
    {
        if (myHitableInfo.TakeType == 0)
        {
            return;
        }
        /*상호작용 가능해졌을 때 외곽선이나 상호작용 키 표시 등 수행*/
        if (value)
        {

            _sr.material = myHitableInfo.OutLineMat;
        }
        else
        {
            _sr.material = tmpMat;
        }
        activeTake = value;
    }

    public override void TakeAttack(float dmg, bool cancelAttack)
    {
        OnHit(dmg);
    }

    public abstract override void TakeKnockBack(float force, Vector2 knockBackDir);

    public abstract override void TakeCC(int CCnum = 0);


    public enum HitableState
    {
        idle = 0,
        collision,
        destroy,
        take
    }
}
