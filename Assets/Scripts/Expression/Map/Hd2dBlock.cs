using UnityEngine;
using System.Collections.Generic;

namespace Expression.Map
{
    [ExecuteAlways]
    public class Hd2dBlock : MonoBehaviour
    {
        protected Material mat;
        protected Vector2Int[] offsets;

        [SerializeField]// SerializeFieldÇ…Ç∑ÇÈÇ±Ç∆Ç≈ÉQÅ[ÉÄäJénå„Ç‡ílÇ™ï€éùÇ≥ÇÍÇÈÇÊÇ§Ç…Ç∑ÇÈ
        protected List<GameObject> quads;
        protected Vector3Int pos;

        public void Initialize(Material mat, Vector2Int[] offsets, Vector3Int pos, MeshFactory meshFactory)
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
                // ÅyébíËÅzÉAÉZÉbÉgÇíºê⁄äÑÇËìñÇƒÇÈèàóùÇîpé~
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

        protected virtual void Generate(MeshFactory meshFactory)
        {

        }
    }
}