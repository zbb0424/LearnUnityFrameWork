using System.Collections;
using UnityEngine;

public class UIManager : MonoBehaviour
{
#region 静态部分
    public static UIManager instance;
    
    public static UIManager getInstance()
    {
        if (instance == null)
        {
            GameObject instanceGo = new GameObject();
            instanceGo.name = "UIManager";

            instance = instanceGo.AddComponent<UIManager>();
        }
        return instance;
    }

    public static void openUI()
    {

    }

    public static void closeUI()
    {

    }
    #endregion

    #region 实例部分
    void Start()
    {
        
    }

    void Update()
    {
        
    }
    #endregion
}
