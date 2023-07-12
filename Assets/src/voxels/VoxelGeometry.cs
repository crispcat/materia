namespace Voxels
{
    using System;
    using UnityEngine;
    using Unity.Collections;
    using System.Runtime.CompilerServices;
    using static UnityEngine.Vector3;
    using static Constants;
    
    public static class VoxelGeometry
    {
        public static readonly int[] Triangles;
        public static readonly Vector3[] Normals;

        static VoxelGeometry()
        {
            // 12 triangles as vertex buffer indices
            Span<int> triangles = stackalloc int[INDICES_PER_HEX]
            {
                0,   3,   6,
                0,   6,   9,
                4,   18,  15,
                4,   18,  7,
                12,  19,  16,
                12,  19,  21,
                1,   22,  13,
                1,   22,  10,
                11,  20,  8,
                11,  20,  23,
                2,   17,  5,
                2,   17,  14
            };
            // max triangles in chunk
            // hypothetical situation when u have lone 32 * 32 * 32 / 2 voxels
            // in one chunk without neighbours
            const int maxTrianglesInChunk = INDICES_PER_HEX * CHUNK_SIZE_3 / 2;
            Triangles = new int[maxTrianglesInChunk];
            for (int i = 0; i < maxTrianglesInChunk; i++)
                Triangles[i] = triangles[i % INDICES_PER_HEX] + i / INDICES_PER_HEX * VERTICES_PER_HEX;

            Span<Vector3> normals = stackalloc Vector3[VERTICES_PER_HEX]
            {
                back,    left,    down,
                back,    right,   down,
                back,    right,   up,
                back,    left,    up,
                forward, left,    down,
                right,   forward, down,
                right,   forward, up,
                forward, left,    up
            };

            const int maxNormalsInChunk = VERTICES_PER_HEX * CHUNK_SIZE_3 / 2;
            Normals = new Vector3[maxNormalsInChunk];
            for (int i = 0; i < maxNormalsInChunk; i++)
                Normals[i] = normals[i % VERTICES_PER_HEX];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddVertices(MemPools.VertexBuffer vertexBuffer, Vector3Int startIndex, Vector3Int size)
        {
            int x = size.x;
            int y = size.y;
            int z = size.z;

            const float s = VOXEL_SIZE_UNITS;

            // 0 1 2
            var start = new Vector3(startIndex.x * s, startIndex.y * s, startIndex.z * s);
            vertexBuffer.Write(start);
            vertexBuffer.Write(start);
            vertexBuffer.Write(start);

            // 3 4 5
            var sx = start + new Vector3(s * x, 0f, 0f);
            vertexBuffer.Write(sx);
            vertexBuffer.Write(sx);
            vertexBuffer.Write(sx);

            // 6 7 8
            var sxsy = start + new Vector3(s * x, s * y, 0f);
            vertexBuffer.Write(sxsy);
            vertexBuffer.Write(sxsy);
            vertexBuffer.Write(sxsy);

            // 9 10 11
            var sy = start + new Vector3(0f, s * y, 0f);
            vertexBuffer.Write(sy);
            vertexBuffer.Write(sy);
            vertexBuffer.Write(sy);

            // 12 13 14
            var sz = start + new Vector3(0f, 0f, s * z);
            vertexBuffer.Write(sz);
            vertexBuffer.Write(sz);
            vertexBuffer.Write(sz);

            // 15 16 17
            var sxsz = start + new Vector3(s * x, 0f, s * z);
            vertexBuffer.Write(sxsz);
            vertexBuffer.Write(sxsz);
            vertexBuffer.Write(sxsz);

            // 18 19 20
            var sxsysz = start + new Vector3(s * x, s * y, s * z);
            vertexBuffer.Write(sxsysz);
            vertexBuffer.Write(sxsysz);
            vertexBuffer.Write(sxsysz);

            // 21 22 23
            var sysz = start + new Vector3(0f, s * y, s * z);
            vertexBuffer.Write(sysz);
            vertexBuffer.Write(sysz);
            vertexBuffer.Write(sysz);

            vertexBuffer.ExtendIfNeed();
        }

        public static void FlushMesh(Mesh mesh, NativeArray<Vector3> vertices, int vertCount)
        {
            int indCount = vertCount / VERTICES_PER_HEX * INDICES_PER_HEX;
            mesh.SetVertices(vertices, 0, vertCount);
            mesh.SetTriangles(Triangles, 0, indCount, submesh: 0);
            mesh.SetNormals(Normals, 0, vertCount);
        }
    }
}