using UnityEngine;
using System.Collections.Generic;

namespace Expression.Map
{
    [ExecuteAlways]
    public class Hd2dBlock : MonoBehaviour
    {
        protected Material mat;
        protected Vector2Int[] offsets;

        [SerializeField]// SerializeFieldにすることでゲーム開始後も値が保持されるようにする
        protected List<GameObject> quads;
        protected Vector3Int pos;

        public void Initialize(Material mat, Vector2Int[] offsets, Vector3Int pos, Hd2dMeshFactory meshFactory)
        {
            this.mat = mat;
            this.offsets = offsets;
            this.pos = pos;
            quads = new List<GameObject>();

            transform.localPosition = pos;
            Generate(meshFactory);
        }

        private void Start()
        {
        }

        private void OnDestroy()
        {
            for (int i = 0; i < quads.Count; i++)
            {
                // 【暫定】アセットを直接割り当てる処理を廃止
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

        protected virtual void Generate(Hd2dMeshFactory meshFactory)
        {

        }
    }
}