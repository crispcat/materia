using System;

/// <summary>
/// Represent fitting-long hexagon triangles immutable sequence to copy from to GPU mem.
/// </summary>
public static class Triangles
{
    public static readonly int[] Sequence;
    public const int HEX_TRIANGLES_INTS = 12 * 3;

    static Triangles()
    {
        // 12 triangles as vertex buffer indices
        Span<int> hexagonTriangles = stackalloc int[HEX_TRIANGLES_INTS]
        {
            0, 2, 1,
            0, 3, 2,
            2, 3, 4,
            2, 4, 5,
            1, 2, 5,
            1, 5, 6,
            0, 7, 4,
            0, 4, 3,
            5, 4, 7,
            5, 7, 6,
            0, 6, 7,
            0, 1, 6
        };
        // max triangles in chunk
        // hypothetical situation when u have lone 32 * 32 * 32 / 2 voxels
        // in one chunk without neighbours
        const int maxTrianglesInChunk = HEX_TRIANGLES_INTS * Chunk.SIZE_3 / 2;
        Sequence = new int[maxTrianglesInChunk];
        for (int i = 0; i < maxTrianglesInChunk; i++)
            Sequence[i] = hexagonTriangles[i % HEX_TRIANGLES_INTS] + i / HEX_TRIANGLES_INTS * 8;
    }
}