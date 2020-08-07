﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StormPist
{
    public static void SetStrategy(out IdleStrategy i, out MoveStrategy m, out DeadStrategy d, out MouseInputStrategy mi, out DashStrategy ds, out AttackStrategy a, out SkillStrategy s, WeaponBase weaponBase)
    {
        i = new StormPistIdleStrategy();
        m = new StormPistMoveStrategy();
        d = new StormPistDeadStrategy();
        mi = new SampleMouseInputStrategy();
        ds = new StormPistDashStrategy();
        a = new StormPistAttackStrategy(weaponBase);
        s = new StormPistSkillStrategy();
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
        weaponBase.SP_FlipY();

        weaponBase.setRotate(weaponBase.WeaponViewDirection + 180);
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
        weaponBase.SP_FlipY();

        weaponBase.setRotate(weaponBase.WeaponViewDirection + 180);
    }
}
public class StormPistDeadStrategy : DeadStrategy
{

    public void SetState(WeaponBase weaponBase)
    {
        //미구현
    }
    public void Update(WeaponBase weaponBase)
    {
        
    }
}

public class StormPistSkillStrategy :  SkillValues,SkillStrategy
{
    Vector2 targetPos;//목표지점
    Vector2 startPos;//시작점
    Vector2 lerpPos;//프레임 당 이동

    float v = 2.5f;//속도
    float vy = 1f;//y가짜 포물선 속도
    float h;
    float t;//경과시간
    float all_t;//전체 이동 시간
    float l;//이동거리
    float skillRange = 1; //스킬 범위

    int maxSkillTargetCount = 3; //스킬 타격 최대 대상 수
    int currentSkillTargetCount;

    GameObject[] stormPistHitEffects = AttackManager.GetInstance().hitEffects;  // 프리펩 넣는 방법을 모름. 일단 넣을 프리펩은 2개로 예상중
    float h_Thunder= 0.3f;
    int stormPistHitEffectinitialCount = 5;
    int stormPistHitEffectincrementCount = 1;
    Pool[] stormPistHitEffectPools;

    public StormPistSkillStrategy() {
        dashCondition = false;
        moveSkillcondition = MoveWhileAttack.Cannot_Move;
    }

    public override void SetCooltime()
    {
        totalCoolTime = 5;
    }

    void PreCalculate() {
       lerpPos =targetPos-startPos;
        l = lerpPos.magnitude;
       lerpPos.Normalize();
        lerpPos *= v;

        h = 0;
        t = 0;
        all_t = l/lerpPos.magnitude;
        
    }

    public void SetState(WeaponBase weaponBase)
    {
        weaponBase.currentMoveCondition = MoveWhileAttack.Cannot_Move;
        if (weaponBase.ViewDirection < 2 || weaponBase.ViewDirection > 6)// (0, 1, 7)  Left      2, 6 은 원하는데로 설정
            weaponBase.setRotate(180);
        else
            weaponBase.setRotate(0);

        weaponBase.setState(PlayerState.skill);
        weaponBase.CanRotateView = false;
        targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        startPos = weaponBase.player.transform.position;
        PreCalculate();
        weaponBase.AnimSpeed = 1 / all_t;

        if (stormPistHitEffectPools == null)
        {
            stormPistHitEffectPools = new Pool[stormPistHitEffects.Length];
            for (int i = 0; i < stormPistHitEffectPools.Length; i++)
            {
                stormPistHitEffectPools[i] = AttackManager.GetInstance().effcetParent.gameObject.AddComponent<Pool>();
                stormPistHitEffectPools[i].poolPrefab = stormPistHitEffects[i];
                stormPistHitEffectPools[i].initialCount = stormPistHitEffectinitialCount;
                stormPistHitEffectPools[i].incrementCount = stormPistHitEffectincrementCount;
                stormPistHitEffectPools[i].Initialize();
            }
        }
    //!TODO
    //날아가는동안 충돌판정 변경 및 지형지물 계산해야함(벽에다가 카직스 e 잘못쓰면)
    //마우스까지가 아닌 최대 거리 있어서 마우스 방향 최대거리로(안쪽이면 그냥 안쪽)

}
    public void Update(WeaponBase weaponBase)
    {
       
        t += Time.deltaTime;

        if (t <= all_t * 0.4f) {
            h = t*vy;
        }
        else if(t < all_t * 0.7f)
        {
            //너무 여러개 생김 if문으로 한번 떨어지게 했더니 안보이는듯 내일 다시 해봄
            //최고점에서 낙뢰
            var t = stormPistHitEffectPools[1].GetObjectDisabled();
            t.transform.position = new Vector2(targetPos.x, targetPos.y + h_Thunder);  //시작점
            t.gameObject.SetActive(true);
            t.GetComponent<Effector>().Alpha(0.4f, 0.7f).And().Move(0.4f, new Vector2(0, -h_Thunder)).And().Disable(0.4f).Play();
            
        }
        else if(t <= all_t)
        {
            h = (all_t-t)*vy;
        }


        float x = startPos.x+lerpPos.x* t;
        float y = startPos.y+lerpPos.y* t+h;

        Vector2 movePos = new Vector2(x, y);
        if (//Vector2.SqrMagnitude(movePos-(Vector2)weaponBase.player.transform.position)>=
            //Vector2.SqrMagnitude(targetPos - (Vector2)weaponBase.player.transform.position))
            t>=all_t)
        {
            movePos = targetPos;
            weaponBase.CanRotateView = true;
            weaponBase.CanAttackCancel = true;
            weaponBase.SetIdle();
            weaponBase.SetPlayerFree();

        //스킬이 끝날 때 이펙트 출력
       
            var e = stormPistHitEffectPools[0].GetObjectDisabled();
            e.transform.position = targetPos;
            e.gameObject.SetActive(true);
            e.gameObject.GetComponent<SpriteRenderer>().rendererPriority = -1;  //Order in Layer -1 로 설정 해야함(틀림)
            e.GetComponent<Effector>().Scale(0.1f, 8f).And().Disable(0.3f).And().Alpha(0.3f, 0.7f).Play();
        
            //스킬이 끝날 때 데미지 판정
            Collider2D[] SkillTargetList = AttackManager.GetInstance().GetTargetList(targetPos, skillRange, 1 << 10);
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
                AttackManager.GetInstance().HandleDamage(60, SkillTargetList[i].GetComponent<FSMbase>());
            }

            weaponBase.AnimSpeed = 1;
        }

        weaponBase.player.SetPosition(movePos);
    }

    public void onWeaponTouch(int colliderType, Collider2D target)
    {

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
    int attackConnectCount;//스태틱 몇명
    public StormPistAttackStrategy(WeaponBase weaponBase) : base(6)
    {
        attackMoveCondition = MoveWhileAttack.Cannot_Move;
        dashCondition = false;
        attackConnectCount = 3;
    }

    public void onWeaponTouch(int colliderType, Collider2D target)
    {
        var fsm = target.GetComponent<FSMbase>();
        if (fsm != null)
        {
            List<Collider2D> alreadyHitTarget = new List<Collider2D>();  //타격된 저장용
            alreadyHitTarget.Add(target.GetComponent<Collider2D>());

            AttackManager.GetInstance().HandleDamage(50, fsm,2);

            for (int i = 0; i < attackConnectCount; i++)
            {
                Collider2D[] targetList = AttackManager.GetInstance().GetTargetList(alreadyHitTarget.Last().transform.position, 10, 1 << 10, alreadyHitTarget);
                if (targetList.Length < 1)
                    break;
                alreadyHitTarget.Add(targetList[0]);
            }
            for (int i = 1; i < alreadyHitTarget.Count; i++)
            {
                AttackManager.GetInstance().HandleDamage(10, alreadyHitTarget[i].GetComponent<FSMbase>(),0);
            }
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
        weaponBase.SP_FlipY();

        weaponBase.setRotate(weaponBase.WeaponViewDirection + 180);
        CountCombo(weaponBase);

        weaponBase.CanRotateView = false;
    }
    public void Update(WeaponBase weaponBase)
    {
        HandleAttackCancel(weaponBase);
        HandleAttackCommand(weaponBase);
        HandleAttackEND(weaponBase, ()=>{ weaponBase.CanRotateView = true; }) ;
    }

}