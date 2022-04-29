﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ApplicationStatusManager 
{
    /// <summary>
    /// 当前程序在哪个状态
    /// </summary>
    public static IApplicationStatus s_currentAppStatus;
    static Dictionary<string,IApplicationStatus> s_status = new Dictionary<string,IApplicationStatus>();

    public static void Init()
    {
        ApplicationManager.s_OnApplicationUpdate += AppUpdate;
    }

    /// <summary>
    /// 切换游戏状态
    /// </summary>
    /// <param name="l_appStatus"></param>
    public static void EnterStatus<T>() where T:IApplicationStatus
    {
        EnterStatus(typeof(T).Name);
    }

    public static void EnterStatus(string l_statusName)
    {
        if (s_currentAppStatus != null)
        {
            s_currentAppStatus.OnExitStatus();
        }

        s_currentAppStatus = GetAppStatus(l_statusName);
        s_currentAppStatus.OnEnterStatus();
    }

    public static IApplicationStatus GetAppStatus<T>() where T:IApplicationStatus
    {
        return GetAppStatus(typeof(T).Name);
    }

    public static IApplicationStatus GetAppStatus(string l_statusName)
    {
        if (s_status.ContainsKey(l_statusName))
        {
            return s_status[l_statusName];
        }
        else
        {
            IApplicationStatus l_statusTmp = (IApplicationStatus)Activator.CreateInstance(Type.GetType(l_statusName));
            s_status.Add(l_statusName, l_statusTmp);

            return l_statusTmp;
        }
    }

    /// <summary>
    /// 应用程序每帧调用
    /// </summary>
    public static void AppUpdate()
    {
        if(s_currentAppStatus != null)
        {
            s_currentAppStatus.OnUpdate();
        }
    }

    /// <summary>
    /// 测试模式，流程入口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static void EnterTestModel<T>() where T : IApplicationStatus
    {
        EnterTestModel(typeof(T).Name);
    }

    public static void EnterTestModel(string l_statusName)
    {
        if (s_currentAppStatus != null)
        {
            s_currentAppStatus.OnExitStatus();
        }

        s_currentAppStatus = GetAppStatus(l_statusName);
        s_currentAppStatus.EnterStatusTestData();
        s_currentAppStatus.OnEnterStatus();
    }
}
