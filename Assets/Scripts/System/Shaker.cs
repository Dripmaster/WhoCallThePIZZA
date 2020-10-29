using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shaker : MonoBehaviour
{
    //lamda expressions :
    //https://docs.microsoft.com/ko-kr/dotnet/csharp/programming-guide/statements-expressions-operators/lambda-expressions

    public delegate float ValueAtNormalizedTime(float time);

    public Transform transformToShake;
    
    float duration;
    ValueAtNormalizedTime MagnitudeAt;
    ValueAtNormalizedTime SpeedAt;
    float magnitude;
    float speed;    //# of shakes in 1 sec
    bool isLoop;
    
    bool isShaking;
    float totalETime;
    Vector3 originalPosition;

    IEnumerator shakeCoroutine;
    
    public void StartShake(float duration, float magnitude, float speed, bool loop = false)
    {
        Reset();
        SetShake(duration, magnitude, speed, loop);
        Shake();
    }
    public void StartShake(float duration, ValueAtNormalizedTime MagnitudeAt, float speed, bool loop = false)
    {
        Reset();
        SetShake(duration, -1, speed, loop);
        this.MagnitudeAt = MagnitudeAt;
        Shake();
    }
    public void StartShake(float duration, ValueAtNormalizedTime MagnitudeAt, ValueAtNormalizedTime SpeedAt, bool loop = false)
    {
        Reset();
        SetShake(duration, -1, -1, loop);
        this.MagnitudeAt = MagnitudeAt;
        this.SpeedAt = SpeedAt;
        Shake();
    }
    public void StopShake(float fadeOutDuration = 0f)
    {
        if(isShaking)
        {
            if(fadeOutDuration == 0f)
            {
                if(shakeCoroutine != null)
                    StopCoroutine(shakeCoroutine);
                Reset();
            }
            else
            {
                isLoop = false;
                if(magnitude == -1)
                    magnitude = MagnitudeAt(totalETime / duration);
                if(speed == -1)
                    speed = SpeedAt(totalETime / duration);
                totalETime = 0f;
                duration = fadeOutDuration;
                float m = magnitude;
                float s = speed;
                MagnitudeAt = (t)=>{return m * (1-t);};
                SpeedAt = (t)=>{return s * (1-t);};
                magnitude = -1;
                speed = -1;
            }
        }
    }
    void SetShake(float duration, float magnitude, float speed, bool loop)
    {
        originalPosition = transformToShake.localPosition;
        this.duration = duration;
        this.magnitude = magnitude;
        this.speed = speed;
        this.isLoop = loop;
        isShaking = true;
    }
    void Awake()
    {
        originalPosition = transformToShake.localPosition;
        Reset(true);
    }
    void Reset(bool resetFunctions = false)
    {
        duration = 0f;
        if(resetFunctions)
        {
            MagnitudeAt = (t) => { return t; };
            SpeedAt = (t) => { return t; };
        }
        magnitude = -1f;
        speed = -1f;
        isLoop = false;
        totalETime = 0f;
        isShaking = false;
        transformToShake.localPosition = originalPosition;
    }
    void Shake()
    {
        if(shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);
        shakeCoroutine = ShakeCoroutine();
        StartCoroutine(shakeCoroutine);
    }
    IEnumerator ShakeCoroutine()
    {
        float eTime = 0f;
        float nextShakeTime = 0f;
        while(true)
        {
            if(eTime >= nextShakeTime)
            {
                Vector3 shakeVec = new Vector3(Random.Range(-1f,1f),Random.Range(-1f,1f), 0f);
                if(magnitude == -1f)
                    shakeVec *= MagnitudeAt(totalETime/duration);
                else
                    shakeVec *= magnitude;
                transformToShake.localPosition = originalPosition + shakeVec;
                eTime -= nextShakeTime;
                if(speed == -1f)
                    nextShakeTime = 1f / SpeedAt(totalETime/duration);
                else
                    nextShakeTime = 1f / speed;
            }

            totalETime += Time.deltaTime;
            eTime += Time.deltaTime;

            if(totalETime >= duration)
            {
                if(isLoop)
                    totalETime -= duration;
                else
                {
                    Reset();
                    break;
                }
            }
            yield return null;
        }
    }
    void OnDisable()
    {
        Reset();
    }


}
