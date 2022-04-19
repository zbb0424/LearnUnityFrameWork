﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniJSON;
using System.Text;

public class MetaDataManager
{
    public const string directoryName = "Meta";
    public static Dictionary<string,object> GetData(string ConfigName)
    {
        string dataJson = "";

        dataJson = ResourceIOTool.ReadStringByFile(GetPath(ConfigName));

        if (dataJson == "")
        {
            return null;
        }
        else
        {
            return Json.Deserialize(dataJson) as Dictionary<string, object>;
        }
    }

    public static void SaveData(string ConfigName,Dictionary<string,object> data)
    {
        ResourceIOTool.WriteStringByFile(GetPath(ConfigName), Json.Serialize(data));
    }

    static string GetPath(string ConfigName)
    {
        StringBuilder builder = new StringBuilder();

#if UNITY_EDITOR
        builder.Append(Application.dataPath);
        builder.Append("/Resources/");
#else
        builder.Append(Application.persistentDataPath);
#endif

        Application.temporaryCachePath;

        builder.Append(directoryName);
        builder.Append("/");
        builder.Append(ConfigName);
        builder.Append(".json");

        return builder.ToString();
    }
}
