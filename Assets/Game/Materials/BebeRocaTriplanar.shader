Shader "Game/Bebe Roca Triplanar"
{
    Properties
    {
        [MainColor] _BaseColor("Color", Color) = (1,1,1,1)
        [MainTexture] _BaseMap("Roca - color", 2D) = "white" {}
        [Normal] _NormalMap("Roca - normales", 2D) = "bump" {}
        _ArmMap("Roca - oclusion, rugosidad y metal", 2D) = "white" {}
        _AoMap("Roca - oclusion", 2D) = "white" {}
        _RoughMap("Roca - rugosidad", 2D) = "white" {}
        _HeightMap("Roca - relieve", 2D) = "gray" {}
        _Tiling("Repeticiones por metro", Range(1,80)) = 18
        _NormalStrength("Intensidad del relieve", Range(0,2)) = 0.7
        _HeightContrast("Contraste del relieve", Range(0,0.5)) = 0.12
        _SmoothnessScale("Intensidad del brillo", Range(0,1)) = 0.45
        [HideInInspector] _Cutoff("Cutoff", Float) = 0.5
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceData.hlsl"

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_NormalMap); SAMPLER(sampler_NormalMap);
            TEXTURE2D(_ArmMap); SAMPLER(sampler_ArmMap);
            TEXTURE2D(_AoMap); SAMPLER(sampler_AoMap);
            TEXTURE2D(_RoughMap); SAMPLER(sampler_RoughMap);
            TEXTURE2D(_HeightMap); SAMPLER(sampler_HeightMap);
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float _Tiling;
                half _NormalStrength;
                half _HeightContrast;
                half _SmoothnessScale;
                half _Cutoff;
            CBUFFER_END
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionOS : TEXCOORD0;
                half3 normalOS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                half fogFactor : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };
            Varyings Vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                VertexPositionInputs pos = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = pos.positionCS;
                output.positionWS = pos.positionWS;
                output.positionOS = input.positionOS.xyz;
                output.normalOS = input.normalOS;
                output.fogFactor = ComputeFogFactor(pos.positionCS.z);
                return output;
            }
            #define TRIPLANAR(tex, samp, p, w) (SAMPLE_TEXTURE2D(tex, samp, (p).zy) * (w).x + SAMPLE_TEXTURE2D(tex, samp, (p).xz) * (w).y + SAMPLE_TEXTURE2D(tex, samp, (p).xy) * (w).z)
            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float3 normalOS = normalize(input.normalOS);
                float3 weights = pow(abs(normalOS), 4.0);
                weights /= max(weights.x + weights.y + weights.z, 0.0001);
                float4x4 objectToWorld = GetObjectToWorldMatrix();
                float3 objectScale = float3(length(objectToWorld._m00_m10_m20), length(objectToWorld._m01_m11_m21), length(objectToWorld._m02_m12_m22));
                float3 p = input.positionOS * objectScale * _Tiling;
                half4 color = TRIPLANAR(_BaseMap, sampler_BaseMap, p, weights) * _BaseColor;
                half3 arm = TRIPLANAR(_ArmMap, sampler_ArmMap, p, weights).rgb;
                half ao = TRIPLANAR(_AoMap, sampler_AoMap, p, weights).r;
                half rough = TRIPLANAR(_RoughMap, sampler_RoughMap, p, weights).r;
                half height = TRIPLANAR(_HeightMap, sampler_HeightMap, p, weights).r;
                half3 nx = UnpackNormalScale(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, p.zy), _NormalStrength);
                half3 ny = UnpackNormalScale(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, p.xz), _NormalStrength);
                half3 nz = UnpackNormalScale(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, p.xy), _NormalStrength);
                float3 signs = float3(normalOS.x < 0 ? -1 : 1, normalOS.y < 0 ? -1 : 1, normalOS.z < 0 ? -1 : 1);
                float3 detail = weights.x * float3(0, nx.y, nx.x * signs.x)
                              + weights.y * float3(ny.x, 0, ny.y * signs.y)
                              + weights.z * float3(nz.x * signs.z, nz.y, 0);
                half3 normalWS = NormalizeNormalPerPixel(TransformObjectToWorldNormal(normalize(normalOS + detail)));
                SurfaceData surface = (SurfaceData)0;
                surface.albedo = color.rgb * (1.0 + (height - 0.5) * _HeightContrast);
                surface.metallic = saturate(arm.b);
                surface.specular = half3(0.04, 0.04, 0.04);
                surface.smoothness = saturate((1.0 - (rough + arm.g) * 0.5) * _SmoothnessScale);
                surface.occlusion = saturate((ao + arm.r) * 0.5);
                surface.alpha = 1;
                surface.normalTS = half3(0,0,1);
                InputData lighting = (InputData)0;
                lighting.positionWS = input.positionWS;
                lighting.normalWS = normalWS;
                lighting.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                lighting.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                lighting.fogCoord = input.fogFactor;
                lighting.vertexLighting = VertexLighting(input.positionWS, normalWS);
                lighting.bakedGI = SampleSH(normalWS);
                lighting.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
                lighting.shadowMask = half4(1,1,1,1);
                half4 result = UniversalFragmentPBR(lighting, surface);
                result.rgb = MixFog(result.rgb, input.fogFactor);
                return result;
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
