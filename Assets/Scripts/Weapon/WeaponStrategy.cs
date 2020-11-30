using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

#region Componet
public abstract class AttackComponent : MonoBehaviour
{
    public IdleStrategy idleStrategy;
    public SkillStrategy skillStrategy;
    public MoveStrategy moveStrategy;
    public DashStrategy dashStrategy;
    public DeadStrategy deadStrategy;
    public MouseInputStrategy mouseInputStrategy;
    public AttackStrategy attackStrategy;
    public HittedStrategy hittedstrategy;
    public abstract void SetStrategy(WeaponBase weaponBase);

}
#endregion

#region Strategy Interfaces
public interface IdleStrategy
{
     void SetState(WeaponBase weaponBase);
     void Update(WeaponBase weaponBase);
}
public interface MoveStrategy
{
     void Update(WeaponBase weaponBase);
     void SetState(WeaponBase weaponBase);
}
public abstract class MoveFunction {
    public void cannotMove(WeaponBase weaponBase)//무빙어택이나 어택하면서 못움직일때 호출
    {
        if (weaponBase.CanAttackCancel)
            weaponBase.setState(PlayerState.move);
    }
    public void cannotMove(WeaponBase weaponBase,MoveWhileAttack move)
    {
        if (weaponBase.CanAttackCancel && move == MoveWhileAttack.Move_Attack)
            weaponBase.setState(PlayerState.move);
    }
    public void attack_Cancel(WeaponBase weaponBase)
    {//움직이면 공격 캔슬될 때 호출
        weaponBase.CanAttackCancel = true;
        weaponBase.setState(PlayerState.move);
    }

}
public interface DeadStrategy
{
     void Update(WeaponBase weaponBase);
     void SetState(WeaponBase weaponBase);
}
public interface DashStrategy
{
     void Update(WeaponBase weaponBase);
     void SetState(WeaponBase weaponBase);
}

public interface HittedStrategy
{
    void Update(WeaponBase weaponBase);
    void SetState(WeaponBase weaponBase);
}
public abstract class DashFunction
{
    public void cannotMove(WeaponBase weaponBase)//무빙어택이나 어택하면서 못움직일때 호출
    {
        if (weaponBase.CanAttackCancel)
            weaponBase.setState(PlayerState.dash);
    }
    public void attack_Cancel(WeaponBase weaponBase)
    {//움직이면 공격 캔슬될 때 호출
        weaponBase.CanAttackCancel = true;
        weaponBase.setState(PlayerState.dash);
    }

}
public interface SkillStrategy
{
    void SetState(WeaponBase weaponBase);
    void Update(WeaponBase weaponBase);
    void onWeaponTouch(int colliderType, IHitable target);
    bool canDash();
    MoveWhileAttack getSkillMoveCondition();
    void StartCool();
    void GetCoolTime(out float remain, out float total);
    void StateEnd();
    void motionEvent(int value);
    void motionEvent(string msg);
}

public interface MouseInputStrategy
{
     void HandleInput(WeaponBase weaponBase);
}
#endregion

#region SkillValues
public abstract class SkillValues
{
    protected MoveWhileAttack moveSkillcondition;
    protected bool dashCondition;

    protected float[] skillCoolTimes;
    protected float remainCoolTime;
    protected float totalCoolTime;
    protected float coolStartTime;
    int skillCombo = 0;
    protected int maxSkillCombo;
    bool isCooldown = false;
    protected PlayerFSM player;

    protected List<IHitable> attackedColliders;

    public SkillValues() {
        player = WeaponBase.instance.player;
        skillCoolTimes = new float[1];
        attackedColliders = new List<IHitable>();
        SetCooltime();
    }

    abstract public void SetCooltime();

    public void initSkillCombo(int maxCombo) {
        if (maxCombo > 1)
        {
            skillCoolTimes = new float[maxCombo];
            maxSkillCombo = maxCombo;
        }
    }
    public bool canDash() {
        return dashCondition;
    }
    public void upCombo() {
        skillCombo++;
        if (skillCombo >= maxSkillCombo)
        {
            skillCombo = 0;
        }
    }

    public void StartCool()
    {
        if (isCooldown)
            return;
        if(maxSkillCombo>1)
        totalCoolTime = skillCoolTimes[skillCombo];
        remainCoolTime = totalCoolTime;
        coolStartTime = Time.realtimeSinceStartup;
        isCooldown = true;
    }

    public void GetCoolTime(out float remain, out float total)
    {
        if (!isCooldown)
        {
            remainCoolTime = 0;
        }
        else
        {
            remainCoolTime = (coolStartTime + totalCoolTime) - Time.realtimeSinceStartup;
            if (remainCoolTime <= 0)
            {
                remainCoolTime = 0;
                isCooldown = false;
            }
        }
        remain = remainCoolTime;
        total = totalCoolTime;
    }
    public MoveWhileAttack getSkillMoveCondition()
    {
        return moveSkillcondition;
    }
    public void HandleSkillEND(WeaponBase weaponBase)
    {//Attack애니메이션이 종료 되었다면 idle로 보내는 애들 업데이트에서 호출
        if (weaponBase.getAnimEnd())
        {
            weaponBase.CanRotateView = true;
            weaponBase.CanAttackCancel = true;
            weaponBase.SetIdle();
            weaponBase.SetPlayerFree();
            weaponBase.SetColliderEnable(false);
        }
    }
    public virtual void motionEvent(int value)
    {

    }
    public virtual void motionEvent(string msg)
    {

    }
    public virtual void StateEnd()
    {

    }
}
#endregion


public static class ViewManager
{
    
    public static void viewMouse(WeaponBase weaponBase)
    {
        bool q;
        viewMouse(weaponBase,out q,0,0);
    }
    public static void viewMouse(WeaponBase weaponBase,float r)
    {
        bool q;
        viewMouse(weaponBase, out q,r);
    }
    public static void viewMouse(WeaponBase weaponBase, float r, float t)
    {
        bool q;
        viewMouse(weaponBase, out q, r, t);
    }
    public static void viewMouse(WeaponBase weaponBase, out bool doFlip)
    {

        viewMouse(weaponBase, out doFlip, 0, 0);
    }
    public static void viewMouse(WeaponBase weaponBase, out bool doFlip, float r)
    {
        weaponBase.setFlip(weaponBase.SP_FlipX());
        doFlip = weaponBase.SP_FlipX();
        viewMouseNoMove(weaponBase, r);
    }
    public static void viewMouse(WeaponBase weaponBase,out bool doFlip,float r, float t)
    {
        weaponBase.setFlip(weaponBase.SP_FlipX());
        doFlip = weaponBase.SP_FlipX();
        if (!doFlip)
            viewMouseNoMove(weaponBase, r);
        else
        {
            viewMouseNoMove(weaponBase, t);
        }
    }
    public static void viewMouseNoMove(WeaponBase weaponBase, float r=0)
    {
        weaponBase.CanRotateView = true;

        weaponBase.setViewPoint();
        weaponBase.SP_FlipX();
        if (weaponBase.snapFrame)
        {
            weaponBase.setRotate((weaponBase.WeaponViewDirection + r));
        }else
        weaponBase.setRotateLerp(weaponBase.WeaponViewDirection+r);

    }
}