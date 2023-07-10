using UnityEngine;

public class TestScene : MonoBehaviour
{
    public Chunk testChunk;

    public void Start()
    {
        //testChunk[0, 0, 0].mat = Material.Test;

        for (int i = 0; i < Chunk.SIZE_3; i++)
            testChunk[i].mat = Material.Test;
    }
}