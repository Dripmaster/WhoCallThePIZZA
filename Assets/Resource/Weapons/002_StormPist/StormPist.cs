﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class StormPist : AttackComponent
{
    public float stepSpeed;//1,2공격 시 한발짝 이동 속도
    public float stepStart;//1,2공격 시 한발짝 이동 시작
    public float stepEnd;//1,2공격 시 한발짝 이동 끝

    public float strongStepSpeed;//3,4공격 시 한발짝 이동 속도
    public float strongStepStart;//3,4공격 시 한발짝 이동 시작
    public float strongStepEnd;//3,4공격 시 한발짝 이동 끝
    public override void SetStrategy(WeaponBase weaponBase)
    {
        idleStrategy = new StormPistIdleStrategy();
        moveStrategy = new StormPistMoveStrategy();
        deadStrategy = new StormPistDeadStrategy();
        mouseInputStrategy = new SampleMouseInputStrategy();
        dashStrategy = new StormPistDashStrategy();
        attackStrategy = new StormPistAttackStrategy(weaponBase);
        skillStrategy = new StormPistSkillStrategy(weaponBase);
        hittedstrategy = new StormPistHittedStrategy();
        
    }
}

public class StormPistIdleStrategy : IdleStrategy
{
    public void SetState(WeaponBase weaponBase)
    {
        if (weaponBase.CanAttackCancel)
            weaponBase.setState((int)PlayerState.idle);
    }
    public void Update(WeaponBase weaponBase)
    {
        weaponBase.SP_FlipX();

        weaponBase.setRotate(weaponBase.WeaponViewDirection);
    }
}
public class StormPistMoveStrategy : MoveFunction,MoveStrategy
{
    public void SetState(WeaponBase weaponBase)
    {
        cannotMove(weaponBase);
    }
    public void Update(WeaponBase weaponBase)
    {
        weaponBase.SP_FlipX();

        weaponBase.setRotate(weaponBase.WeaponViewDirection);
    }
}
public class StormPistDeadStrategy : DeadStrategy
{

    public void SetState(WeaponBase weaponBase)
    {
        //미구현
        weaponBase.setState(PlayerState.dead);
    }
    public void Update(WeaponBase weaponBase)
    {
        
    }
}

public class StormPistSkillStrategy :  SkillValues,SkillStrategy
{
    Vector2 targetPos;//목표지점
    Vector2 startPos;//시작점
    Vector2 lerpPos;//프레임 당 이동(방향벡터)

    float v = 7f;//속도
    float vy = 5f;//y가짜 포물선 속도
    float vyCalculated;//y가짜 포물선 속도를 이동거리에 반비례하게 적용
    float h;
    float t;//경과시간
    float all_t;//전체 이동 시간
    float l;//이동거리
    float skillRange = 1; //스킬 범위

    float maxRange = 4; // 최대도약거리

    int maxSkillTargetCount = 5; //스킬 타격 최대 대상 수
    int currentSkillTargetCount;
    AttackMessage m;

    GameObject[] stormPistHitEffects;  // 
    //float h_Thunder= 0.3f;
    int stormPistHitEffectinitialCount = 5;
    int stormPistHitEffectincrementCount = 1;
    static Pool[] stormPistHitEffectPools;
    Transform effcetParent;

    float stormPistSkillDamage1 = 1;//번개
    float stormPistSkillDamage2 = 0.5f;//충격파
    bool had_Thunder;

    public StormPistSkillStrategy(WeaponBase weaponBase) {
        dashCondition = false;
        moveSkillcondition = MoveWhileAttack.Cannot_Move;
        m = new AttackMessage(); 
        
        if (stormPistHitEffectPools == null)
        {
            var e = weaponBase.GetComponentInChildren<WeaponEffects>();
            stormPistHitEffects = e.Effects;
            effcetParent = e.effectParent;
            stormPistHitEffectPools = new Pool[stormPistHitEffects.Length];
            for (int i = 0; i < stormPistHitEffectPools.Length; i++)
            {
                stormPistHitEffectPools[i] = EffectManager.GetInstance().effectParent.gameObject.AddComponent<Pool>();
                stormPistHitEffectPools[i].poolPrefab = stormPistHitEffects[i];
                stormPistHitEffectPools[i].initialCount = stormPistHitEffectinitialCount;
                stormPistHitEffectPools[i].incrementCount = stormPistHitEffectincrementCount;
                stormPistHitEffectPools[i].Initialize();
            }
        }
    }

    public override void SetCooltime()
    {
        totalCoolTime = 1;
    }

    void PreCalculate() {
        //!TODO(나중에)
        //벽 못넘게 Ray쏴서 확인해서 targetPos 조절
       lerpPos =targetPos-startPos;

        if (lerpPos.sqrMagnitude >= maxRange * maxRange)
        {
            lerpPos.Normalize();
            targetPos = startPos + lerpPos * maxRange;
            l = maxRange;
            
        }
        else
        {

            l = lerpPos.magnitude;
            lerpPos.Normalize();
        }

        lerpPos *= v;

        h = 0;
        t = 0;
        all_t = l/lerpPos.magnitude;
        vyCalculated = vy / all_t;
    }

    public void SetState(WeaponBase weaponBase)
    {
        weaponBase.currentMoveCondition = MoveWhileAttack.Cannot_Move;
        if (weaponBase.ViewDirection < 2 || weaponBase.ViewDirection > 5)// (0, 1, 7,6)  right
            weaponBase.setRotate(0, true);
        else
            weaponBase.setRotate(0, true);

        targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        startPos = player.transform.position;
        PreCalculate();
        weaponBase.AnimSpeed = 1 / all_t;
        weaponBase.setState(PlayerState.skill);
        weaponBase.CanRotateView = false;
        had_Thunder = false;
        player.IgnoreEnemyPlayerCollison(true);
        
    //!TODO
    //날아가는동안 충돌판정 변경 및 지형지물 계산해야함(벽에다가 카직스 e 잘못쓰면)

}
    public void Update(WeaponBase weaponBase)
    {
       
        t += Time.deltaTime;

        float x = lerpPos.x;
        float y = lerpPos.y+h;

        if (t <= all_t * 0.2f) {
            h =vyCalculated;
        }
        else if(t <= all_t * 0.8f)
        {
            
            if (t <= all_t * 0.25f)
            {
                h = Mathf.Lerp(h, 0, Time.deltaTime * 10);
            }else if (t >= all_t * 0.75f)
            {
                h = Mathf.Lerp(h, -vyCalculated, Time.deltaTime * 10);
            }
            else
            {
                h = 0;
            }
        }
        else if(t <= all_t)
        {
            h = -vyCalculated;
        }
        else
        {
            h = 0;
        }
        



        Vector2 movePos = new Vector2(x, y);
        if (had_Thunder == false&&
            t>=all_t*0.7f)
        {
            //스킬이 거의 끝날 때 번개 판정
            Collider2D[] SkillTargetList = AttackManager.GetInstance().GetTargetList(weaponBase.player.transform.position, skillRange, 1 << 10); ;
            int discoveredTargetCount = SkillTargetList.Length;

            //타격대상 개수 확인
            if(discoveredTargetCount < 1)
            {
                currentSkillTargetCount = 0;
            }
            else if(discoveredTargetCount < maxSkillTargetCount)
            {
                currentSkillTargetCount = discoveredTargetCount;
            }
            else
            {
                currentSkillTargetCount = maxSkillTargetCount;
            }

            //데미지
            for(int i = 0; i < currentSkillTargetCount; i++)
            {
                E_ThunderDown(SkillTargetList[i].transform.position);
                AttackManager.GetInstance().HandleAttack(ThunderHandle, SkillTargetList[i].GetComponent<FSMbase>(),player,stormPistSkillDamage1);
            }
            had_Thunder = true;
        }
        if(t >= all_t)
        {
            weaponBase.CanRotateView = true;
            weaponBase.CanAttackCancel = true;
            weaponBase.SetIdle();
            weaponBase.SetPlayerFree();
            had_Thunder = false;

            //스킬이 끝날 때 이펙트 출력
            E_GroundDown(); 
            
            Collider2D[] SkillTargetList = AttackManager.GetInstance().GetTargetList(player.transform.position, skillRange, 1 << 10); ;
            int discoveredTargetCount = SkillTargetList.Length;

            //타격대상 개수 확인
            if (discoveredTargetCount < 1)
            {
                currentSkillTargetCount = 0;
            }
            else if (discoveredTargetCount < maxSkillTargetCount)
            {
                currentSkillTargetCount = discoveredTargetCount;
            }
            else
            {
                currentSkillTargetCount = maxSkillTargetCount;
            }

            //데미지
            for (int i = 0; i < currentSkillTargetCount; i++)
            {
                AttackManager.GetInstance().HandleAttack(GroundHandle, SkillTargetList[i].GetComponent<FSMbase>(), player, stormPistSkillDamage2, false, true);
            }
            weaponBase.AnimSpeed = 1;
        }
        else
        {
            weaponBase.player.AddPosition(movePos);
        }

    }

    public void onWeaponTouch(int colliderType, Collider2D target)
    {

    }
    void E_ThunderDown(Vector2 point) {

        var t = stormPistHitEffectPools[0].GetObjectDisabled(effcetParent);
        t.transform.position = point;  //적 위치
        t.gameObject.SetActive(true);
        t.GetComponent<Effector>().Alpha(0.4f, 0.7f).And().Disable(1f).Play();
    }
    void E_GroundDown() {

        var e = stormPistHitEffectPools[1].GetObjectDisabled(effcetParent);
        e.transform.position = (Vector2)player.transform.position - new Vector2(0,0.1f); ;
        e.gameObject.SetActive(true);
        e.GetComponent<Effector>().Disable(1f).And().Alpha(0.3f, 0.7f).Play();
    }
    AttackMessage ThunderHandle(FSMbase target, FSMbase sender, float attackPoint)
    {
        m.effectType = EffectType.MID;
        m.critEffectType = EffectType.CRIT;
        m.FinalDamage = sender.status.getCurrentStat(STAT.AtkPoint) * attackPoint;

        target.status.AddBuff(new Electrified(1, target));
        return m;
    }
    AttackMessage GroundHandle(FSMbase target, FSMbase sender, float attackPoint)
    {
        m.effectType = EffectType.SMALL;
        m.critEffectType = EffectType.SMALL;
        m.FinalDamage = sender.status.getCurrentStat(STAT.AtkPoint) * attackPoint;
        m.CalcKnockBack(target, sender, 2,2);

        int r = UnityEngine.Random.Range(0, 100);
        if (r < 20)
        {
            target.status.AddBuff(new Electrified(1, target));
        }
        return m;
    }
    public override void StateEnd()
    {
        player.IgnoreEnemyPlayerCollison(false);
        WeaponBase.instance.AnimSpeed = 1;
    }
}


public class StormPistDashStrategy : DashFunction,DashStrategy
{
    public void SetState(WeaponBase weaponBase)
    {
        cannotMove(weaponBase);
    }
    public void Update(WeaponBase weaponBase)
    {

    }
}
public class StormPistAttackStrategy : AttackValues, AttackStrategy
{
    int attackConnectCount=3;//스태틱 몇명
    float[] Damages;
    AttackMessage m;
    WeaponBase weaponBase;
    StormPist stormpist;
    public StormPistAttackStrategy(WeaponBase weaponBase) : base(6)
    {
        stormpist = weaponBase.WeaponComponent() as StormPist;
        this.weaponBase = weaponBase;
        m.effectType = EffectType.SMALL;
        m.critEffectType = EffectType.CRIT;

        attackMoveCondition = MoveWhileAttack.Cannot_Move;
        dashCondition = false;
        Damages =new float[] {
            1,
            1,
            1.2f,
            1,
            1,
            1.2f };
    }
    AttackMessage attackHandle(FSMbase target, FSMbase sender, float attackPoint) {
        m.FinalDamage = sender.status.getCurrentStat(STAT.AtkPoint) * attackPoint;
        m.CalcKnockBack(target, sender, 1,1);

        //20퍼센트 확률로 감전
        int r = UnityEngine.Random.Range(0, 100);
        if (r < 20)
        {
            target.status.AddBuff(new Electrified(1, target));
        }
        return m;
    }
    public void onWeaponTouch(int colliderType, Collider2D target)
    {
        if (attackedColliders.Contains(target))
            return;
        var fsm = target.GetComponent<FSMbase>();
        if (fsm != null)
        {//!TODO 한 공격에 한번만 맞게 할 것
            List<Collider2D> alreadyHitTarget = new List<Collider2D>();  //타격된 저장용
            alreadyHitTarget.Add(target.GetComponent<Collider2D>());
            AttackManager.GetInstance().HandleAttack(attackHandle, fsm, player, Damages[tempAtkCount], false, true);

            for (int i = 0; i < attackConnectCount; i++)
            {
                Collider2D[] targetList = AttackManager.GetInstance().GetTargetList(alreadyHitTarget.Last().transform.position, 10, 1 << 10, alreadyHitTarget);
                if (targetList.Length < 1)
                    break;
                alreadyHitTarget.Add(targetList[0]);
            }
            for (int i = 1; i < alreadyHitTarget.Count; i++)
            {
                AttackManager.GetInstance().HandleAttack(attackHandle, alreadyHitTarget[i].GetComponent<FSMbase>(),player, Damages[tempAtkCount], false, true);
            }
            attackedColliders.Add(target);
        }
    }

    public override void SetCoolTimes()
    {
        coolTimes[0] = 0.2f;
        coolTimes[1] = 0.2f;
        coolTimes[2] = 0.5f;
        coolTimes[3] = 0.2f;
        coolTimes[4] = 0.2f;
        coolTimes[5] = 0.5f;
    }

    public void SetState(WeaponBase weaponBase)
    {
        weaponBase.CanRotateView = true;
        weaponBase.setViewPoint();
        weaponBase.SP_FlipX();

        weaponBase.setRotate(weaponBase.WeaponViewDirection, true);
        CountCombo(weaponBase);

        weaponBase.CanRotateView = false;
    }
    public void Update(WeaponBase weaponBase)
    {
        if (ATK_COMBO_COUNT % 3 != 0 &&
            this.weaponBase.getAnimProgress() <= stormpist.stepEnd && this.weaponBase.getAnimProgress() >= stormpist.stepStart)
            player.moveFoward(stormpist.stepSpeed);
        else if (this.weaponBase.getAnimProgress() <= stormpist.strongStepEnd && this.weaponBase.getAnimProgress() >= stormpist.strongStepStart)
            player.moveFoward(stormpist.strongStepSpeed);

        HandleAttackCancel(weaponBase);
        HandleAttackCommand(weaponBase);
        HandleAttackEND(weaponBase, ()=>{ weaponBase.CanRotateView = true; }) ;
    }
    public override void StateEnd()
    {
        weaponBase.SetColliderEnable(false);
    }
    public override void motionEvent(string msg)
    {
        if(msg == "MoveConditionTo_Move_Attack")
            weaponBase.currentMoveCondition = MoveWhileAttack.Move_Attack;
    }

}

public class StormPistHittedStrategy : HittedStrategy
{
    public void SetState(WeaponBase weaponBase)
    {
        weaponBase.CanRotateView = false;
        weaponBase.setState(PlayerState.hitted);
    }
    public void Update(WeaponBase weaponBase)
    {

    }
}