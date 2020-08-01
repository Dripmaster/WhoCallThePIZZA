using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StormPist
{
    public static void SetStrategy(out IdleStrategy i, out MoveStrategy m, out DeadStrategy d, out MouseInputStrategy mi, out DashStrategy ds, out AttackStrategy a, out SkillStrategy s, WeaponBase weaponBase)
    {
        i = new StormPistIdleStrategy();
        m = new StormPistMoveStrategy();
        d = new StormPistDeadStrategy();
        mi = new SampleMouseInputStrategy();
        ds = new StormPistDashStrategy();
        a = new StormPistAttackStrategy(weaponBase);
        s = new StormPistSkillStrategy();
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
        weaponBase.SP_FlipY();

        weaponBase.setRotate(weaponBase.WeaponViewDirection + 180);
    }
}
public class StormPistMoveStrategy : MoveFunction,MoveStrategy
{
    public void SetState(WeaponBase weaponBase)
    {
        cannotMove(weaponBase);
    }
    public void Update(WeaponBase weaponBase)
    {
        weaponBase.SP_FlipY();

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

public class StormPistSkillStrategy :  SkillStrategy
{
    Vector2 targetPos;//목표지점
    Vector2 startPos;//시작점
    Vector2 lerpPos;//프레임 당 이동

    float vy = 0.01f;//최고 높이
    float h;
    float t;//경과시간
    int skillFrame = 0;

    void PreCalculate() {
       lerpPos =Vector2.Lerp(startPos, targetPos,1f/60)-startPos;

        h = 0;
        t = 0;
        skillFrame = 0;
    }

    public void SetState(WeaponBase weaponBase)
    {
        if (weaponBase.ViewDirection < 2 || weaponBase.ViewDirection > 6)// (0, 1, 7)  Left      2, 6 은 원하는데로 설정
            weaponBase.setRotate(180);
        else
            weaponBase.setRotate(0);

        weaponBase.setState(PlayerState.skill);
        weaponBase.CanRotateView = false;
        targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        startPos = weaponBase.player.transform.position;
        PreCalculate();
    }
    public void Update(WeaponBase weaponBase)
    {
        if (skillFrame > 60)
        {
            weaponBase.CanRotateView = true;
            weaponBase.CanAttackCancel = true;
            weaponBase.SetIdle();
            weaponBase.SetPlayerFree();
        }
        if (skillFrame < 30) {
            h += vy;

        }
        else
        {
                h -= vy;
        }
        skillFrame++;
        t += Time.deltaTime;

        float x = startPos.x+lerpPos.x* skillFrame;
        float y = startPos.y+lerpPos.y* skillFrame + h;

        Vector2 movePos = new Vector2(x, y);
        weaponBase.player.SetPosition(movePos);
    }
}


public class StormPistDashStrategy : DashFunction,DashStrategy
{
    public void SetState(WeaponBase weaponBase)
    {
        cannotMove(weaponBase);
    }
    public void Update(WeaponBase weaponBase)
    {

    }
}
public class StormPistAttackStrategy : AttackValues, AttackStrategy
{
    public StormPistAttackStrategy(WeaponBase weaponBase) : base(2)
    {
        attackMoveCondition = MoveWhileAttack.Move_Attack;
        ATK_COMBO_COUNT = 6;
        tempAtkCount = 6;
    }

    public void onWeaponTouch(int colliderType, FSMbase target)
    {
        //TODO:스태틱 효과 및 공격이벤트
    }

    public MoveWhileAttack getAttackMoveCondition()
    {
        return attackMoveCondition;
    }
    public void SetState(WeaponBase weaponBase)
    {

        weaponBase.CanRotateView = true;
        weaponBase.setViewPoint();
        weaponBase.SP_FlipY();

        weaponBase.setRotate(weaponBase.WeaponViewDirection + 180);
        CountCombo(weaponBase);

        weaponBase.CanRotateView = false;
    }
    public void Update(WeaponBase weaponBase)
    {
        HandleAttackCancel(weaponBase);
        HandleAttackCommand(weaponBase);
        HandleAttackEND(weaponBase, ()=>{ weaponBase.CanRotateView = true; }) ;
    }

    public bool canDash()
    {
        return true;
    }
}