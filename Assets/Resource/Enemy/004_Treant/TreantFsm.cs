using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TreantFsm : EnemyBase
{

    //투사체
    Transform TreantLaunchTransform;
    public BulletBase bulletPrefab;
    Pool TreantBulletPool;
    int TreantBulletinitialCount = 16;
    int TreantBulletincrementCount = 16;
    public Vector2 bulletDir;
    public Vector2 playerDir;
    public float bulletSpeed = 5;
    float randomAngle;
    Quaternion qRandomAngle;

    public DroppedItemBase myDropItem;
    bool attackTrigger = false;
    AttackMessage m;
    new void Awake()
    {
        //투사체 풀
        if (TreantBulletPool == null)
        {
            TreantBulletPool = AttackManager.GetInstance().bulletParent.gameObject.AddComponent<Pool>();
            TreantBulletPool.incrementCount = TreantBulletincrementCount;
            TreantBulletPool.initialCount = TreantBulletinitialCount;
            TreantBulletPool.poolPrefab = bulletPrefab.gameObject;
            TreantBulletPool.Initialize();
        }

        TreantLaunchTransform = this.gameObject.transform;
        base.Awake();
        setStateType(typeof(TreantEnum));
    }
    new void OnEnable()
    {
        base.OnEnable();
        setState((int)TreantEnum.idle);
    }
    public override void initData()
    {
        status.setStat(STAT.hp, 80);
        status.setStat(STAT.AtkPoint, 3);
        status.setStat(STAT.moveSpeed, 0);
        status.init();
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
        disDetect = 10;
        disMaxDetect = 100;
        disAttackRange = 10;
        //끝
        tempAttackCount = 0;
        aggroTime = 0;
        coolStartTime = 0;
        hadAttack = false;
    }
    IEnumerator idle()//플레이어가 맵에 들어오기 전
    {
        float tmpTime = 0;
        do
        {
            tmpTime += Time.deltaTime;
            if (detectPlayer(disDetect))
            {
                setState((int)TreantEnum.attack);
            }
            yield return null;
        } while (!newState);
    }
    IEnumerator aggro()//플레이어가 맵에 들어온 뒤 공격한 후
    {
        float tmpTime = 0;
        do
        {
            tmpTime += Time.deltaTime;
            if (tmpTime >= aggroTime)
            {
                if (detectPlayer(disMaxDetect))
                {
                    setState((int)TreantEnum.attack);
                }
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
            if (attackTrigger)
            {
                attackTrigger = false;
                leafAttack();
            }
            if (animEnd)
            {
                setState((int)TreantEnum.aggro);
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
    public void leafAttack()
    {
        for (int i = 0; i < 4; i++) 
        {
            oneLeafAttack();
        }

        hadAttack = true;
    }

    void oneLeafAttack()
    {
        randomAngle = (UnityEngine.Random.Range(-3, 4)) * 15;                 //-45 -30 -15 0 15 30 45 랜덤방향 설정
        qRandomAngle = Quaternion.AngleAxis(randomAngle, new Vector3(0, 0, 1));
        playerDir = (player.transform.position - TreantLaunchTransform.position);

        bulletDir = qRandomAngle * playerDir;
        bulletDir.Normalize();

        var b = TreantBulletPool.GetObjectDisabled().GetComponent<BulletBase>();
        b.transform.position = TreantLaunchTransform.position;
        b.dir = bulletDir;
        b.speed = bulletSpeed;
        b.touched += TreantBulletTouched;
        b.gameObject.SetActive(true);
    }
    bool TreantBulletTouched(Collider2D collision)
    {
        var fsm = collision.GetComponent<FSMbase>();
        if (fsm != null)
        {

            AttackManager.GetInstance().HandleAttack(bulletHandle, fsm, playerFsm, 1.3f);

        }
        return true;
    }
    AttackMessage bulletHandle(FSMbase target, FSMbase sender, float attackPoint)
    {
        m.FinalDamage = sender.status.getCurrentStat(STAT.AtkPoint) * attackPoint;

        return m;
    }
    public override void sendAttackMessage(int attackType)
    {
        attackTrigger = true;
    }
    public override void TakeAttack(float dmg, bool cancelAttack = false)
    {
        if (status.getCurrentStat(STAT.hp) <= 0)
        {
            return;
        }

        status.ChangeStat(STAT.hp, -dmg);
        if (status.getCurrentStat(STAT.hp) <= 0)
        {
            setState((int)TreantEnum.dead);
        }
    }
    public override void DropItem()
    {
        // ItemDropSystem.MyInstance.DropItem(transform.position,myDropItem);
    }
    public enum TreantEnum
    {//슬라임의 스테이트(고유 번호 고정)
        idle = 0,
        aggro,
        attack,
        dead,
    }

    public override void TakeKnockBack(float force, Vector2 knockBackDir)
    {
        
    }

    public override void KnockBackEnd()
    {

    }

    public override void TakeCC(int CCnum)
    {
        
    }
    public override void CCfree()
    {
        
    }

}

