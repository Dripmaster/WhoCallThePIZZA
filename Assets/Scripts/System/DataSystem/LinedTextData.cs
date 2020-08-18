using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    LinedTextData : 여러줄의 텍스트로 구성된 파일
    예)
    AAA
    BBB
    CCC
*/
public class LinedTextData : FileData
{
    List<string> datas;

    public LinedTextData(string filePath) : base(filePath)
    {
        datas = new List<string>();
    }

    public override void Load()
    {
        string rawText = FileData.ReadFile(filePath);
        string[] lines = FileData.SplitLines(rawText);

        datas.AddRange(lines);

        isLoaded = true;
    }
    public string Get(int index)
    {
#if UNITY_EDITOR
        if(!isLoaded) Debug.LogWarning(filePath + " is used before Init().");
#endif
        return datas[index];
    }
    public string this[int index]
    {
        get{
            return Get(index);
        }
    }
}