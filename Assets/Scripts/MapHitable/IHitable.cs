using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IHitable : MonoBehaviour
{
    [HideInInspector]
    public StatusBase status;

    [HideInInspector]
    public bool showEffect = true;
    public abstract void TakeAttack(float dmg, bool cancelAttack);
    public abstract void TakeKnockBack(float force, Vector2 knockBackDir);
    public abstract void TakeCC(int CCnum = 0);
    public abstract void initData();
}
