using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Util.Map;

public class MapEventTest
{

    [UnityTest]
    public IEnumerator RaadEventTest()
    {
        GameObject gameObject = new GameObject("Test");
        gameObject.AddComponent<ActionEnvironment>();
        gameObject.AddComponent<SpriteRenderer>();

        ActionProcessor eventObject = gameObject.AddComponent<ActionProcessor>();
        eventObject.SetScriptText(new TextAsset("{メッセージ}(うーっす)"));

        // Start()待ち
        yield return null;

        eventObject.ReadScript2();
    }
}
