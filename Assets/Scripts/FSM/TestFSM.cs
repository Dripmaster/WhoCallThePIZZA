using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestFSM : FSMbase
{
    public float moveSpeed;
    
    new void Awake()
    {
        base.Awake();
        Physics2D.IgnoreCollision(GetComponent<BoxCollider2D>(), GetComponentsInChildren<BoxCollider2D>()[1]);
        setStateType(typeof(PlayerState));
        initData();
    }
    new void OnEnable()
    {
        base.OnEnable();
        setState((int)PlayerState.move);
    }
    void initData()
    {//현재는 임시 데이터
        status.setStat(STAT.hp, 50);
        status.setStat(STAT.AtkPoint, 5);
        status.setStat(STAT.moveSpeed, moveSpeed);
        status.init();
    }
    IEnumerator move()
    {
        do
        {
            _rigidbody2D.MovePosition((Vector2)transform.position + new Vector2(-Time.deltaTime * moveSpeed, 0));
            yield return null;
        } while (!newState);
    }

    IEnumerator CC()
    {
        do
        {
            if (!status.IsBuff(BUFF.Electrified))//CC상태 쭉 추가
            {
                setState((int)PlayerState.idle);
            }
            yield return null;
        } while (!newState);
    }

    public override void TakeAttack(float dmg)
    {
        Debug.Log("데미지 : "+dmg+" -> "+transform.name);
    }
}
