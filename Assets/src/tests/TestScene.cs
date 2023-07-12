using Voxels;
using UnityEngine;
using static Constants;

public class TestScene : MonoBehaviour
{
    public VoxelChunk testChunk;

    public void Start()
    {
        //testChunk[0, 0, 0].mat = Material.Test;

        for (int i = 0; i < CHUNK_SIZE_3; i++)
            testChunk[i].material = VoxelMaterialKind.Test;
    }
}