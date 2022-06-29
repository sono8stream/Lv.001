using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class Hd2dSlope : Hd2dBlock
{

    protected override void Generate()
    {
        const int meshes = 5;
        Vector3[] poses = new Vector3[meshes] {
            new Vector3(0,0,0),
            new Vector3(0.5f,0,0),
            new Vector3(0,0,0.5f),
            new Vector3(-0.5f,0,0),
            new Vector3(0,-0.5f,-0),
        };
        Vector3[] angles = new Vector3[meshes] {
            new Vector3(45,0,0),
            new Vector3(0,-90,0),
            new Vector3(0,-180,0),
            new Vector3(0,90,0),
            new Vector3(-90,0,0),
        };
        Vector3[][] vertices = new Vector3[5][];
        vertices[0] = new Vector3[4]        {
            new Vector2(-0.5f,-0.5f),
            new Vector2(-0.5f,0.5f),
            new Vector2(0.5f,-0.5f),
            new Vector2(-0.5f,-0.5f),
        };

        for (int i = 0; i < meshes; i++)
        {
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.transform.SetParent(transform);
            quad.transform.localPosition = poses[i];
            quad.transform.localEulerAngles = angles[i];
            quad.transform.localScale = Vector3.one;
            quad.GetComponent<Renderer>().material = mat;
            var triangles = new int[3] { 0, 1, 2 };
            quad.GetComponent<MeshFilter>().mesh.SetTriangles(triangles, 0);
            var vartices = new Vector3[3] {
                new Vector2(-0.5f,-0.5f),
                new Vector2(-0.5f,0.5f),
                new Vector2(0.5f,-0.5f)
            };
            quad.GetComponent<MeshFilter>().mesh.SetVertices(vartices);
            quad.GetComponent<MeshFilter>().mesh.SetUVs(0, GetTriUvs(offsets[i]));
            quads.Add(quad);
        }
    }

    private Vector2[] GetTriUvs(Vector2Int offset)
    {
        Vector2[] res = new Vector2[3];
        float xUnit = 0.125f;
        float yUnit = 0.00390625f;
        res[0] = new Vector2(xUnit * offset.x, 1 - yUnit * (offset.y + 1));
        res[1] = new Vector2(xUnit * offset.x, 1 - yUnit * offset.y);
        res[2] = new Vector2(xUnit * (offset.x + 1), 1 - yUnit * (offset.y + 1));
        return res;
    }
}