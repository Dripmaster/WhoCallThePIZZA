using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 
     구현중단!
     
     
     
     */
public abstract class HitFuncStrategy
{
    public HitableBase objectBase;
    public abstract void HitFunc(float dmg);
}
public abstract class CollisionFuncStrategy
{
    public HitableBase objectBase;
    public abstract void CollsionFunc();
}
public abstract class TakeFuncStrategy
{
    public HitableBase objectBase;
    public abstract void TakeFunc();
}
public class DefaultHitFunc : HitFuncStrategy
{
    public override void HitFunc(float Dmg)
    {
        objectBase.setState((int)HitableState.destroy);
    }
}
public class DefaultHPHitFunc : HitFuncStrategy
{
    public override void HitFunc(float Dmg)
    {
        if(objectBase.currentHp<=0)
            objectBase.setState((int)HitableState.destroy);
        else
        {
            objectBase.currentHp -= Dmg;
        }
    }
}
public class DefaultCollsionFunc : CollisionFuncStrategy
{
    public override void CollsionFunc()
    {
        objectBase.setState((int)HitableState.collision);
    }
}
public class DefaultTakeFunc : TakeFuncStrategy
{
    bool isUsed = false;
    public override void TakeFunc()
    {

        if (!isUsed)
        {
            WeaponBase.instance.player.status.AddBuff(new Bleeding(5, 3, WeaponBase.instance.player));

            objectBase.setState((int)HitableState.take);
        }
        isUsed = true;
    }
}
public enum HitableState
{
    idle = 0,
    collision,
    destroy,
    take
}
public class HitableBase : FSMbase
{
    HitFuncStrategy hitFuncStrategy;
    CollisionFuncStrategy collisionFuncStrategy;
    TakeFuncStrategy takeFuncStrategy;
    public Hitableinfo myHitableInfo;
    float maxHp;
    public float currentHp;
    Material tmpMat;

    
    HitableState hitableState;
    bool activeTake;

    public new void Awake()
    {
        base.Awake();
        setStateType(typeof(HitableState));
    }
    public new void OnEnable()
    {
        base.OnEnable();
        hitableState = HitableState.idle;
        activeTake = false;
        setState((int)HitableState.idle);
        status.init();

        _animator.runtimeAnimatorController = myHitableInfo.AnimController;
        tmpMat = _sr.material;
        _sr.sprite = myHitableInfo.DefaultSprite;
        setStrategy();
    }
    void setStrategy()
    {
        switch (myHitableInfo.HitType)
        {
            case 1:
                hitFuncStrategy = new DefaultHitFunc();
                break;
            case 2:
                hitFuncStrategy = new DefaultHPHitFunc();
                maxHp = myHitableInfo.MaxHp;
                currentHp = maxHp;
                break;
            default:
                break;
        }
        switch (myHitableInfo.CollisionType)
        {
            case 1:
                collisionFuncStrategy = new DefaultCollsionFunc();
                break;
            default:
                break;
        }
        switch (myHitableInfo.TakeType)
        {
            case 1:
                takeFuncStrategy = new DefaultTakeFunc();
                break;
            default:
                break;
        }
        if(hitFuncStrategy!=null)
        hitFuncStrategy.objectBase = this;
        if (collisionFuncStrategy !=null)
        collisionFuncStrategy.objectBase = this;
        if(takeFuncStrategy!=null)
        takeFuncStrategy.objectBase = this;
    }


    IEnumerator idle()
    {
        do
        {
            if (activeTake)
            {
                if (InputSystem.Instance.getKeyDown(InputKeys.TakeBtn))
                    TakeFunc();
            }
            yield return null;
        } while (!newState);
    }
    IEnumerator collision()
    {
        do
        {
            if (activeTake)
            {
                if (InputSystem.Instance.getKeyDown(InputKeys.TakeBtn))
                    TakeFunc();
            }
            yield return null;
            if(animEnd)
                setState((int)HitableState.idle);
        } while (!newState);
    }
    IEnumerator destroy()
    {
        getCollider().enabled = false;
        do
        {
            if (activeTake)
            {
                if (InputSystem.Instance.getKeyDown(InputKeys.TakeBtn))
                    TakeFunc();
            }
            yield return null;
            if (animEnd)
                gameObject.SetActive(false);
        } while (!newState);
    }
    IEnumerator take()
    {
        getCollider().enabled = false;
        ActivateTake(false);
        do
        {
            yield return null;
            if (animEnd)
                gameObject.SetActive(false);
        } while (!newState);
    }
    public void ActivateTake(bool value)
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
    public void HitFunc(float Dmg = 0)
    {
        if(hitFuncStrategy!=null)
        hitFuncStrategy.HitFunc(Dmg);
    }
    public void CollisionFunc()
    {
        if(collisionFuncStrategy !=null)
        collisionFuncStrategy.CollsionFunc();
    }
    public void TakeFunc()
    {
        if(takeFuncStrategy!=null)
        takeFuncStrategy.TakeFunc();
        ActivateTake(false);

    }

    void HandleEnter(Collider2D collision)
    {
        if(hitableState == HitableState.idle)
        {
            if(collision.GetComponent<FSMbase>() != null)
            {
                CollisionFunc();
            }
        }
        if (collision.GetComponent<PlayerFSM>() != null)
        {
            ActivateTake(true);
        }
        hitableState = HitableState.collision;
    }
    void HandleExit(Collider2D collision)
    {
        if(activeTake&& collision.GetComponent<PlayerFSM>() != null)
        {
            ActivateTake(false);
        }
        hitableState = HitableState.idle;
    }
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

    public override void TakeAttack(float dmg, bool cancelAttack)
    {
        HitFunc(dmg);
    }

 

    public override void TakeCC(int CCnum = 0)
    {
    }

    public override void CCfree()
    {
    }

    public override void KnockBackEnd()
    {
    }

    public override void moveFoward(StepForwardValues sfv)
    {
      
    }

    public override void TakeKnockBack(float force, Vector2 knockBackDir)
    {
    }

    public override void initData()
    {
    }
}
