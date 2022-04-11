using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 动画类型
/// </summary>
public enum AnimType
{
    UGUI_alpha,
    UGUI_anchoredPosition
}

/// <summary>
/// 插值算法类型
/// </summary>
public enum InteType
{
    linear
}

public delegate void AnimCallBack(params object[] arg);

public class AnimSystem : MonoBehaviour
{
    #region 静态部分

    private static AnimSystem instance;

    public static AnimSystem getInstance()
    {
        if (instance == null)
        {
            GameObject animGo = new GameObject();
            animGo.name = "AnimSystem";
            instance = animGo.AddComponent<AnimSystem>();
        }
        return instance;
    }

    //改变透明度的动画
    public static void uguiAlpha(GameObject animObject, float fromAlpha, float toAlpha, float time, InteType interpolationType, AnimCallBack callBack = null, bool isChild = true)
    {
        AnimData DataTmp = getAnimData(animObject, time, interpolationType, callBack);

        DataTmp.animType = AnimType.UGUI_alpha;
        DataTmp.formAlpha = fromAlpha;
        DataTmp.toAlpha = toAlpha;
        DataTmp.uguiAlphaInit(isChild);

        getInstance().animList.Add(DataTmp);
    }

    //改变位置的动画
    public static void uguiMove(GameObject animObject, Vector3 from, Vector3 to, float time, InteType interpolationType, AnimCallBack callBack = null)
    {
        AnimData DataTmp = getAnimData(animObject, time, interpolationType, callBack);

        DataTmp.animType = AnimType.UGUI_anchoredPosition;
        DataTmp.formPos = from;
        DataTmp.toPos = to;
        DataTmp.uguiPositionInit();

        getInstance().animList.Add(DataTmp);
    }

    private static AnimData getAnimData(GameObject animObject, float time, InteType interpolationType, AnimCallBack callBack = null)
    {
        AnimData DataTmp = new AnimData();

        DataTmp.interpolationType = interpolationType;
        DataTmp.animGo = animObject;
        DataTmp.currentTime = 0;
        DataTmp.totalTime = time;
        DataTmp.callBack = callBack;

        return DataTmp;
    }
    #endregion

    #region 实例部分
    public List<AnimData> animList = new List<AnimData>();

    void Update()
    {
        for (int i = 0; i < animList.Count; i++)
        {
            //执行Update
            animList[i].executeUpdate();

            if (animList[i].isDone == true)
            {
                //执行回调
                animList[i].executeCallBack();

            }
        }
    }
    #endregion
}
