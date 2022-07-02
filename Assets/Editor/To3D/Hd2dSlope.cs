using UnityEngine;
using System.Collections.Generic;

public class Hd2dSlope : Hd2dBlock
{
    enum MeshType
    {
        Rectangle, Triangle
    }

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

        var factory = new Hd2d.MeshFactory();
        for (int i = 0; i < meshes; i++)
        {
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.transform.SetParent(transform);
            quad.transform.localPosition = poses[i];
            quad.transform.localEulerAngles = angles[i];
            quad.transform.localScale = Vector3.one;
            quad.GetComponent<Renderer>().sharedMaterial = new Material(mat);
            /*
            var triangles = new int[3] { 0, 1, 2 };
            quad.GetComponent<MeshFilter>().mesh.SetTriangles(triangles, 0);
            var vartices = new Vector3[3] {
                new Vector2(-0.5f,-0.5f),
                new Vector2(-0.5f,0.5f),
                new Vector2(0.5f,-0.5f)
            };
            quad.GetComponent<MeshFilter>().mesh.SetVertices(vartices);
            Debug.Log(quad.GetComponent<MeshFilter>().mesh.vertices.Length);
            quad.GetComponent<MeshFilter>().mesh.SetUVs(0, selector.GetTriUvs(offsets[i]));
            */
            quad.GetComponent<MeshFilter>().sharedMesh= factory.CreateMesh(Hd2d.MeshType.Triangle, offsets[i]);
            quads.Add(quad);
        }
    }
}