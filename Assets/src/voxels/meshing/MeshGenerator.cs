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
    
    public static void FindAndAddMesh(Chunk chunk, Vector3Int startVoxel, MeshGeneratorState state)
    {
        var size = new Vector3Int();

        for (int ix = startVoxel.x + 1; ix < Chunk.SIZE_1; ix++)
        {
            int i = Chunk.FlatIndex(ix, startVoxel.y, startVoxel.z);
            if (chunk[i].mat == Material.Empty || state.included[i])
                break;
            state.included[i] = true;
            size.x++;
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
            size.y++;
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
            size.z++;
        }
        
        Geometry.AddHexVertices(chunk.vertices, startVoxel, size);
    }
}