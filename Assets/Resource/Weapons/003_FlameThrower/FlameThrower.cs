using UnityEngine;


public class FlameThrower : AttackComponent
{
    public override void SetStrategy(WeaponBase weaponBase)
    {
        idleStrategy = new FlameThrowerIdleStrategy();
        moveStrategy = new FlameThrowerMoveStrategy();
        deadStrategy = new FlameThrowerDeadStrategy();
        mouseInputStrategy = new FlameThrowerMouseInputStrategy();
        dashStrategy = new FlameThrowerDashStrategy();
        attackStrategy = new FlameThrowerAttackStrategy(weaponBase);
        skillStrategy = new FlameThrowerSkillStrategy(weaponBase);
        hittedstrategy = new FlameThrowerHittedStrategy();

    }
}

public class FlameThrowerIdleStrategy : IdleStrategy
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
        ViewManager.viewMouse(weaponBase);
    }
}
public class FlameThrowerMoveStrategy : MoveFunction, MoveStrategy
{
    public void SetState(WeaponBase weaponBase)
    {
        cannotMove(weaponBase);
    }
    public void Update(WeaponBase weaponBase)
    {
        ViewManager.viewMouse(weaponBase);
    }
}
public class FlameThrowerDeadStrategy : DeadStrategy
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
public class FlameThrowerMouseInputStrategy : MouseInputStrategy
{
    public void HandleInput(WeaponBase weaponBase)
    {
        /////기본 공격
        if (!weaponBase.isDash && InputSystem.Instance.getKey(InputKeys.MB_L_click))
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

        ////스킬
        if (!weaponBase.isDash && InputSystem.Instance.getKeyDown(InputKeys.SkillBtn))
        {
            if (!weaponBase.IsSkillCoolTimeRemain() && weaponBase.CanAttackCancel)
            {
                weaponBase.CanAttackCancel = false;
                weaponBase.SetSkill(true); // 이거 맞나
            }
        }

    }
}
public class FlameThrowerSkillStrategy : SkillValues, SkillStrategy
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
    public FlameThrowerSkillStrategy(WeaponBase weaponBase)
    {
        dashCondition = false;
        moveSkillcondition = MoveWhileAttack.Cannot_Move;
        m = new AttackMessage();
        weapon = weaponBase;
        if (FT_SkillEffectsPool == null)
        {
            var e = weaponBase.GetComponentInChildren<WeaponEffects>();
            FT_SkillEffects = e.Effects;
            effcetParent = e.effectParent;

            FT_SkillEffectsPool = EffectManager.GetInstance().effectParent.gameObject.AddComponent<Pool>();
            FT_SkillEffectsPool.poolPrefab = FT_SkillEffects[0];
            FT_SkillEffectsPool.initialCount = FT_SkillEffectsinitialCount;
            FT_SkillEffectsPool.incrementCount = FT_SkillEffectsincrementCount;
            FT_SkillEffectsPool.Initialize();
            
        }
    }

    public override void SetCooltime()
    {
        skillCoolTimes[0] = 6;
    }


    public void SetState(WeaponBase weaponBase)
    {
        weaponBase.currentMoveCondition = moveSkillcondition;
        weaponBase.setState(PlayerState.skill);

        ViewManager.viewMouse(weaponBase);
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

    AttackMessage explosionHandle(IHitable target, FSMbase sender, float attackPoint)
    {
        m.effectType = EffectType.BIG;
        m.critEffectType = EffectType.CRIT;
        m.FinalDamage = sender.status.getCurrentStat(STAT.AtkPoint) * attackPoint;
        return m;
    }
    public void onWeaponTouch(int colliderType, IHitable target)
    {
        if (colliderType == 0)
            return;
       //!TODO 한 공격에 한번만 맞게 할 것
            attackedColliders.Add(target);
            AttackManager.GetInstance().HandleAttack(explosionHandle, target, player, 8f);
  
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

}


public class FlameThrowerDashStrategy : DashFunction, DashStrategy
{
    public void SetState(WeaponBase weaponBase)
    {
        attack_Cancel(weaponBase);
        weaponBase.attackComboCount = 1;
    }
    public void Update(WeaponBase weaponBase)
    {

    }
}
public class FlameThrowerAttackStrategy : AttackValues, AttackStrategy
{
    Transform flameTransform;
    AttackMessage m;
    float tempTime;   
    float burnTime; // 화상 시간
    float burnDmg;
    Vector2 flameDir;
    float dirChangeTime = 1f;

    //이펙트용 변수
    GameObject[] FT_AttackEffects;
    static Pool FT_AttackEffectsPool;
    Transform effcetParent;
    int FT_AttackEffectsinitialCount = 60;
    int FT_AttackEffectsincrementCount = 30;



    WeaponBase weapon;

    public FlameThrowerAttackStrategy(WeaponBase weaponBase) : base()
    {
        burnTime = 1f;
        burnDmg = 5f;
        dashCondition = true;
        m = new AttackMessage();
        weapon = weaponBase;
        flameTransform = weaponBase.transform.Find("FlameThrowerParent/FlameThrower/FlamePosition");
        if (FT_AttackEffectsPool == null)
        {
            var e = weaponBase.GetComponentInChildren<WeaponEffects>();
            FT_AttackEffects = e.Effects;
            effcetParent = e.effectParent;

            FT_AttackEffectsPool = EffectManager.GetInstance().effectParent.gameObject.AddComponent<Pool>();
            FT_AttackEffectsPool.poolPrefab = FT_AttackEffects[1];
            FT_AttackEffectsPool.initialCount = FT_AttackEffectsinitialCount;
            FT_AttackEffectsPool.incrementCount = FT_AttackEffectsincrementCount;
            FT_AttackEffectsPool.Initialize();

        }

        tempAtkCount = 0;
        m.effectType = EffectType.SMALL;
        m.critEffectType = EffectType.CRIT;

        attackMoveCondition = MoveWhileAttack.Move_Attack;
        dashCondition = true;
    }

    AttackMessage flameHandle(IHitable target, FSMbase sender, float attackPoint)
    {
        m.FinalDamage = sender.status.getCurrentStat(STAT.AtkPoint) * attackPoint;

        target.status.AddBuff(new Burn(burnTime, burnDmg, target));
        return m;
    }
    public void onWeaponTouch(int colliderType, IHitable target)
    {
        if (colliderType == 1)
            return;
            
            AttackManager.GetInstance().HandleAttack(flameHandle, target, player, 1.3f);
            
            attackedColliders.Add(target);
    }
    public override void motionEvent(int value)
    {
        for(int i=0; i < 10; i++)
            E_FTFlame(value);  
    }
    void E_FTFlame(int position)  //직선에 가까운것에서 곡선까지 4개로 쪼개는방법
    {
        var t = FT_AttackEffectsPool.GetObjectDisabled(effcetParent);
        t.transform.position = flameTransform.position + new Vector3(0, position * 0.5f, 0);
        float r = UnityEngine.Random.Range(0, 30);
        t.transform.rotation = Quaternion.Euler(flameTransform.rotation.x, flameTransform.rotation.y, flameTransform.rotation.z + r);
        t.gameObject.SetActive(true);
        Quaternion R_quaternion = Quaternion.Euler(0, 0, r);
        Vector3 finalDir = flameDir;
        finalDir = R_quaternion * finalDir;

        float duration = 2;
        t.GetComponent<Effector>().Move(duration, finalDir * 3f).Then().Disable().Play();
        //한 함수에 4개의 t.GetComponent<Effector>().를 만들고 랜덤으로 재생
    }

    public override void SetCoolTimes()
    {
        coolTimes[0] = 0.33f;
    }

    public void SetState(WeaponBase weaponBase)
    {
        if (weaponBase.objectState != PlayerState.attack)
            weaponBase.setState(PlayerState.attack);
        weaponBase.currentMoveCondition = attackMoveCondition;
        tempTime = 0;
        weaponBase.weakedSpeed = 0.7f;
        flameDir = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - player.transform.position);
        flameDir.Normalize();
    }
    public void Update(WeaponBase weaponBase)
    {
                ViewManager.viewMouse(weaponBase);

        flameDir = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - player.transform.position);
            flameDir.Normalize();
            HandleAttackEND(weaponBase);
        
    }
    public override void StateEnd()
    {
        weapon.SetColliderEnable(false);
    }
}

public class FlameThrowerHittedStrategy : HittedStrategy
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