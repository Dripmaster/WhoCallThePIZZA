using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleWeapon {
    public static void SetStrategy(out IdleStrategy i, out MoveStrategy m, out DeadStrategy d, out MouseInputStrategy mi, out DashStrategy ds, out AttackStrategy a, out SkillStrategy s, WeaponBase weaponBase) {
        i = new SampleIdleStrategy();
        m = new SampleMoveStrategy();
        d = new SampleDeadStrategy();
        mi = new SampleMouseInputStrategy();
        ds= new SampleDashStrategy();
        a = new SampleAttackStrategy(weaponBase);
        s = new SampleSkillStrategy();
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
            if (weaponBase.CanAttackCancel)
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
            if (weaponBase.CanAttackCancel)
            {
                weaponBase.CanAttackCancel = false;

                if (weaponBase.getMoveAttackCondition() == MoveWhileAttack.Move_Attack)
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
public class SampleSkillStrategy : SkillStrategy
{
    public void SetState(WeaponBase weaponBase)
    {
        weaponBase.setState(PlayerState.skill);
    }
    public void Update(WeaponBase weaponBase)
    {

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
{
    public SampleAttackStrategy(WeaponBase weaponBase) : base(2)
    {
        ATK_COMMAND_PROGRESS_START = 0.3f;
        ATK_COMMAND_PROGRESS_END = 0.7f;
        attackMoveCondition = MoveWhileAttack.Move_Attack;
    }

    public bool canDash()
    {
        return true;
    }

    public MoveWhileAttack getAttackMoveCondition()
    {
        return attackMoveCondition;
    }

    public void onWeaponTouch(int colliderType, FSMbase target)
    {
        if (colliderType == 0)
        {
            AttackManager.GetInstance().HandleDamage(50, target);
        }
        else
        {
            AttackManager.GetInstance().HandleDamage(5, target);
        }
    }

    public void SetState(WeaponBase weaponBase)
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

        CountCombo(weaponBase);
    }
    public void Update(WeaponBase weaponBase)
    {
        HandleAttackCancel(weaponBase);
        HandleAttackCommand(weaponBase);
        HandleAttackEND(weaponBase);
    }


}