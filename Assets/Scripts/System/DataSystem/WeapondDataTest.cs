using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeapondDataTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        PropertyFloatData weapondData = new PropertyFloatData("weapondData.csv");
        weapondData.Load();

        Dictionary<string,float> data = weapondData.Get("LightningGauntlet");
        print(data["damage"]);
        print(weapondData.Get("DancingArrow")["coolTime"]);


        TableData tableData = new TableData("test.txt");
        tableData.Load();

        print(tableData.Get("fields")["def"]);
        print(tableData.Get("bos1","id"));
        print(tableData["bos1"]["id"]);


        DictionaryData dicData = new DictionaryData("test.txt");
        dicData.Load();

        print(dicData.Get("bos1"));
        print(dicData["bos1"]);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
