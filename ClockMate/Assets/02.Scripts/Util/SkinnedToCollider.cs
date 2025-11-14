using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SkinnedToCollider : MonoBehaviour
{
    public SkinnedMeshRenderer skinnedMeshRenderer;
    public MeshCollider meshCollider;

    private void OnEnable()
    {
        BakeOnce();
    }

    void BakeOnce()
    {
        Mesh baked = new Mesh();
        skinnedMeshRenderer.BakeMesh(baked);
        meshCollider.sharedMesh = baked;
    }
}
