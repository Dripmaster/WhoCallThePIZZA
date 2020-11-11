using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Lance : AttackComponent
{
    public float devideRatio = 0.75f;// 전체애니에서 돌진부분 차지 부분 0~1
    public float chargeSpeed = 10f; //돌진속도
    public float maxChargeTime = 2;//최대 충전 시간

    public GameObject lanceChargeEffect1;
    public Color charge1Color;
    public GameObject lanceChargeEffect2;
    public Color charge2Color;
    public GameObject lanceFullChargeEffect;
    public Color fullChargeColor;

    public int[] skillAngles;
    public override void SetStrategy(WeaponBase weaponBase)
    {
        idleStrategy = new LanceIdleStrategy();
        moveStrategy = new LanceMoveStrategy();
        deadStrategy = new LanceDeadStrategy();
        mouseInputStrategy = new LanceMouseInputStrategy();
        dashStrategy = new LanceDashStrategy();
        attackStrategy = new LanceAttackStrategy(weaponBase);
        skillStrategy = new LanceSkillStrategy(weaponBase);
        hittedstrategy = new LanceHittedStrategy();
    }
    public void Awake()
    {
        SetChargeEffectColors();
    }
    void SetChargeEffectColors()
    {
        if (lanceChargeEffect1 == null)
            return;
        lanceChargeEffect1.GetComponent<ParticleColorChanger>().SetColor(charge1Color);
        lanceChargeEffect2.GetComponent<ParticleColorChanger>().SetColor(charge2Color);
        lanceFullChargeEffect.GetComponent<ParticleColorChanger>().SetColor(fullChargeColor);
    }
}

public class LanceIdleStrategy : IdleStrategy
{
    Lance lance;
    public void SetState(WeaponBase weaponBase)
    {
        if (lance == null)
        {
            lance = weaponBase.WeaponComponent() as Lance;
        }
        if (weaponBase.CanAttackCancel)
        {
            weaponBase.setState((int)PlayerState.idle);
            weaponBase.setRotate(0);
        }
    }
    public void Update(WeaponBase weaponBase)
    {
        weaponBase.setFlip( weaponBase.SP_FlipX());

        weaponBase.CanRotateView = true;
        weaponBase.setViewPoint();
        weaponBase.SP_FlipX();
        weaponBase.setRotate(weaponBase.WeaponViewDirection);
    }
}
public class LanceMoveStrategy : MoveFunction, MoveStrategy
{
    Lance lance;
    public void SetState(WeaponBase weaponBase)
    {
        cannotMove(weaponBase);
    }
    public void Update(WeaponBase weaponBase)
    {
        weaponBase.setFlip(weaponBase.SP_FlipX());

        weaponBase.CanRotateView = true;
        weaponBase.setViewPoint();
        weaponBase.SP_FlipX();
        weaponBase.setRotate(weaponBase.WeaponViewDirection);
    }
}
public class LanceDeadStrategy : DeadStrategy
{
    Lance lance;
    public void SetState(WeaponBase weaponBase)
    {
        //미구현
        weaponBase.setState(PlayerState.dead);
    }
    public void Update(WeaponBase weaponBase)
    {

    }
}
public class LanceMouseInputStrategy : MouseInputStrategy
{
    Lance lance;
    public void HandleInput(WeaponBase weaponBase)
    {
        /////기본 공격
        if (!weaponBase.isDash && InputSystem.Instance.getKeyDown(InputKeys.MB_L_click))
        {
            if (!weaponBase.IsAttackCoolTimeRemain() && weaponBase.CanAttackCancel)
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
        if (weaponBase.attackComboCount==0 && InputSystem.Instance.getKeyUp(InputKeys.MB_L_click))
        {
            weaponBase.SetAttack(true);
        }
        else if (InputSystem.Instance.getKeyUp(InputKeys.MB_L_click))
        {
            //weaponBase.attackComboCount = 0;
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
public class LanceSkillStrategy : SkillValues, SkillStrategy
{
    AttackMessage m;
    float tempTime;//경과시간
    bool colliderEnable;//콜라이더 켜졌는지
    //이펙트용 변수
    GameObject[] lanceEffects;
    static Pool[] lanceSkillEffectsPools;
    Transform effcetParent;
    Transform lanceTransform;
    int lanceSkillEffectsinitialCount = 14;
    int lanceSkillEffectsincrementCount = 7;
    float flip;
    WeaponBase weaponBase;
    Lance lance;
    int skillStack;
    public LanceSkillStrategy(WeaponBase weaponBase)
    {
        lance = weaponBase.WeaponComponent() as Lance;
        dashCondition = false;
        moveSkillcondition = MoveWhileAttack.Move_Attack;
        m = new AttackMessage();
        this.weaponBase = weaponBase;
        lanceTransform = weaponBase.transform.Find("LanceParent/Lance/LanceHead");
        if(lanceSkillEffectsPools == null)
        {
            var e = weaponBase.GetComponentInChildren<WeaponEffects>();
            lanceEffects = e.Effects;
            effcetParent = e.effectParent;
            lanceSkillEffectsPools = new Pool[lanceEffects.Length];
            for (int i = 0; i < lanceSkillEffectsPools.Length; i++)
            {
                lanceSkillEffectsPools[i] = EffectManager.GetInstance().effectParent.gameObject.AddComponent<Pool>();
                lanceSkillEffectsPools[i].poolPrefab = lanceEffects[i];
                lanceSkillEffectsPools[i].initialCount = lanceSkillEffectsinitialCount;
                lanceSkillEffectsPools[i].incrementCount = lanceSkillEffectsincrementCount;
                lanceSkillEffectsPools[i].Initialize();
            }
        }
        skillStack = 0;
    }

    public override void SetCooltime()
    {
        totalCoolTime = 4;
    }


    public void SetState(WeaponBase weaponBase)
    {
        weaponBase.currentMoveCondition = moveSkillcondition;
        weaponBase.setState(PlayerState.skill); 
        
        weaponBase.CanRotateView = true;
        weaponBase.setViewPoint();
        if (weaponBase.SP_FlipX())
        {
            weaponBase.setRotate(weaponBase.WeaponViewDirection, true);
            flip = 180;
        }
        else
        {
            weaponBase.setRotate(weaponBase.WeaponViewDirection, true);
            flip = 0;

        }
        weaponBase.CanRotateView = false;

        weaponBase.SetColliderEnable(true);
        tempTime = 0;
        colliderEnable = true;
        weaponBase.weakedSpeed = 0.8f;
    }
    public void Update(WeaponBase weaponBase)
    {
        tempTime += Time.deltaTime;
        if(tempTime>=0.083f)
        {
            tempTime = 0;
            colliderEnable = !colliderEnable;
            attackedColliders.Clear();
            weaponBase.SetColliderEnable(colliderEnable);
        }
        HandleSkillEND(weaponBase);
    }

    AttackMessage stingHandle(FSMbase target, FSMbase sender, float attackPoint)
    {
        m.effectType = EffectType.SMALL;
        m.critEffectType = EffectType.CRIT;
        m.FinalDamage = sender.status.getCurrentStat(STAT.AtkPoint) * attackPoint;
        return m;
    }
    public void onWeaponTouch(int colliderType, Collider2D target)
    {
        if (attackedColliders.Contains(target))
            return;
        var fsm = target.GetComponent<FSMbase>();
        if (fsm != null)
        {//!TODO 한 공격에 한번만 맞게 할 것
            attackedColliders.Add(target);
            AttackManager.GetInstance().HandleAttack(stingHandle, fsm, player, 0.4f);
        }
    }

    public override void motionEvent(string msg)
    {
        if (msg == "SkillEffect")
            E_LanceStings();
        else if (msg == "ResetStack")
            skillStack = 0;
        else if (msg == "LastSting")
            LastSting();
    }
    float SkillAlphaEffectCurve(float t)
    {
        float stayTime = 0.4f;
        float result = (t - stayTime) * (1f / stayTime);
        return result > 0 ? result : 0;
    }
    void E_LanceStings()
    {
        var t = lanceSkillEffectsPools[0].GetObjectDisabled(effcetParent);
        t.transform.rotation = Quaternion.identity;
        t.transform.Rotate(0, 0, flip + weaponBase.WeaponViewDirection + lance.skillAngles[skillStack]);
        t.transform.position = lanceTransform.position + t.transform.right * 0.8f;
        //t.transform.rotation = lanceTransform.rotation;

        t.transform.localScale = lanceTransform.localScale;
        t.gameObject.SetActive(true);

        float duration = 0.3f;  //Wait(duration/2).Then().
        t.GetComponent<Effector>().Alpha(duration/2, 0, SkillAlphaEffectCurve).And().Scale(duration, 0.5f).And().Move(duration, t.transform.right * 0.25f)
                                    .Then().Disable().Play();
        skillStack++;
    }
    void LastSting()
    {
        var t = lanceSkillEffectsPools[0].GetObjectDisabled(effcetParent);
        t.transform.rotation = Quaternion.identity;
        t.transform.Rotate(0, 0, flip + weaponBase.WeaponViewDirection);
        t.transform.position = lanceTransform.position + t.transform.right * 0.8f;
        //t.transform.rotation = lanceTransform.rotation;
        t.transform.localScale = lanceTransform.localScale*1.5f;
        t.gameObject.SetActive(true);

        float duration = 0.5f;  //Wait(duration/2).Then().
        t.GetComponent<Effector>().Alpha(duration / 2, 0, SkillAlphaEffectCurve).And().Scale(duration, 0.8f).And().Move(duration, t.transform.right * 0.4f)
                                    .Then().Disable().Play();
        skillStack++;
    }
    public override void StateEnd()
    {
        weaponBase.SetColliderEnable(false);
    }

}
             
             
public class LanceDashStrategy : DashFunction, DashStrategy
{
    Lance lance;
    public void SetState(WeaponBase weaponBase)
    {
        attack_Cancel(weaponBase);
        weaponBase.attackComboCount = 1;
    }
    public void Update(WeaponBase weaponBase)
    {

    }
}
public class LanceAttackStrategy : AttackValues, AttackStrategy
{
    float tempTime;//경과시간
    int effectLevel;//이펙트 단계
    float[] Damages;
    AttackMessage m;
    bool firstTime = true;

    Vector2 chargeDir; // 돌진 방향
    float animTimeAll; // 전체 애니 재생시간(계산 후)
    float animTimeRush; // 전체에서 돌진부분 재생시간(계산 후)
    float chargeLength;// 돌진 거리
    float pierceTime; // 방어구 파괴 시간

    WeaponBase weapon;
    Lance lance;
    public void preCalculate()
    {
        chargeDir = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - player.transform.position);
        chargeDir.Normalize();
        if(tempTime>=lance.maxChargeTime)
            tempTime = lance.maxChargeTime;
        chargeLength = tempTime*1.5f + 3;
        if (tempTime >= 1)
            pierceTime = (tempTime-1)*2+3;
        tempTime = 0;
        float t = chargeLength / lance.chargeSpeed;
        animTimeRush = t * lance.devideRatio;
        animTimeAll = t * (1- lance.devideRatio) + animTimeRush;
        weapon.AnimSpeed = 1 / animTimeAll;
    }
    public LanceAttackStrategy(WeaponBase weaponBase) : base(3,0.8f)
    {
        weapon = weaponBase;
        lance = weapon.WeaponComponent() as Lance;
        tempAtkCount = 1;
        m.effectType = EffectType.SMALL;
        m.critEffectType = EffectType.CRIT;

        attackMoveCondition = MoveWhileAttack.Move_Attack;
        dashCondition = true;
        Damages = new float[] {
            1,
            2f };
    }
    AttackMessage stingHandle(FSMbase target, FSMbase sender, float attackPoint)
    {
        m.FinalDamage = sender.status.getCurrentStat(STAT.AtkPoint) * attackPoint;

        return m;
    }
    AttackMessage rushHandle(FSMbase target, FSMbase sender, float attackPoint)
    {
        m.FinalDamage = sender.status.getCurrentStat(STAT.AtkPoint) * attackPoint ;

        target.status.AddBuff(new Pierced(pierceTime, target));

        Vector2 dir = (target.transform.position - sender.transform.position).normalized;
        Vector3 v3Original = chargeDir;
        Quaternion firstRotation = Quaternion.FromToRotation(v3Original, dir);
        float dirDot = firstRotation.eulerAngles.z;
        Vector3 v3Dest;
        Quaternion qUpDir;
        float reflectionValue = 0;
        if (dirDot >= 0 && dirDot<180) {
            qUpDir = Quaternion.Euler(0, 0, 90);
            
        }
        else
        {
            qUpDir = Quaternion.Euler(0, 0, -90);
            reflectionValue = 180;
        }

        v3Original = qUpDir * v3Original;
        float secondRotation = Quaternion.FromToRotation(dir, v3Original).eulerAngles.z * 0.5f;
        
        v3Dest = Quaternion.Euler(0, 0, dirDot + secondRotation + reflectionValue) * chargeDir;

        v3Dest.Normalize();
       
        m.CalcKnockBack(v3Dest, 3,3);
        return m;
    }
    public void onWeaponTouch(int colliderType, Collider2D target)
    {
        if (attackedColliders.Contains(target))
            return;
        var fsm = target.GetComponent<FSMbase>();
        if (fsm != null)
        {//!TODO 한 공격에 한번만 맞게 할 것
            if (tempAtkCount == 1)//그냥찌르기
            {
                AttackManager.GetInstance().HandleAttack(stingHandle, fsm, player, Damages[tempAtkCount-1]);
            }else if(tempAtkCount == 2)
            {//돌진찌르기
                AttackManager.GetInstance().HandleAttack(rushHandle, fsm, player, Damages[tempAtkCount - 1],false,true);
                
            }
            attackedColliders.Add(target);
        }
    }

    public override void SetCoolTimes()
    {
        coolTimes[0] = 0;
        coolTimes[1] = 0.1f;
        coolTimes[2] = 0.1f;
    }

    public void SetState(WeaponBase weaponBase)
    {
        if (firstTime)
        {
            ChargeStart(weaponBase);
            firstTime = false;
            return;
        }
        if(weaponBase.attackComboCount!=0)
        tempAtkCount = weaponBase.attackComboCount;
        switch (tempAtkCount)
        {
            case 0:
                ChargeEnd(weaponBase);
                break;
            default:
                ChargeStart(weaponBase);
                break;

        }
    }
    
    void SetFullChargeEffect(bool active)
    {
        if (lance.lanceFullChargeEffect == null)
            return;
        lance.lanceFullChargeEffect.SetActive(active);
    }
    void StartChargeEffect(bool isFullCharge)
    {
        if (lance.lanceChargeEffect1 == null)
            return;
        if (isFullCharge)
            lance.lanceChargeEffect2.SetActive(true);
        else
            lance.lanceChargeEffect1.SetActive(true);
    }
    public void Update(WeaponBase weaponBase)
    {
        if (tempAtkCount == 0)//차징
        {
            tempTime = UpdateCharge(tempTime);
            if (effectLevel == 1 && tempTime >= 2f)
            {
                //불붙여
                StartChargeEffect(true);
                //발광이펙트
                SetFullChargeEffect(true);
                effectLevel++;
            }
            else if (effectLevel == 0 && tempTime >= 1f)
            {
                //발광이펙트
                StartChargeEffect(false);
                effectLevel++;
            }

            weaponBase.setFlip(weaponBase.SP_FlipX());
            weaponBase.CanRotateView = true;
            weaponBase.setViewPoint();
            weaponBase.setRotate(weaponBase.WeaponViewDirection);
        }
        else
        {
            if (tempAtkCount == 2)
            {
                tempTime += Time.deltaTime;
                if (tempTime < animTimeRush)
                {
                    if (tempTime >= animTimeRush * 0.7f)
                    {
                        tempSpeed = Mathf.Lerp(tempSpeed,0 , Time.deltaTime*10);
                    }
                    player.AddPosition(chargeDir * tempSpeed);

                }
                else
                {
                    weaponBase.nowAttack = false;
                }
            }

            
            HandleAttackCancel(weaponBase);
            HandleAttackEND(weaponBase, endAttack);
        }
    }


    float tempSpeed;
    void endAttack(WeaponBase weaponBase)
    {
        weapon.AnimSpeed = 1;
        weaponBase.CanRotateView = true;
        weaponBase.CanAttackCancel = true;
        weaponBase.setRotate(0, true);
        weaponBase.nowAttack = false;
        float r, t;
        GetCoolTime(out r,out t);
        StartCool();
    }
    void ChargeEnd(WeaponBase weaponBase)
    {
        if (tempTime <= 0.4f)
        {//찌르기
            DoAttack(weaponBase, 1);
            tempAtkCount = 1;
        }
        else
        {//돌진찌르기
            preCalculate();
            DoAttack(weaponBase, 2);
            tempAtkCount = 2;
            tempSpeed = lance.chargeSpeed;
            player.IgnoreEnemyPlayerCollison(true);
        }
        weaponBase.CanRotateView = false;
        weaponBase.nowAttack = true;
        attackedColliders.Clear();
        weaponBase.SetColliderEnable(true);
        weaponBase.currentMoveCondition = MoveWhileAttack.Cannot_Move;
        dashCondition = false;
        SetFullChargeEffect(false);
    }
    void ChargeStart(WeaponBase weaponBase)
    {
        StartCharge(weaponBase, out tempTime, lance.maxChargeTime);
        effectLevel = 0;
        dashCondition = true;
        weaponBase.weakedSpeed = 0.8f;
    }
    public override void StateEnd()
    {
        player.IgnoreEnemyPlayerCollison(false);
        weapon.SetColliderEnable(false);
    }
    public override void motionEvent(int value)
    {
        if(value == -1)
        {
            StateEnd();
        }
    }
}
             
public class LanceHittedStrategy : HittedStrategy
{
    Lance lance;
    public void SetState(WeaponBase weaponBase)
    {
        weaponBase.CanRotateView = false;
        weaponBase.setState(PlayerState.hitted);
    }
    public void Update(WeaponBase weaponBase)
    {

    }
}