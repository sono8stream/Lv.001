using UnityEngine;
using System.Collections.Generic;

namespace Expression.Map
{
    public class Hd2dCube : Hd2dBlock
    {
        protected override void Generate(Hd2dMeshFactory meshFactory)
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

                quad.GetComponent<Renderer>().sharedMaterial = mat;// = new Material(mat);
                quad.GetComponent<MeshFilter>().sharedMesh = meshFactory.CreateMesh(MeshType.Rectangle, offsets[i]);
            }
        }
    }
}