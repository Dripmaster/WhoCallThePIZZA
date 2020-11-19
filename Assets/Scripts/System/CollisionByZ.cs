using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CollisionByZ
{
   public static bool Zcheck(ZSystem one, ZSystem two )
    {
        if(one!=null && two != null)
        {
            if(one.Z == two.Z)
            {
                return true;
            }
        }
        return false;
    }
}
