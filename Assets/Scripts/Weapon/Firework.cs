﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Firework : AttackComponent
{
    public override void SetStrategy(WeaponBase weaponBase)
    {
        idleStrategy = new FireworkIdleStrategy();
        moveStrategy = new FireworkMoveStrategy();
        deadStrategy = new FireworkDeadStrategy();
        mouseInputStrategy = new FireworkMouseInputStrategy();
        dashStrategy = new FireworkDashStrategy();
        attackStrategy = new FireworkAttackStrategy(weaponBase);
        skillStrategy = new FireworkSkillStrategy(weaponBase);
        hittedstrategy = new FireworkHittedStrategy();
    }
}


public class FireworkIdleStrategy : IdleStrategy
{
    public void SetState(WeaponBase weaponBase)
    {
        if (weaponBase.CanAttackCancel)
        {
            weaponBase.setState((int)PlayerState.idle);
        }
    }
    public void Update(WeaponBase weaponBase)
    {
        weaponBase.setFlip(weaponBase.SP_FlipX()); //여기서했는데
        weaponBase.SP_FlipX();  //여기서도 해야되나?

        weaponBase.CanRotateView = true;
        weaponBase.setViewPoint();
        weaponBase.setRotate(weaponBase.WeaponViewDirection * 0.5f);
    }
}

public class FireworkMoveStrategy : MoveFunction, MoveStrategy
{
    public void SetState(WeaponBase weaponBase)
    {
        cannotMove(weaponBase);
    }
    public void Update(WeaponBase weaponBase)
    {
        weaponBase.setFlip(weaponBase.SP_FlipX());
        weaponBase.SP_FlipX();

        weaponBase.CanRotateView = true;
        weaponBase.setViewPoint();
        weaponBase.setRotate(weaponBase.WeaponViewDirection * 0.5f);
    }
}

public class FireworkDeadStrategy : DeadStrategy
{

    public void SetState(WeaponBase weaponBase)
    {
        //미구현
        weaponBase.setState(PlayerState.dead);
    }
    public void Update(WeaponBase weaponBase)
    {

    }
}

public class FireworkMouseInputStrategy : MouseInputStrategy
{
    public void HandleInput(WeaponBase weaponBase)
    {
        /////기본 공격
        if (!weaponBase.isDash && InputSystem.Instance.getKeyDown(InputKeys.MB_L_click))
        {
            weaponBase.attackComboCount = 0;
            if (!weaponBase.IsAttackCoolTimeRemain() && weaponBase.CanAttackCancel)
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

        if (InputSystem.Instance.getKeyUp(InputKeys.MB_L_click))
        {
            weaponBase.attackComboCount = 2;
        }

        ////스킬
        if (!weaponBase.isDash && InputSystem.Instance.getKeyDown(InputKeys.SkillBtn))
        {
            if (!weaponBase.IsSkillCoolTimeRemain() && weaponBase.CanAttackCancel)
            {
                weaponBase.CanAttackCancel = false;
                if (weaponBase.getMoveSkillCondition() == MoveWhileAttack.Move_Attack)
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

public class FireworkDashStrategy : DashFunction, DashStrategy
{
    public void SetState(WeaponBase weaponBase)
    {
        attack_Cancel(weaponBase);
        weaponBase.attackComboCount = 1;
    }
    public void Update(WeaponBase weaponBase)
    {

    }
}  //손안봄

public class FireworkAttackStrategy : AttackValues, AttackStrategy
{

    public BulletBase bulletPrefab;
    Pool bulletPool;
    int FW_bulletinitialCount = 10;
    int FW_bulletincrementCount = 1;
    public Vector3 bulletDir;
    public float Speed;  //몇으로 해야하지

    public string tmpMessage;

    float time = 0;


    Transform bulletTransform;
    AttackMessage m;
    float tempTime = 0;


    WeaponBase weapon;

    public FireworkAttackStrategy(WeaponBase weaponBase) : base(3, 0.8f)
    {
        if (bulletPool == null)
        {
            bulletPool = AttackManager.GetInstance().bulletParent.gameObject.AddComponent<Pool>();
            bulletPool.incrementCount = FW_bulletincrementCount;
            bulletPool.initialCount = FW_bulletinitialCount;
            bulletPool.poolPrefab = bulletPrefab.gameObject;
            bulletPool.Initialize();
        }

        dashCondition = true;
        m = new AttackMessage();
        weapon = weaponBase;
        

        tempAtkCount = 0;

        attackMoveCondition = MoveWhileAttack.Move_Attack;
    }

    AttackMessage bulletHandle(FSMbase target, FSMbase sender, float attackPoint)
    {
        m.FinalDamage = sender.status.getCurrentStat(STAT.AtkPoint) * attackPoint;

        return m;
    }
    public void onWeaponTouch(int colliderType, Collider2D target)
    {
      

    }
    

    public override void SetCoolTimes()
    {
        totalCoolTime = 0.33f;
    }

    public void SetState(WeaponBase weaponBase)
    {
        if (weaponBase.objectState != PlayerState.attack)
            weaponBase.setState(PlayerState.attack);
        weaponBase.currentMoveCondition = attackMoveCondition;
        weaponBase.SetColliderEnable(true);
        time = 0;
        weaponBase.CanRotateView = true;
        weaponBase.setViewPoint();
        weaponBase.SP_FlipX();
        tempAtkCount = weaponBase.attackComboCount;
        weaponBase.setRotate(weaponBase.WeaponViewDirection, true);
        bulletDir = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - player.transform.position);
        bulletDir.Normalize();
        tempTime = 0;
        weaponBase.CanRotateView = false;
    }
    public void Update(WeaponBase weaponBase)
    {
        if(tempAtkCount == 0)
        {
            tempTime = 0;
        }
        else
        {
            tempTime = 0.33f;
        }


        if (tempAtkCount == 1 || tempAtkCount == 0)
        {
            
            weaponBase.CanRotateView = true;
            weaponBase.setViewPoint();
            weaponBase.SP_FlipX();
            weaponBase.setRotate(weaponBase.WeaponViewDirection, true);
            bulletDir = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - player.transform.position);
            bulletDir.Normalize();//!TODO 마우스 방향이 아니라  +-45도 랜덤 방향 설정
            //!TODO 랜덤 사거리 추가(랜덤지속시간으로 사거리 조정)
            //!TODO 곡선 궤적
            time += Time.deltaTime;
            if (time >= tempTime)
            {
                time = 0;
                var b = bulletPool.GetObjectDisabled().GetComponent<BulletBase>();
                //위에 실제 다른곳에서 호출 시 parent 설정 해줘야함
                b.transform.position = AttackManager.GetInstance().bulletParent.transform.position;
                b.dir = bulletDir;
                b.speed = Speed;
                b.touched += fireworkBulletTouched;
                b.gameObject.SetActive(true);  //SetActive(false) ㅇㄷ?? 거기다가 파티클도 넣어야함
                CountCombo(weaponBase);
                tempAtkCount = weaponBase.attackComboCount;
            } 

            weaponBase.CanRotateView = false;
            
        }
        else
        {
            HandleAttackCancel(weaponBase);
            HandleAttackEND(weaponBase);
        }

    }

    bool fireworkBulletTouched(Collider2D collision)
    {//!TODO 넉백 추가
        var fsm = collision.GetComponent<FSMbase>();
        if (fsm != null)
        {

            AttackManager.GetInstance().HandleAttack(bulletHandle, fsm, player, 1.3f);

        }
        return true;
    }
    
    public override void StateEnd()
    {
        weapon.SetColliderEnable(false);
    }
}  

public class FireworkSkillStrategy : SkillValues, SkillStrategy
{
    public BulletBase bulletPrefab;
    Pool bulletPool;
    int FW_bulletinitialCount = 10;
    int FW_bulletincrementCount = 1;
    public Vector3 bulletDir;
    public float Speed;

    public string tmpMessage;

    float time = 0;


    Transform bulletTransform;
    AttackMessage m;
    float SkilltempTime;


    WeaponBase weapon;
    public FireworkSkillStrategy(WeaponBase weaponBase)
    {
        if (bulletPool == null)
        {
            bulletPool = AttackManager.GetInstance().bulletParent.gameObject.AddComponent<Pool>();
            bulletPool.incrementCount = FW_bulletincrementCount;
            bulletPool.initialCount = FW_bulletinitialCount;
            bulletPool.poolPrefab = bulletPrefab.gameObject;
            bulletPool.Initialize();
        }

        dashCondition = true;
        m = new AttackMessage();
        weapon = weaponBase;

        moveSkillcondition = MoveWhileAttack.Move_Attack;
    }

    public override void SetCooltime()
    {
        totalCoolTime = 4;
    }


    public void SetState(WeaponBase weaponBase)
    {
        weaponBase.currentMoveCondition = moveSkillcondition;
        weaponBase.setState(PlayerState.skill);

        SkilltempTime = 0.18f;
    }
    public void Update(WeaponBase weaponBase)
    {
        weaponBase.CanRotateView = true;
        weaponBase.setViewPoint();
        weaponBase.SP_FlipX();
        weaponBase.setRotate(weaponBase.WeaponViewDirection, true);

        time += Time.deltaTime;
        if (time >= SkilltempTime)
        {
            time = 0;
            var b = bulletPool.GetObjectDisabled().GetComponent<BulletBase>();
            //위에 실제 다른곳에서 호출 시 parent 설정 해줘야함
            b.transform.position = AttackManager.GetInstance().bulletParent.transform.position;
            b.dir = bulletDir;
            b.speed = Speed;
            b.touched += fireworkBulletTouched;
            b.gameObject.SetActive(true);
        }

        HandleSkillEND(weaponBase);
    }

    AttackMessage bulletHandle(FSMbase target, FSMbase sender, float attackPoint)
    {
        m.FinalDamage = sender.status.getCurrentStat(STAT.AtkPoint) * attackPoint;

        return m;
    }

    public void onWeaponTouch(int colliderType, Collider2D target)
    {
        
    }

    bool fireworkBulletTouched(Collider2D collision)
    {
        var fsm = collision.GetComponent<FSMbase>();
        if (fsm != null)
        {

            AttackManager.GetInstance().HandleAttack(bulletHandle, fsm, player, 1.3f);

        }
        return true;
    }
    public override void StateEnd()
    {
        weapon.SetColliderEnable(false);
    }

} 

public class FireworkHittedStrategy : HittedStrategy
{
    public void SetState(WeaponBase weaponBase)
    {
        weaponBase.CanRotateView = false;
        weaponBase.setState(PlayerState.hitted);
    }
    public void Update(WeaponBase weaponBase)
    {

    }
} 