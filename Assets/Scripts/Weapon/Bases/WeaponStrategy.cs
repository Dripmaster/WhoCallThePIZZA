using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

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

public interface CCStrategy
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
public interface AttackStrategy
{
    void Update(WeaponBase weaponBase);
    void SetState(WeaponBase weaponBase);
    void onWeaponTouch(int colliderType, Collider2D target);
    MoveWhileAttack getAttackMoveCondition();

    bool canDash();
    void GetCoolTime(out float remain, out float total);
    void StartCool();
    void motionEvent(int value);
    void StateEnd();
}
public interface SkillStrategy
{
    void SetState(WeaponBase weaponBase);
    void Update(WeaponBase weaponBase);
    void onWeaponTouch(int colliderType, Collider2D target);
    bool canDash();
    MoveWhileAttack getSkillMoveCondition();
    void StartCool();
    void GetCoolTime(out float remain, out float total);
    void StateEnd();
    void motionEvent(int value);
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

    protected List<Collider2D> attackedColliders;

    public SkillValues() {
        SetCooltime();
        player = WeaponBase.instance.player;
        attackedColliders = new List<Collider2D>();
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
    public virtual void StateEnd()
    {

    }
}
#endregion


#region AttackValues
public abstract class AttackValues {
    public int ATK_COMBO_COUNT = 3;//공격 콤보
    public float ATK_CANCEL_PROGRESS = 0.8f; //공격 캔슬 가능 진행도
    public float ATK_COMMAND_PROGRESS_START = 0f; // 이 진행도 부터 공격 범위
    public float ATK_COMMAND_PROGRESS_END = 0.8f; // 이 진행도 까지 공격 범위
    public int tempAtkCount; 
    public bool CancelConditonOnce;
    public bool AttackOnce;
    public bool dashCondition;
    protected MoveWhileAttack attackMoveCondition;
    protected float[] coolTimes;
    protected float remainCoolTime;
    protected float totalCoolTime;
    protected float coolStartTime;
    bool isCooldown = false;
    protected PlayerFSM player;

    protected List<Collider2D> attackedColliders;

    public AttackValues(int ATK_COMBO_count= 3,float ATK_CANCEL_progress = 0.8f, float ATK_COMMAND_progress_START = 0f, float ATK_COMMAND_progress_END = 1f) {
        ATK_COMBO_COUNT = ATK_COMBO_count;
        ATK_CANCEL_PROGRESS = ATK_CANCEL_progress;
        ATK_COMMAND_PROGRESS_START = ATK_COMMAND_progress_START;
        ATK_COMMAND_PROGRESS_END = ATK_COMMAND_progress_END;
        tempAtkCount = ATK_COMBO_count;
        coolTimes = new float[ATK_COMBO_count];
        SetCoolTimes();
        player = WeaponBase.instance.player;
        attackedColliders = new List<Collider2D>();
    }
    abstract public void SetCoolTimes();
    public void CountCombo(WeaponBase weaponBase)
    {//공격 시 마다 어택콤보 늘어날때 setState에서 호출
        weaponBase.currentMoveCondition = attackMoveCondition;
        tempAtkCount++;
        if (tempAtkCount >= ATK_COMBO_COUNT)
        {
            tempAtkCount = 0;
        }
        weaponBase.SetComboCount(tempAtkCount);
        if(weaponBase.objectState != PlayerState.attack)
        weaponBase.setState(PlayerState.attack);
    }
    public void DoAttack(WeaponBase weaponBase, int combo)
    {
        weaponBase.currentMoveCondition = attackMoveCondition;
        weaponBase.SetComboCount(combo);
        if (weaponBase.objectState != PlayerState.attack)
            weaponBase.setState(PlayerState.attack);
    }
    public void HandleAttackCancel(WeaponBase weaponBase) {//ATK_CANCEL_PROGRESS에 도달하면 끊고 이동 및 다음 공격이 가능해지는 애들 업데이트에서 호출
        if (!CancelConditonOnce && weaponBase.getAnimProgress() >= ATK_CANCEL_PROGRESS)
        {//프로그래스 이상 진행 시 끊고 이동 가능, 다음 공격 가능
            weaponBase.currentMoveCondition = MoveWhileAttack.Move_Cancel_Attack;
            weaponBase.CanAttackCancel = true;
            CancelConditonOnce = true;
            StartCool();
        }
        HandleOnceInit(weaponBase);
    }
    public void HandleAttackCommand(WeaponBase weaponBase)
    {//스타트 프로그래스되면 나우 어택 on, end 프로그래스 되면 off
        float progress = weaponBase.getAnimProgress();
        if (!weaponBase.nowAttack)
        {
            if (progress >= ATK_COMMAND_PROGRESS_END)
                return;

            if (progress >= ATK_COMMAND_PROGRESS_START)
            {
                weaponBase.nowAttack = true;
                attackedColliders.Clear();
                weaponBase.SetColliderEnable(true);
            }
        }
        else {
            if (progress >= ATK_COMMAND_PROGRESS_END)
            {
                weaponBase.nowAttack = false;
            }
        }
    }
    public void HandleAttackEND(WeaponBase weaponBase) {//Attack애니메이션이 종료 되었다면 idle로 보내는 애들 업데이트에서 호출
        if (weaponBase.getAnimEnd())
        {
            weaponBase.CanRotateView = true;
            weaponBase.CanAttackCancel = true;
            weaponBase.SetIdle();
            weaponBase.SetPlayerFree();
            weaponBase.SetColliderEnable(false);
        }
    }
    public void HandleAttackEND(WeaponBase weaponBase, Action<WeaponBase> callback)
    {//Attack애니메이션이 종료 되었다면 idle로 보내는 애들 업데이트에서 호출
        if (weaponBase.getAnimEnd())
        {
            HandleAttackEND(weaponBase);
            callback.Invoke(weaponBase);
        }
    }
    public void HandleAttackEND(WeaponBase weaponBase,Action callback)
    {//Attack애니메이션이 종료 되었다면 idle로 보내는 애들 업데이트에서 호출
        if (weaponBase.getAnimEnd())
        {
            HandleAttackEND(weaponBase);
            callback.Invoke();
        }
    }
    public void HandleOnceInit(WeaponBase weaponBase) {
        if (CancelConditonOnce && weaponBase.getAnimProgress() <= 0.1f)
        {
            setOnce();
        }
    }
    public void setOnce() {

        CancelConditonOnce = false;
        AttackOnce = false;
    }

    public void StartCool() {
        if (isCooldown)
            return;
        if (coolTimes == null)
            return;
        try
        {

            totalCoolTime = coolTimes[tempAtkCount];
            remainCoolTime = totalCoolTime;
            coolStartTime = Time.realtimeSinceStartup;
            isCooldown = true;
        }
        catch (Exception)
        {

            
        }
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
    public bool canDash()
    {
        return dashCondition;
    }
    public MoveWhileAttack getAttackMoveCondition()
    {
        return attackMoveCondition;
    }

    public virtual void motionEvent(int value)
    {

    }
    public virtual void StateEnd()
    {

    }
}
#endregion
