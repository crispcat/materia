using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using static UnityEngine.Vector3;

/// <summary>
/// Represent fitting-long hexagon immutable sequences to copy from to GPU mem.
/// </summary>
public static class Geometry
{
    public const int HEX_INDICIES = 12 * 3;
    public const int VERTICES_PER_VOXEL = 24;
    
    public static readonly int[] Triangles;
    public static readonly Vector3[] Normals;

    static Geometry()
    {
        // 12 triangles as vertex buffer indices
        Span<int> triangles = stackalloc int[HEX_INDICIES]
        {
             0,   3,   6,
             0,   6,   9,
             4,  18,  15,
             4,  18,   7,
            12,  19,  16,
            12,  19,  21,
             1,  22,  13,
             1,  22,  10,
            11,  20,   8,
            11,  20,  23,
             2,  17,   5,
             2,  17,  14
        };
        // max triangles in chunk
        // hypothetical situation when u have lone 32 * 32 * 32 / 2 voxels
        // in one chunk without neighbours
        const int maxTrianglesInChunk = HEX_INDICIES * Chunk.SIZE_3 / 2;
        Triangles = new int[maxTrianglesInChunk];
        for (int i = 0; i < maxTrianglesInChunk; i++)
            Triangles[i] = triangles[i % HEX_INDICIES] + i / HEX_INDICIES * VERTICES_PER_VOXEL;
        
        Span<Vector3> normals = stackalloc Vector3[VERTICES_PER_VOXEL]
        {
            back,     left,     down,
            back,     right,    down,
            back,     right,    up,
            back,     left,     up,
            forward,  left,     down,
            right,    forward,  down,
            right,    forward,  up,
            forward,  left,     up
        };
        
        const int maxNormalsInChunk = VERTICES_PER_VOXEL * Chunk.SIZE_3 / 2;
        Normals = new Vector3[maxNormalsInChunk];
        for (int i = 0; i < maxNormalsInChunk; i++)
            Normals[i] = normals[i % VERTICES_PER_VOXEL];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddHexVertices(List<Vector3> vertices, Vector3Int istart, Vector3Int isize)
    {
        int x = isize.x;
        int y = isize.y;
        int z = isize.z;
        
        const float s = Voxel.SIZE;
        
        // 0 1 2
        var start = new Vector3(istart.x * s, istart.y * s, istart.z * s);
        vertices.Add(start);
        vertices.Add(start);
        vertices.Add(start);
        
        // 3 4 5
        var sx = start + new Vector3(s * x, 0f, 0f);
        vertices.Add(sx);
        vertices.Add(sx);
        vertices.Add(sx);
        
        // 6 7 8
        var sxsy = start + new Vector3(s * x, s * y, 0f);
        vertices.Add(sxsy);
        vertices.Add(sxsy);
        vertices.Add(sxsy);
        
        // 9 10 11
        var sy = start + new Vector3(0f, s * y, 0f);
        vertices.Add(sy);
        vertices.Add(sy);
        vertices.Add(sy);
        
        // 12 13 14
        var sz = start + new Vector3(0f, 0f, s * z);
        vertices.Add(sz);
        vertices.Add(sz);
        vertices.Add(sz);
        
        // 15 16 17
        var sxsz = start + new Vector3(s * x, 0f, s * z);
        vertices.Add(sxsz);
        vertices.Add(sxsz);
        vertices.Add(sxsz);
        
        // 18 19 20
        var sxsysz = start + new Vector3(s * x, s * y, s * z);
        vertices.Add(sxsysz);
        vertices.Add(sxsysz);
        vertices.Add(sxsysz);
        
        // 21 22 23
        var sysz = start + new Vector3(0f, s * y, s * z);
        vertices.Add(sysz);
        vertices.Add(sysz);
        vertices.Add(sysz);
    }
    
    
}