using UnityEngine;
using System.Collections.Generic;

public class Hd2dBlock : MonoBehaviour
{
    protected Material mat;
    protected Vector2Int[] offsets;
    protected List<GameObject> quads;
    protected Vector3Int pos;

    public void Initialize(Material mat, Vector2Int[] offsets, Vector3Int pos)
    {
        this.mat = mat;
        this.offsets = offsets;
        this.pos = pos;
        quads = new List<GameObject>();
    }

    private void Start()
    {
        transform.localPosition = pos;

        Generate();
    }

    private void OnDestroy()
    {
        for (int i = 0; i < quads.Count; i++)
        {
            // �y�b��z�}�e���A�����j������Ȃ��s�������
            DestroyImmediate(quads[i].GetComponent<Renderer>().material);
            DestroyImmediate(quads[i].GetComponent<MeshFilter>().mesh);
        }
    }

    protected virtual void Generate()
    {

    }
}