using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerGetter : MonoBehaviour
{
    public int order;
    public string layerName;
    public int layerId;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        var renderer = GetComponent<MeshRenderer>();
        order = renderer.sortingOrder;
        layerName = renderer.sortingLayerName;
        layerId=renderer.sortingLayerID;
    }
}
