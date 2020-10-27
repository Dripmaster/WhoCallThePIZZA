using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class WeaponBase : MonoBehaviour
{
    /// <summary>
    /// 플레이어에게 넘겨줄 animType
    /// </summary>

    public WeaponType weaponType;

    public MoveWhileAttack currentMoveCondition;
    public bool CanAttackCancel;
    public bool CanRotateView;
    public bool isDash;
    public int attackComboCount;
    public bool nowAttack;

    Animator _animator;
    public PlayerState objectState;
    protected bool newState;
    private float animSpeed;
    int viewDirection;

    IdleStrategy idleStrategy;
    SkillStrategy skillStrategy;
    MoveStrategy moveStrategy;
    DashStrategy dashStrategy;
    DeadStrategy deadStrategy;
    MouseInputStrategy mouseInputStrategy;
    AttackStrategy attackStrategy;
    HittedStrategy hittedstrategy;

    public PlayerFSM player;
    public static WeaponBase instance;

    Transform Rotator;
    bool flipValue;
    float tempX;
    float tempScaleX;
    float tempScaleY;
    Collider2D[] colliders;
    Vector2 vLerpTarget;


    public float weakedSpeed;

    WeaponInfo[] equipedWeapons;
    int currentWeapon;
    private WeaponBase() {
        instance = this;
    }
    public float AnimSpeed
    {
        get
        {
            return animSpeed;
        }
        set
        {
            animSpeed = value;
            if (_animator != null)
            {
                _animator.SetFloat("SpeedParam", animSpeed);
            }
        }
    }
    public int ViewDirection //0~7 0 = 왼쪽, 시계방향으로 증가
    {
        get
        {
            return viewDirection;
        }
        set
        {
            if (viewDirection != value % 8)
            {
                viewDirection = value % 8;
                if (_animator != null)
                {
                    //_animator.SetFloat("BlendParam", (ViewDirection * 0.1f));
                }
            }
        }
    }

    public float WeaponViewDirection;
    public float MouseViewDegree;
    protected void Awake()
    {
    }
    public void setWeapon(int num)
    {
        equipedWeapons[currentWeapon].gameObject.SetActive(false);
        currentWeapon = num;
        equipedWeapons[currentWeapon].transform.SetAsFirstSibling();
        equipedWeapons[currentWeapon].gameObject.SetActive(true);
        setWeapon(equipedWeapons[currentWeapon].wType);
        player.refreshWeapon((int)weaponType);
        attackComboCount = 0;
        SetIdle();
    }
    public void setWeapon(WeaponType weaponType) {
        this.weaponType = weaponType;
        InitData();
    }
    void setStateEnum(WeaponType weaponType) {
        switch (weaponType)
        {
            case WeaponType.sampleWeapon:
                SampleWeapon.SetStrategy(out idleStrategy, out moveStrategy, out deadStrategy, out mouseInputStrategy, out dashStrategy, out attackStrategy, out hittedstrategy, out skillStrategy, this);
                //스트래티지들 싹다 세팅
                //애니메이션 컨트롤러 변경
                break;
            case WeaponType.StormPist:
                StormPist.SetStrategy(out idleStrategy, out moveStrategy, out deadStrategy, out mouseInputStrategy, out dashStrategy, out attackStrategy, out hittedstrategy, out skillStrategy, this);
                break;
            case WeaponType.Lance:
                Lance.SetStrategy(out idleStrategy, out moveStrategy, out deadStrategy, out mouseInputStrategy, out dashStrategy, out attackStrategy, out hittedstrategy, out skillStrategy, this);
                break;
            case WeaponType.FlameThrower:
                FlameThrower.SetStrategy(out idleStrategy, out moveStrategy, out deadStrategy, out mouseInputStrategy, out dashStrategy, out attackStrategy, out hittedstrategy, out skillStrategy, this);
                break;
            case WeaponType.Firework:
                Firework.SetStrategy(out idleStrategy, out moveStrategy, out deadStrategy, out mouseInputStrategy, out dashStrategy, out attackStrategy, out hittedstrategy, out skillStrategy, this);
                break;
            default:
                break;
        }
    }
    /// 플레이어 스테이트 변경
    /// 무기 스테이트 변경
    /// 무기별 왼쪽클릭 핸들링
    /// 이렇게 말고 스트래티지 패턴으로 해야하지않나...?
    /// 그럼 어택 카운트같은거 해서 애니메이션 바꾸는건 어떻게?->인자로 무기 넘겨
    /// 애니메이커에 기존 컨트롤러 모션 추가 기능 넣어야 함
    /// 무기 상속이 아니라 진짜 그냥 스트래티지들로 해서 할 수 있게 짤수도 있을거 같은데?
    /// 무기에따라 애니메이션하고 stateEnum만 설정하는식으로.... 생각해보면 될듯함
    /// idle move 좌클릭 우클릭 ... 스트레티지
    /// 1. 무기 타입 설정
    /// 2. 애니메이션 컨트롤 설정
    /// 3. 스트래티지 설정
    public void InitData() {
        setStateEnum(weaponType);
        colliders = transform.GetComponentsInChildren<Collider2D>();
        SetColliderEnable(false);

        _animator = transform.GetChild(0).GetComponentInChildren<Animator>();
        Rotator = transform.GetChild(0);
        vLerpTarget = Rotator.localPosition;
        tempX = Rotator.transform.localPosition.x;
        tempScaleX = Rotator.transform.localScale.x;
        tempScaleY = Rotator.transform.localScale.y;

        weakedSpeed = 1;
        ViewDirection = 6;
        isDash = false;
        AnimSpeed = 1;
        attackComboCount = 0;
        CanAttackCancel = true;
        CanRotateView = true;
        currentMoveCondition = 0;
        newState = false;
        nowAttack = false;
        flipValue = false;
    }
    public void SetPlayerFree() {

        if (player.MoveInput())
            player.setState((int)PlayerState.move);
        else
            player.setState((int)PlayerState.idle);

    }
    public void SetIdle(bool playerSet = false) {
        idleStrategy.SetState(this);
        if (playerSet) {
            player.setState((int)PlayerState.idle);
        }
    }
    public void SetMove(bool playerSet = false) {
        moveStrategy.SetState(this);
        if (playerSet)
        {
            player.setState((int)PlayerState.move);
        }
    }
    public void SetDead(bool playerSet = false) {
        deadStrategy.SetState(this);
        if (playerSet)
        {
            player.setState((int)PlayerState.dead);
        }
    }
    public void SetSkill(bool playerSet = false)
    {
        skillStrategy.SetState(this);
        if (playerSet)
        {
            player.setState((int)PlayerState.skill);
        }
    }
    public void SetAttack(bool playerSet = false)
    {
        attackStrategy.SetState(this);
        if (playerSet)
        {
            player.setState((int)PlayerState.attack);
        }
        Rotator.localPosition = vLerpTarget;
        Rotator.localScale = new Vector3(tempScaleX, tempScaleY);
    }
    public void SetHitted(bool playerSet = false)
    {//TODO : 하던거 캔슬하게(어차피 캔슬 되지만 추가작업 필요 할 수 있음)
        hittedstrategy.SetState(this);
        if (playerSet)
        {
            player.setState((int)PlayerState.hitted);
        }
    }
    public void MouseInput() {
        mouseInputStrategy.HandleInput(this);
    }
    public void SetDash(bool playerSet = false) {
        dashStrategy.SetState(this);
        if (playerSet)
        {
            player.setState((int)PlayerState.dash);
        }
    }
    public bool CanDash() {
        if (objectState == PlayerState.attack)
            return attackStrategy.canDash();
        else if(objectState == PlayerState.skill)
            return skillStrategy.canDash();
        else
            return true;
    }
    protected void OnEnable()
    {
        if (player == null)
        {
            player = GameObject.FindWithTag("Player").GetComponent<PlayerFSM>();
        }

        equipedWeapons = GetComponentsInChildren<WeaponInfo>();
        equipedWeapons[1].gameObject.SetActive(false);
        equipedWeapons[2].gameObject.SetActive(false);
        equipedWeapons[3].gameObject.SetActive(false);
        currentWeapon = 0;
        setWeapon(equipedWeapons[currentWeapon].wType);

        SetIdle();
        StartCoroutine(FSMmain());
    }
    public bool getAnimEnd(float targetTime=0.99f) {
        if (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= targetTime) {
            return true;
        }
        return false;
    }
    public float getAnimProgress()
    {
        return _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }
    public void setState(PlayerState state,bool animChage = true)
    {
        objectState = state;
        newState = true;
        if (animChage)
        {
            _animator.SetInteger("State", (int)objectState);
            _animator.SetInteger("ComboCount", attackComboCount);
        }
        weakedSpeed = 1f;
        //attackComboCount = 0;
    }
    public void setRotate(float value,bool isAttack=false) {
        if (!isAttack&&Rotator.localScale.x * tempScaleX < 0)
            value = -value;
        Rotator.rotation = Quaternion.Euler(0, 0, value);
    }
    public void setFlipScaleX(float value)//플립
    {
        if (tempScaleX !=value)
        {
            tempScaleX = value;
        }
    }
    public void setFlipScaleY(float value)//플립
    {
        if (tempScaleY != value)
        {
            tempScaleY = value;
        }
    }
    public bool SP_FlipX() 
    {
        bool flip = true;
        if (ViewDirection < 2 || ViewDirection > 5)
        {// (0, 1, 7, 6)  Right
            setFlipScaleX(1);
            flip = false;
        }
        else
            setFlipScaleX(-1);
        return flip;
    }
    public void SP_FlipY()  
    {
        if (ViewDirection < 2 || ViewDirection > 5)
            // (0, 1, 7,6)  Right
            setFlipScaleY(1);
        else
            setFlipScaleY(-1);
    }
    public void setFlip(bool value)//한손무기 위치플립
    {
        flipValue = value;
        if (flipValue)
        {
            vLerpTarget = new Vector3(tempX * -1, Rotator.localPosition.y);
        }
        else {
            vLerpTarget = new Vector3(tempX, Rotator.localPosition.y);
        }
    }
    IEnumerator FSMmain()
    {
        while (true)
        {
            newState = false;
            yield return StartCoroutine((objectState).ToString());
        }
    }
    IEnumerator idle()
    {
        do
        {
            idleStrategy.Update(this);
            MouseInput();
            yield return null;
        } while (!newState);
    }
    IEnumerator move()
    {
        do
        {
            moveStrategy.Update(this);
            MouseInput();
            yield return null;
        } while (!newState);
    }
    IEnumerator dash()
    {
        isDash = true;
        do
        {
            dashStrategy.Update(this);
            MouseInput();
            yield return null;
        } while (!newState);
        isDash = false;
    }
    
    IEnumerator attack()
    {
        do
        {
            attackStrategy.Update(this);
            MouseInput();
            yield return null;
        } while (!newState);
        attackStrategy.StartCool();
        attackStrategy.StateEnd();
        nowAttack = false;
        CanRotateView = true;
        CanAttackCancel = true;
        AnimSpeed = 1;
        SetColliderEnable(false);
    }
    IEnumerator skill()
    {
        do
        {
            skillStrategy.Update(this);
            MouseInput();
            yield return null;
        } while (!newState);
        skillStrategy.StartCool();
        skillStrategy.StateEnd();
        CanRotateView = true;
        CanAttackCancel = true;
        SetColliderEnable(false);
        AnimSpeed = 1;
    }
    IEnumerator dead()
    {
        do
        {
            yield return null;
        } while (!newState);
    }
    IEnumerator hitted()
    {
        do
        {
            hittedstrategy.Update(this);
            //MouseInput();
            yield return null;
        } while (!newState);
        CanRotateView = true;
    }
    public void onWeaponTouch(int colliderType, Collider2D target) {
        if (objectState == PlayerState.skill)
            skillStrategy.onWeaponTouch(colliderType, target);


        if (!nowAttack)
        {
            return;
        }
        attackStrategy.onWeaponTouch(colliderType,target);
    }
    public void setViewPoint() {
        player.SetViewPoint();
    }

    public PlayerState getPlayerState()
    {
        return player.getState();
    }
    public PlayerState getState() {
        return objectState;
    }
    public MoveWhileAttack getMoveAttackCondition()
    {
        return attackStrategy.getAttackMoveCondition();
    }
    public MoveWhileAttack getMoveSkillCondition()
    {
        return skillStrategy.getSkillMoveCondition();
    }
    public void SetComboCount(int c)
    {
        attackComboCount = c;
        _animator.SetInteger("ComboCount", c);
        player.SetComboCount(attackComboCount);
    }
    public bool IsAttackCoolTimeRemain()
    {
        float r;
        float t;
        attackStrategy.GetCoolTime(out r, out t);
        if (r <= 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    public bool IsSkillCoolTimeRemain()
    {
        float r;
        float t;
        skillStrategy.GetCoolTime(out r, out t);
        if (r <= 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    public Animator GetAnimatior() {
        return _animator;
    }
    public void SetColliderEnable(bool v)
    {
        foreach (var item in colliders)
        {
            item.enabled = v;
        }
    }
    public void motionEvent(int value)
    {
        //!TODO 모든 스테이트에 대해 모션 이벤트 받도록
        //스트래티지 인터페이스 수정 필요.
        switch (objectState)
        {
            case PlayerState.idle:
                break;
            case PlayerState.move:
                break;
            case PlayerState.attack:
                attackStrategy.motionEvent(value);
                break;
            case PlayerState.dead:
                break;
            case PlayerState.skill:
                skillStrategy.motionEvent(value);
                break;
            case PlayerState.dash:
                break;
            case PlayerState.hitted:
                break;
            default:
                break;
        }
    }
    private void FixedUpdate()
    {
        if ((vLerpTarget - (Vector2)Rotator.localPosition).sqrMagnitude >= 0.0001f)
        {
            Rotator.localPosition =
                Vector2.Lerp(Rotator.localPosition, vLerpTarget,
                Time.deltaTime*5);

            if (Rotator.localScale.x != tempScaleX&&vLerpTarget.x * Rotator.localPosition.x >= 0)
            {
                Rotator.localScale = new Vector3(tempScaleX, tempScaleY);
            }
        }
        else
        {
            Rotator.localPosition = vLerpTarget;
            if (Rotator.localScale.x != tempScaleX)
            {
                Rotator.localScale = new Vector3(tempScaleX, tempScaleY);
            }
        }

        
    }

    private void Update()
    {
        
        if (InputSystem.Instance.getKeyDown(InputKeys.WeaponSwapBtn))
        {
            if (objectState == PlayerState.idle || objectState == PlayerState.move)
            {
                int num = currentWeapon;
                if (num == equipedWeapons.Length - 1)
                {
                    num = -1;
                }
                setWeapon(num + 1);
            }
        }
    }
}