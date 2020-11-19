using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSwapSys : MonoBehaviour
{

    public float swapTimeBase;
    float swapTime;
    float useTime = 0;
    public Image attackBar;
    float attackAmount;

    // Start is called before the first frame update
    void Awake()
    {
        swapTime = swapTimeBase;
    }
    private void OnEnable()
    {
        StartCoroutine(lerpAttackbar());
    }
    IEnumerator lerpAttackbar()
    {

        do
        {
            if (attackBar.fillAmount - attackAmount < 0.01f)
            {
                attackBar.fillAmount = Mathf.Lerp(attackBar.fillAmount, attackAmount, Time.deltaTime * 5);
            }
            else
            {
                attackBar.fillAmount = attackAmount;

            }
            yield return null;
        } while (gameObject.activeInHierarchy);
    }
    // Update is called once per frame
    void Update()
    {
        useTime += Time.deltaTime;
        if(useTime>=swapTime)
        {
            useTime = 0;
            WeaponBase.instance.WeaponSwap();
            attackAmount = 0;
        }
        else
        {
            attackAmount = useTime/swapTime;
        }
    }
}
