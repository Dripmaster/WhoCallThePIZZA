using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;

public class SampleWeapon {
    public static void SetStrategy(out IdleStrategy i, out MoveStrategy m, out DeadStrategy d, out MouseInputStrategy mi, out DashStrategy ds, out AttackStrategy a, out CCStrategy c, out SkillStrategy s, WeaponBase weaponBase)
    { 
        i = new SampleIdleStrategy();
        m = new SampleMoveStrategy();
        d = new SampleDeadStrategy();
        mi = new SampleMouseInputStrategy();
        ds= new SampleDashStrategy();
        a = new SampleAttackStrategy(weaponBase);
        s = new SampleSkillStrategy();
        c = new SampleCCStrategy();
    }
}
public class SampleIdleStrategy : IdleStrategy
{

    public void SetState(WeaponBase weaponBase)
    {
        if (weaponBase.CanAttackCancel)
            weaponBase.setState((int)PlayerState.idle);
    }
    public void Update(WeaponBase weaponBase)
    {
        weaponBase.setRotate(weaponBase.WeaponViewDirection);
        switch (weaponBase.ViewDirection)
        {
            case 0:
            case 1:
            case 7:
                weaponBase.setFlip(true);
                break;
            case 2:
            case 6:
            case 3:
            case 4:
            case 5:weaponBase.setFlip(false);
                break;
        }
    }
}
public class SampleMoveStrategy : MoveFunction,MoveStrategy
{

    public void SetState(WeaponBase weaponBase)
    {
        cannotMove(weaponBase);
    }
    
    public void Update(WeaponBase weaponBase)
    {
        weaponBase.setRotate(weaponBase.WeaponViewDirection);
        switch (weaponBase.ViewDirection)
        {
            case 0:
            case 1:
            case 7:
                weaponBase.setFlip(true);
                break;
            case 2:
            case 6:
            case 3:
            case 4:
            case 5:
                weaponBase.setFlip(false);
                break;
        }
    }
}
public class SampleDeadStrategy : DeadStrategy
{

    public void SetState(WeaponBase weaponBase)
    {
        weaponBase.setState(PlayerState.dead);
    }
    public void Update(WeaponBase weaponBase)
    {

    }
}
public class SampleMouseInputStrategy : MouseInputStrategy
{
    public void HandleInput(WeaponBase weaponBase)
    {
        /////기본 공격
        if (!weaponBase.isDash&&InputSystem.Instance.getKey(InputKeys.MB_L_click))
        {
            if (!weaponBase.IsAttackCoolTimeRemain()&&weaponBase.CanAttackCancel)
            {
                weaponBase.CanAttackCancel = false;

                if (weaponBase.getMoveAttackCondition() == MoveWhileAttack.Move_Attack)
                {
                    ///움직이면서 공격이 되는 애면
                    ///움직이면서 공격 할 때
                    ///SetAttack(false)호출해야함(플레이어는 계속 움직이고 무기만 공격상태)
                    if (weaponBase.getPlayerState() != PlayerState.move)
                    {
                        weaponBase.SetAttack(true);
                    }
                    else
                    {
                        weaponBase.SetAttack(false);
                    }
                }
                else
                {
                    weaponBase.SetAttack(true);
                }
            }
        }
        ////스킬
        if (!weaponBase.isDash && InputSystem.Instance.getKeyDown(InputKeys.SkillBtn))
        {
            if (!weaponBase.IsSkillCoolTimeRemain() && weaponBase.CanAttackCancel)
            {
                weaponBase.CanAttackCancel = false;
                //!TODO skill도 따로 확인할 것
                if (weaponBase.getMoveSkillCondition() == MoveWhileAttack.Move_Attack)
                {
                    if (weaponBase.getPlayerState() != PlayerState.move)
                    {
                        weaponBase.SetSkill(true);
                    }
                    else
                    {
                        weaponBase.SetSkill(false);
                    }
                }
                else
                {
                    weaponBase.SetSkill(true);
                }
            }
        }

    }
}
public class SampleSkillStrategy : SkillValues, SkillStrategy
{
    Transform headTransform;
    BoxCollider2D headCollider;
    List<Transform> headChain;
    Transform headChainP;
    float v=9;//head속도
    float v2 = 7;//날라가는 속도
    WeaponBase weaponBase;
    bool collisionFlag = false;
    Vector2 targetPos;
    Vector2 moveDir;
    Vector2 tempPos;
    Vector2 startPos;
    float distance = 1.7f;
    Pool pool;
    GameObject[] chains;

    int chainCount = 100;
    public SampleSkillStrategy()
    {
        moveSkillcondition = MoveWhileAttack.Cannot_Move;
        headChain = new List<Transform>();
        chains = GameObject.FindGameObjectsWithTag("chain");
        dashCondition = false;
    }
    public override void SetCooltime() {
        totalCoolTime = 3;
    }

    public void onWeaponTouch(int colliderType, Collider2D target)
    {
        if (target.GetComponent<FSMbase>() != null)
        {
            collisionFlag = true;
            targetPos = target.transform.position;
            moveDir = targetPos - (Vector2)weaponBase.transform.position;
            moveDir.Normalize();
            moveDir *= v2;
            tempPos = headChainP.transform.position;
            startPos = weaponBase.player.transform.position;
            for (int i = 0; i < chains.Length; i++)
            {
                chains[i].SetActive(false);
            }
        }
    }

    public void SetState(WeaponBase weaponBase)
    {
        if (this.weaponBase == null)
            this.weaponBase = weaponBase;
        if (headTransform == null)
        {
            headTransform = weaponBase.transform.Find("ironhookParent/ironhook/headChain/head");

            headCollider = headTransform.GetComponent<BoxCollider2D>();
        }
        if (headChainP == null)
            headChainP = weaponBase.transform.Find("ironhookParent/ironhook/headChain");
        if (pool == null)
        {
            pool = weaponBase.GetComponentInChildren<Pool>();
        }
        weaponBase.setRotate(weaponBase.WeaponViewDirection+150);
        switch (weaponBase.ViewDirection)
        {
            case 0:
            case 1:
            case 7:
                weaponBase.setFlip(true);
                break;
            case 2:
            case 6:
            case 3:
            case 4:
            case 5:
                weaponBase.setFlip(false);
                break;
        }
        weaponBase.transform.Find("ironhookParent/ironhook").transform.localScale = new Vector3(1,1,1);
        weaponBase.GetAnimatior().enabled = false;
        collisionFlag = false;
        weaponBase.currentMoveCondition = MoveWhileAttack.Cannot_Move;
        weaponBase.setState(PlayerState.skill);
        weaponBase.CanRotateView = false;
        headChain.Add(headTransform);
        headCollider.enabled = true;
    }
    public void Update(WeaponBase weaponBase)
    {
        if (!collisionFlag)
        {
            foreach (var item in headChain)
            {
                item.localPosition += new Vector3(0, v * Time.deltaTime, 0);
            }

            if((headChain.Last().localPosition).sqrMagnitude > 0.16f * 0.16f)
            {
                var p = pool.GetObjectDisabled();
                p.transform.localPosition = Vector2.zero;
                p.gameObject.SetActive(true);
                headChain.Add(p.transform);
            }
            if(headChain.Count> chainCount)
            {
                skillEnd();
            }
        }
        else
        {
            weaponBase.player.AddPosition(moveDir * Time.deltaTime);
            headChainP.transform.position = tempPos;

            int a = (int)((targetPos - (Vector2)weaponBase.player.transform.position).sqrMagnitude/0.16f*0.16f);
           
            if (headChain.Count > a && headChain.Count>1)
            {
                headChain.Last().gameObject.SetActive(false);
                headChain.Last().transform.localPosition = Vector2.zero;
                headChain.Remove(headChain.Last());
            }
            if ((targetPos - (Vector2)weaponBase.player.transform.position).sqrMagnitude < distance * distance)
            {
                skillEnd();
            }
        }
    }
    void skillEnd() {
        weaponBase.CanRotateView = true;
        weaponBase.CanAttackCancel = true;
        collisionFlag = false;
        headCollider.enabled = false;
        for (int i = headChain.Count-1; i > 0; i--)
        {
            headChain[i].transform.localPosition = Vector2.zero;
            headChain[i].gameObject.SetActive(false);
        }
        headChain.Clear();
        weaponBase.GetAnimatior().enabled = true;
        for (int i = 0; i < chains.Length; i++)
        {
            chains[i].SetActive(true);
        }
        weaponBase.SetIdle();
        weaponBase.SetPlayerFree();
    }
    
}
public class SampleDashStrategy : DashFunction, DashStrategy
{
    public void SetState(WeaponBase weaponBase)
    {
        attack_Cancel(weaponBase);
    }
    public void Update(WeaponBase weaponBase)
    {

    }
}
public class SampleAttackStrategy : AttackValues, AttackStrategy
{//!TODO 공격중 이속50% 낮추기->웨폰베이스에 변수 만들어서 ㄱ
    AttackMessage m;
    float[] Damages;

    GameObject[]ironHookEffects;  // 프리펩 넣는 방법을 모름. 일단 넣을 프리펩은 2개로 예상중
    //float h_Thunder= 0.3f;
    int ironHookEffectsinitialCount = 10;
    int ironHookEffectsincrementCount = 5;
    Pool[] ironHookEffectsPools;
    Transform effcetParent;

    Transform chainHead;
    int effectLevel = 0;

    float distance = 1.7f;

    public SampleAttackStrategy(WeaponBase weaponBase) : base(2,0.8f,0.3f,0.7f)
    {
        attackMoveCondition = MoveWhileAttack.Move_Attack;
        dashCondition = true; 
        Damages = new float[] {
            0.3f,
            0.35f }; 
        m = new AttackMessage();
        if (ironHookEffectsPools == null)
        {
            var e = weaponBase.GetComponentInChildren<WeaponEffects>();
            ironHookEffects = e.Effects;
            effcetParent = e.effcetParent;
            ironHookEffectsPools = new Pool[ironHookEffects.Length];
            for (int i = 0; i < ironHookEffectsPools.Length; i++)
            {
                ironHookEffectsPools[i] = AttackManager.GetInstance().effcetParent.gameObject.AddComponent<Pool>();
                ironHookEffectsPools[i].poolPrefab = ironHookEffects[i];
                ironHookEffectsPools[i].initialCount = ironHookEffectsinitialCount;
                ironHookEffectsPools[i].incrementCount = ironHookEffectsincrementCount;
                ironHookEffectsPools[i].Initialize();
            }
        }
            chainHead = weaponBase.transform.Find("ironhookParent/ironhook/headChain/head");
    }
    public override void SetCoolTimes()
    {
        coolTimes[0] = 0.2f;
        coolTimes[1] = 0.3f;
    }


    public void onWeaponTouch(int colliderType, Collider2D target)
    {
        //!TODO fsm만이 아니라 그냥 오브젝트들도 다 
        //!TODO 한 공격에 여러체인 맞는거 수정할 것
        //!TODO 한 공격에 한번만 맞게 할 것
        var fsm = target.GetComponent<FSMbase>();
        if (fsm != null)
        {
            if (colliderType == 0)
            {
                AttackManager.GetInstance().HandleAttack(AttackHandle, fsm,player,Damages[tempAtkCount] * 4);
            }
            else
            {
                AttackManager.GetInstance().HandleAttack(AttackHandle, fsm,player, Damages[tempAtkCount]);
            }
        }
    }
    AttackMessage AttackHandle(FSMbase target, FSMbase sender, float attackPoint)
    {
        m.EffectNum = 0;
        m.Cri_EffectNum = 0;
        m.FinalDamage = sender.status.getCurrentStat(STAT.AtkPoint) * attackPoint;
        if(m.CriCalculate(sender.status.getCurrentStat(STAT.CriticalPoint),
            sender.status.getCurrentStat(STAT.CriticalDamage))){
            target.status.AddBuff(new Bleeding(5,3,target));
        }
        return m;
    }

    public void SetState(WeaponBase weaponBase)
    {
        weaponBase.setRotate(weaponBase.WeaponViewDirection-45);
        switch (weaponBase.ViewDirection)
        {
            case 0:
            case 1:
            case 7:
                weaponBase.setFlip(true);
                break;
            case 2:
            case 6:
            case 3:
            case 4:
            case 5:
                weaponBase.setFlip(false);
                break;
        }

        CountCombo(weaponBase);
        effectLevel = 0;
        weaponBase.weakedSpeed = 0.5f;
    }
    public void Update(WeaponBase weaponBase)
    {
        HandleAttackCancel(weaponBase);
        HandleAttackCommand(weaponBase);
        HandleAttackEND(weaponBase);

    }
    public override void motionEvent(int value)
    {
        if(value == 0)
        {
            E_Slash(chainHead.position);
        }
    }
    void E_Slash(Vector2 point)
    {

        point = point-(Vector2)player.transform.position;
        point.Normalize();
        point *= distance;
        point += (Vector2)player.transform.position;
        var t = ironHookEffectsPools[0].GetObjectDisabled(effcetParent);
        t.transform.position = point;
        if (tempAtkCount == 0)
            t.transform.localScale = new Vector2(1, -1);
        else
        {
            t.transform.localScale = new Vector2(1, 1);

        }
        t.transform.rotation = Quaternion.FromToRotation(Vector2.right,(point- (Vector2)player.transform.position).normalized);
        t.gameObject.SetActive(true);


        //!TODO Effector에 자동 복원 추가 하고 지우기
        t.GetComponent<SpriteRenderer>().color = new Color(1,1,1,1);


        float duration= 1;
        switch (effectLevel)
        {
            case 0:
                duration = 0.3f;
                break;
            case 1:
                duration = 0.45f;
                break;
            case 2:
                duration = 0.6f;
                break;
            default:
                break;
        }
        effectLevel++;
        t.GetComponent<Effector>().Alpha(duration, 0f).Then().Disable().Play();
    }
}

public class SampleCCStrategy : CCStrategy
{
    public void SetState(WeaponBase weaponBase)
    {
        weaponBase.CanRotateView = false;
        weaponBase.setState(PlayerState.CC);
    }
    public void Update(WeaponBase weaponBase)
    {

    }
}