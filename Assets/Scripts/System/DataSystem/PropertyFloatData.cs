using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

using Line = System.Collections.Generic.Dictionary<string, float>;


/*
    PropertyFloatData : 속성목록 형식의 csv 파일 (float 형식)
    예)
    AAA propertyA1 num propertyA2 num
    BBB propertyB1 num
    CCC propertyC1 num propertyC2 num propertyC3 num
*/
public class PropertyFloatData : FileData
{
    Dictionary<string, Line> datas;

    public PropertyFloatData(string filePath) : base(filePath)
    {
        datas = new Dictionary<string, Line>();
    }

    public override void Load()
    {
        string rawText = ReadFile(filePath);
        string[] lines = SplitLines(rawText);
        for(int i=0; i<lines.Length; i++)
        {
            string[] words = lines[i].Split('\t');
            datas.Add(words[0],new Line());
            for(int j=2; j<words.Length; j+=2)
            {   
                datas[words[0]].Add(words[j-1], float.Parse(words[j]));
            }
        }

        isLoaded = true;
    }

    public Dictionary<string,float> Get(string itemName)
    {
#if UNITY_EDITOR
        if(!isLoaded) Debug.LogWarning(filePath + " is used before Init().");
#endif
        return datas[itemName];
    }
    public float Get(string itemName, string fieldName)
    {
        return Get(itemName)[fieldName];
    }

    public Dictionary<string,float> this[string itemName]
    {
        get{
            return Get(itemName);
        }
    }
    public float this[string itemName, string fieldName]
    {
        get{
            return Get(itemName, fieldName);
        }
    }

}
