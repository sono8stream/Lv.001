using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class Hd2dCube : Hd2dBlock
{
    protected override void Generate()
    {
        const int quadCount = 6;
        Vector3[] poses = new Vector3[quadCount] {
            new Vector3(0,0,-0.5f),
            new Vector3(0.5f,0,0),
            new Vector3(0,0,0.5f),
            new Vector3(-0.5f,0,0),
            new Vector3(0,0.5f,0),
            new Vector3(0,-0.5f,-0),
        };
        Vector3[] angles = new Vector3[quadCount] {
            new Vector3(0,0,0),
            new Vector3(0,-90,0),
            new Vector3(0,-180,0),
            new Vector3(0,90,0),
            new Vector3(90,0,0),
            new Vector3(-90,0,0),
        };

        for (int i = 0; i < quadCount; i++)
        {
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.transform.SetParent(transform);
            quad.transform.localPosition = poses[i];
            quad.transform.localEulerAngles = angles[i];
            quad.transform.localScale = Vector3.one;
            quads.Add(quad);
            quad.GetComponent<Renderer>().material = mat;
            quad.GetComponent<MeshFilter>().mesh.SetUVs(0, GetUvs(offsets[i]));
        }
    }

    private Vector2[] GetUvs(Vector2Int offset)
    {
        Vector2[] res = new Vector2[4];
        float xUnit = 0.125f;
        float yUnit = 0.00390625f;
        res[0] = new Vector2(xUnit * offset.x, 1 - yUnit * (offset.y + 1));
        res[1] = new Vector2(xUnit * (offset.x + 1), 1 - yUnit * (offset.y + 1));
        res[2] = new Vector2(xUnit * offset.x, 1 - yUnit * offset.y);
        res[3] = new Vector2(xUnit * (offset.x + 1), 1 - yUnit * offset.y);
        return res;
    }
}