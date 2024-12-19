using UnityEngine;

public static class UnityExtensions
{
    // TODO: Documentation
    public static bool ContainsLayer(LayerMask layerMask, int targetLayer)
    {
        return layerMask == (layerMask | (1 << targetLayer));
    }
}
