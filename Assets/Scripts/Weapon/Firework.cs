using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Firework
{
    public static void SetStrategy(out IdleStrategy i, out MoveStrategy m, out DeadStrategy d, out MouseInputStrategy mi, out DashStrategy ds, out AttackStrategy a, out HittedStrategy c, out SkillStrategy s, WeaponBase weaponBase)
    {
        i = new FireworkIdleStrategy();
        m = new FireworkMoveStrategy();
        d = new FireworkDeadStrategy();
        mi = new FireworkMouseInputStrategy();
        ds = new FireworkDashStrategy();
        a = new FireworkAttackStrategy(weaponBase);
        s = new FireworkSkillStrategy(weaponBase);
        c = new FireworkHittedStrategy();
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
            weaponBase.attackComboCount = 1;
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
    public float Speed;

    public string tmpMessage;

    float time = 0;


    Transform bulletTransform;
    AttackMessage m;
    float tempTime = 2; //유지시간


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
        dashCondition = true;
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
        tempTime = 0;
        weaponBase.weakedSpeed = 0.7f;
        weaponBase.CanRotateView = true;
        weaponBase.setViewPoint();
        weaponBase.SP_FlipX();
        tempAtkCount = weaponBase.attackComboCount;
        weaponBase.setRotate(weaponBase.WeaponViewDirection, true);
        bulletDir = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - player.transform.position);
        bulletDir.Normalize();

        weaponBase.CanRotateView = false;
    }
    public void Update(WeaponBase weaponBase)
    {
        if (tempAtkCount == 0)
        {
            tempAtkCount = weaponBase.attackComboCount;
            
            weaponBase.CanRotateView = true;
            weaponBase.setViewPoint();
            weaponBase.SP_FlipX();
            weaponBase.setRotate(weaponBase.WeaponViewDirection, true);
            bulletDir = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - player.transform.position);
            bulletDir.Normalize();

            time += Time.deltaTime;
            if (time >= 2)
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

            weaponBase.CanRotateView = false;
            
        }
        else
        {
            HandleAttackCancel(weaponBase);
            HandleAttackEND(weaponBase);
        }

    }

    bool fireworkBulletTouched(Collider2D collision)
    {
        Debug.Log("충돌 with:" + collision.name);
        return true;
    }
    
    public override void StateEnd()
    {
        weapon.SetColliderEnable(false);
    }
}  //하는중

public class FireworkSkillStrategy : SkillValues, SkillStrategy
{
    AttackMessage m;
    float tempTime;//경과시간
    bool colliderEnable;//콜라이더 켜졌는지
    //이펙트용 변수
    GameObject[] FT_SkillEffects;
    static Pool FT_SkillEffectsPool;
    Transform effcetParent;
    int FT_SkillEffectsinitialCount = 2;
    int FT_SkillEffectsincrementCount = 1;
    WeaponBase weapon;
    public FireworkSkillStrategy(WeaponBase weaponBase)
    {
        dashCondition = false;
        moveSkillcondition = MoveWhileAttack.Cannot_Move;
        m = new AttackMessage();
        weapon = weaponBase;
        if (FT_SkillEffectsPool == null)
        {
            var e = weaponBase.GetComponentInChildren<WeaponEffects>();
            FT_SkillEffects = e.Effects;
            effcetParent = e.effcetParent;

            FT_SkillEffectsPool = AttackManager.GetInstance().effcetParent.gameObject.AddComponent<Pool>();
            FT_SkillEffectsPool.poolPrefab = FT_SkillEffects[0];
            FT_SkillEffectsPool.initialCount = FT_SkillEffectsinitialCount;
            FT_SkillEffectsPool.incrementCount = FT_SkillEffectsincrementCount;
            FT_SkillEffectsPool.Initialize();

        }
    }

    public override void SetCooltime()
    {
        totalCoolTime = 6;
    }


    public void SetState(WeaponBase weaponBase)
    {
        weaponBase.currentMoveCondition = moveSkillcondition;
        weaponBase.setState(PlayerState.skill);

        weaponBase.CanRotateView = true;
        weaponBase.setViewPoint();
        weaponBase.setRotate(weaponBase.WeaponViewDirection, true);
        weaponBase.CanRotateView = false;

        weaponBase.SetColliderEnable(true);
        tempTime = 0;
        colliderEnable = true;
        weaponBase.weakedSpeed = 0.8f;
    }
    public void Update(WeaponBase weaponBase)
    {
        //랜스처럼 일정시간 되면 콜라이더 꺼야함?

        HandleSkillEND(weaponBase);
    }

    AttackMessage explosionHandle(FSMbase target, FSMbase sender, float attackPoint)
    {
        m.EffectNum = 2;
        m.Cri_EffectNum = 2;
        m.FinalDamage = sender.status.getCurrentStat(STAT.AtkPoint) * attackPoint;
        return m;
    }
    public void onWeaponTouch(int colliderType, Collider2D target)
    {
        if (colliderType == 0)
            return;
        var fsm = target.GetComponent<FSMbase>();
        if (fsm != null)
        {//!TODO 한 공격에 한번만 맞게 할 것
            attackedColliders.Add(target);
            AttackManager.GetInstance().HandleAttack(explosionHandle, fsm, player, 8f);
        }
    }

    public override void motionEvent(int value)
    {
        if (value == 0)
            E_FTCellExplosion();
    }
    void E_FTCellExplosion()
    {
        var t = FT_SkillEffectsPool.GetObjectDisabled(effcetParent);
        t.transform.position = weapon.transform.position;   //생성자(맞나?)에서 따오는거랑 차이점 있음?
        t.gameObject.SetActive(true);

        float duration = 1;
        t.GetComponent<Effector>().Scale(duration, 3f).Then().Alpha(duration, 0f).Then().Disable().Play();

    }
    public override void StateEnd()
    {
        weapon.SetColliderEnable(false);
    }

}  //손안봄

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
}  //손안봄