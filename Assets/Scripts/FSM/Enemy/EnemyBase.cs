using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBase : FSMbase
{
    protected float moveSpeed;
    protected float aggroSpeed;
    protected Vector2 moveDir;
    protected float targetZ;
    protected Transform player;
    protected PlayerFSM playerFsm;
    protected float[] coolTimes;
    protected float[] damages;
    protected int attackCount;//최대 공격 콤보 최소 : 1
    protected int tempAttackCount;//현재 공격 콤보
    protected float disAttackRange;//사정거리
    protected float tPatrol;//패트롤 시간
    protected float tIdle;//idle 시간
    protected float disDetect;//탐지거리
    protected float disMaxDetect;//추격거리
    protected float aggroTime;//남은 추격시간(0일 시 공격 가능)
    protected float coolStartTime;//쿨타임 시작 시간(real time)
    protected bool hadAttack;//공격 했는지

    public new void Awake()
    {
        base.Awake();
        Physics2D.IgnoreCollision(GetComponent<CircleCollider2D>(), GetComponentsInChildren<CircleCollider2D>()[1]);
        initData();
        player = WeaponBase.instance.player.transform;
    }
    public new void OnEnable()
    {
        base.OnEnable();
        if (playerFsm == null)
        {
            playerFsm = WeaponBase.instance.player;
        }
    }
    public void FixedUpdate()
    {
        if (targetZ != 0)
        {
            zSystem.Z += targetZ * Time.deltaTime;
            targetZ = 0;
        }
    }
    public override void setZ(float z)
    {
        base.setZ(z);
    }
    public bool movePatrol(int state)
    {
        bool result = false;
        if (objectState == state && moveDir != Vector2.zero)
        {
            result = true;
            _rigidbody2D.MovePosition((Vector2)transform.position + moveDir * status.getCurrentStat(STAT.moveSpeed) * Time.deltaTime);
        }
        return result;
    }
    public bool moveAggro(int state)
    {
        bool result = false;
        if (objectState == state && moveDir != Vector2.zero)
        {
            result = true;
            _rigidbody2D.MovePosition((Vector2)transform.position + moveDir * status.getCurrentStat(STAT.moveSpeed) * Time.deltaTime);
        }
        return result;
    }
    public bool moveKnockBack()
    {
        bool result = false;
        if (knockDir.sqrMagnitude> 0)
        {
            result = true;
            knockDir = Vector2.Lerp(knockDir,Vector2.zero,Time.deltaTime);

            Vector2 tempPos = transform.position;
            Vector2 targetPos = (Vector2)transform.position + knockDir;
            if (knockDir.sqrMagnitude <= 0.0001 || (targetPos-tempPos).sqrMagnitude<=0.0001)
            {
                clearKnockBack();
            }

            _rigidbody2D.MovePosition(targetPos);
        }
        return result;
    }/*
    public bool moveKnockBack()
    {
        bool result = false;
        if (knockBackDistance > 0)
        {
            result = true;
            Vector2 movePos = Vector2.Lerp(transform.position, knockDir, Time.deltaTime * knockBackVelocity);
            if ((movePos - (Vector2)transform.position).sqrMagnitude <= 0.0001)
            {
                clearKnockBack();
            }
            _rigidbody2D.MovePosition(movePos);
        }
        return result;
    }*/
    public abstract void initData();
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
    public bool canAttackPlayer()
    {
        if (Time.realtimeSinceStartup >= coolStartTime + aggroTime && Vector2.Distance(player.position, transform.position) <= disAttackRange)
        {
            if(CollisionByZ.Zcheck(zSystem,playerFsm.GetZSystem()))
            return true;
        }
        return false;
    }
    IEnumerator idle()
    {
        do
        {

            yield return null;
        } while (!newState);
    }
    IEnumerator patrol()
    {
        do
        {

            yield return null;
        } while (!newState);
    }
    public void setAggro(int aggroState)
    {
        if (CCreamin())
        {
            return;
        }
        setState(aggroState);
        moveDir = (player.position - transform.position).normalized;
    }
    IEnumerator aggro()
    {
        do
        {

            yield return null;
        } while (!newState);
    }
    IEnumerator attack()
    {
        do
        {

            yield return null;
        } while (!newState);
    }
    public void doSimpleAttack()
    {
        if(Vector2.Distance(player.position, transform.position) <= disAttackRange)
        { 
            hadAttack = true;
            AttackManager.Instance.SimpleDamage(damages[tempAttackCount] *
                status.getCurrentStat(STAT.AtkPoint), playerFsm);
        }
    }
    public override abstract void TakeAttack(float dmg, bool cancelAttack = false);
  //  public override abstract void TakeKnockBack(float distance, float velocity, Vector2 knockBackDir);
    public override abstract void KnockBackEnd();
    public override abstract void TakeCC(int CCnum);
    public override abstract void CCfree();
    public void clearKnockBack()
    {
        knockDir = Vector2.zero;
        knockBackDistance = 0;
        knockBackVelocity = 0;
    }
    public void SetCollidersTriggerNotTerrain(bool value)
    {
        getChildCollider().isTrigger = value;
        if(value == true)
        gameObject.layer = 16;
        else
        {
            gameObject.layer = 10;
        }
        //getCollider().isTrigger = value;
    }

    public override abstract void DropItem();
    public override void moveFoward(StepForwardValues sfv)
    {
        SetCollidersTriggerNotTerrain(true);
        stepFoward.SetStep(sfv, viewDir,SetCollidersTriggerNotTerrain,false);
    }
    public abstract void sendAttackMessage(int attackType);
}
