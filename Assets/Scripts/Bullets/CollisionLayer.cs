using UnityEngine;

public enum CollisionLayer
{
    Default = 1 << 0,
    //Wall = 1 << 6,
    Enemy = 1 << 7,
}

public class LayerMaskHelper
{
    public static uint GetLayerMaskFromLayers(CollisionLayer layer1)
    {
        return (uint)layer1;
    }
}
