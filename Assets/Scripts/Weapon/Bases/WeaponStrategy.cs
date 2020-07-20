﻿using System.Collections;
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
public interface AttackStrategy
{
    void Update(WeaponBase weaponBase);
    void SetState(WeaponBase weaponBase);
}
public interface MouseInputStrategy
{
     void HandleInput(WeaponBase weaponBase);
}
#endregion
public abstract class AttackValues {
    public int ATK_COMBO_COUNT = 3;
    public float ATK_CANCEL_PROGRESS = 0.8f;
    public float ATK_COMMAND_PROGRESS_START = 0f;
    public float ATK_COMMAND_PROGRESS_END = 0.8f;
    public int tempAtkType = 0;
    public int tempAtkCount;
    public bool CancelConditonOnce;
    public bool AttackOnce;
    public AttackValues(int ATK_COMBO_count= 3,float ATK_CANCEL_progress = 0.8f, float ATK_COMMAND_progress_START = 0f, float ATK_COMMAND_progress_END = 1f) {
        ATK_COMBO_COUNT = ATK_COMBO_count;
        ATK_CANCEL_PROGRESS = ATK_CANCEL_progress;
        ATK_COMMAND_PROGRESS_START = ATK_COMMAND_progress_START;
        ATK_COMMAND_PROGRESS_END = ATK_COMMAND_progress_END;
        tempAtkCount = ATK_COMBO_count;
    }
    public void CountCombo(WeaponBase weaponBase, int attackState, MoveWhileAttack moveWhileAttack)
    {//공격 시 마다 어택콤보 늘어날때 setState에서 호출
        weaponBase.currentMoveCondition = moveWhileAttack;
        tempAtkCount++;
        if (tempAtkCount >= ATK_COMBO_COUNT)
        {
            tempAtkCount = 0;
        }
        weaponBase.attackAnimType = tempAtkType + tempAtkCount;
        weaponBase.attackComboCount = tempAtkCount;

        weaponBase.setState(attackState);
    }
    public void HandleAttackCancel(WeaponBase weaponBase) {//ATK_CANCEL_PROGRESS에 도달하면 끊고 이동 및 다음 공격이 가능해지는 애들 업데이트에서 호출
        if (!CancelConditonOnce && weaponBase.getAnimProgress() >= ATK_CANCEL_PROGRESS)
        {//프로그래스 이상 진행 시 끊고 이동 가능, 다음 공격 가능
            weaponBase.currentMoveCondition = MoveWhileAttack.Move_Cancel_Attack;
            weaponBase.CanAttackCancel = true;
            CancelConditonOnce = true;
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
            }
        }
        else {
            if (progress >= ATK_COMMAND_PROGRESS_END)
            {
                weaponBase.nowAttack = false;
            }
        }
    }
    public void HandleAttackEND(WeaponBase weaponBase, int idleState) {//Attack애니메이션이 종료 되었다면 idle로 보내는 애들 업데이트에서 호출
        if (weaponBase.getAnimEnd())
        {
            weaponBase.setState(idleState);
            weaponBase.SetIdle(true);
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
}