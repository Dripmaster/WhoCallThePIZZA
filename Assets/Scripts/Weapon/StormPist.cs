using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StormPist
{
    public static void SetStrategy(out IdleStrategy i, out MoveStrategy m, out DeadStrategy d, out MouseInputStrategy mi, out DashStrategy ds, out AttackStrategy a, WeaponBase weaponBase)
    {
        i = new StormPistIdleStrategy();
        m = new StormPistMoveStrategy();
        d = new StormPistDeadStrategy();
        mi = new StormPistMouseInputStrategy();
        ds = new StormPistDashStrategy();
        a = new StormPistAttackStrategy(weaponBase);
    }
}

public class StormPistIdleStrategy : IdleStrategy
{
    SpriteRenderer SR_pist1;
    SpriteRenderer SR_pist2;
    public void SetState(WeaponBase weaponBase)
    {
        SR_pist1 = weaponBase.transform.Find("stormpist/pists/pist1").GetComponent<SpriteRenderer>();
        SR_pist2 = weaponBase.transform.Find("stormpist/pists/pist2").GetComponent<SpriteRenderer>();
        if (weaponBase.CanAttackCancel)
            weaponBase.setState((int)PlayerState.idle);
    }
    public void Update(WeaponBase weaponBase)
    {
        if (weaponBase.ViewDirection >= 2 && weaponBase.ViewDirection <= 6)  //오른쪽(3, 4, 5)
        {
            SR_pist1.sortingOrder = 1;
            SR_pist2.sortingOrder = -1;
        }
        else // 왼쪽 (0, 1, 2, 6, 7)
        {
            SR_pist1.sortingOrder = -1;
            SR_pist2.sortingOrder = 1;
        }
    }
}
public class StormPistMoveStrategy : MoveStrategy
{
    SpriteRenderer SR_pist1;
    SpriteRenderer SR_pist2;
    public void SetState(WeaponBase weaponBase)
    {
        SR_pist1 = weaponBase.transform.Find("stormpist/pists/pist1").GetComponent<SpriteRenderer>();
        SR_pist2 = weaponBase.transform.Find("stormpist/pists/pist2").GetComponent<SpriteRenderer>();
        if (weaponBase.CanAttackCancel)
            weaponBase.setState((int)PlayerState.move);
    }
    public void Update(WeaponBase weaponBase)
    {
        if (weaponBase.ViewDirection >= 2 && weaponBase.ViewDirection <= 6)  //오른쪽(3, 4, 5)
        {
            SR_pist1.sortingOrder = 1;
            SR_pist2.sortingOrder = -1;
        }
        else // 왼쪽 (0, 1, 2, 6, 7)
        {
            SR_pist1.sortingOrder = -1;
            SR_pist2.sortingOrder = 1;
        }
    }
}
public class StormPistDeadStrategy : DeadStrategy
{

    public void SetState(WeaponBase weaponBase)
    {
        //미구현
    }
    public void Update(WeaponBase weaponBase)
    {

    }
}
public class StormPistMouseInputStrategy : MouseInputStrategy
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
public class StormPistDashStrategy : DashStrategy
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
public class StormPistAttackStrategy : AttackValues, AttackStrategy
{
    public StormPistAttackStrategy(WeaponBase weaponBase) : base(2)
    {
        tempAtkType = weaponBase.attackAnimType;
        ATK_COMBO_COUNT = 6;
        tempAtkCount = 6;
    }
    public void SetState(WeaponBase weaponBase)
    {
        CountCombo(weaponBase, (int)PlayerState.attack, MoveWhileAttack.Move_Attack);
    }
    public void Update(WeaponBase weaponBase)
    {
        HandleAttackCancel(weaponBase);
        HandleAttackCommand(weaponBase);
        HandleAttackEND(weaponBase, (int)PlayerState.idle);
    }
}