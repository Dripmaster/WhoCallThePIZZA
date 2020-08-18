using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Lance
{
    public static void SetStrategy(out IdleStrategy i, out MoveStrategy m, out DeadStrategy d, out MouseInputStrategy mi, out DashStrategy ds, out AttackStrategy a, out CCStrategy c, out SkillStrategy s, WeaponBase weaponBase)
    {
        i = new LanceIdleStrategy();
        m = new LanceMoveStrategy();
        d = new LanceDeadStrategy();
        mi = new LanceMouseInputStrategy();
        ds = new LanceDashStrategy();
        a = new LanceAttackStrategy(weaponBase);
        s = new LanceSkillStrategy();
        c = new LanceCCStrategy();
    }
}

public class LanceIdleStrategy : IdleStrategy
{
    public void SetState(WeaponBase weaponBase)
    {
        if (weaponBase.CanAttackCancel)
            weaponBase.setState((int)PlayerState.idle);
    }
    public void Update(WeaponBase weaponBase)
    {
        weaponBase.setFlip( weaponBase.SP_FlipX());
    }
}
public class LanceMoveStrategy : MoveFunction, MoveStrategy
{
    public void SetState(WeaponBase weaponBase)
    {
        cannotMove(weaponBase);
    }
    public void Update(WeaponBase weaponBase)
    {
        weaponBase.setFlip(weaponBase.SP_FlipX());

    }
}
public class LanceDeadStrategy : DeadStrategy
{

    public void SetState(WeaponBase weaponBase)
    {
        //미구현
    }
    public void Update(WeaponBase weaponBase)
    {

    }
}
public class LanceMouseInputStrategy : MouseInputStrategy
{
    public void HandleInput(WeaponBase weaponBase)
    {
        /////기본 공격
        if (!weaponBase.isDash && InputSystem.Instance.getKeyDown(InputKeys.MB_L_click))
        {
            if (!weaponBase.IsAttackCoolTimeRemain() && weaponBase.CanAttackCancel)
            {
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
public class LanceSkillStrategy : SkillValues, SkillStrategy
{
    AttackMessage m;

    public LanceSkillStrategy()
    {
        dashCondition = false;
        moveSkillcondition = MoveWhileAttack.Cannot_Move;
        m = new AttackMessage();
    }

    public override void SetCooltime()
    {
        totalCoolTime = 4;
    }


    public void SetState(WeaponBase weaponBase)
    {
        weaponBase.currentMoveCondition = MoveWhileAttack.Cannot_Move;
        weaponBase.setState(PlayerState.skill);
        weaponBase.CanRotateView = false;
    }
    public void Update(WeaponBase weaponBase)
    {

    }

    public void onWeaponTouch(int colliderType, Collider2D target)
    {

    }
}
             
             
public class LanceDashStrategy : DashFunction, DashStrategy
{
    public void SetState(WeaponBase weaponBase)
    {
        cannotMove(weaponBase);
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
    public LanceAttackStrategy(WeaponBase weaponBase) : base(3)
    {
        tempAtkCount = 0;
        m.EffectNum = 0;
        m.Cri_EffectNum = 2;

        attackMoveCondition = MoveWhileAttack.Cannot_Move;
        dashCondition = false;
        Damages = new float[] {
            1,
            2f };
    }
    AttackMessage attackHandle(FSMbase target, FSMbase sender, float attackPoint)
    {
        m.FinalDamage = sender.status.getCurrentStat(STAT.AtkPoint) * attackPoint;

        
        return m;
    }
    public void onWeaponTouch(int colliderType, Collider2D target)
    {
        var fsm = target.GetComponent<FSMbase>();
        if (fsm != null)
        {//!TODO 한 공격에 한번만 맞게 할 것
           
        }
    }

    public override void SetCoolTimes()
    {
        coolTimes[0] = 0;
        coolTimes[1] = 0.5f;
        coolTimes[2] = 0.5f;
    }

    public void SetState(WeaponBase weaponBase)
    {
        if (firstTime)
        {
            ChargeStart(weaponBase);
            firstTime = false;
            return;
        }

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
    public void Update(WeaponBase weaponBase)
    {
        if(tempAtkCount == 0)//차징
        {
            tempTime += Time.deltaTime;
            if (effectLevel==1&&tempTime >= 2f)
            {
                //불붙여
                //발광이펙트
                effectLevel++;
            }
            else if (effectLevel == 0&&tempTime >= 1f)
            {
                //발광이펙트
                effectLevel++;
            }
        }
        else
        {
            HandleAttackEND(weaponBase,endAttack);
        }
    }
    void endAttack(WeaponBase weaponBase)
    {
        weaponBase.CanRotateView = true;
        weaponBase.CanAttackCancel = true;
        weaponBase.setRotate(0);
        weaponBase.nowAttack = false;
        float r, t;
        GetCoolTime(out r,out t);
        StartCool();
    }
    void ChargeEnd(WeaponBase weaponBase)
    {
        StartCool();
        if (tempTime <= 0.4f)
        {//찌르기
            DoAttack(weaponBase, 1);
            tempAtkCount = 1;
        }
        else
        {//돌진찌르기
            DoAttack(weaponBase, 2);
            tempAtkCount = 2;
        }
        weaponBase.nowAttack = true;
        weaponBase.CanAttackCancel = false;
    }
    void ChargeStart(WeaponBase weaponBase)
    {
        tempTime = 0;
        weaponBase.CanRotateView = true;
        weaponBase.setViewPoint();
        if (weaponBase.SP_FlipX())
        {
            weaponBase.setRotate(weaponBase.WeaponViewDirection);
        }
        else
        {
            weaponBase.setRotate(weaponBase.WeaponViewDirection + 180);

        }
        DoAttack(weaponBase, 0);
        effectLevel = 0;
        tempAtkCount = 0;
        weaponBase.CanRotateView = false;
    }

}
             
public class LanceCCStrategy : CCStrategy
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