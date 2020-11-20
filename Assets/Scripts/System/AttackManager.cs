using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

public class AttackManager : MonoBehaviour
{
    #region Singletone
    private static AttackManager instance;
    public static AttackManager Instance
    {
        get
        {
            if (instance == null)
            {
                var obj = FindObjectOfType<AttackManager>();
                if (obj != null)
                {
                    instance = obj;
                }
                else
                {
                    var newSingleton = new GameObject("AttackManager Class").AddComponent<AttackManager>();
                    instance = newSingleton;
                }
            }
            return instance;
        }
        private set
        {
            instance = value;
        }
    }
    public static AttackManager GetInstance() {
        return Instance;
    }
    #endregion
    public PlayerFSM player;

    public Transform bulletParent;
    public EffectManager effectManager;
    void Awake()
    {
        var objs = FindObjectsOfType<AttackManager>();
        if (objs.Length != 1)
        {
            Destroy(gameObject);
            return;
        }
        effectManager = EffectManager.GetInstance();
    }
    public Collider2D[] GetTargetList(Vector2 point, float Range, int layerMask, List<Collider2D> exceptList)  //원형범위 + 제외대상 제외
    {

        Collider2D[] colliders = GetTargetList(point, Range, layerMask);
        List<Collider2D> colliderList = colliders.ToList();
        foreach (var item in exceptList)
        {
            colliderList.Remove(item);
        }
        return colliderList.ToArray();

    }
    public Collider2D[] GetTargetList(Vector2 point, float DegreeRange, Vector2 ViewDirection, float Range, int layerMask, List<Collider2D> exceptList) // 부채꼴형범위 + 제외대상 제외
    {

        Collider2D[] colliders = GetTargetList(point, DegreeRange, ViewDirection, Range, layerMask);
        List<Collider2D> colliderList = colliders.ToList();
        foreach (var item in exceptList)
        {
            colliderList.Remove(item);
        }
        return colliderList.ToArray();

    }

    public Collider2D[] GetTargetList(Vector2 point, float Range, int layerMask) // 원형 가까운 순 정렬
    { 
        Collider2D[] colliders = Physics2D.OverlapCircleAll(point, Range, layerMask);

        Collider2D temp;
        for (int i = 0; i < colliders.Length; i++)
        {
            for (int j = i; j < colliders.Length - i - 1; j++)
            {
                if (((Vector2)colliders[j].transform.position - point).sqrMagnitude > ((Vector2)colliders[j + 1].transform.position - point).sqrMagnitude)
                {
                    temp = colliders[j];
                    colliders[j] = colliders[j + 1];
                    colliders[j + 1] = temp;

                }
            }
        }
        return colliders;
    }
    public Collider2D[] GetTargetList(Vector2 point, float DegreeRange, Vector2 ViewDirection, float Range, int layerMask) //부채꼴형 가까운 순 정렬
    {
        List<Collider2D> colliders = Physics2D.OverlapCircleAll(point, Range, layerMask).ToList();
        for (int i = colliders.Count - 1; i >= 0; i--)
        {
            if (Mathf.Abs(Vector2.Angle(ViewDirection, (Vector2)colliders[i].transform.position - point)) > DegreeRange / 2) {
                colliders.Remove(colliders[i]);
            }
        }

        Collider2D temp;
        for (int i = 0; i < colliders.Count; i++)
        {
            for (int j = i; j < colliders.Count - i - 1; j++)
            {
                if (((Vector2)colliders[j].transform.position - point).sqrMagnitude > ((Vector2)colliders[j + 1].transform.position - point).sqrMagnitude)
                {
                    temp = colliders[j];
                    colliders[j] = colliders[j + 1];
                    colliders[j + 1] = temp;

                }
            }
        }
        return colliders.ToArray();
    }
    /*HandleDamage
    public bool HandleDamage(float atkPoint, FSMbase target, StatusBase status, out float ResultAttackPoint) {
        int p = random.Next(100);
        bool r = false;
        atkPoint *= status.getCurrentStat(STAT.AtkPoint);
        if (p < status.getCurrentStat(STAT.CriticalPoint))
        {
            atkPoint *= status.getCurrentStat(STAT.CriticalDamage);
            r = true;
        }
        target.TakeAttack(atkPoint);
        ResultAttackPoint = atkPoint;
        return r;
    }
    public bool HandleDamage(float atkPoint, FSMbase target, int hitEffectNum, StatusBase status, int cri_hitEffectNum = 0)
    {
        bool r = HandleDamage(atkPoint, target, status, out atkPoint);
        if (!r)
            defaultEffect(target, hitEffectNum);
        else
            defaultEffect(target, cri_hitEffectNum);
        return r;
    }
    */
    public void SimpleDamage(float Dmg, IHitable target) {
        //TODO : cri계산??
        target.TakeAttack(Dmg,false);
        var mHit = target as HitableBase;
        if (mHit == null)
            effectManager.defaultEffect(target,0);
    }
    public void HandleAttack(attackFunc attack, IHitable target, FSMbase sender, float attackPoint, bool cancelAttack = false, bool isKnockBack = false)
    {
        AttackMessage m = attack.Invoke(target, sender, attackPoint);
        if(!m.criCalculated)
            m.CriCalculate(sender.status.getCurrentStat(STAT.CriticalPoint), sender.status.getCurrentStat(STAT.CriticalDamage));
        m.CalcDefense(target,sender);
        target.TakeAttack(m.FinalDamage,cancelAttack);
        if (isKnockBack)
        {
            target.TakeKnockBack(m.knockBackVelocity/50, m.knockBackDir);
        }
        var mHit = target as HitableBase;
        if(effectManager != null)
        {
            if (mHit == null)
                effectManager.defaultEffect(target, m.isCritical ? m.critEffectType : m.effectType);
        }
    }
}
public delegate AttackMessage attackFunc(IHitable target, FSMbase sender, float attackPoint);
public struct AttackMessage
{
    public float FinalDamage;
    public EffectType effectType;
    public EffectType critEffectType;
    public bool isCritical;
    public bool criCalculated;
    public Vector2 knockBackDir;
    public float knockBackVelocity;
    public float knockBackDistance;

    public bool CriCalculate(float criPoint,float criticalDamage) {
        int r = Random.Range(0,100);
        if (r < criPoint)
        {
            isCritical = true;
            FinalDamage *= criticalDamage;
        }
        else
        {
            isCritical = false;
        }
        criCalculated = true;
        return isCritical;
    }

    public void CalcDefense(IHitable target, FSMbase sender)
    {
        if (target.status.IsBuff(BUFF.Bleeding))
        {
            FinalDamage *= 1.3f;
        }
        if (!target.status.IsBuff(BUFF.Pierced))
        {
            FinalDamage -= FinalDamage*target.status.getCurrentStat(STAT.DefensePoint);
        }
        else
        {
            FinalDamage *=1.2f;
        }
    }
    public void CalcKnockBack(Vector2 knockBackDir,float knockBackVelocity,float knockBackDistance)
    {
        this.knockBackVelocity = knockBackVelocity;
        this.knockBackDistance = knockBackDistance;
        this.knockBackDir = knockBackDir;
    }
    public void CalcKnockBack(IHitable target, FSMbase sender, float knockBackVelocity, float knockBackDistance)
    {
        this.knockBackVelocity = knockBackVelocity;
        this.knockBackDistance = knockBackDistance;
        knockBackDir = (target.transform.position - sender.transform.position).normalized;
    }
}
/*
public class AttackMessage {
    public StatusBase target;
    public StatusBase sender;
    public float atkPoint;

    public AdditionalAttack[] additionalAttacks;
    public AttackMessage(float atkPoint,StatusBase target,StatusBase sender,params AdditionalAttack[] additionalAttack) {
        this.atkPoint = atkPoint;
        this.target = target;
        this.sender = sender;
        additionalAttacks = additionalAttack;
    }
    public void SetAdditionalAttacks(params AdditionalAttack[] additionalAttack)
    {
        additionalAttacks=additionalAttack;
    }
}
public class AdditionalAttack//공격 시 뭔가가 일어날 때
{
    public float Point;//발동확률
    public float Damage;//발동 시 추가 데미지(sender공격력과 곱해져서 별도로 들어감)
    public BUFF buff;//발동 시 걸릴 버프
    public bool buffTarget = false;//false = 적, true = 본인
    public delegate bool action(StatusBase target,StatusBase sender);//확률이 아니라 조건문으로도 발동 가능
}*/