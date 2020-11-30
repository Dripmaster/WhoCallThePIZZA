using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
public interface AttackStrategy
{
    void Update(WeaponBase weaponBase);
    void SetState(WeaponBase weaponBase);
    void onWeaponTouch(int colliderType, IHitable target);
    MoveWhileAttack getAttackMoveCondition();

    bool canDash();
    void GetCoolTime(out float remain, out float total);
    void StartCool();
    void motionEvent(int value);
    void motionEvent(string msg);

    void StateEnd();
}
#region AttackValues
public abstract class AttackValues
{
    public int ATK_COMBO_COUNT = 3;//공격 콤보
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
    WeaponBase weaponBase;
    string msg_moveTo = "MoveConditionTo_Move_Attack";
    string msg_moveCancel = "MoveConditionTo_Move_Cancel_Attack";
    string msg_AttackOn = "AttackOn";
    string msg_AttackOff = "AttackOff";
    string msg_AttackEnd = "AttackEnd";

    protected List<IHitable> attackedColliders;

    public AttackValues(int ATK_COMBO_count = 1)
    {
        ATK_COMBO_COUNT = ATK_COMBO_count;
        tempAtkCount = ATK_COMBO_count;
        coolTimes = new float[ATK_COMBO_count];
        SetCoolTimes();
        weaponBase = WeaponBase.instance;
        player = WeaponBase.instance.player;
        attackedColliders = new List<IHitable>();
    }
    abstract public void SetCoolTimes();

    public void StartCharge(WeaponBase weaponBase, out float tempTime, float maxChargeTime)
    {
        weaponBase.currentMoveCondition = attackMoveCondition;
        weaponBase.SetComboCount(0);
        if (weaponBase.objectState != PlayerState.attack)
            weaponBase.setState(PlayerState.attack);
        tempAtkCount = 0;
        tempTime = 0;
        if (player.status.IsBuff(BUFF.BatteryCharged))
        {
            tempTime = maxChargeTime;
            player.status.EndBuff(BUFF.BatteryCharged);
        }
    }
    public float UpdateCharge(float tempTime)
    {
        return tempTime + Time.deltaTime;
    }

    public void CountCombo(WeaponBase weaponBase)
    {//공격 시 마다 어택콤보 늘어날때 setState에서 호출
        weaponBase.currentMoveCondition = attackMoveCondition;
        tempAtkCount++;
        if (tempAtkCount >= ATK_COMBO_COUNT)
        {
            tempAtkCount = 0;
        }
        weaponBase.SetComboCount(tempAtkCount);
        if (weaponBase.objectState != PlayerState.attack)
            weaponBase.setState(PlayerState.attack);
    }
    public void DoAttack(WeaponBase weaponBase, int combo)
    {
        weaponBase.currentMoveCondition = attackMoveCondition;
        weaponBase.SetComboCount(combo);
        if (weaponBase.objectState != PlayerState.attack)
            weaponBase.setState(PlayerState.attack);
    }
    public void HandleAttackCancel(WeaponBase weaponBase)
    {//ATK_CANCEL_PROGRESS에 도달하면 끊고 이동 및 다음 공격이 가능해지는 애들 업데이트에서 호출
        if (!CancelConditonOnce)
        {//프로그래스 이상 진행 시 끊고 이동 가능, 다음 공격 가능
            weaponBase.currentMoveCondition = MoveWhileAttack.Move_Cancel_Attack;
            weaponBase.CanAttackCancel = true;
            CancelConditonOnce = true;
            StartCool();
        }
        HandleOnceInit(weaponBase);
    }
    public void HandleAttackCommand(WeaponBase weaponBase, int v)
    {//스타트 프로그래스되면 나우 어택 on, end 프로그래스 되면 off
        if (v == 0)
        {
            weaponBase.nowAttack = true;
            attackedColliders.Clear();
            weaponBase.SetColliderEnable(true);
        }
        else if (v == 1)
        {
            weaponBase.nowAttack = false;
            weaponBase.SetColliderEnable(false);
        }
    }
    public void HandleAttackEND(WeaponBase weaponBase)
    {//Attack애니메이션이 종료 되었다면 idle로 보내는 애들 업데이트에서 호출
        if (weaponBase.getAnimEnd())
        {
            weaponBase.CanRotateView = true;
            weaponBase.CanAttackCancel = true;
            weaponBase.SetIdle();
            weaponBase.SetPlayerFree();
            weaponBase.SetColliderEnable(false);
            StartCool();
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
    public void HandleAttackEND(WeaponBase weaponBase, Action callback)
    {//Attack애니메이션이 종료 되었다면 idle로 보내는 애들 업데이트에서 호출
        if (weaponBase.getAnimEnd())
        {
            HandleAttackEND(weaponBase);
            callback.Invoke();
        }
    }
    public void HandleOnceInit(WeaponBase weaponBase)
    {
        if (CancelConditonOnce && weaponBase.getAnimProgress() <= 0.1f)
        {
            setOnce();
        }
    }
    public void setOnce()
    {

        CancelConditonOnce = false;
        AttackOnce = false;
    }

    public void StartCool()
    {
        if (isCooldown)
            return;
        if (coolTimes == null)
            return;
        try
        {

            totalCoolTime = coolTimes[tempAtkCount];
            if (totalCoolTime <= 0)
                return;
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
    public virtual void motionEvent(string msg)
    {
        if (msg == msg_moveTo)
        {
            weaponBase.currentMoveCondition = MoveWhileAttack.Move_Attack;
        }
        if (msg == msg_moveCancel)
            HandleAttackCancel(weaponBase);
        if (msg == msg_AttackOn)
            HandleAttackCommand(weaponBase, 0);
        if (msg == msg_AttackOff)
            HandleAttackCommand(weaponBase, 1);
        if (msg == msg_AttackEnd)
            HandleAttackEND(weaponBase);
    }
    public virtual void StateEnd()
    {

    }
}
#endregion