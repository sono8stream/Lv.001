using UnityEngine;
public class Cube : MonoBehaviour
{
    [SerializeField, Header("メッシュレンダラー")]
    private MeshRenderer m_meshRenderer;
    public MeshRenderer meshRenderer
    {
        get { return m_meshRenderer; }
    }
    void Start()
    {
        if (meshRenderer != null)
        {
            var meshFilter = meshRenderer.GetComponent<MeshFilter>();
            var mesh = meshFilter.mesh;
            var uvs = new Vector2[mesh.vertices.Length];
            // 1
            uvs[0] = new Vector2(0.0f, 0.5f);
            uvs[1] = new Vector2(0.125f, 0.5f);
            uvs[2] = new Vector2(0.0f, 0.50390625f);
            uvs[3] = new Vector2(0.125f, 0.50390625f);
            // 22
            uvs[4] = new Vector2(0.334f, 0.333f);
            uvs[5] = new Vector2(0.666f, 0.333f);
            uvs[8] = new Vector2(0.334f, 0.0f);
            uvs[9] = new Vector2(0.666f, 0.0f);
            // 3
            uvs[6] = new Vector2(1.0f, 0.0f);
            uvs[7] = new Vector2(0.667f, 0.0f);
            uvs[10] = new Vector2(1.0f, 0.333f);
            uvs[11] = new Vector2(0.667f, 0.333f);
            // 4
            uvs[12] = new Vector2(0.0f, 0.334f);
            uvs[13] = new Vector2(0.0f, 0.666f);
            uvs[14] = new Vector2(0.333f, 0.666f);
            uvs[15] = new Vector2(0.333f, 0.334f);
            // 5
            uvs[16] = new Vector2(0.334f, 0.334f);
            uvs[17] = new Vector2(0.334f, 0.666f);
            uvs[18] = new Vector2(0.666f, 0.666f);
            uvs[19] = new Vector2(0.666f, 0.334f);
            // 6
            uvs[20] = new Vector2(0.667f, 0.334f);
            uvs[21] = new Vector2(0.667f, 0.666f);
            uvs[22] = new Vector2(1.0f, 0.666f);
            uvs[23] = new Vector2(1.0f, 0.334f);
            mesh.uv = uvs;
        }
    }
}