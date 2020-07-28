﻿using System.Collections;
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
    public void SetState(WeaponBase weaponBase)
    {
        if (weaponBase.CanAttackCancel)
            weaponBase.setState((int)PlayerState.idle);
    }
    public void Update(WeaponBase weaponBase)
    {
        if(weaponBase.ViewDirection <= 2 || weaponBase.ViewDirection > 6)// (0, 1, 7)  Left      2, 6 은 원하는데로 설정
            weaponBase.setFlipScaleY(-1); // -1 : 위아래 뒤집기
        else
            weaponBase.setFlipScaleY(1); // 원래 대로

        weaponBase.setRotate(weaponBase.WeaponViewDirection + 180);
    }
}
public class StormPistMoveStrategy : MoveStrategy
{
    public void SetState(WeaponBase weaponBase)
    {
        if (weaponBase.CanAttackCancel)
            weaponBase.setState((int)PlayerState.move);
    }
    public void Update(WeaponBase weaponBase)
    {
        if (weaponBase.ViewDirection <= 2 || weaponBase.ViewDirection > 6)// (0, 1, 7)  Left   2, 6 은 원하는데로 설정
            weaponBase.setFlipScaleY(-1); // -1 : 위아래 뒤집기
        else
            weaponBase.setFlipScaleY(1); // 원래 대로

        weaponBase.setRotate(weaponBase.WeaponViewDirection + 180);
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
                weaponBase.CanRotateView = true;
                weaponBase.setViewPoint();
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

    public void onWeaponTouch(int colliderType, FSMbase target)
    {
        //TODO:스태틱 효과 및 공격이벤트
    }

    public void SetState(WeaponBase weaponBase)
    {
        if (weaponBase.ViewDirection <= 2 || weaponBase.ViewDirection > 6)// (0, 1, 7)  Left      2, 6 은 원하는데로 설정
            weaponBase.setFlipScaleY(-1); // -1 : 위아래 뒤집기
        else
            weaponBase.setFlipScaleY(1); // 원래 대로

        weaponBase.setRotate(weaponBase.WeaponViewDirection + 180);
        CountCombo(weaponBase, (int)PlayerState.attack, MoveWhileAttack.Move_Attack);

        weaponBase.CanRotateView = false;
    }
    public void Update(WeaponBase weaponBase)
    {
        HandleAttackCancel(weaponBase);
        HandleAttackCommand(weaponBase);
        HandleAttackEND(weaponBase, (int)PlayerState.idle, ()=>{ weaponBase.CanRotateView = true; }) ;
    }

}