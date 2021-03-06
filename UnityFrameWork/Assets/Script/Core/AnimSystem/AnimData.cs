using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

public class AnimData
{
    #region 参数

    //基本变量
    public GameObject m_animGameObejct;
    public AnimType m_animType;
    public InteType m_interpolationType = InteType.Default ;
    public PathType m_pathType = PathType.Line;
    public RepeatType m_playType = RepeatType.Once;

    //进度控制变量
    public bool m_isDone = false;
    public float m_currentTime = 0;
    public float m_totalTime = 0;

    //V3
    public Vector3 m_fromV3;
    public Vector3 m_toV3;

    //V2
    public Vector2 m_fromV2;
    public Vector2 m_toV2;

    //Float
    public float m_fromFloat = 0;
    public float m_toFloat = 0;
    
    //Color
    public Color m_fromColor;
    public Color m_toColor;

    //动画回调
    public object[] m_parameter;
    public AnimCallBack m_callBack;
    
    //其他设置
    public bool m_isChild = false;
    public bool m_isLocal = false;

    //控制点
    public Vector3[] m_v3Contral = null; //二阶取第一个用，三阶取前两个
    public float[] m_floatContral = null;

    //自定义函数
    public AnimCustomMethodVector3 m_customMethodV3;
    public AnimCustomMethodVector2 m_customMethodV2;
    public AnimCustomMethodFloat m_customMethodFloat;

    //缓存变量
    RectTransform m_rectRransform;
    Transform m_transform;

    #endregion

    #region 核心函数

    public void executeUpdate()
    {
        m_currentTime += Time.deltaTime;

        if (m_currentTime > m_totalTime)
        {
            m_currentTime = m_totalTime;
            m_isDone = true;
        }

        switch (m_animType)
        {
            case AnimType.UGUI_Alpha: UguiAlpha(); break;
            case AnimType.UGUI_AnchoredPosition: UguiPosition(); break;
            case AnimType.UGUI_SizeDetal: SizeDelta(); break;


            case AnimType.Position: Position(); break;
            case AnimType.LocalPosition: LocalPosition(); break;
            case AnimType.LocalScale: LocalScale(); break;


            case AnimType.Custom_Vector3: CustomMethodVector3(); break;
            case AnimType.Custom_Vector2: CustomMethodVector2(); break;
            case AnimType.Custom_Float:   CustomMethodFloat(); break;
        }
    }

    //动画播放完毕执行回调
    public void executeCallBack()
    {
        try
        {
            if (m_callBack != null)
            {
                m_callBack(m_parameter);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    //动画循环逻辑
    public bool AnimReplayLogic()
    {
        switch (m_playType)
        {
            case RepeatType.Once:
                return false;

            case RepeatType.Loop:
                m_isDone = false;
                m_currentTime = 0;
                return true;

            case RepeatType.PingPang:

                ExchangeV2();
                ExchangeAlpha();
                ExchangePos();
                m_isDone = false;
                m_currentTime = 0;
                return true;
        }

        return false;
    }

    #region 循环逻辑

    public void ExchangeV2()
    {
        Vector2 Vtmp = m_fromV2;
        m_fromV2 = m_toV2;
        m_toV2 = Vtmp;

    }
    public void ExchangePos()
    {

        Vector3 Vtmp = m_fromV3;
        m_fromV3 = m_toV3;
        m_toV3 = Vtmp;

    }
    public void ExchangeAlpha()
    {
        float alphaTmp = m_fromFloat;
        m_fromFloat = m_toFloat;
        m_toFloat = alphaTmp;
    }

    #endregion

    #endregion

    #region 初始化

    public void Init()
    {
        switch (m_animType)
        {
            case AnimType.UGUI_Alpha: UguiAlphaInit(m_isChild); break;
            case AnimType.UGUI_AnchoredPosition: UguiPositionInit(); break;
            case AnimType.UGUI_SizeDetal: UguiPositionInit(); break;
            case AnimType.Position: TransfromInit(); break;
            case AnimType.LocalPosition: TransfromInit(); break;
            case AnimType.LocalScale: TransfromInit(); break;
        }

        if (m_pathType != PathType.Line)
        {
            BezierInit();
        }
    }

    #endregion

    #region CustomMethod

    public void CustomMethodFloat()
    {
        try
        {
            m_customMethodFloat(GetInterpolation(m_fromFloat, m_toFloat));
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    public void CustomMethodVector2()
    {
        try
        {
            m_customMethodV2(GetInterpolationV3(m_fromV2, m_toV2));
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    public void CustomMethodVector3()
    {
        try
        {
            m_customMethodV3(GetInterpolationV3(m_fromV3, m_toV3));
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    #endregion

    #region 贝塞尔曲线

    /// <summary>
    /// 贝塞尔初始化（根据随机范围进行控制点随机）
    /// </summary>
    public void BezierInit()
    {
        if (m_v3Contral == null)
        {
            if (m_floatContral != null)
            {
                m_v3Contral = new Vector3[3];
                m_v3Contral[0] = UnityEngine.Random.insideUnitSphere * m_floatContral[0] + m_fromV3;
                m_v3Contral[1] = UnityEngine.Random.insideUnitSphere * m_floatContral[1] + m_toV3;

                //二阶贝塞尔，控制点以起点与终点的中间为随机中心
                if (m_pathType == PathType.Bezier2)
                {
                    m_v3Contral[0] = UnityEngine.Random.insideUnitSphere * m_floatContral[0] + (m_fromV3 + m_toV3) * 0.5f;
                }
            }
            else
            {
                Debug.LogError("bezierInit(): v3Contral && floatContral == null");
            }
        }

        //如果在平面内（z轴相同），控制点也要在该平面内
        if (m_fromV3.z == m_toV3.z)
        {
            m_v3Contral[0].z = m_fromV3.z;
            m_v3Contral[1].z = m_toV3.z;
        }
    }
    /// <summary>
    /// 贝塞尔专用算法
    /// </summary>
    Vector3 GetBezierInterpolationV3(Vector3 oldValue, Vector3 aimValue)
    {
        Vector3 result = new Vector3(
            GetInterpolation(oldValue.x, aimValue.x),
            0,
            0
        );
        float n_finishingRate = (result.x - oldValue.x) / (aimValue.x - oldValue.x);
        n_finishingRate = Mathf.Clamp(n_finishingRate, -1, 2);

        switch (m_pathType)
        {
            case PathType.Bezier2: return Bezier2(oldValue, aimValue, n_finishingRate, m_v3Contral);
            case PathType.Bezier3: return Bezier3(oldValue, aimValue, n_finishingRate, m_v3Contral);
            default: return Vector3.zero;
        }

    }

    /// <summary>
    /// 二阶贝塞尔曲线函数
    /// </summary>

    Vector3 Bezier2(Vector3 startPos, Vector3 endPos, float n_time, Vector3[] v3_ControlPoint)
    {
        return (1 - n_time) * (1 - n_time) * startPos + 2 * (1 - n_time) * n_time * v3_ControlPoint[0] + n_time * n_time * endPos;
    }

    /// <summary>
    /// 三阶贝塞尔曲线函数
    /// </summary>
    Vector3 Bezier3(Vector3 startPos, Vector3 endPos, float n_time, Vector3[] t_ControlPoint)
    {
        return (1 - n_time) * (1 - n_time) * (1 - n_time) * startPos + 3 * (1 - n_time) * (1 - n_time) * n_time * t_ControlPoint[0] + 3 * (1 - n_time) * n_time * n_time * t_ControlPoint[1] + n_time * n_time * n_time * endPos;
    }

    #endregion

    #region UGUI

    #region UGUI_alpha

    List<Image> m_animObjectList_Image = new List<Image>();
    List<Text> m_animObjectList_Text = new List<Text>();

    List<Color> m_oldColor = new List<Color>();

    public void UguiAlphaInit(bool isChild)
    {
        m_animObjectList_Image = new List<Image>();
        m_oldColor = new List<Color>();

        if (isChild)
        {
            Image[] images = m_animGameObejct.GetComponentsInChildren<Image>();
            for(int i = 0; i < images.Length; i++)
            {
                if (images[i].transform.GetComponent<Mask>() == null)
                {
                    m_animObjectList_Image.Add(images[i]);
                    m_oldColor.Add(images[i].color);
                }
                else
                {
                    //Debug.LogError("name:" + images[i].gameObject.name);
                }
            }

            Text[] texts = m_animGameObejct.GetComponentsInChildren<Text>();

            for (int i = 0; i < texts.Length; i++)
            {
                m_animObjectList_Text.Add(texts[i]);
                m_oldColor.Add(texts[i].color);
            }
        }
        else
        {
            m_animObjectList_Image.Add(m_animGameObejct.GetComponent<Image>());
            m_oldColor.Add(m_animGameObejct.GetComponent<Image>().color);
        }

        setUGUIAlpha(m_fromFloat);
    }

    void UguiAlpha()
    {
        setUGUIAlpha(GetInterpolation(m_fromFloat, m_toFloat));
    }

    public void setUGUIAlpha(float a)
    {
        Color newColor = new Color();

        int index = 0;
        for (int i = 0; i < m_animObjectList_Image.Count; i++)
        {
            newColor = m_oldColor[index];
            newColor.a = a;
            m_animObjectList_Image[i].color = newColor;

            index++;
        }

        for (int i = 0; i < m_animObjectList_Text.Count; i++)
        {
            newColor = m_oldColor[index];
            newColor.a = a;
            m_animObjectList_Text[i].color = newColor;

            index++;
        }
    }



    void SizeDelta()
    {
        if (m_rectRransform == null)
        {
            Debug.LogError(m_transform.name + "缺少RectTransform组件，不能进行sizeDelta变换！！");
            return;
        }
        m_rectRransform.sizeDelta = GetInterpolationV3(m_fromV2, m_toV2);
    }

    #endregion

    #region UGUI_position

    public void UguiPositionInit()
    {
        m_rectRransform = m_animGameObejct.GetComponent<RectTransform>();
    }

    void UguiPosition()
    {
        m_rectRransform.anchoredPosition3D = GetInterpolationV3(m_fromV3, m_toV3);
    }

    #endregion
    #endregion

    #region Transfrom

    public void TransfromInit()
    {
        m_transform = m_animGameObejct.transform;
    }

    void Position()
    {
        m_transform.position = GetInterpolationV3(m_fromV3, m_toV3);
    }

    void LocalPosition()
    {
        m_transform.localPosition = GetInterpolationV3(m_fromV3, m_toV3);
    }

    void LocalScale()
    {
        m_transform.localScale = GetInterpolationV3(m_fromV3, m_toV3);
    }

    #endregion

    #region Color

    #endregion

    #region 插值算法

    #region 总入口

    float GetInterpolation(float oldValue, float aimValue)
    {
        switch (m_interpolationType)
        {
            case InteType.Default:
            case InteType.Linear: return Mathf.Lerp(oldValue, aimValue, m_currentTime / m_totalTime);
            case InteType.InBack: return InBack(oldValue, aimValue, m_currentTime, m_totalTime);
            case InteType.OutBack: return OutBack(oldValue, aimValue, m_currentTime, m_totalTime);
            case InteType.InOutBack: return InOutBack(oldValue, aimValue, m_currentTime, m_totalTime);
            case InteType.OutInBack: return OutInBack(oldValue, aimValue, m_currentTime, m_totalTime);
            case InteType.InQuad: return InQuad(oldValue, aimValue, m_currentTime, m_totalTime);
            case InteType.OutQuad: return OutQuad(oldValue, aimValue, m_currentTime, m_totalTime);
            case InteType.InoutQuad: return InoutQuad(oldValue, aimValue, m_currentTime, m_totalTime);
            case InteType.InCubic: return InCubic(oldValue, aimValue, m_currentTime, m_totalTime);
            case InteType.OutCubic: return OutCubic(oldValue, aimValue, m_currentTime, m_totalTime);
            case InteType.InoutCubic: return InoutCubic(oldValue, aimValue, m_currentTime, m_totalTime);
            case InteType.InQuart: return InQuart(oldValue, aimValue, m_currentTime, m_totalTime);
            case InteType.OutQuart: return OutQuart(oldValue, aimValue, m_currentTime, m_totalTime);
            case InteType.InOutQuart: return InOutQuart(oldValue, aimValue, m_currentTime, m_totalTime);
            case InteType.OutInQuart: return OutInQuart(oldValue, aimValue, m_currentTime, m_totalTime);
            case InteType.InQuint: return InQuint(oldValue, aimValue, m_currentTime, m_totalTime);
            case InteType.OutQuint: return OutQuint(oldValue, aimValue, m_currentTime, m_totalTime);
            case InteType.InOutQuint: return InOutQuint(oldValue, aimValue, m_currentTime, m_totalTime);
            case InteType.OutInQuint: return OutInQuint(oldValue, aimValue, m_currentTime, m_totalTime);
            case InteType.InSine: return InSine(oldValue, aimValue, m_currentTime, m_totalTime);
            case InteType.OutSine: return OutSine(oldValue, aimValue, m_currentTime, m_totalTime);
            case InteType.InOutSine: return InOutSine(oldValue, aimValue, m_currentTime, m_totalTime);
            case InteType.OutInSine: return OutInSine(oldValue, aimValue, m_currentTime, m_totalTime);
            case InteType.InExpo: return InExpo(oldValue, aimValue, m_currentTime, m_totalTime);
            case InteType.OutExpo: return OutExpo(oldValue, aimValue, m_currentTime, m_totalTime);
            case InteType.InOutExpo: return InOutExpo(oldValue, aimValue, m_currentTime, m_totalTime);
            case InteType.OutInExpo: return OutInExpo(oldValue, aimValue, m_currentTime, m_totalTime);
        }

        return 0;
    }

    Vector3 GetInterpolationV3(Vector3 oldValue, Vector3 aimValue)
    {
        Vector3 result = Vector3.zero;

        if (m_pathType == PathType.Line)
        {
            result = new Vector3(
                GetInterpolation(oldValue.x, aimValue.x),
                GetInterpolation(oldValue.y, aimValue.y),
                GetInterpolation(oldValue.z, aimValue.z)
            );
        }
        else
        {
            result = GetBezierInterpolationV3(oldValue, aimValue);
        }

        return result;
    }


    #endregion

    public float InBack(float b, float to, float t, float d)
    {
        float s = 1.70158f;
        float c = to - b;
        t = t / d;

        return c * t * t * ((s + 1) * t - s) + b;
    }

    public float OutBack(float b, float to, float t, float d, float s = 1.70158f)
    {
        float c = to - b;

        t = t / d - 1;

        return c * (t * t * ((s + 1) * t + s) + 1) + b;

    }

    public float InOutBack(float b, float to, float t, float d, float s = 1.70158f)
    {
        float c = to - b;
        s = s * 1.525f;
        t = t / d * 2;
        if (t < 1)
            return c / 2 * (t * t * ((s + 1) * t - s)) + b;
        else
        {
            t = t - 2;
            return c / 2 * (t * t * ((s + 1) * t + s) + 2) + b;
        }
    }

    public float OutInBack(float b, float to, float t, float d, float s = 1.70158f)
    {
        float c = to - b;
        if (t < d / 2)
        {
            t *= 2;
            c *= 0.5f;
            t = t / d * 2;

            t = t / d - 1;
            return c * (t * t * ((s + 1) * t + s) + 1) + b;
        }

        else
        {
            t = t * 2 - d;
            b += c * 0.5f;
            c *= 0.5f;


            if (t < 1)
                return c / 2 * (t * t * ((s + 1) * t - s)) + b;
            else
            {
                t = t - 2;
                return c / 2 * (t * t * ((s + 1) * t + s) + 2) + b;
            }
        }

    }

    public float InQuad(float b, float to, float t, float d)
    {
        float c = to - b;
        t = t / d;
        return (float)(c * Math.Pow(t, 2) + b);
    }

    public float OutQuad(float b, float to, float t, float d)
    {
        float c = to - b;
        t = t / d;
        return (float)(-c * t * (t - 2) + b);
    }

    public float InoutQuad(float b, float to, float t, float d)
    {
        float c = to - b;
        t = t / d * 2;
        if (t < 1)
            return (float)(c / 2 * Math.Pow(t, 2) + b);
        else
            return -c / 2 * ((t - 1) * (t - 3) - 1) + b;

    }
    public float InCubic(float b, float to, float t, float d)
    {
        float c = to - b;
        t = t / d;
        return (float)(c * Math.Pow(t, 3) + b);

    }
    public float OutCubic(float b, float to, float t, float d)
    {
        float c = to - b;
        t = t / d - 1;
        return (float)(c * (Math.Pow(t, 3) + 1) + b);

    }
    public float InoutCubic(float b, float to, float t, float d)
    {
        float c = to - b;
        t = t / d * 2;
        if (t < 1)
            return c / 2 * t * t * t + b;
        else
        {
            t = t - 2;
            return c / 2 * (t * t * t + 2) + b;
        }
    }
    public float InQuart(float b, float to, float t, float d)
    {
        float c = to - b;
        t = t / d;
        return (float)(c * Math.Pow(t, 4) + b);

    }
    public float OutQuart(float b, float to, float t, float d)
    {
        float c = to - b;
        t = t / d - 1;
        return (float)(-c * (Math.Pow(t, 4) - 1) + b);

    }

    public float InOutQuart(float b, float to, float t, float d)
    {
        float c = to - b;
        t = t / d * 2;
        if (t < 1)
            return (float)(c / 2 * Math.Pow(t, 4) + b);
        else
        {
            t = t - 2;
            return (float)(-c / 2 * (Math.Pow(t, 4) - 2) + b);
        }

    }
    public float OutInQuart(float b, float to, float t, float d)
    {
        if (t < d / 2)
        {
            float c = to - b;
            t *= 2;
            c *= 0.5f;
            t = t / d - 1;

            return (float)(-c * (Math.Pow(t, 4) - 1) + b);
        }
        else
        {
            float c = to - b;
            t = t * 2 - d;
            b = b + c * 0.5f;
            c *= 0.5f;
            t = t / d;


            return (float)(c * Math.Pow(t, 4) + b);

        }
    }

    public float InQuint(float b, float to, float t, float d)
    {
        float c = to - b;
        t = t / d;
        return (float)(c * Math.Pow(t, 5) + b);

    }

    public float OutQuint(float b, float to, float t, float d)
    {
        float c = to - b;
        t = t / d - 1;
        return (float)(c * (Math.Pow(t, 5) + 1) + b);
    }

    public float InOutQuint(float b, float to, float t, float d)
    {
        float c = to - b;
        t = t / d * 2;
        if (t < 1)
            return (float)(c / 2 * Math.Pow(t, 5) + b);
        else
        {
            t = t - 2;
            return (float)(c / 2 * (Math.Pow(t, 5) + 2) + b);

        }

    }

    public float OutInQuint(float b, float to, float t, float d)
    {
        float c = to - b;
        if (t < d / 2)
        {
            t *= 2;
            c *= 0.5f;
            t = t / d - 1;
            return (float)(c * (Math.Pow(t, 5) + 1) + b);
        }
        else
        {
            t = t * 2 - d;
            b = b + c * 0.5f;
            c *= 0.5f;

            t = t / d;
            return (float)(c * Math.Pow(t, 5) + b);
        }
    }

    public float InSine(float b, float to, float t, float d)
    {
        float c = to - b;
        return (float)(-c * Math.Cos(t / d * (Math.PI / 2)) + c + b);

    }

    public float OutSine(float b, float to, float t, float d)
    {
        float c = to - b;
        return (float)(c * Math.Sin(t / d * (Math.PI / 2)) + b);
    }

    public float InOutSine(float b, float to, float t, float d)
    {
        float c = to - b;
        return (float)(-c / 2 * (Math.Cos(Math.PI * t / d) - 1) + b);

    }
    public float OutInSine(float b, float to, float t, float d)
    {
        float c = to - b;
        if (t < d / 2)
        {
            t *= 2;
            c *= 0.5f;
            return (float)(c * Math.Sin(t / d * (Math.PI / 2)) + b);
        }
        else
        {
            t = t * 2 - d;
            b += c * 0.5f;
            c *= 0.5f;
            return (float)(-c * Math.Cos(t / d * (Math.PI / 2)) + c + b);

        }
    }
    public float InExpo(float b, float to, float t, float d)
    {
        float c = to - b;
        if (t == 0)
            return b;
        else
            return (float)(c * Math.Pow(2, 10 * (t / d - 1)) + b - c * 0.001f);
    }
    public float OutExpo(float b, float to, float t, float d)
    {
        float c = to - b;
        if (t == d)
            return b + c;
        else
            return (float)(c * 1.001 * (-Math.Pow(2, -10 * t / d) + 1) + b);

    }
    public float InOutExpo(float b, float to, float t, float d)
    {
        float c = to - b;
        if (t == 0)
            return b;
        if (t == d)
            return (b + c);

        t = t / d * 2;

        if (t < 1)
            return (float)(c / 2 * Math.Pow(2, 10 * (t - 1)) + b - c * 0.0005f);
        else
        {
            t = t - 1;
            return (float)(c / 2 * 1.0005 * (-Math.Pow(2, -10 * t) + 2) + b);

        }
    }

    public float OutInExpo(float b, float to, float t, float d)
    {
        float c = to - b;
        if (t < d / 2)
        {
            t *= 2;
            c *= 0.5f;
            if (t == d)
                return b + c;
            else
                return (float)(c * 1.001 * (-Math.Pow(2, -10 * t / d) + 1) + b);
        }
        else
        {
            t = t * 2 - d;
            b += c * 0.5f;
            c *= 0.5f;
            if (t == 0)
                return b;
            else
                return (float)(c * Math.Pow(2, 10 * (t / d - 1)) + b - c * 0.001f);

        }
    }

    //outInExpo,
    //inBack,
    //outBack,
    //inOutBack,
    //outInBack,

    #endregion
}