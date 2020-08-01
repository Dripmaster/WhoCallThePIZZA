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

    Animator _animator;
    PlayerState objectState;
    protected bool newState;
    int viewDirection;
    public int attackComboCount;
    public bool nowAttack;
    private float animSpeed;
    IdleStrategy idleStrategy;
    SkillStrategy skillStrategy;
    MoveStrategy moveStrategy;
    DashStrategy dashStrategy;
    DeadStrategy deadStrategy;
    MouseInputStrategy mouseInputStrategy;
    AttackStrategy attackStrategy;
    public PlayerFSM player;
    public static WeaponBase instance;
    Transform Rotator;
    bool flipValue;
    float tempX;
    float tempScaleX;
    float tempScaleY;
    private WeaponBase() {
        instance = this;
    }
    protected float AnimSpeed
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
    protected void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        InitData();
    }
    public void setWeapon(WeaponType weaponType) {
        this.weaponType = weaponType;
        InitData();
    }
    void setStateEnum(WeaponType weaponType) {
        switch (weaponType)
        {
            case WeaponType.sampleWeapon:
                SampleWeapon.SetStrategy(out idleStrategy, out moveStrategy, out deadStrategy, out mouseInputStrategy, out dashStrategy, out attackStrategy, out skillStrategy, this);
                //스트래티지들 싹다 세팅
                //애니메이션 컨트롤러 변경
                break;
            case WeaponType.StormPist:
                StormPist.SetStrategy(out idleStrategy, out moveStrategy, out deadStrategy, out mouseInputStrategy, out dashStrategy, out attackStrategy, out skillStrategy, this);
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
        else
            return true;
    }
    protected void OnEnable()
    {
        ViewDirection = 6;
        SetIdle();
        isDash = false;
        AnimSpeed = 1;
        attackComboCount = 0;
        CanAttackCancel = true;
        CanRotateView = true;
        currentMoveCondition = 0;
        newState = false;
        nowAttack = false;
        Rotator = transform.GetChild(0);
        flipValue = false;
        tempX = Rotator.transform.localPosition.x;
        tempScaleX = Rotator.transform.localScale.x;
        tempScaleY = Rotator.transform.localScale.y;
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
        attackComboCount = 0;
    }
    public void setRotate(float value) {
        Rotator.rotation = Quaternion.Euler(0, 0, value);
    }
    public void setFlipScaleX(float value)//플립
    {
        if (tempScaleX !=value)
        {
            tempScaleX = value;
            Rotator.transform.localScale = new Vector3(tempScaleX, Rotator.transform.localScale.y);
        }
    }
    public void setFlipScaleY(float value)//플립
    {
        if (tempScaleY != value)
        {
            tempScaleY = value;
            Rotator.transform.localScale = new Vector3(Rotator.transform.localScale.x, tempScaleY);
        }
    }

    public void SP_FlipY()  // StormPist용 flip
    {
        if (ViewDirection < 2 || ViewDirection > 6)// (0, 1, 7)  Left      2, 6 은 원하는데로 설정
            setFlipScaleY(-1); // -1 : 위아래 뒤집기
        else
            setFlipScaleY(1); // 원래 대로
    }
    public void setFlip(bool value)//한손무기 위치플립
    {
        flipValue = value;
        if (flipValue)
        {
            Rotator.transform.localPosition = new Vector3(tempX * -1, Rotator.transform.localPosition.y);
        }
        else {
            Rotator.transform.localPosition = new Vector3(tempX, Rotator.transform.localPosition.y);
        }
    }
    IEnumerator FSMmain()
    {
        while (true)
        {
            newState = false;
            yield return StartCoroutine(((PlayerState)objectState).ToString());
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
        nowAttack = false;
    }
    IEnumerator skill()
    {
        do
        {
            skillStrategy.Update(this);
            MouseInput();
            yield return null;
        } while (!newState);
    }
    public void onWeaponTouch(int colliderType, FSMbase target) {
        if (!nowAttack)
            return;

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
    public MoveWhileAttack getMoveAttackCondition() {
        return attackStrategy.getAttackMoveCondition();
    }
    public void SetComboCount(int c)
    {
        attackComboCount = c;
        player.SetComboCount(attackComboCount);
    }
}