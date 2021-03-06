﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SickSlime : SlimeFsm
{
    //투사체
    AttackMessage m;
    Transform SickSlimeLaunchTransform;
    public GameObject SickBulletPrefab;
    Pool SickBulletPool;
    int SickBulletinitialCount = 3;
    int SickBulletincrementCount = 1;
    Vector2 startPos;
    Vector2 targetPos;

    public float vy = 5f;//y가짜 포물선 속도         몇으로 해야하는 거임???
    public float all_t = 1;//전체 이동 시간

    new void Awake()
    {
        if(SickBulletPool == null)
        {
            SickBulletPool = AttackManager.GetInstance().bulletParent.gameObject.AddComponent<Pool>();
            SickBulletPool.incrementCount = SickBulletincrementCount;
            SickBulletPool.initialCount = SickBulletinitialCount;
            SickBulletPool.poolPrefab = SickBulletPrefab;
            SickBulletPool.Initialize();
        }

        SickSlimeLaunchTransform = this.gameObject.transform;
        base.Awake();
        setStateType(typeof(SlimeState));
    }

    public override void initData()
    {
        status.setStat(STAT.hp, 20);
        status.setStat(STAT.AtkPoint, 1);
        status.setStat(STAT.moveSpeed, 2);
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
        disDetect = 7;
        disMaxDetect = 7;
        disAttackRange = 7;
        //Status setting

        tempAttackCount = 0;
        aggroTime = 0;
        coolStartTime = 0;
        hadAttack = false;
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
                rangedAttack();//원거리 공격 추가
                hadAttack = true;
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

    public void rangedAttack()
    {
        targetPos = player.transform.position;
        startPos = SickSlimeLaunchTransform.position;

        var b = SickBulletPool.GetObjectDisabled().GetComponent<SickBullet>();
        b.transform.position = SickSlimeLaunchTransform.position;
        b.targetPos = targetPos;
        b.startPos = startPos;
        b.vy = this.vy ;
        b.all_t = this.all_t;
        b.touched = null;
        b.touched += SickBulletTouched;
        b.gameObject.SetActive(true);
    }

    bool SickBulletTouched(Collider2D collision)
    {
        var fsm = collision.GetComponent<PlayerFSM>();
        if (fsm != null)
        {
            AttackManager.GetInstance().HandleAttack(bulletHandle, fsm, this, damages[0]);
            return true;
        }
        return false;
    }
    AttackMessage bulletHandle(IHitable target, FSMbase sender, float attackPoint)
    {
        m.FinalDamage = sender.status.getCurrentStat(STAT.AtkPoint) * attackPoint;
        target.status.AddBuff(new Poisoned(5, 3, target));
        return m;
    }
}
