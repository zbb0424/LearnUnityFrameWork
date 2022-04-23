﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class UISystemEvent : MonoBehaviour 
{
    public static UISystemEvent s_instance;

    public static Dictionary<UIEvent, UICallBack> m_allUIEvents = new Dictionary<UIEvent, UICallBack>();
    public static Dictionary<string, Dictionary<UIEvent, UICallBack>> m_singleUIEvents = new Dictionary<string, Dictionary<UIEvent, UICallBack>>();

    public void Awake()
    {
        s_instance = this;
    }

    /// <summary>
    /// 每个UI都会派发的事件
    /// </summary>
    /// <param name="Event">事件类型</param>
    /// <param name="callback">回调函数</param>
    public static void RegisterAllUIEvent(UIEvent l_UIEvent, UICallBack l_CallBack)
    {
        if (m_allUIEvents.ContainsKey(l_UIEvent))
        {
            m_allUIEvents[l_UIEvent] += l_CallBack;
        }
        else
        {
            m_allUIEvents.Add(l_UIEvent,l_CallBack);
        }
    }

    /// <summary>
    /// 注册单个UI派发的事件
    /// </summary>
    /// <param name="Event">事件类型</param>
    /// <param name="callback"回调函数></param>
    public static void RegisterEvent(string l_UIName,UIEvent l_UIEvent, UICallBack l_CallBack)
    {
        if (m_singleUIEvents.ContainsKey(l_UIName))
        {
            if (m_singleUIEvents[l_UIName].ContainsKey(l_UIEvent))
            {
                m_singleUIEvents[l_UIName][l_UIEvent] += l_CallBack;
            }
            else
            {
                m_singleUIEvents[l_UIName].Add(l_UIEvent,l_CallBack);
            }
        }
        else
        {
            m_singleUIEvents.Add(l_UIName,new Dictionary<UIEvent,UICallBack>());
            m_singleUIEvents[l_UIName].Add(l_UIEvent, l_CallBack);
        }
    }

    public static void Dispatch(UIWindowBase l_UI, UIEvent l_UIEvent,params object[] l_objs)
    {
        if (m_allUIEvents.ContainsKey(l_UIEvent))
        {
            try
            {
                m_allUIEvents[l_UIEvent](l_UI, l_objs);
            }
            catch(Exception e)
            {
                Debug.LogError("UISystemEvent Dispatch error:" + e.ToString());
            }
        }

        if (m_singleUIEvents.ContainsKey(l_UI.name))
        {
            if (m_singleUIEvents[l_UI.name].ContainsKey(l_UIEvent))
            {
                try
                {
                    m_singleUIEvents[l_UI.name][l_UIEvent](l_UI, l_objs);
                }
                catch (Exception e)
                {
                    Debug.LogError("UISystemEvent Dispatch error:" + e.ToString());
                }
            }
        }
    }


	// Use this for initialization
	void Start () 
    {
	
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}
}
