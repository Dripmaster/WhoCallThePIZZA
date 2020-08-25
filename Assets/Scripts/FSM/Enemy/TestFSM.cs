using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestFSM : FSMbase
{
    public float moveSpeed;
    public EnemyType enemyType;
    float aggroSpeed;
    Vector2 moveDir;
    Transform player;
    PlayerFSM playerFsm;
    CircleCollider2D playerCollider;
    float[] coolTimes;
    float[] damages;
    int attackCount;
    int tempAttackCount;
    float tPatrol;
    float tIdle;
    float disDetect;
    float disMaxDetect;
    float disAttackRange;
    float aggroTime;
    float coolStartTime;
    bool hadAttack;

    new void Awake()
    {
        base.Awake();
        Physics2D.IgnoreCollision(GetComponent<CircleCollider2D>(), GetComponentsInChildren<CircleCollider2D>()[1]);
        setStateType(typeof(EnemyState));
        initData();
    }
    new void OnEnable()
    {
        base.OnEnable();
        setState((int)EnemyState.idle);
    }
    private void FixedUpdate()
    {
        if (objectState==(int)EnemyState.patrol&&moveDir != Vector2.zero)
        {
            _rigidbody2D.MovePosition((Vector2)transform.position + moveDir * moveSpeed * Time.deltaTime);
        }
        if (objectState == (int)EnemyState.aggro && moveDir != Vector2.zero)
        {
            _rigidbody2D.MovePosition((Vector2)transform.position + moveDir * aggroSpeed * Time.deltaTime);
        }
        if (knockDegree > 0)
        {
            Vector2 movePos = Vector2.Lerp(transform.position, knockDir, Time.deltaTime);
            if((movePos - (Vector2)transform.position).sqrMagnitude <= 0.0001)
            {
                KnockBackEnd();
            }
            _rigidbody2D.MovePosition(movePos);
            
        }
    }
    void initData()
    {//현재는 임시 데이터
        status.setStat(STAT.hp, 20);
        status.setStat(STAT.AtkPoint, 5);
        status.setStat(STAT.moveSpeed, moveSpeed);
        status.init();
        //슬라임 데이터 임시로 넣어둠
        coolTimes = new float[]
        {
            3
        };
        damages = new float[]
        {
            1
        };
        attackCount = 1;
        tPatrol = 3;
        tIdle = 2;
        disDetect = 5;
        disMaxDetect = 7;
        disAttackRange = 1;
        aggroSpeed = 1.5f;
        //끝
        tempAttackCount = 0;
        aggroTime = 0;
        coolStartTime = 0;
        hadAttack = false;
    }
    public bool detectPlayer(float dis) {
        Collider2D c =  Physics2D.OverlapCircle(transform.position, dis,1<<9);
        if (c != null)
        {
            if (player == null)
            {
                player = c.transform;
                playerFsm = c.GetComponent<PlayerFSM>();
            }
            return true;
        }
        return false;
    }
    IEnumerator idle()
    {
        float tmpTime = 0;
        do
        {
            tmpTime += Time.deltaTime;
            if (detectPlayer(disDetect))
            {
                setAggro();
            }
            else if (tmpTime >= tIdle)
            {
                setState((int)EnemyState.patrol);
                moveDir = new Vector2(Random.Range(-1f,1f), Random.Range(-1f, 1f)).normalized;
            }
            yield return null;
        } while (!newState);
    }
    IEnumerator patrol()
    {
        float tmpTime = 0;
        do
        {
            tmpTime += Time.deltaTime;
            if (detectPlayer(disDetect))
            {
                setAggro();
            }
            else if (tmpTime >= tPatrol)
            {
                setState((int)EnemyState.idle);
            }
            yield return null;
        } while (!newState);
    }
    void setAggro()
    {
        if (CCreamin())
        {
            return;
        }
        setState((int)EnemyState.aggro);
        aggroTime = 0;
        moveDir = (player.position - transform.position).normalized;
    }
    IEnumerator aggro()
    {
        do
        {
            moveDir = (player.position - transform.position).normalized;
            if (!detectPlayer(disMaxDetect))
            {
                setState((int)EnemyState.idle);
            }else
            if (Time.realtimeSinceStartup>=coolStartTime+aggroTime&& Vector2.Distance(player.position, transform.position) <= disAttackRange)
            {
                setState((int)EnemyState.attack);
            }
            yield return null;
        } while (!newState);
    }
    IEnumerator attack()
    {
        aggroTime = 0;
        hadAttack = false;
        SetComboCount(tempAttackCount);
        do
        {
            //TODO: 공격 시 몇% 애니 재생 시 판정으로 할 지
            //      아니면 애니메이션에서 키프레임에서 이벤트 호출시킬지
            //      정해야함
            if (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.99f)
            {//attack Anim 종료
                doAttack();
                setAggro();
            }
            yield return null;
        } while (!newState);
        if (hadAttack)
        {
            aggroTime = coolTimes[tempAttackCount];
            tempAttackCount++;
            if (tempAttackCount >= attackCount)
            {
                tempAttackCount = 0;
            }
            coolStartTime = Time.realtimeSinceStartup;
        }
    }
    public void doAttack()
    {
        if(Vector2.Distance(player.position, transform.position) <= disAttackRange)
        { 
            hadAttack = true;
            AttackManager.Instance.SimpleDamage(damages[tempAttackCount] *
                status.getCurrentStat(STAT.AtkPoint), playerFsm);
        }
    }
    public override void TakeAttack(float dmg, bool cancelAttack = false)
    {
        //TODO
        //바로 setAggro 하지말고 Hitted(넉백) 끝나고 어그로 ㄱㄱ
        if (cancelAttack)
        {
            setAggro();
        }
        status.ChangeStat(STAT.hp,-dmg);
        if (status.getCurrentStat(STAT.hp) <= 0)
        {
            setState((int)EnemyState.dead);
        }
    }
    public override void TakeKnockBack(float degree, Vector2 knockBackDir)
    {
        if(objectState != (int)EnemyState.attack)
        {
            setState((int)EnemyState.hitted);
        }
        if(playerFsm == null)
        {
            playerFsm = WeaponBase.instance.player;
            playerCollider = playerFsm.getChildCollider();
        }
        knockDir = knockBackDir.normalized * degree+(Vector2)transform.position;
        knockDegree = degree;
        Physics2D.IgnoreCollision(getCollider(),playerFsm.getChildCollider());
        getChildCollider().isTrigger = true;
    }
    public void KnockBackEnd()
    {
        knockDir = Vector2.zero;
        knockDegree = 0;
        Physics2D.IgnoreCollision(getCollider(), playerFsm.getChildCollider(),false);
        getChildCollider().isTrigger = false;

    }
    public override void TakeCC()
    {
        setState((int)EnemyState.CC);
    }
    public override void CCfree()
    {
        setState((int)EnemyState.idle);
    }
}
