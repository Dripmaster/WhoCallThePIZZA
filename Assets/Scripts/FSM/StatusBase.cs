using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class StatusBase
{
    public float[] CurrentStats;
    public float[] Stats;
    public float[] StatValuePlus;
    public float[] StatValueMultiply;
    public List<Buff> buffs;
    public StatusBase()
    {
        Stats = new float[Enum.GetValues(typeof(STAT)).Length];
        StatValuePlus = new float[Enum.GetValues(typeof(STAT)).Length+
            Enum.GetValues(typeof(BUFF)).Length];
        StatValueMultiply = new float[Enum.GetValues(typeof(STAT)).Length +
            Enum.GetValues(typeof(BUFF)).Length];
        for (int i = 0; i < StatValueMultiply.Length; i++)
        {
            StatValueMultiply[i] = 1;
        }
        buffs = new List<Buff>();
    }
    public void setCurrentStat(STAT s,float value) {
        CurrentStats[(int)s] = value;
    }
    public float getCurrentStat(STAT s)
    {
     return CurrentStats[(int)s];
    }
    public void setStat(STAT s, float value)
    {
        Stats[(int)s] = value;
    }
    public float getStat(STAT s)
    {
        return Stats[(int)s];
    }
    public void ChangeStat(STAT s, float value, bool Multiply = false)
    {//주의 : 곱연산은 +10퍼 일 시 1.1로 줄 것
        
        if(Multiply)
            StatValueMultiply[(int)s] *= value;
        else
            StatValuePlus[(int)s] += value;
        CurrentStats[(int)s] = (Stats[(int)s] + StatValuePlus[(int)s]) * StatValueMultiply[(int)s];
    }
    public void AddBuff(Buff buff)
    {//TODO : 버프 시작, 진행중, 종료 시각효과 넣을 것
        if (getCurrentStat(STAT.hp) <= 0)
            return;
        StatValuePlus[(int)buff.buffName + Stats.Length] += 1;
        buff.tempTime = Time.realtimeSinceStartup;
        buff.StartBuff();
        buffs.Add(buff);
    }
    public bool IsBuff(BUFF buff)
    {
        return StatValuePlus[(int)buff+Stats.Length] >0 ;
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
                buffs.Remove(buffs[i]);
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
    public void EndBuff(Buff buff)
    {
        StatValuePlus[(int)buff.buffName + Stats.Length] -= 1;
        buff.EndBuff();
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

}
public class Buff {//상속해서 사용, 필요없으면 안해도됨
    public BUFF buffName;//아이콘, 시각표시 이펙트용 구분
    public STAT ChangeSTAT = STAT.NONE;//바꿀 스탯(없다면 NONE)
    public float ChangeSize;//스탯 바꿀 크기
    //바꿀 스탯이 많으면 상속해서 변수 여러개 만들기
    public float totalTime;//지속시간(전체)
    public float tempTime;//시작시간
    public bool isOn;//시간과 관계없는 버프디버프인지(Off시 false)
    protected FSMbase target;

    public Buff()
    {
    }
    public Buff(float time, BUFF buff,FSMbase target,bool isCC = false,bool isOn = false)
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
    public void SetTarget(FSMbase target)
    {
        this.target = target;
    }
    public Effector showEffect(GameObject o)
    {
        //!TODO
        //각각에서 호출 하지말고 bool로 받아서 호출된다거나..
        //파일명들을 문자열 배열로 캐싱..
        var e =GameObject.Instantiate(o, target.transform.parent).GetComponent<Effector>();
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
    public Bleeding(float time,float Dmg, FSMbase target)
    {
        buffName = BUFF.Bleeding;
        totalTime = time;
        Damage = Dmg;
        dealTime = 0;
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

public class Burn : Buff
{
    public float Damage;
    float dealTime;
    float dealedDamage;
    public Burn(float time, float Dmg, FSMbase target)
    {
        buffName = BUFF.Burn;
        totalTime = time;
        Damage = Dmg;
        dealTime = 0;
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
    public Electrified(float time, FSMbase target)
    {
        buffName = BUFF.Electrified;
        totalTime = time;
        SetTarget(target);
    }
    public override void StartBuff()
    {
        target.TakeCC();
        showEffect(Resources.Load<GameObject>("BuffEffect/Electrified"))
       .Alpha(0.5f, 0.3f).And().Disable(0.5f,true).Play();
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
    public Pierced(float time, FSMbase target)
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