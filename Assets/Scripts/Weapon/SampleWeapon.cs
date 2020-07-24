using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleWeapon {
    public static void SetStrategy(out IdleStrategy i, out MoveStrategy m, out DeadStrategy d, out MouseInputStrategy mi, out DashStrategy ds, out AttackStrategy a, WeaponBase weaponBase) {
        i = new SampleIdleStrategy();
        m = new SampleMoveStrategy();
        d = new SampleDeadStrategy();
        mi = new SampleMouseInputStrategy();
        ds= new SampleDashStrategy();
        a = new SampleAttackStrategy(weaponBase);
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
        weaponBase.setRotate(weaponBase.ViewDirection * -45f);
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
public class SampleMoveStrategy : MoveStrategy
{

    public void SetState(WeaponBase weaponBase)
    {
        if (weaponBase.CanAttackCancel)
            weaponBase.setState((int)PlayerState.move);
    }
    public void Update(WeaponBase weaponBase)
    {
        weaponBase.setRotate(weaponBase.ViewDirection * -45f);
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
        weaponBase.setState((int)PlayerState.dead);
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

        CountCombo(weaponBase, (int)PlayerState.attack, MoveWhileAttack.Move_Attack);
    }
    public void Update(WeaponBase weaponBase)
    {
        HandleAttackCancel(weaponBase);
        HandleAttackCommand(weaponBase);
        HandleAttackEND(weaponBase, (int)PlayerState.idle);
    }
}