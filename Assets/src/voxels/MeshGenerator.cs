namespace Voxels
{
    using System;
    using Unity.Jobs;
    using Unity.Burst;
    using UnityEngine;
    using Unity.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using static Constants;

    public struct MeshGeneratorState
    {
        public bool[] included;

        private static Stack<MeshGeneratorState> pool;

        static MeshGeneratorState()
        {
            pool = new Stack<MeshGeneratorState>(Environment.ProcessorCount);
            for (int i = 0; i < Environment.ProcessorCount; i++)
                pool.Push(new MeshGeneratorState { included = new bool[CHUNK_SIZE_3] });
        }

        public static MeshGeneratorState Alloc()
        {
            return pool.TryPeek(out var data)
                ? data
                : new MeshGeneratorState { included = new bool[CHUNK_SIZE_3] };
        }

        public static void Free(MeshGeneratorState state)
        {
            pool.Push(state);
            Array.Clear(state.included, 0, state.included.Length);
        }
    }

    [BurstCompile]
    public class GenMeshJob : IJob
    {
        public int vertexCount;
        public MeshGeneratorState state;
        public NativeArray<Voxel> voxels;
        public MemPools.VertexBuffer vertexBuffer;

        public void Execute()
        {
            for (int z = 0; z < CHUNK_SIZE_1; z++)
            for (int y = 0; y < CHUNK_SIZE_1; y++)
            for (int x = 0; x < CHUNK_SIZE_1; x++)
            {
                var i = VoxelChunk.FlatIndex(x, y, z);
                if (voxels[i].material != VoxelMaterialKind.Empty && !state.included[i])
                    FindHex(new Vector3Int(x, y, z));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void FindHex(Vector3Int startVoxel)
        {
            var size = new Vector3Int();

            for (int ix = startVoxel.x + 1; ix < CHUNK_SIZE_1; ix++)
            {
                int i = VoxelChunk.FlatIndex(ix, startVoxel.y, startVoxel.z);
                if (voxels[i].material == VoxelMaterialKind.Empty || state.included[i])
                    break;
                state.included[i] = true;
                size.x++;
            }

            for (int iy = startVoxel.y + 1; iy < CHUNK_SIZE_1; iy++)
            {
                bool included = true;
                for (int ix = startVoxel.x; ix < CHUNK_SIZE_1; ix++)
                {
                    int i = VoxelChunk.FlatIndex(ix, iy, startVoxel.z);
                    if (voxels[i].material != VoxelMaterialKind.Empty && !state.included[i]) continue;
                    included = false;
                    break;
                }

                if (!included) break;
                for (int ix = startVoxel.x; ix < CHUNK_SIZE_1; ix++)
                    state.included[VoxelChunk.FlatIndex(ix, iy, startVoxel.z)] = true;
                size.y++;
            }

            for (int iz = startVoxel.z + 1; iz < CHUNK_SIZE_1; iz++)
            {
                bool included = true;
                for (int ix = startVoxel.x; ix < CHUNK_SIZE_1; ix++)
                for (int iy = startVoxel.y; iy < CHUNK_SIZE_1; iy++)
                {
                    int i = VoxelChunk.FlatIndex(ix, iy, iz);
                    if (voxels[i].material != VoxelMaterialKind.Empty && !state.included[i]) continue;
                    included = false;
                    break;
                }

                if (!included) break;
                for (int ix = startVoxel.x; ix < CHUNK_SIZE_1; ix++)
                for (int iy = startVoxel.y; iy < CHUNK_SIZE_1; iy++)
                    state.included[VoxelChunk.FlatIndex(ix, iy, iz)] = true;
                size.z++;
            }

            VoxelGeometry.AddVertices(vertexBuffer, startVoxel, size);
        }
    }
}