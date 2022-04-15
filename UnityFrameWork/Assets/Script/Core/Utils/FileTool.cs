using UnityEngine;
using System.IO;
using System.Collections;

public class FileTool
{
    public static void CreateFilePath(string filePath)
    {
        string newPathDir = Path.GetDirectoryName(filePath);

        if (!Directory.Exists(newPathDir))
            Directory.CreateDirectory(newPathDir);
    }
}
