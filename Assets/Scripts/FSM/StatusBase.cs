using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StatusBase
{
    public Dictionary<STAT,float>  CurrentStats;
    public Dictionary<STAT,float>  Stats;
    public Dictionary<STAT,float>  StatValuePlus;
    public Dictionary<STAT,float>  StatValueMultiply;
    public Dictionary<BUFF, float> BuffImmune;

    public Dictionary<BUFF, float> BuffValuePlus;
    public Dictionary<BUFF, float> BuffValueMultiply;

    public List<Buff> buffs;

    public class STATtypeComparer : IEqualityComparer<STAT>
    {
        public bool Equals(STAT x, STAT y)
        {
            return x == y;
        }

        public int GetHashCode(STAT obj)
        {
            return (int)obj;
        }
    }
    public class BUFFtypeComparer : IEqualityComparer<BUFF>
    {
        public bool Equals(BUFF x, BUFF y)
        {
            return x == y;
        }

        public int GetHashCode(BUFF obj)
        {
            return (int)obj;
        }
    }

    public StatusBase()
    {
        Stats = new Dictionary<STAT, float>(new STATtypeComparer());
        StatValuePlus = new Dictionary<STAT, float>(new STATtypeComparer());
        StatValueMultiply = new Dictionary<STAT, float>(new STATtypeComparer());

        foreach (var item in Enum.GetValues(typeof(STAT)))
        {
            Stats.Add((STAT)item, 0);
            StatValuePlus.Add((STAT)item, 0);
            StatValueMultiply.Add((STAT)item, 1);
        }

        BuffImmune = new Dictionary<BUFF, float>(new BUFFtypeComparer());

        BuffValuePlus = new Dictionary<BUFF, float>(new BUFFtypeComparer());
        BuffValueMultiply = new Dictionary<BUFF, float>(new BUFFtypeComparer());


        foreach (var item in Enum.GetValues(typeof(BUFF)))
        {
            BuffImmune.Add((BUFF)item, 0);
            BuffValuePlus.Add((BUFF)item, 0);
            BuffValueMultiply.Add((BUFF)item, 1);
        }
        buffs = new List<Buff>();
    }
    public void setCurrentStat(STAT s,float value) {
        CurrentStats[s] = value;
    }
    public float getCurrentStat(STAT s)
    {
     return CurrentStats[s];
    }
    public void setStat(STAT s, float value)
    {
        Stats[s] = value;
    }
    public float getStat(STAT s)
    {
        return Stats[s];
    }
    public void ChangeStat(STAT s, float value, bool Multiply = false)
    {//주의 : 곱연산은 +10퍼 일 시 1.1로 줄 것
        
        if(Multiply)
            StatValueMultiply[s] *= value;
        else
            StatValuePlus[s] += value;
        CurrentStats[s] = (Stats[s] + StatValuePlus[s]) * StatValueMultiply[s];
    }
    public void AddBuff(Buff buff,bool reFresh = true)
    {//TODO : 버프 시작, 진행중, 종료 시각효과 넣을 것
        if (BuffImmune[buff.buffName] == 1)
        {
            return;
        }
        if (getCurrentStat(STAT.hp) <= 0)
            return;
        if (reFresh)
        {
            var tempBuff = GetBuff(buff.buffName);
            if(tempBuff != null)
            {
                tempBuff.tempTime = Time.realtimeSinceStartup;
                tempBuff.totalTime = buff.totalTime;
                tempBuff.StartBuff();
            }
            else
            {
                AddBuff(buff, false);
            }
        }
        else
        {
            BuffValuePlus[buff.buffName] += 1;
            buff.tempTime = Time.realtimeSinceStartup;
            buff.StartBuff();
            buffs.Add(buff);
        }
    }
    public bool IsBuff(BUFF buff)
    {
        return BuffValuePlus[buff] >0 ;
    }
    public void UpdateBuff()
    {
        for (int i = buffs.Count - 1; i >= 0; i--)
        {
            if (buffs[i].RemainTime() > 0)
            {
                buffs[i].Update();
            }
            else if(!buffs[i].isOn)
            {
                EndBuff(buffs[i]);
            }
        }
    }
    public void ClearAllBuffs()
    {
        foreach (var item in buffs)
        {
            EndBuff(item);
        }
        buffs.Clear();
    }
    public Buff GetBuff(BUFF buff)
    {
        if (BuffValuePlus[buff] > 0)
        {
            foreach (var item in buffs)
            {
                if(item.buffName == buff)
                {
                    return item;
                }
            }
        }
        return null;
    }
    public Buff GetBuffs(BUFF buff)
    {
        /*필요시 구현*/
        return null;
    }
    public void EndBuff(BUFF buff)
    {
        EndBuff(GetBuff(buff));
    }
    public void EndBuff(Buff buff)
    {
        if (buff != null)
        {
            BuffValuePlus[buff.buffName] -= 1;
            buff.EndBuff(); 
            buffs.Remove(buff);
        }
    }

    public void init() {
        CurrentStats = Stats;
    }
}
public enum STAT { //전투용 스탯
    hp =  0,
    moveSpeed,
    AtkPoint,
    CriticalPoint,
    CriticalDamage,
    DefensePoint,






    NONE
}
public enum BUFF//아이콘, 시각표시 이펙트용 구분
{
    Electrified = 0,
    Pierced,
    Poisoned,
    Burn,
    Cloak,
    Bleeding,
    SuperArmor,
    Stuned,
    BatteryCharged,

}
public class Buff {//상속해서 사용, 필요없으면 안해도됨
    public BUFF buffName;//아이콘, 시각표시 이펙트용 구분
    public STAT ChangeSTAT = STAT.NONE;//바꿀 스탯(없다면 NONE)
    public float ChangeSize;//스탯 바꿀 크기
    //바꿀 스탯이 많으면 상속해서 변수 여러개 만들기
    public float totalTime;//지속시간(전체)
    public float tempTime;//시작시간
    public bool isOn;//시간과 관계없는 버프디버프인지(Off시 false)
    protected IHitable target;

    public Buff()
    {
    }
    public Buff(float time, BUFF buff,IHitable target,bool isCC = false,bool isOn = false)
    {
        buffName = buff;

        this.isOn = isOn;
        if (!isOn)
            totalTime = time;
        else
            totalTime = 0;
        if (isCC)
            target.TakeCC();
        SetTarget(target);
    }
    public void SetTarget(IHitable target)
    {
        this.target = target;
    }
    public Effector showEffect(GameObject o)
    {
        //!TODO
        //각각에서 호출 하지말고 bool로 받아서 호출된다거나..
        //파일명들을 문자열 배열로 캐싱..
        var e =GameObject.Instantiate(o, target.transform).GetComponent<Effector>();
        e.transform.position = target.transform.position;
        return e;
    }
    public virtual void StartBuff()
    {
        target.status.ChangeStat(ChangeSTAT, ChangeSize);
    }
    public virtual void Update() { 
        
    }
    public virtual void EndBuff() {
        target.status.ChangeStat(ChangeSTAT, -ChangeSize);
    }
    public float RemainTime() {
        return tempTime + totalTime - Time.realtimeSinceStartup;
    }
}
public class Bleeding : Buff
{
    public float Damage;
    float dealTime;
    float dealedDamage;
    public Bleeding(float time,float Dmg, IHitable target)
    {
        buffName = BUFF.Bleeding;
        totalTime = time;
        Damage = Dmg;
        dealTime = 0;
        dealedDamage = 0;
        SetTarget(target);
    }
    public override void StartBuff()
    {
        AttackManager.Instance.SimpleDamage(Damage / totalTime, target); 
        dealedDamage += Damage / totalTime;
    }
    public override void Update() {
        if (dealedDamage >= Damage)
            return;
        dealTime += Time.deltaTime;
        if (dealTime >= 1)
        {
            dealTime -= 1;
            AttackManager.Instance.SimpleDamage(Damage / totalTime, target);
            dealedDamage += Damage / totalTime;
        }
            
    }
}

public class Poisoned : Buff
{
    public float Damage;
    float dealTime;
    float dealedDamage;
    public Poisoned(float time, float Dmg, IHitable target)
    {
        buffName = BUFF.Bleeding;
        totalTime = time;
        Damage = Dmg;
        dealTime = 0;
        dealedDamage = 0;
        SetTarget(target);
    }
    public override void StartBuff()
    {
        AttackManager.Instance.SimpleDamage(Damage / totalTime, target);
        dealedDamage += Damage / totalTime;
    }
    public override void Update()
    {
        if (dealedDamage >= Damage)
            return;
        dealTime += Time.deltaTime;
        if (dealTime >= 1)
        {
            dealTime -= 1;
            AttackManager.Instance.SimpleDamage(Damage / totalTime, target);
            dealedDamage += Damage / totalTime;
        }

    }
}
public class Burn : Buff
{
    public float Damage;
    float dealTime;
    float dealedDamage;
    public Burn(float time, float Dmg, IHitable target)
    {
        buffName = BUFF.Burn;
        totalTime = time;
        Damage = Dmg;
        dealTime = 0;
        dealedDamage = 0;
        SetTarget(target);
    }
    public override void StartBuff()
    {
        AttackManager.Instance.SimpleDamage(Damage / totalTime, target);
        dealedDamage += Damage / totalTime;
    }
    public override void Update()
    {
        if (dealedDamage >= Damage)
            return;
        dealTime += Time.deltaTime;
        if (dealTime >= 1)
        {
            dealTime -= 1;
            AttackManager.Instance.SimpleDamage(Damage / totalTime, target);
            dealedDamage += Damage / totalTime;
        }

    }
}

public class Electrified : Buff
{
    public Electrified(float time, IHitable target)
    {
        buffName = BUFF.Electrified;
        totalTime = time;
        SetTarget(target);
    }
    public override void StartBuff()
    {
        target.TakeCC();
        showEffect(Resources.Load<GameObject>("BuffEffect/Electrified"));
    }
    public override void Update()
    {
        // 행동중인 동작 캔슬?
    }

    public override void EndBuff()
    {

    }
}
public class Pierced : Buff
{
    public Pierced(float time, IHitable target)
    {
        buffName = BUFF.Pierced;
        totalTime = time;
        SetTarget(target);
    }
    public override void StartBuff()
    {
        showEffect(Resources.Load<GameObject>("BuffEffect/Pierced"))
       .Alpha(1f, 0.1f).And().Disable(1f, true).Play();
    }
    public override void Update()
    {
    }

    public override void EndBuff()
    {

    }

}
public class BatteryCharged : Buff
{
    public BatteryCharged(float time, IHitable target)
    {
        buffName = BUFF.BatteryCharged;
        totalTime = time;
        SetTarget(target);
    }
    public override void StartBuff()
    {
        showEffect(Resources.Load<GameObject>("BuffEffect/BatteryCharged"))
       .Alpha(0.5f, 0.3f).And().Disable(0.5f, true).Play();
    }
    public override void Update()
    {

    }

    public override void EndBuff()
    {

    }
}