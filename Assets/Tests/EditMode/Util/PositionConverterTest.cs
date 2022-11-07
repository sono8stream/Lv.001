using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Util.Map;

public class PositionConverterTest
{
    [Test]
    public void GetNormalizedUnityPosTest()
    {
        const int caseCount = 4;
        Vector2[] inputPoses = new Vector2[caseCount]{
            new Vector2(1.3f,5.1f),new Vector2(4.7f, 5.6f),
            new Vector2(0.5f,0.5f),new Vector2(-1.4f,-2.7f), };
        Vector2[] outputPoses = new Vector2[caseCount]{
            new Vector2(1.5f,5.5f),new Vector2(4.5f, 5.5f),
            new Vector2(0.5f,0.5f),new Vector2(-1.5f,-2.5f), };
        for (int i = 0; i < caseCount; i++)
        {
            Vector2 normalizedPos = PositionConverter.GetNormalizedUnityPos(inputPoses[i]);
            Assert.AreEqual(outputPoses[i], normalizedPos);
        }
    }

    [Test]
    public void GetUnityPosTest()
    {
        const int caseCount = 3;
        GetUnityPosTestCase[] testCases = new GetUnityPosTestCase[caseCount]{
            new GetUnityPosTestCase(new Vector2Int(0, 0), 10, new Vector2(0.5f, 9.5f)),
            new GetUnityPosTestCase(new Vector2Int(10, 9), 10, new Vector2(10.5f, 0.5f)),
            new GetUnityPosTestCase(new Vector2Int(3, 0), 1, new Vector2(3.5f, 0.5f)),
            };

        for (int i = 0; i < caseCount; i++)
        {
            Vector2 unityPos = PositionConverter.GetUnityPos(testCases[i].inputPos, testCases[i].mapHeight);
            Assert.AreEqual(testCases[i].outputPos, unityPos);
        }
    }


    class GetUnityPosTestCase
    {
        public GetUnityPosTestCase(Vector2Int inputPos, int mapHeight, Vector2 outputPos)
        {
            this.inputPos = inputPos;
            this.mapHeight = mapHeight;
            this.outputPos = outputPos;
        }

        public Vector2Int inputPos;
        public int mapHeight;
        public Vector2 outputPos;
    }

    [Test]
    public void GetGeneralPosTest()
    {
        const int caseCount = 3;
        GetGeneralPosTestCase[] testCases = new GetGeneralPosTestCase[caseCount]{
            new GetGeneralPosTestCase(new Vector2(0.5f, 9.5f), 10, new Vector2Int(0, 0)),
            new GetGeneralPosTestCase(new Vector2(3.5f, 0.5f), 10, new Vector2Int(3, 9)),
            new GetGeneralPosTestCase(new Vector2(0.5f, 3.5f), 5, new Vector2Int(0, 1)),
            };

        for (int i = 0; i < caseCount; i++)
        {
            Vector2Int generalPos = PositionConverter.GetGeneralPos(testCases[i].inputPos, testCases[i].mapHeight);
            Assert.AreEqual(testCases[i].outputPos, generalPos);
        }
    }


    class GetGeneralPosTestCase
    {
        public GetGeneralPosTestCase(Vector2 inputPos, int mapHeight, Vector2Int outputPos)
        {
            this.inputPos = inputPos;
            this.mapHeight = mapHeight;
            this.outputPos = outputPos;
        }

        public Vector2 inputPos;
        public int mapHeight;
        public Vector2Int outputPos;
    }
}
