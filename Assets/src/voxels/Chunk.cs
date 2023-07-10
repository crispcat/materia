using UnityEngine;
using System.Diagnostics;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Debug = UnityEngine.Debug;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Chunk : MonoBehaviour
{
    public const int SIZE_1 = 32;
    public const int SIZE_2 = SIZE_1 * SIZE_1;
    public const int SIZE_3 = SIZE_2 * SIZE_1;
    
    [HideInInspector]
    public Mesh mesh;
    
    [HideInInspector]
    public Voxel[] voxels;
    
    [HideInInspector]
    public List<Vector3> vertices;

    public void Awake()
    {
        voxels = new Voxel[SIZE_3];
        vertices = new List<Vector3>(SIZE_3 / 10);
        mesh = new Mesh { name = $"{gameObject.name}_chunk" };
        mesh.indexFormat = IndexFormat.UInt16;
        GetComponent<MeshFilter>().mesh = mesh;
    }

#if DEBUG
    private Stopwatch stopwatch = new();
#endif
    
    public void FixedUpdate()
    {
#if DEBUG
        stopwatch.Restart();
#endif
        vertices.Clear();
        GenMesh();
        FlushMesh();
#if DEBUG
        stopwatch.Stop();
        Debug.Log($"Chunk built in {stopwatch.Elapsed.Milliseconds.ToString()} ms.");
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FlatIndex(int x, int y, int z) => z * SIZE_2 + y * SIZE_1 + x;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FlatIndex(Vector3Int pos) => pos.z * SIZE_2 + pos.y * SIZE_1 + pos.x;
    
    public ref Voxel this[int x, int y, int z] => ref voxels[FlatIndex(x, y, z)];

    public ref Voxel this[Vector3Int pos] => ref voxels[FlatIndex(pos)];
    
    public ref Voxel this[int index] => ref voxels[index];

    public void GenMesh()
    {
        var state = MeshGenerator.AllocState();
        for (int z = 0; z < SIZE_1; z++)
        for (int y = 0; y < SIZE_1; y++)
        for (int x = 0; x < SIZE_1; x++)
            if (this[x, y, z].mat != Material.Empty && !state.included[FlatIndex(x, y, z)])
                MeshGenerator.FindAndAllocMesh(this, new Vector3Int(x, y, z), state);
        MeshGenerator.FreeState(state);
    }

    public void FlushMesh()
    {
        mesh.SetVertices(vertices);
        int trianglesCount = vertices.Count / 8 * 12 * 3;
        mesh.SetTriangles(Triangles.Sequence, trianglesStart: 0, trianglesCount, submesh: 0);
    }
}