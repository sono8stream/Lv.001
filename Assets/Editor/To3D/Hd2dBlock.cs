using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
public class Hd2dBlock : MonoBehaviour
{
    protected Material mat;
    protected Vector2Int[] offsets;

    [SerializeField]// SerializeFieldにすることでゲーム開始後も値が保持されるようにする
    protected List<GameObject> quads;
    protected Vector3Int pos;

    public void Initialize(Material mat, Vector2Int[] offsets, Vector3Int pos)
    {
        this.mat = mat;
        this.offsets = offsets;
        this.pos = pos;
        quads = new List<GameObject>();

        transform.localPosition = pos;
        Generate();
    }

    private void Start()
    {
    }

    private void OnDestroy()
    {
        for (int i = 0; i < quads.Count; i++)
        {
            if (Application.isPlaying)
            {
                Destroy(quads[i].GetComponent<Renderer>().sharedMaterial);
                Destroy(quads[i].GetComponent<MeshFilter>().sharedMesh);
            }
            else
            {
                DestroyImmediate(quads[i].GetComponent<Renderer>().sharedMaterial);
                DestroyImmediate(quads[i].GetComponent<MeshFilter>().sharedMesh);
            }
        }
    }

    protected virtual void Generate()
    {

    }
}