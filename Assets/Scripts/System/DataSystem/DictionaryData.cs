using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    LinedTextData : 사전 형식의 텍스트로 구성된 파일
    예)
    AAA PropertyA
    BBB PropertyB
    CCC PropertyC
*/

public class DictionaryData : FileData
{
    Dictionary<string, string> datas;

    public DictionaryData(string filePath) : base(filePath)
    {
        datas = new Dictionary<string, string>();
    }

    public override void Load()
    {
        string rawText = ReadFile(filePath);
        string[] lines = SplitLines(rawText);
        for(int i=0; i<lines.Length; i++)
        {
            string[] words = lines[i].Split('\t');
            datas.Add(words[0],words[1]);
        }

        isLoaded = true;
    }

    public string Get(string itemName)
    {
        return Get(itemName);
    }
    public string this[string itemName]
    {
        get{
            return Get(itemName);
        }
    }

}
