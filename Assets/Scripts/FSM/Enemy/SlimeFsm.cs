using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeFsm : EnemyBase
{
    public DroppedItemBase myDropItem;
    public float jumpWaitTime  =1f;
    bool attackTrigger = false;
    new void Awake()
    {
        base.Awake();
        setStateType(typeof(SlimeState));
    }
    new void OnEnable()
    {
        base.OnEnable();
        setState((int)SlimeState.idle);
    }
    void FixedUpdate()
    {
        moveAggro((int)SlimeState.jump);
        moveKnockBack();
    }
    public override void initData()
    {
        status.setStat(STAT.hp, 50);
        status.setStat(STAT.AtkPoint, 10);
        status.setStat(STAT.moveSpeed, 1);
        status.init();
        coolTimes = new float[]
        {
            3
        };
        damages = new float[]
        {
            1
        };
        attackCount = 1;
        tPatrol = 3;
        tIdle = 2;
        disDetect = 5;
        disMaxDetect = 7;
        disAttackRange = 1;
        //끝
        tempAttackCount = 0;
        aggroTime = 0;
        coolStartTime = 0;
        hadAttack = false;
    }
    IEnumerator idle()
    {
        float tmpTime = 0;
        do
        {
            tmpTime += Time.deltaTime;
            if (detectPlayer(disDetect))
            {
                setAggro((int)SlimeState.jump);
            }
            yield return null;
        } while (!newState);
    }
    IEnumerator jump()
    {
        _animator.SetTrigger("OneShot");
        float tempTime = 0;
        do
        {
            if (animEnd)
            {
                moveDir = Vector2.zero;
                tempTime += Time.deltaTime;
                if (tempTime < jumpWaitTime)
                {
                    yield return null;
                    continue;
                }
                else
                {
                    setState((int)SlimeState.jump);
                    yield return null;
                    continue;
                }
            }
            moveDir = (player.position - transform.position).normalized;
            if (!detectPlayer(disMaxDetect))
            {
                setState((int)SlimeState.idle);
            }
            else
            if (canAttackPlayer())
            {
                setState((int)SlimeState.attack);
            }
            yield return null;
        } while (!newState);
    }
    IEnumerator hitted()
    {
        do
        {
            yield return null;
            if (animEnd&&!CCreamin())
            {
                CCfree();
            }
        } while (!newState);
    }
    IEnumerator attack()
    {
        aggroTime = 0;
        hadAttack = false;
        SetComboCount(tempAttackCount);
        do
        {
           if (attackTrigger)
            {
                attackTrigger = false;
                doSimpleAttack();
            }
            if (animEnd)
            {
                setAggro((int)SlimeState.jump);
            }
            yield return null;
        } while (!newState);
        if (hadAttack)
        {
            aggroTime = coolTimes[tempAttackCount];
            tempAttackCount++;
            if (tempAttackCount >= attackCount)
            {
                tempAttackCount = 0;
            }
            coolStartTime = Time.realtimeSinceStartup;
        }
    }
    public override void  sendAttackMessage(int attackType)
    {
        attackTrigger = true;
    }
    public override void TakeAttack(float dmg, bool cancelAttack = false)
    {
        if (status.getCurrentStat(STAT.hp) <= 0)
        {
            return;
        }

        status.ChangeStat(STAT.hp, -dmg);
        if (status.getCurrentStat(STAT.hp) <= 0)
        {
            setState((int)SlimeState.dead);
        }
        else
        {
            setState((int)SlimeState.hitted);
            _animator.SetTrigger("OneShot");
            hittedNextState = (int)SlimeState.jump;
        }
    }
    public override void TakeKnockBack(float force, Vector2 knockBackDir)
    {
        if (status.getCurrentStat(STAT.hp) <= 0)
        {
            return;
        }
        _animator.SetTrigger("OneShot");
        setState((int)SlimeState.hitted);
        hittedNextState = (int)SlimeState.jump;

        knockDir = knockBackDir.normalized*force;
        SetCollidersTriggerNotTerrain(true);
    }
    
    public override void KnockBackEnd()
    {

    }

    public override void TakeCC(int CCnum)
    {
        setState((int)SlimeState.hitted,CCnum);
        
        SetCollidersTriggerNotTerrain(true);
    }
    public override void CCfree()
    {
        setState(hittedNextState);
        SetCollidersTriggerNotTerrain(false);
    }
    public override void DropItem()
    {
       // ItemDropSystem.MyInstance.DropItem(transform.position,myDropItem);
    }
    public enum SlimeState
    {//슬라임의 스테이트(고유 번호 고정)
        idle = 0,
        jump,
        attack,
        dead,
        hitted,
    }
}
