using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeFsm : EnemyBase
{
    new void Awake()
    {
        base.Awake();
        setStateType(typeof(SlimeState));
    }
    new void OnEnable()
    {
        base.OnEnable();
        setState((int)SlimeState.patrol);
    }
    void FixedUpdate()
    {
        movePatrol((int)SlimeState.patrol);
        moveAggro((int)SlimeState.aggro);
        moveKnockBack();
    }
    public override void initData()
    {
        status.setStat(STAT.hp, 20);
        status.setStat(STAT.AtkPoint, 5);
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
                setAggro((int)SlimeState.aggro);
            }
            else if (tmpTime >= tIdle)
            {
                setState((int)SlimeState.patrol);

            }
            yield return null;
        } while (!newState);
    }
    IEnumerator patrol()
    {
        float tmpTime = 0;
        moveDir = Vector2.zero;
        do
        {
            tmpTime += Time.deltaTime;
            if (detectPlayer(disDetect))
            {
                setAggro((int)SlimeState.aggro);
            }
            else if (tmpTime >= tPatrol + tIdle)
            {
                tmpTime = 0;
                moveDir = Vector2.zero;
            }
            else if (moveDir == Vector2.zero && tmpTime >= tIdle)
            {
                moveDir = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
            }
            yield return null;
        } while (!newState);
    }
    IEnumerator aggro()
    {
        do
        {

            moveDir = (player.position - transform.position).normalized;
            if (!detectPlayer(disMaxDetect))
            {
                setState((int)SlimeState.patrol);
            }
            else
            if (Time.realtimeSinceStartup >= coolStartTime + aggroTime && Vector2.Distance(player.position, transform.position) <= disAttackRange)
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
            if (animEnd&&knockBackDistance <= 0&&!CCreamin())
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
            //TODO: 공격 시 몇% 애니 재생 시 판정으로 할 지
            //      아니면 애니메이션에서 키프레임에서 이벤트 호출시킬지
            //      정해야함 / 키프레임 하기로 정해짐 나중에 제대로 애니메이션 나오면 적용
            if (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.99f)
            {//attack Anim 종료
                doSimpleAttack();
                setAggro((int)SlimeState.aggro);
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
    public override void TakeAttack(float dmg, bool cancelAttack = false)
    {
        status.ChangeStat(STAT.hp, -dmg);
        if (status.getCurrentStat(STAT.hp) <= 0)
        {
            setState((int)SlimeState.dead);
        }
        else
        {
            setState((int)SlimeState.hitted);
            _animator.SetTrigger("OneShot");
            hittedNextState = (int)SlimeState.aggro;
        }
    }
    public override void TakeKnockBack(float distance, float velocity, Vector2 knockBackDir)
    {

        if (status.getCurrentStat(STAT.hp) <= 0)
        {
            return;
        }
        _animator.SetTrigger("OneShot");
        setState((int)SlimeState.hitted);
        hittedNextState = (int)SlimeState.aggro;
        knockBackDistance = distance;
        knockBackVelocity = velocity;

        knockDir = knockBackDir.normalized * distance + (Vector2)transform.position;
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

    public enum SlimeState
    {//슬라임의 스테이트(고유 번호 고정)
        patrol = 0,
        aggro,
        attack,
        dead,
        hitted,
    }
}
