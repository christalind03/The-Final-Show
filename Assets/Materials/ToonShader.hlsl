// This toon shader was created by MinionsArt, whose work can be found on Patreon and YouTube.
// https://www.patreon.com/posts/lit-toon-shader-54740865
// https://www.youtube.com/watch?v=FIP6I1x6lMA
void ToonShader_float(
    in float Offset,
    in float Smoothness,
    in float3 Normal,
    in float3 ObjectPosition,
    in float3 WorldPosition,
    in float4 Tinting,
    out float3 Output,
    out float ShadowAttenuation
)
{
    #ifdef SHADERGRAPH_PREVIEW
        Output = float3(0.5, 0.5, 0);
        ShadowAttenuation = 0;
    #else
        #if SHADOWS_SCREEN
            half4 shadowCoordinates = ComputeScreenPos(ObjectPosition);
        #else
            half4 shadowCoordinates = TransformWorldToShadowCoord(WorldPosition);
        #endif

        #if _MAIN_LIGHT_SHADOWS || _MAIN_LIGHT_SHADOWS_CASCADE
            Light lightSource = GetMainLight(shadowCoordinates);
        #else
            Light lightSource = GetMainLight();
        #endif

        half dotProduct = dot(Normal, lightSource.direction) * 0.5 + 0.5;
        half colorTransition = smoothstep(Offset, Offset + Smoothness, dotProduct);

        colorTransition *= lightSource.shadowAttenuation;

        Output = lightSource.color * (colorTransition + Tinting.xyz);
        ShadowAttenuation = lightSource.shadowAttenuation;
    #endif
}