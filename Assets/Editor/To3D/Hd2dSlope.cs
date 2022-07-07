using UnityEngine;
using System.Collections.Generic;
using Expression.Map;

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
        Vector3[] scales = new Vector3[meshes]
        {
            new Vector3(1,Mathf.Sqrt(2),1),
            Vector3.one,
            Vector3.one,
            Vector3.one,
            Vector3.one,
        };

        MeshType[] meshTypes = new MeshType[meshes] {
            MeshType.Rectangle,
            MeshType.RightTriangle,
            MeshType.Rectangle,
            MeshType.LeftTriangle,
            MeshType.Rectangle,
        };

        var factory = new MeshFactory();
        for (int i = 0; i < meshes; i++)
        {
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.transform.SetParent(transform);
            quad.transform.localPosition = poses[i];
            quad.transform.localEulerAngles = angles[i];
            quad.transform.localScale = scales[i];
            quad.GetComponent<Renderer>().sharedMaterial = new Material(mat);
            quad.GetComponent<MeshFilter>().sharedMesh= factory.CreateMesh(meshTypes[i], offsets[i]);
            quads.Add(quad);
        }
    }
}