using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class StormPist
{
    public static void SetStrategy(out IdleStrategy i, out MoveStrategy m, out DeadStrategy d, out MouseInputStrategy mi, out DashStrategy ds, out AttackStrategy a, out CCStrategy c, out SkillStrategy s, WeaponBase weaponBase)
    {
        i = new StormPistIdleStrategy();
        m = new StormPistMoveStrategy();
        d = new StormPistDeadStrategy();
        mi = new SampleMouseInputStrategy();
        ds = new StormPistDashStrategy();
        a = new StormPistAttackStrategy(weaponBase);
        s = new StormPistSkillStrategy();
        c = new StormPistCCStrategy();
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

    int maxSkillTargetCount = 5; //스킬 타격 최대 대상 수
    int currentSkillTargetCount;
    AttackMessage m;

    GameObject[] stormPistHitEffects;  // 프리펩 넣는 방법을 모름. 일단 넣을 프리펩은 2개로 예상중
    //float h_Thunder= 0.3f;
    int stormPistHitEffectinitialCount = 5;
    int stormPistHitEffectincrementCount = 1;
    Pool[] stormPistHitEffectPools;
    Transform effcetParent;

    float stormPistSkillDamage1 = 1;//번개
    float stormPistSkillDamage2 = 0.5f;//충격파
    bool had_Thunder;

    public StormPistSkillStrategy() {
        dashCondition = false;
        moveSkillcondition = MoveWhileAttack.Cannot_Move;
        m = new AttackMessage();
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
        had_Thunder = false;
        if (stormPistHitEffectPools == null)
        {
            var e = weaponBase.GetComponentInChildren<WeaponEffects>();
            stormPistHitEffects = e.Effects;
            effcetParent = e.effcetParent;
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
        }
        else if(t <= all_t)
        {
            h = (all_t-t)*vy;
        }


        float x = startPos.x+lerpPos.x* t;
        float y = startPos.y+lerpPos.y* t+h;

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
            movePos = targetPos;
            weaponBase.CanRotateView = true;
            weaponBase.CanAttackCancel = true;
            weaponBase.SetIdle();
            weaponBase.SetPlayerFree();
            had_Thunder = false;

            //스킬이 끝날 때 이펙트 출력
            E_GroundDown(); 
            
            Collider2D[] SkillTargetList = AttackManager.GetInstance().GetTargetList(weaponBase.player.transform.position, skillRange, 1 << 10); ;
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
                AttackManager.GetInstance().HandleAttack(GroundHandle, SkillTargetList[i].GetComponent<FSMbase>(), player, stormPistSkillDamage2);
            }
            weaponBase.AnimSpeed = 1;
        }

        weaponBase.player.SetPosition(movePos);
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
        e.transform.position = targetPos - new Vector2(0,0.1f); ;
        e.gameObject.SetActive(true);
        e.GetComponent<Effector>().Disable(1f).And().Alpha(0.3f, 0.7f).Play();
    }
    AttackMessage ThunderHandle(FSMbase target, FSMbase sender, float attackPoint)
    {
        m.EffectNum = 1;
        m.Cri_EffectNum = 1;
        m.FinalDamage = sender.status.getCurrentStat(STAT.AtkPoint) * attackPoint;

        //감전
        Debug.Log("낙뢰 감전");
        target.status.AddBuff(new Electrified(1, target));
        return m;
    }
    AttackMessage GroundHandle(FSMbase target, FSMbase sender, float attackPoint)
    {
        m.EffectNum = 0;
        m.Cri_EffectNum = 0;
        m.FinalDamage = sender.status.getCurrentStat(STAT.AtkPoint) * attackPoint;

        // 20퍼센트 확률로 감전
        int r = UnityEngine.Random.Range(0, 100);
        if (r < 20)
        {
            Debug.Log("착지 감전");
            target.status.AddBuff(new Electrified(1, target));
        }
        return m;
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
    public StormPistAttackStrategy(WeaponBase weaponBase) : base(6)
    {
        m.EffectNum = 0;
        m.Cri_EffectNum = 2;

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

        //20퍼센트 확률로 감전
        int r = UnityEngine.Random.Range(0, 100);
        if (r < 20)
        {
            Debug.Log("평타 감전");
            target.status.AddBuff(new Electrified(1, target));
        }
        return m;
    }
    public void onWeaponTouch(int colliderType, Collider2D target)
    {
        var fsm = target.GetComponent<FSMbase>();
        if (fsm != null)
        {//!TODO 한 공격에 한번만 맞게 할 것
            List<Collider2D> alreadyHitTarget = new List<Collider2D>();  //타격된 저장용
            alreadyHitTarget.Add(target.GetComponent<Collider2D>());
            if (tempAtkCount == 2 || tempAtkCount == 5)
                m.knockBackDegree = 0.5f;
            else
                m.knockBackDegree = 0.3f;
            AttackManager.GetInstance().HandleAttack(attackHandle, fsm,player,Damages[tempAtkCount]);

            for (int i = 0; i < attackConnectCount; i++)
            {
                Collider2D[] targetList = AttackManager.GetInstance().GetTargetList(alreadyHitTarget.Last().transform.position, 10, 1 << 10, alreadyHitTarget);
                if (targetList.Length < 1)
                    break;
                alreadyHitTarget.Add(targetList[0]);
            }
            m.knockBackDegree = 0.1f;
            for (int i = 1; i < alreadyHitTarget.Count; i++)
            {
                AttackManager.GetInstance().HandleAttack(attackHandle, alreadyHitTarget[i].GetComponent<FSMbase>(),player, Damages[tempAtkCount]);
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

public class StormPistCCStrategy : CCStrategy
{
    public void SetState(WeaponBase weaponBase)
    {
        weaponBase.CanRotateView = false;
    }
    public void Update(WeaponBase weaponBase)
    {

    }
}