using UnityEngine;
using System.Collections;
using System.IO;

public class FileTool  
{
    /// <summary>
    /// 判断有没有这个文件路径，如果没有则创建它
    /// </summary>
    /// <param name="filepath"></param>
    public static void CreatFilePath(string filepath)
    {
        string newPathDir = Path.GetDirectoryName(filepath);

        if (!Directory.Exists(newPathDir))
            Directory.CreateDirectory(newPathDir);
    }

    /// <summary>
    /// 删除某个目录下的所有子目录和子文件，但是保留这个目录
    /// </summary>
    /// <param name="path"></param>
    static void DeleteDirectory(string path)
    {
        string[] directorys = Directory.GetDirectories(path);

        for (int i = 0; i < directorys.Length; i++)
        {
            string pathTmp = directorys[i];

            if (Directory.Exists(pathTmp))
            {
                Directory.Delete(pathTmp, true);
            }
        }

        string[] files = Directory.GetFiles(path);

        for (int i = 0; i < files.Length; i++)
        {
            string pathTmp = files[i];

            if (File.Exists(pathTmp))
            {
                File.Delete(pathTmp);
            }
        }
    }
}
