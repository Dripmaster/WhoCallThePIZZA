using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mushrooms : MapObject
{
    bool isUsed = false;

    public override void OnOtherEnter(Collider2D collision)
    {    }

    public override void OnOtherExit(Collider2D collision)
    {    }

    public override void OnPlayerEnter()
    {    }

    public override void OnPlayerExit()
    {    }

    public override void TakeCC(int CCnum = 0)
    {    }

    public override void TakeKnockBack(float force, Vector2 knockBackDir)
    {    }

    public override void DoTake()
    {
        base.DoTake();
        
        if (!isUsed)
        {
            WeaponBase.instance.player.status.AddBuff(new Bleeding(5, 3, WeaponBase.instance.player));

            hitableState = HitableState.take;
            DoDestroy();
        }
        isUsed = true;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
        
    }
}
