public static partial class Constants
{
    public const float VOXEL_SIZE_UNITS = .05f;
        
    public const int CHUNK_SIZE_1 = 32;
    public const int CHUNK_SIZE_2 = CHUNK_SIZE_1 * CHUNK_SIZE_1;
    public const int CHUNK_SIZE_3 = CHUNK_SIZE_2 * CHUNK_SIZE_1;
        
    public const int INDICES_PER_HEX = 12 * 3;
    public const int VERTICES_PER_HEX = 24;
}