using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Util.Map;
using UI.Action;
using UI.Map;

public class MapEventTest
{
    [UnityTest]
    public IEnumerator RaadEventTest()
    {
        GameObject gameObject = new GameObject("Test");
        gameObject.AddComponent<ActionEnvironment>();
        gameObject.AddComponent<ActionProcessor>();
        gameObject.AddComponent<SpriteRenderer>();

        ActionProcessor processor = gameObject.AddComponent<ActionProcessor>();
        EventObject eventObject = gameObject.AddComponent<EventObject>();

        // Start()‘Ò‚¿
        yield return null;

        processor.StartActions(eventObject);
    }
}
