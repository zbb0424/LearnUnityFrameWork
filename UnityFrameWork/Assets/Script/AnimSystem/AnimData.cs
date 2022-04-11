using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;

public class AnimData : MonoBehaviour
{
    public AnimType animType;
    public InteType interpolationType;

    public GameObject animGo;

    public float currentTime = 0;
    public float totalTime = 0;

    public bool isDone = false;

    public object[] parameter;
    public AnimCallBack callBack;

    //执行Update
    public void executeUpdate()
    {
        currentTime += Time.deltaTime;

        if (currentTime > totalTime)
        {
            currentTime = totalTime;
            isDone = true;
        }

        switch (animType)
        {
            case AnimType.UGUI_alpha:
                uguiAlpha();
                break;
            case AnimType.UGUI_anchoredPosition:
                uguiPosition();
                break;
        }
    }

    //执行回调
    public void executeCallBack()
    {
        if (callBack != null)
        {
            callBack(parameter);
        }
    }

    private float getInterpolation(float oldVaule, float aimValue)
    {
        switch (interpolationType)
        {
            case InteType.linear:
                return Mathf.Lerp(oldVaule, aimValue, currentTime / totalTime);
        }

        return 0;
    }

    private Vector3 getInterpolationV3(Vector3 oldValue, Vector3 aimValue)
    {
        Vector3 result = new Vector3(
            getInterpolation(oldValue.x, aimValue.x),
            getInterpolation(oldValue.y, aimValue.y),
            getInterpolation(oldValue.z, aimValue.z)
            );
        return result;
    }

    #region UGUI

    RectTransform rectTransform;

    #region UGUI_alpha

    List<Image> animObjectList_Image = new List<Image>();
    List<Text> animObjectList_Text = new List<Text>();

    List<Color> oldColor = new List<Color>();

    public float formAlpha = 0;
    public float toAlpha = 0;

    //初始化透明度（List存储和透明度有关的Image和Text）
    public void uguiAlphaInit(bool isChild)
    {
        animObjectList_Image = new List<Image>();
        oldColor = new List<Color>();

        if (isChild)
        {
            Image[] images = animGo.GetComponentsInChildren<Image>();

            for (int i = 0; i < images.Length; i++)
            {
                animObjectList_Image.Add(images[i]);
                oldColor.Add(images[i].color);
            }

            Text[] texts = animGo.GetComponentsInChildren<Text>();

            for (int i = 0; i < texts.Length; i++)
            {
                animObjectList_Text.Add(texts[i]);
                oldColor.Add(texts[i].color);
            }
        }
        else
        {
            animObjectList_Image.Add(animGo.GetComponent<Image>());
            oldColor.Add(animGo.AddComponent<Image>().color);
        }
    }

    //改变透明度
    public void uguiAlpha()
    {
        Color newColor = new Color();

        int index = 0;
        for (int i = 0; i < animObjectList_Image.Count; i++)
        {
            newColor = oldColor[index];
            newColor.a = getInterpolation(formAlpha, toAlpha);
            animObjectList_Image[i].color = newColor;

            index++;
        }

        for (int i = 0; i < animObjectList_Text.Count; i++)
        {
            newColor = oldColor[index];
            newColor.a = getInterpolation(formAlpha, toAlpha);
            animObjectList_Text[i].color = newColor;

            index++;
        }
    }

    #endregion

    #region UGUI_position

    public Vector3 formPos;

    public Vector3 toPos;

    //初始化位置
    public void uguiPositionInit()
    {
        rectTransform = animGo.GetComponent<RectTransform>();
    }

    //改变位置
    public void uguiPosition()
    {
        rectTransform.anchoredPosition3D = getInterpolationV3(formPos, toPos);
    }

    #endregion

    #endregion

    #region 插值算法

    #endregion
}
