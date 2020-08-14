using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class StatusBase
{
    public float[] CurrentStats;
    public float[] Stats;
    public float[] StatValue;
    public List<Buff> buffs;
    public StatusBase()
    {
        Stats = new float[Enum.GetValues(typeof(STAT)).Length];
        StatValue = new float[Enum.GetValues(typeof(STAT)).Length+
            Enum.GetValues(typeof(BUFF)).Length];
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
    public void ChangeStat(STAT s, float value)
    {
        StatValue[(int)s] += value;
    }
    public void AddBuff(Buff buff)
    {
        StatValue[(int)buff.buffName + Stats.Length] += 1;
        buff.StartBuff();
        buff.tempTime = Time.realtimeSinceStartup;
        buffs.Add(buff);
    }
    public bool IsBuff(BUFF buff)
    {
        return StatValue[(int)buff+Stats.Length] >0 ;
    }
    public void UpdateBuff()
    {
        for (int i = buffs.Count - 1; i >= 0; i--)
        {
            if (buffs[i].RemainTime() > 0)
            {
                buffs[i].Update();
            }
            else
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
        StatValue[(int)buff.buffName + Stats.Length] -= 1;
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
    Bleeding,
    SuperArmor
}
public class Buff {//상속해서 사용, 필요없으면 안해도됨
    public BUFF buffName;//아이콘, 시각표시 이펙트용 구분
    public STAT ChangeSTAT = STAT.NONE;//바꿀 스탯(없다면 NONE)
    public float ChangeSize;//스탯 바꿀 크기
    //바꿀 스탯이 많으면 상속해서 변수 여러개 만들기
    public float totalTime;//지속시간(전체)
    public float tempTime;//시작시간
    protected FSMbase target; 
    public void SetTarget(FSMbase target)
    {
        this.target = target;
    }
    public virtual void StartBuff()
    {
        target.status.ChangeStat(ChangeSTAT, ChangeSize);
    }
    public virtual void Update() { 
        
    }
    public void EndBuff() {
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
        target.status.ChangeStat(ChangeSTAT, ChangeSize);
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