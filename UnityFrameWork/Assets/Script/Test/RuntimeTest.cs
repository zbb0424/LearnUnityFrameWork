using UnityEngine;
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
    }
}
