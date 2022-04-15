﻿using UnityEngine;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Collections;
using System.Collections.Generic;

public class MD5Tool
{
    public static string GetFileMD5(string filePath)
    {
        try
        {
            FileInfo fileTmp = new FileInfo(filePath);
            if (fileTmp.Exists)
            {
                FileStream fs = new FileStream(filePath, FileMode.Open);
                int len = (int)fs.Length;
                byte[] data = new byte[len];
                fs.Close();

                return GetMD5(data);
            }
            return "";
        }
        catch (FileNotFoundException e)
        {

            Debug.Log(e.Message);
            return "";
        }
    }

    public static string GetMD5(byte[] data)
    {
        MD5 md5= new MD5CryptoServiceProvider();
        byte[] result = md5.ComputeHash(data);
        string fileMD5 = "";
        foreach (byte item in result)
        {
            fileMD5 += Convert.ToString(item, 16);
        }
        if (!String.IsNullOrEmpty(fileMD5))
        {
            return fileMD5;
        }

        return "";
    }
}
