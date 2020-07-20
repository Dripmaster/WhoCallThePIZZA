using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SampleIdleStrategy : IdleStrategy
{

    public void SetState(WeaponBase weaponBase)
    {
        if (weaponBase.CanAttackCancel)
            weaponBase.setState((int)SampleWeaponState.idle);
    }
    public void Update(WeaponBase weaponBase)
    {
        weaponBase.setRotate(weaponBase.ViewDirection * -45f);
    }
}
public class SampleMoveStrategy : MoveStrategy
{

    public void SetState(WeaponBase weaponBase)
    {
        if (weaponBase.CanAttackCancel)
            weaponBase.setState((int)SampleWeaponState.move);
    }
    public void Update(WeaponBase weaponBase)
    {
        weaponBase.setRotate(weaponBase.ViewDirection * -45f);
    }
}
public class SampleDeadStrategy : DeadStrategy
{

    public void SetState(WeaponBase weaponBase)
    {
        weaponBase.setState((int)SampleWeaponState.dead);
    }
    public void Update(WeaponBase weaponBase)
    {

    }
}
public class SampleMouseInputStrategy : MouseInputStrategy
{
    public void HandleInput(WeaponBase weaponBase)
    {
        if (Input.GetMouseButton(0))
        {
            if (weaponBase.CanAttackCancel)
            {
                weaponBase.CanAttackCancel = false;
                weaponBase.SetAttack(true);
            }
        }
    }
}
public class SampleDashStrategy : DashStrategy
{
    public void SetState(WeaponBase weaponBase)
    {
        //weaponBase.setState((int)SampleWeaponState.move);
    }
    public void Update(WeaponBase weaponBase)
    {
        weaponBase.setRotate(weaponBase.ViewDirection * -45f);
    }
}
public class SampleAttackStrategy : AttackValues, AttackStrategy
{
    public SampleAttackStrategy(WeaponBase weaponBase) : base(2)
    {
        tempAtkType = weaponBase.attackAnimType;
        ATK_COMMAND_PROGRESS_START = 0.3f;
        ATK_COMMAND_PROGRESS_END = 0.7f;
    }
    public void SetState(WeaponBase weaponBase)
    {
        weaponBase.setRotate(weaponBase.ViewDirection * -45f);
        CountCombo(weaponBase, (int)SampleWeaponState.attack, MoveWhileAttack.Move_Attack);
    }
    public void Update(WeaponBase weaponBase)
    {
        HandleAttackCancel(weaponBase);
        HandleAttackCommand(weaponBase);
        HandleAttackEND(weaponBase, (int)SampleWeaponState.idle);
    }
}