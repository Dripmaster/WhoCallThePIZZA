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
        List<Collider2D> alreadyHitTarget = new List<Collider2D>();  //타격된 대상 저장용
        alreadyHitTarget.Add(target.GetComponent<Collider2D>());

        AttackManager.GetInstance().HandleDamage(50, target);
        Collider2D[] targetList1 = AttackManager.GetInstance().GetTargetList(target.GetComponent<Transform>().position, 10, 1<<10);
        //리스트나 어래이에서 타격된대상 제외 하고 Length 확인

        if (targetList1.Length < 1)
        {
            AttackManager.GetInstance().HandleDamage(5, target); // 탐지 됬는지 확인용
            return;
        }

        FSMbase ES_target1 = targetList1[1].GetComponent<FSMbase>();
        AttackManager.GetInstance().HandleDamage(50, ES_target1);
        Collider2D[] targetList2 = AttackManager.GetInstance().GetTargetList(ES_target1.GetComponent<Transform>().position, 10, 1<<10);

        if (targetList2.Length < 1)
            return;
        
        FSMbase ES_target2 = targetList2[0].GetComponent<FSMbase>();
        AttackManager.GetInstance().HandleDamage(50, ES_target2);
        Collider2D[] targetList3 = AttackManager.GetInstance().GetTargetList(ES_target2.GetComponent<Transform>().position, 10, 1<<10);

        if (targetList3.Length < 1)
            return;

        FSMbase ES_target3 = targetList3[1].GetComponent<FSMbase>();
        AttackManager.GetInstance().HandleDamage(50, ES_target3);

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