using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    float v = 2.5f;//속도
    float vy = 1f;//y가짜 포물선 속도
    float h;
    float t;//경과시간
    float all_t;//전체 이동 시간
    float l;//이동거리
   

    void PreCalculate() {
       lerpPos =targetPos-startPos;
        l = lerpPos.magnitude;
       lerpPos.Normalize();
        lerpPos *= v;

        h = 0;
        t = 0;
        all_t = l/lerpPos.magnitude;
        
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
        weaponBase.AnimSpeed = 1 / all_t;
    }
    public void Update(WeaponBase weaponBase)
    {
       
        t += Time.deltaTime;

        if (t <= all_t * 0.4f) {
            h = t*vy;
        }
        else if(t < all_t * 0.7f)
        {
            
        }else if(t <= all_t)
        {
            h = (all_t-t)*vy;
        }


        float x = startPos.x+lerpPos.x* t;
        float y = startPos.y+lerpPos.y* t+h;

        Vector2 movePos = new Vector2(x, y);
        if (//Vector2.SqrMagnitude(movePos-(Vector2)weaponBase.player.transform.position)>=
            //Vector2.SqrMagnitude(targetPos - (Vector2)weaponBase.player.transform.position))
            t>=all_t)
        {
            movePos = targetPos;
            weaponBase.CanRotateView = true;
            weaponBase.CanAttackCancel = true;
            weaponBase.SetIdle();
            weaponBase.SetPlayerFree();

            weaponBase.AnimSpeed = 1;
        }

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
    int attackConnectCount;
    public StormPistAttackStrategy(WeaponBase weaponBase) : base(2)
    {
        attackMoveCondition = MoveWhileAttack.Move_Attack;
        ATK_COMBO_COUNT = 6;
        tempAtkCount = 6;
        attackConnectCount = 3;
    }

    public void onWeaponTouch(int colliderType, FSMbase target)
    {
        List<Collider2D> alreadyHitTarget = new List<Collider2D>();  //타격된 저장용
        alreadyHitTarget.Add(target.GetComponent<Collider2D>());

        AttackManager.GetInstance().HandleDamage(50, target);

        for (int i = 0; i < attackConnectCount; i++)
        {
            Collider2D[] targetList = AttackManager.GetInstance().GetTargetList(alreadyHitTarget.Last().transform.position, 10, 1<<10,alreadyHitTarget);
            if (targetList.Length < 1)
                break;
            alreadyHitTarget.Add(targetList[0]);
        }
        for (int i = 1; i < alreadyHitTarget.Count; i++)
        {
            AttackManager.GetInstance().HandleDamage(10,alreadyHitTarget[i].GetComponent<FSMbase>());

        }

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