using System;

[Serializable]
public struct Voxel
{
    public byte hp;
    public Material mat;
    public const float SIZE = .05f;
}