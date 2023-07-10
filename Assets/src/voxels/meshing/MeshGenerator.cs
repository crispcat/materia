using System;
using UnityEngine;
using System.Collections.Generic;

public struct MeshGeneratorState
{
    public bool[] included;

    public MeshGeneratorState Alloc()
    {
        included = new bool[Chunk.SIZE_3];
        return this;
    }

    public MeshGeneratorState Unset()
    {
        Array.Clear(included, 0, included.Length);
        return this;
    }
}

public static class MeshGenerator
{
    private static Stack<MeshGeneratorState> dataPool;

    static MeshGenerator()
    {
        dataPool = new Stack<MeshGeneratorState>(Environment.ProcessorCount);
        for (int i = 0; i < Environment.ProcessorCount; i++)
            dataPool.Push(new MeshGeneratorState().Alloc());
    }

    public static MeshGeneratorState AllocState()
    {
        return dataPool.TryPeek(out var data) ? data.Unset() : new MeshGeneratorState().Alloc();
    }

    public static void FreeState(MeshGeneratorState state)
    {
        dataPool.Push(state);
    }
    
    public static void FindAndAllocMesh(Chunk chunk, Vector3Int startVoxel, MeshGeneratorState state)
    {
        int x = 1;
        int y = 1;
        int z = 1;

        for (int ix = startVoxel.x + 1; ix < Chunk.SIZE_1; ix++)
        {
            int i = Chunk.FlatIndex(ix, startVoxel.y, startVoxel.z);
            if (chunk[i].mat == Material.Empty || state.included[i])
                break;
            state.included[i] = true;
            x++;
        }

        for (int iy = startVoxel.y + 1; iy < Chunk.SIZE_1; iy++)
        {
            bool included = true;
            for (int ix = startVoxel.x; ix < Chunk.SIZE_1; ix++)
            {
                int i = Chunk.FlatIndex(ix, iy, startVoxel.z);
                if (chunk[i].mat != Material.Empty && !state.included[i]) continue;
                included = false;
                break;
            }
            if (!included) break;
            for (int ix = startVoxel.x; ix < Chunk.SIZE_1; ix++)
                state.included[Chunk.FlatIndex(ix, iy, startVoxel.z)] = true;
            y++;
        }

        for (int iz = startVoxel.z + 1; iz < Chunk.SIZE_1; iz++)
        {
            bool included = true;
            for (int ix = startVoxel.x; ix < Chunk.SIZE_1; ix++)
            for (int iy = startVoxel.y; iy < Chunk.SIZE_1; iy++)
            {
                int i = Chunk.FlatIndex(ix, iy, iz);
                if (chunk[i].mat != Material.Empty && !state.included[i]) continue;
                included = false;
                break;
            }
            if (!included) break;
            for (int ix = startVoxel.x; ix < Chunk.SIZE_1; ix++)
            for (int iy = startVoxel.y; iy < Chunk.SIZE_1; iy++)
                state.included[Chunk.FlatIndex(ix, iy, iz)] = true;
            z++;
        }
        
        const float s = Voxel.SIZE;
        var start = new Vector3(startVoxel.x * s, startVoxel.y * s, startVoxel.z * s);
        chunk.vertices.Add(start);
        chunk.vertices.Add(start + new Vector3(s * x, 0f,    0f));
        chunk.vertices.Add(start + new Vector3(s * x, s * y, 0f));
        chunk.vertices.Add(start + new Vector3(0f,    s * y, 0f));
        chunk.vertices.Add(start + new Vector3(0f,    s * y, s * z));
        chunk.vertices.Add(start + new Vector3(s * x, s * y, s * z));
        chunk.vertices.Add(start + new Vector3(s * x, 0f,    s * z));
        chunk.vertices.Add(start + new Vector3(0f,    0f,    s * z));
    }
}