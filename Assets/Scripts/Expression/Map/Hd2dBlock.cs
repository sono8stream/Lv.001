using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace Expression.Map
{
    [ExecuteAlways]
    public class Hd2dBlock : MonoBehaviour
    {
        protected Material mat;
        // �y�b��zUI�w�̋@�\�Ȃ̂ŁA�������Ă���
        protected Vector2Int[] offsets;

        [SerializeField]// SerializeField�ɂ��邱�ƂŃQ�[���J�n����l���ێ������悤�ɂ���
        protected List<GameObject> quads;
        protected Vector3Int pos;
        protected int sortingOrder;

        public void Initialize(Material mat, Vector2Int[] offsets,
            Vector3Int pos, Hd2dMeshFactory meshFactory,int sortingOrder)
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
                // �y�b��z�A�Z�b�g�𒼐ڊ��蓖�Ă鏈����p�~
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