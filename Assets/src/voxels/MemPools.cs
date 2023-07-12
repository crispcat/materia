using System;
using UnityEngine;
using System.Linq;
using Unity.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public static partial class MemPools
{
    private const int VERTEX_BUFFER_KINDS = 7;
    
    private static readonly int[] vertexBufferSizes = new int[VERTEX_BUFFER_KINDS]
    {
        24,
        648,
        1_944,
        5_832,
        17_496,
        52_488,
        393_216,
    };

    private static readonly int[] vertexBuffersCount = new int[VERTEX_BUFFER_KINDS]
    {
        16384,
        8192,
        4096,
        2048,
        1024,
        32,
        16,
    };

    private static readonly Stack<NativeArray<Vector3>>[] vertexBufferPools = new Stack<NativeArray<Vector3>>[VERTEX_BUFFER_KINDS]
    {
        new(Enumerable.Repeat(new NativeArray<Vector3>(vertexBufferSizes[0], Allocator.Persistent), vertexBuffersCount[0])),
        new(Enumerable.Repeat(new NativeArray<Vector3>(vertexBufferSizes[1], Allocator.Persistent), vertexBuffersCount[1])),
        new(Enumerable.Repeat(new NativeArray<Vector3>(vertexBufferSizes[2], Allocator.Persistent), vertexBuffersCount[2])),
        new(Enumerable.Repeat(new NativeArray<Vector3>(vertexBufferSizes[3], Allocator.Persistent), vertexBuffersCount[3])),
        new(Enumerable.Repeat(new NativeArray<Vector3>(vertexBufferSizes[4], Allocator.Persistent), vertexBuffersCount[4])),
        new(Enumerable.Repeat(new NativeArray<Vector3>(vertexBufferSizes[5], Allocator.Persistent), vertexBuffersCount[5])),
        new(Enumerable.Repeat(new NativeArray<Vector3>(vertexBufferSizes[6], Allocator.Persistent), vertexBuffersCount[6])),
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static NativeArray<Vector3> GetVertexBufferArr(int kind)
    {
        return vertexBufferPools[kind].TryPop(out var buff) ? buff
            : new NativeArray<Vector3>(vertexBufferSizes[kind], Allocator.Persistent);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static VertexBuffer AllocVertexBuffer(int kind)
    {
        return new VertexBuffer(GetVertexBufferArr(kind), kind);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ReturnVertexBuffer(ref VertexBuffer buff)
    {
        vertexBufferPools[buff.kind].Push(buff.mem);
        buff = default;
    }
}

public static partial class MemPools
{
    public struct VertexBuffer
    {
        public int kind;
        private int writeIndex;
        public NativeArray<Vector3> mem;

        public VertexBuffer(NativeArray<Vector3> mem, int kind = 0)
        {
            this.mem = mem;
            this.kind = kind;
            writeIndex = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(Vector3 vertex)
        {
            mem[writeIndex++] = vertex;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExtendIfNeed()
        {
            if (writeIndex != vertexBufferSizes[kind])
                return;
            if (kind == VERTEX_BUFFER_KINDS)
                throw new Exception($"Cannot realloc a bigger buffer. Buffer already have max size {writeIndex}. Kind: {kind}");
            vertexBufferPools[kind].Push(mem);
            mem = GetVertexBufferArr(++kind);
        }
    }
}