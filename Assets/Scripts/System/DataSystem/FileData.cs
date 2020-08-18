using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;

public abstract class FileData
{
    protected string filePath;
    protected bool isLoaded;

    protected FileData(string filePath)
    {
        this.filePath = filePath;
        isLoaded = false;
    }

    public abstract void Load();

    
    protected static string ReadFile(string filePath)
    {
        string path = Application.dataPath + "/" + filePath;
#if UNITY_EDITOR
        if (!File.Exists(path)) Debug.LogWarning("Can't find " + path);
#endif
        string data = File.ReadAllText(path, System.Text.Encoding.UTF8);
        return data;
    }
    
    static string LINE_SPLIT = @"\r\n|\n\r|\n|\r";

    protected static string[] SplitLines(string text)
    {
        return Regex.Split(text, LINE_SPLIT);
    }
    
}
