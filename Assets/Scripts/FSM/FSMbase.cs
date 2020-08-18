using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**
* @brief FSM을 가지는 오브젝트들의 부모 클래스.
* @details 상태와 방향에 맞는 애니메이션 재생을 담당하며, 상태 변화에 따라 코루틴을 실행한다. *FSMmain 클래스를 컴포넌트로 연결금지!
* @author Dripmaster, 한글닉이최고
* @date 2020-05-17
* @version 0.0.1
*
*/
public abstract class FSMbase : MonoBehaviour
{
    protected Animator _animator;
    protected SpriteRenderer _sr;
    protected int objectState;
    Type stateType;
    protected bool newState = false;
    int viewDirection;
    private float animSpeed;
    protected Rigidbody2D _rigidbody2D;
    public StatusBase status;
    protected float AnimSpeed {
        get {
            return animSpeed;
        }
        set {
            animSpeed = value;
            if (_animator != null)
            {
                _animator.SetFloat("SpeedParam", animSpeed);
            }
        }
    }
    public int ViewDirection {
        get {
            return viewDirection;
        }
        set {
            if (viewDirection != value % 8)
            {
                viewDirection = value % 8;
                if (_animator != null)
                {
                   // _animator.SetFloat("BlendParam", (ViewDirection *0.1f));
                }
            }
        }
    }
    protected void Awake()
    {
        _animator = GetComponent<Animator>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
        status = new StatusBase();
    }
    protected void Update()
    {
        status.UpdateBuff();
    }
    protected void OnEnable()
    {
        ViewDirection = 6;
        setState(0);
        AnimSpeed = 1;
        StartCoroutine(FSMmain());
    }
    protected void setStateType(Type t) {
        stateType = t;
    }
    public void setState(int state) {
        objectState = state;
        newState = true;
        _animator.SetInteger("State",objectState);
    }
    public void setState(int state,int combo)
    {
        objectState = state;
        newState = true;
        _animator.SetInteger("State", objectState);
        _animator.SetInteger("ComboCount", combo);
    }
    public void SetComboCount(int c)
    {
        _animator.SetInteger("ComboCount", c);
    }
    IEnumerator FSMmain() {
        while (true)
        {
            newState = false;
            yield return StartCoroutine(Enum.GetName(stateType, objectState));
        }
    }
    IEnumerator idle() {
        do
        {
            yield return null;
        } while (!newState);
    }
    IEnumerator move()
    {
        do
        {
            yield return null;
        } while (!newState);
    }
    IEnumerator CC()
    {
        do
        {
            if (!status.IsBuff(BUFF.Electrified))//CC상태 쭉 추가(속박 등 나중엔 배열로)
            {
                CCfree();
            }
            yield return null;
        } while (!newState);
    }
    IEnumerator dead()
    {
        _sr.color = new Color(0.2f, 0.2f, 0.2f);
        do
        {
            yield return null;
        } while (!newState);
    }
    public abstract void TakeAttack(float dmg);
    public abstract void TakeCC();
    public abstract void CCfree();
}
