using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// 打包设置管理器
/// </summary>
public class PackageConfigEditorWindow : EditorWindow
{
    [MenuItem("Tool/打包设置编辑器")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(PackageConfigEditorWindow)).minSize = new Vector2(600, 400);
    }

    //所有依赖包
    List<EditPackageConfig> relyPackages = new List<EditPackageConfig>();

    //所有普通包
    List<EditPackageConfig> bundles = new List<EditPackageConfig>();

    #region GUI
    bool isFoldRelyPackages = false; //是否展开依赖包
    bool isFoldBundles = false;      //是否展开普通包

    Vector2 scrollPos = new Vector2();

    string[] RelyPackageNames = new string[1];

    GUIStyle errorMsg = new GUIStyle();
    GUIStyle warnMsg = new GUIStyle();

    GUIContent title = new GUIContent();

    private void OnGUI()
    {
        title.text = "打包设置编辑器";

        base.titleContent = title;

        errorMsg.normal.textColor = Color.red;
        warnMsg.normal.textColor = Color.yellow;
        UpdateRelyPackNames();

        scrollPos = GUILayout.BeginScrollView(scrollPos);

        isFoldRelyPackages = EditorGUILayout.Foldout(isFoldRelyPackages, "依赖包：");
        if (isFoldRelyPackages)
        {
            //依赖包视图
            RelyPackagesView();

            EditorGUILayout.Space();
        }

        EditorGUI.indentLevel = 0;
        isFoldBundles = EditorGUILayout.Foldout(isFoldBundles, "AssetsBundle：");
        if (isFoldBundles)
        {
            //bundle包视图
            BundlesView();
        }

        GUILayout.EndScrollView();

        if (GUILayout.Button("检查包依赖性"))
        {
            CheckPackage();
        }

        if (GUILayout.Button("生成打包设置文件"))
        {
            CreatePackageFile();
        }

        if (GUILayout.Button("打包"))
        {
            Package();
        }
    }

    /// <summary>
    /// 依赖包视图
    /// </summary>
    void RelyPackagesView()
    {
        for (int i = 0; i < relyPackages.Count; i++)
        {
            EditorGUI.indentLevel = 2;
            EditorGUILayout.BeginHorizontal();
            relyPackages[i].isFold = EditorGUILayout.Foldout(relyPackages[i].isFold, relyPackages[i].name);

            if (GUILayout.Button("删除"))
            {
                relyPackages.RemoveAt(i);
                continue;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel = 3;
            if (relyPackages[i].isFold)
            {
                //名称
                EditorGUI.indentLevel = 4;
                relyPackages[i].name = EditorGUILayout.TextField("name:", relyPackages[i].name);

                //加载路径
                relyPackages[i].path = "RelyAssetsBundlePath/" + relyPackages[i].name;
                EditorGUILayout.LabelField("Path:", relyPackages[i].path);

                //子资源视图
                relyPackages[i].isFold_objects = EditorGUILayout.Foldout(relyPackages[i].isFold_objects, "Objects");
                EditorGUI.indentLevel = 5;
                if (relyPackages[i].isFold_objects)
                {
                    ObjectListView(relyPackages[i].objects);
                }
            }

            MessageView(relyPackages[i]);
        }

        EditorGUILayout.Space();
        EditorGUI.indentLevel = 1;
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Button：");

        if (GUILayout.Button("增加一个依赖包"))
        {
            EditPackageConfig editPackageConfigTmp = new EditPackageConfig();
            editPackageConfigTmp.name = "NewRelyAssetsBundle" + relyPackages.Count;

            relyPackages.Add(editPackageConfigTmp);
        }
        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// 子资源视图
    /// </summary>
    /// <param name="objects"></param>
    void ObjectListView(List<Object> objects)
    {
        EditorGUILayout.LabelField("Size:" + objects.Count);
        for (int i = 0; i < objects.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            objects[i] = EditorGUILayout.ObjectField(objects[i], typeof(Object));

            if (GUILayout.Button("删除"))
            {
                objects.RemoveAt(i);
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("Button:");
        if (GUILayout.Button("增加选中资源"))
        {
            Object[] selects = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);

            for (int j = 0; j < selects.Length; j++)
            {
                if (!isExists(objects, selects[j]))
                {
                    objects.Add(selects[j]);
                }
                else
                {
                    Debug.Log(selects[j].ToString() + " has Exists");
                }
            }
        }

        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// 消息视图
    /// </summary>
    /// <param name="package"></param>
    void MessageView(EditPackageConfig package)
    {
        for (int i = 0; i < package.errorMsg.Count; i++)
        {
            EditorGUILayout.LabelField("ERROR:" + package.errorMsg[i], errorMsg);
        }

        for (int i = 0; i < package.warnMsg.Count; i++)
        {
            EditorGUILayout.LabelField("WARN: " + package.warnMsg[i], warnMsg);
        }
    }

    void BundlesView()
    {
        for (int i = 0; i < bundles.Count; i++)
        {
            EditorGUI.indentLevel = 2;
            EditorGUILayout.BeginHorizontal();
            bundles[i].isFold = EditorGUILayout.Foldout(bundles[i].isFold, bundles[i].name);
            if (GUILayout.Button("删除"))
            {
                bundles.RemoveAt(i);
                continue;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel = 3;
            if (bundles[i].isFold)
            {
                EditorGUI.indentLevel = 4;
                if (bundles[i].mainObject != null)
                {
                    bundles[i].name = bundles[i].mainObject.name;
                }
                else
                {
                    bundles[i].name = "Null";
                }

                //主资源
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("MainObject:");
                bundles[i].mainObject = EditorGUILayout.ObjectField(bundles[i].mainObject, typeof(Object));
                EditorGUILayout.EndHorizontal();

                //依赖包
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("依赖包:");
                bundles[i].relyPackagesMask = EditorGUILayout.MaskField(bundles[i].relyPackagesMask, RelyPackageNames);
                EditorGUILayout.EndHorizontal();

                //Debug.Log(" bundles[i].relyPackagesMask:  " + bundles[i].relyPackagesMask);

                //加载路径
                EditorGUILayout.LabelField("Path: ", bundles[i].path);
                bundles[i].isFold_objects = EditorGUILayout.Foldout(bundles[i].isFold_objects, "Objects");

                //子资源视图
                EditorGUI.indentLevel = 5;
                if (bundles[i].isFold_objects)
                {
                    ObjectListView(bundles[i].objects);
                }
            }

            //消息视图
            MessageView(bundles[i]);
        }

        EditorGUI.indentLevel = 1;
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Button：");

        if (GUILayout.Button("增加选中Bundle包"))
        {
            Object[] selects = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);

            if (selects.Length > 0)
            {
                for (int i = 0; i < selects.Length; i++)
                {
                    AddAssetBundle(selects[i]);
                }
            }
            else
            {
                Debug.Log("未选中任何资源");
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    #endregion

    #region 支持函数
    /// <summary>
    /// 子资源是否冲突
    /// </summary>
    /// <param name="objects"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
    bool isExists(List<Object> objects, Object obj)
    {
        foreach (Object item in objects)
        {
            if (item.Equals(obj))
            {
                return true;
            }
        }
        return false;
    }

    void UpdateRelyPackNames()
    {
        RelyPackageNames = new string[relyPackages.Count];
        for (int i = 0; i < relyPackages.Count; i++)
        {
            RelyPackageNames[i] = relyPackages[i].name;
        }
    }

    /// <summary>
    /// 增加选中的bundle包
    /// </summary>
    /// <param name="obj"></param>
    void AddAssetBundle(Object obj)
    {
        if (isExist_AllBundle(obj))
        {
            Debug.Log(obj.name + "已经存在");
        }
        else
        {
            EditPackageConfig editPackageConfigTmp = new EditPackageConfig();
            editPackageConfigTmp.name = obj.name;
            editPackageConfigTmp.mainObject = obj;

            Object[] res = GetCorrelationResource(obj);

            //判断依赖包中含不含有该资源，如果有，则不将此资源放入bundle中
            for (int i = 0; i < res.Length; i++)
            {
                for (int j = 0; j < res.Length; j++)
                {
                    if (isExist_Bundle(res[i], relyPackages[j]))
                    {
                        editPackageConfigTmp.relyPackagesMask = editPackageConfigTmp.relyPackagesMask | 1 << i;
                        continue;
                    }
                }

                editPackageConfigTmp.objects.Add(res[i]);
            }

            bundles.Add(editPackageConfigTmp);
        }
    }

    /// <summary>
    /// 判断一个资源是否已经在bundle列表中
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    bool isExist_AllBundle(Object obj)
    {
        for (int i = 0; i < bundles.Count; i++)
        {
            if (bundles[i].mainObject.Equals(obj))
            {
                return true;
            }

            if (isExist_Bundle(obj, bundles[i]))
            {
                return true;
            }
        }

        for (int i = 0; i < relyPackages.Count; i++)
        {
            if (isExist_Bundle(obj, relyPackages[i]))
            {
                return true;
            }
        }
        return false;
    }

    bool isExist_Bundle(Object obj, EditPackageConfig package)
    {
        for (int i = 0; i < package.objects.Count; i++)
        {
            if (package.objects[i].Equals(obj))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 获取所有相关资源
    /// </summary>
    /// <param name="go">目标对象</param>
    /// <returns>所有相关资源</returns>
    Object[] GetCorrelationResource(Object go)
    {
        Object[] roots = new Object[] { go };
        return EditorUtility.CollectDependencies(roots);
    }
    #endregion

    #region 按钮回调函数
    /// <summary>
    /// 检查依赖包
    /// </summary>
    void CheckPackage()
    {

    }

    /// <summary>
    /// 生成打包设置文件
    /// </summary>
    void CreatePackageFile()
    {

    }

    /// <summary>
    /// 打包
    /// </summary>
    void Package()
    {

    }
    #endregion
}

public class EditPackageConfig
{
    public bool isFold = false; //是否折叠
    public string name = "";
    public string path = ""; //存放路径

    public List<Object> objects = new List<Object>(); //所有子资源
    public bool isFold_objects = false; //子资源是否折叠

    //bundle独有
    public Object mainObject; //主资源
    public int relyPackagesMask;
    public List<string> relyPackages = new List<string>(); //所有依赖包

    public List<string> warnMsg = new List<string>(); //错误日志
    public List<string> errorMsg = new List<string>(); //警告日志
}
