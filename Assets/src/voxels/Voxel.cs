namespace Voxels
{
    using System;
    [Serializable]
    public struct Voxel
    {
        public byte health;
        public VoxelMaterialKind material;
    }
}