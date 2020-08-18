using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    TableData : 표 형식의 csv 파일
    예)
    id     PropertyA   PropertyB   PropertyC
    id1    valueA1     valueB1     valueC1
    id2    valueA2     valueB2     valueC2 
*/
public class TableData : FileData
{
    Dictionary<string, string[]> datas;
    Dictionary<string, int> fields;

    public TableData(string filePath) : base(filePath)
    {
        datas = new Dictionary<string, string[]>();
        fields = new Dictionary<string, int>();
    }

    public override void Load()
    {
        string rawText = FileData.ReadFile(filePath);
        string[] lines = FileData.SplitLines(rawText);

        for(int i=0; i<lines.Length; i++)
        {
            string[] words = lines[i].Split('\t');
            if(i != 0)
                datas.Add(words[0],words);
            else
            {
                datas.Add("fields",words);
                for(int j=0; j<words.Length; j++)
                    fields.Add(words[j], j);
            }
        }

        isLoaded = true;
    }
    public Dictionary<string,string> Get(string itemName)
    {
#if UNITY_EDITOR
        if(!isLoaded) Debug.LogWarning(filePath + " is used before Init().");
#endif

        Dictionary<string,string> result = new Dictionary<string,string>();
        string[] fields = datas["fields"];
        string[] item = datas[itemName];
        for(int i=0; i<fields.Length; i++)
            result.Add(fields[i], item[i]);
        return result;
    }
    public string Get(string itemName, string field)
    {
#if UNITY_EDITOR
        if(!isLoaded) Debug.LogWarning(filePath + " is used before Init().");
#endif
        return datas[itemName][fields[field]];
    }

    public Dictionary<string,string> this[string itemName]
    {
        get{
            return Get(itemName);
        }
    }
    public string this[string itemName, string fieldName]
    {
        get{
            return Get(itemName,fieldName);
        }
    }
}
