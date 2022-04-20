﻿using UnityEngine;
using System.Collections;

public class RuntimeTest : MonoBehaviour
{
    void Start()
    {
        BundleConfigManager.Initialize();
        ResourceManager.gameLoadType = ResLoadType.Streaming;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.A))
        {
            GameObject testTmp = (GameObject)ResourceManager.Load("GameObject");

            Instantiate(testTmp);
        }

        if (Input.GetKey(KeyCode.B))
        {
            GameObject testTmp = (GameObject)ResourceManager.Load("UItest");

            Instantiate(testTmp);
        }

        if (Input.GetKey(KeyCode.C))
        {
            AssetsBundleManager.UnLoadBundle("UItest");
        }



        if (Input.GetKey(KeyCode.D))
        {
            loadCount++;
            ResourceManager.LoadAsync("UItest", (LoadState state, object obj) =>
            {
                if (state.isDone)
                {
                    callbackCount++;
                    Debug.Log(state.progress);
                    GameObject go = (GameObject)obj;

                    Instantiate(go);

                    Debug.Log(loadCount + "  " + callbackCount);
                }
                else
                {
                    Debug.Log(state.progress);
                }
            });
        }
    }

    int loadCount = 0;
    int callbackCount = 0;
}
