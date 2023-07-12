namespace Voxels
{
    using UnityEngine;
    using Unity.Collections;
    using System.Diagnostics;
    using UnityEngine.Rendering;
    using System.Runtime.CompilerServices;
    using Debug = UnityEngine.Debug;
    using static Constants;

    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class VoxelChunk : MonoBehaviour
    {
        [HideInInspector] 
        public Mesh mesh;
        public NativeArray<Voxel> voxels;
        private MemPools.VertexBuffer vertexBuffer;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FlatIndex(int x, int y, int z) => z * CHUNK_SIZE_2 + y * CHUNK_SIZE_1 + x;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FlatIndex(Vector3Int pos) => pos.z * CHUNK_SIZE_2 + pos.y * CHUNK_SIZE_1 + pos.x;

        public ref Voxel this[int x, int y, int z] => ref voxels.GetRef(FlatIndex(x, y, z));

        public ref Voxel this[Vector3Int pos] => ref voxels.GetRef(FlatIndex(pos));

        public ref Voxel this[int index] => ref voxels.GetRef(index);

        public void Awake()
        {
            voxels = new NativeArray<Voxel>(CHUNK_SIZE_3, Allocator.Persistent);
            GetComponent<MeshFilter>().mesh = mesh = new Mesh
            {
                name = $"{gameObject.name}_chunk",
                indexFormat = IndexFormat.UInt16
            };
            RenderMesh();
        }

        #if DEBUG
        private Stopwatch stopwatch = new();
        #endif
        public void FixedUpdate()
        {
            #if DEBUG
            stopwatch.Restart();
            #endif
            
            RenderMesh();
            
            #if DEBUG
            stopwatch.Stop();
            Debug.Log($"Chunk {gameObject.name} built in {stopwatch.Elapsed.Milliseconds.ToString()} ms.");
            #endif
        }

        private void RenderMesh()
        {
            var genMeshJob = new GenMeshJob
            {
                voxels = voxels,
                vertexBuffer = vertexBuffer,
                state = MeshGeneratorState.Alloc(),
            };
        }
    }
}