using UnityEngine;
using System.Collections.Generic;

namespace Expression.Map
{
    public class Hd2dPlane : Hd2dBlock
    {
        protected override void Generate(Hd2dMeshFactory meshFactory)
        {
            UnityEngine.Assertions.Assert.AreEqual(1, offsets.Length);

            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.transform.SetParent(transform);
            quad.transform.localPosition = new Vector3(0, 0, -0.5f);
            quad.transform.localEulerAngles = Vector3.zero;
            quad.transform.localScale = Vector3.one;
            quad.GetComponent<Renderer>().sharedMaterial = mat;
            quad.GetComponent<MeshFilter>().sharedMesh = meshFactory.CreateMesh(MeshType.Rectangle, offsets[0]);
        }
    }
}