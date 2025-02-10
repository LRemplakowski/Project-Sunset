// Made with Amplify Shader Editor v1.9.8
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Amplify Scripted Decal Shader"
{
	Properties
    {
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		_NormalMap("Normal Map", 2D) = "white" {}
		_BaseMap("Base Map", 2D) = "white" {}
		_MaskMap("Mask Map", 2D) = "white" {}
		_DetailMap("Detail Map", 2D) = "white" {}
		_DetailNormals("Detail Normals", 2D) = "bump" {}
		_AmbientOcclusionRemapping("Ambient Occlusion Remapping", Range( 0 , 1)) = 0.5
		_DetailOpacityOverride("Detail Opacity Override", Range( 0 , 1)) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

		[HideInInspector] _DrawOrder("_DrawOrder", Int) = 0
		[HideInInspector][Enum(Depth Bias, 0, View Bias, 1)] _DecalMeshBiasType("_DecalMeshBiasType", Int) = 0
		[HideInInspector] _DecalMeshDepthBias("_DecalMeshDepthBias", Float) = 0.0
		[HideInInspector] _DecalMeshViewBias("_DecalMeshViewBias", Float) = 0.0
		[HideInInspector] _DecalStencilWriteMask("_DecalStencilWriteMask", Int) = 16
		[HideInInspector] _DecalStencilRef("_DecalStencilRef", Int) = 16
		[HideInInspector][ToggleUI] _AffectAlbedo("Boolean", Float) = 1
		[HideInInspector][ToggleUI] _AffectNormal("Boolean", Float) = 1
        [HideInInspector][ToggleUI] _AffectAO("Boolean", Float) = 1
        [HideInInspector][ToggleUI] _AffectMetal("Boolean", Float) = 1
        [HideInInspector][ToggleUI] _AffectSmoothness("Boolean", Float) = 1
        [HideInInspector][ToggleUI] _AffectEmission("Boolean", Float) = 1
		[HideInInspector] _DecalColorMask0("_DecalColorMask0", Int) = 0
		[HideInInspector] _DecalColorMask1("_DecalColorMask1", Int) = 0
		[HideInInspector] _DecalColorMask2("_DecalColorMask2", Int) = 0
		[HideInInspector] _DecalColorMask3("_DecalColorMask3", Int) = 0

        [HideInInspector][NoScaleOffset] unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset] unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset] unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}

		[HideInInspector] _DecalBlend("_DecalBlend", Range(0.0, 1.0)) = 0.5
		[HideInInspector] _NormalBlendSrc("_NormalBlendSrc", Float) = 0.0
		[HideInInspector] _MaskBlendSrc("_MaskBlendSrc", Float) = 1.0
		[HideInInspector] _DecalMaskMapBlueScale("_DecalMaskMapBlueScale", Range(0.0, 1.0)) = 1.0

		//[HideInInspector]_Unity_Identify_HDRP_Decal("_Unity_Identify_HDRP_Decal", Float) = 1.0
	}

    SubShader
    {
		LOD 0

		
        Tags { "RenderPipeline"="HDRenderPipeline" "RenderType"="Opaque" "Queue"="Geometry" }

		HLSLINCLUDE
		#pragma target 4.5
		#pragma exclude_renderers glcore gles gles3 ps4 ps5 
		#pragma multi_compile_instancing
		#pragma instancing_options renderinglayer

		#define SUPPORT_GLOBAL_MIP_BIAS

		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"

		struct SurfaceDescription
        {
            float3 BaseColor;
            float Alpha;
            float3 NormalTS;
            float NormalAlpha;
            float Metallic;
            float Occlusion;
            float Smoothness;
            float MAOSAlpha;
			float3 Emission;
        };
		ENDHLSL

		
        Pass
		{
			
			Name "DBufferProjector"
			Tags { "LightMode"="DBufferProjector" }

            Stencil
            {
            	Ref [_DecalStencilRef]
            	WriteMask [_DecalStencilWriteMask]
            	Comp Always
            	Pass Replace
            }


			Cull Front
			ZWrite Off
			ZTest Greater

			Blend 0 SrcAlpha OneMinusSrcAlpha, Zero OneMinusSrcAlpha
			Blend 1 SrcAlpha OneMinusSrcAlpha, Zero OneMinusSrcAlpha
			Blend 2 SrcAlpha OneMinusSrcAlpha, Zero OneMinusSrcAlpha
			Blend 3 Zero OneMinusSrcColor

			ColorMask[_DecalColorMask0]
			ColorMask[_DecalColorMask1] 1
			ColorMask[_DecalColorMask2] 2
			ColorMask[_DecalColorMask3] 3

			HLSLPROGRAM

            #pragma shader_feature_local_fragment _MATERIAL_AFFECTS_ALBEDO
            #pragma shader_feature_local_fragment _COLORMAP
            #pragma shader_feature_local_fragment _MATERIAL_AFFECTS_NORMAL
            #pragma shader_feature_local_fragment _MATERIAL_AFFECTS_MASKMAP
            #define _MATERIAL_AFFECTS_EMISSION
            #pragma shader_feature_local_fragment _EMISSIVEMAP
            #pragma shader_feature_local_fragment _MASKMAP
            #define ASE_VERSION 19800
            #define ASE_SRP_VERSION 170003


            #pragma vertex Vert
            #pragma fragment Frag

			#pragma multi_compile_fragment DECALS_3RT DECALS_4RT
			#pragma multi_compile_fragment _ DECAL_SURFACE_GRADIENT

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/NormalSurfaceGradient.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/Functions.hlsl"

			#define SHADERPASS SHADERPASS_DBUFFER_PROJECTOR
			#define SUPPORT_GLOBAL_MIP_BIAS

            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/Decal.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/DecalPrepassBuffer.hlsl"

			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0


            struct AttributesMesh
            {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
				
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

      		struct PackedVaryingsToPS
			{
				float4 positionCS : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_RELATIVE_WORLD_POS)
				float3 positionRWS : TEXCOORD0;
				#endif
				
                UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

            CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            float4 _DetailMap_ST;
            float4 _NormalMap_ST;
            float4 _DetailNormals_ST;
            float4 _MaskMap_ST;
            float _DetailOpacityOverride;
            float _AmbientOcclusionRemapping;
            float _DrawOrder;
			float _NormalBlendSrc;
			float _MaskBlendSrc;
			float _DecalBlend;
			int   _DecalMeshBiasType;
            float _DecalMeshDepthBias;
			float _DecalMeshViewBias;
            float _DecalStencilWriteMask;
            float _DecalStencilRef;
			#ifdef _MATERIAL_AFFECTS_ALBEDO
            float _AffectAlbedo;
			#endif
			#ifdef _MATERIAL_AFFECTS_NORMAL
            float _AffectNormal;
			#endif
            #ifdef _MATERIAL_AFFECTS_MASKMAP
            float _AffectAO;
			float _AffectMetal;
            float _AffectSmoothness;
			#endif
			#ifdef _MATERIAL_AFFECTS_EMISSION
            float _AffectEmission;
			#endif
            float _DecalColorMask0;
            float _DecalColorMask1;
            float _DecalColorMask2;
            float _DecalColorMask3;
            CBUFFER_END

			sampler2D _BaseMap;
			float4 GlobalTint;
			float GlobalContrast;
			sampler2D _DetailMap;
			float GlobalAlbedoOpacity;
			sampler2D _NormalMap;
			sampler2D _DetailNormals;
			float DetailNormalStrength;
			sampler2D _MaskMap;


            #if (SHADERPASS == SHADERPASS_DBUFFER_MESH) || (SHADERPASS == SHADERPASS_FORWARD_EMISSIVE_MESH)
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT // Always present as we require it also in case of anisotropic lighting
            #define ATTRIBUTES_NEED_TEXCOORD0

            #define VARYINGS_NEED_POSITION_WS
            #define VARYINGS_NEED_TANGENT_TO_WORLD
            #define VARYINGS_NEED_TEXCOORD0
            #endif

			
            void GetSurfaceData(SurfaceDescription surfaceDescription, FragInputs fragInputs, float3 V, PositionInputs posInput, float angleFadeFactor, out DecalSurfaceData surfaceData)
            {
                #if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_FORWARD_EMISSIVE_PROJECTOR)
                    float4x4 normalToWorld = UNITY_ACCESS_INSTANCED_PROP(Decal, _NormalToWorld);
                    float fadeFactor = clamp(normalToWorld[0][3], 0.0f, 1.0f) * angleFadeFactor;
                    float2 scale = float2(normalToWorld[3][0], normalToWorld[3][1]);
                    float2 offset = float2(normalToWorld[3][2], normalToWorld[3][3]);
                    fragInputs.texCoord0.xy = fragInputs.texCoord0.xy * scale + offset;
                    fragInputs.texCoord1.xy = fragInputs.texCoord1.xy * scale + offset;
                    fragInputs.texCoord2.xy = fragInputs.texCoord2.xy * scale + offset;
                    fragInputs.texCoord3.xy = fragInputs.texCoord3.xy * scale + offset;
                    fragInputs.positionRWS = posInput.positionWS;
                    fragInputs.tangentToWorld[2].xyz = TransformObjectToWorldDir(float3(0, 1, 0));
                    fragInputs.tangentToWorld[1].xyz = TransformObjectToWorldDir(float3(0, 0, 1));
                #else
                    #ifdef LOD_FADE_CROSSFADE
                    LODDitheringTransition(ComputeFadeMaskSeed(V, posInput.positionSS), unity_LODFade.x);
                    #endif

                    float fadeFactor = 1.0;
                #endif

                ZERO_INITIALIZE(DecalSurfaceData, surfaceData);

                #ifdef _MATERIAL_AFFECTS_EMISSION
                #endif

                #ifdef _MATERIAL_AFFECTS_ALBEDO
                    surfaceData.baseColor.xyz = surfaceDescription.BaseColor;
                    surfaceData.baseColor.w = surfaceDescription.Alpha * fadeFactor;
                #endif

                #ifdef _MATERIAL_AFFECTS_NORMAL
                    #ifdef DECAL_SURFACE_GRADIENT
                        #if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_FORWARD_EMISSIVE_PROJECTOR)
                            float3x3 tangentToWorld = transpose((float3x3)normalToWorld);
                        #else
                            float3x3 tangentToWorld = fragInputs.tangentToWorld;
                        #endif

                        surfaceData.normalWS.xyz = SurfaceGradientFromTangentSpaceNormalAndFromTBN(surfaceDescription.NormalTS.xyz, tangentToWorld[0], tangentToWorld[1]);
                    #else
                        #if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR)
                            surfaceData.normalWS.xyz = mul((float3x3)normalToWorld, surfaceDescription.NormalTS);
                        #elif (SHADERPASS == SHADERPASS_DBUFFER_MESH) || (SHADERPASS == SHADERPASS_FORWARD_PREVIEW)

                            surfaceData.normalWS.xyz = normalize(TransformTangentToWorld(surfaceDescription.NormalTS, fragInputs.tangentToWorld));
                        #endif
                    #endif

                    surfaceData.normalWS.w = surfaceDescription.NormalAlpha * fadeFactor;
                #else
                    #if (SHADERPASS == SHADERPASS_FORWARD_PREVIEW)
                        #ifdef DECAL_SURFACE_GRADIENT
                            surfaceData.normalWS.xyz = float3(0.0, 0.0, 0.0);
                        #else
                            surfaceData.normalWS.xyz = normalize(TransformTangentToWorld(float3(0.0, 0.0, 0.1), fragInputs.tangentToWorld));
                        #endif
                    #endif
                #endif

                #ifdef _MATERIAL_AFFECTS_MASKMAP
                    surfaceData.mask.z = surfaceDescription.Smoothness;
                    surfaceData.mask.w = surfaceDescription.MAOSAlpha * fadeFactor;

                    #ifdef DECALS_4RT
                        surfaceData.mask.x = surfaceDescription.Metallic;
                        surfaceData.mask.y = surfaceDescription.Occlusion;
                        surfaceData.MAOSBlend.x = surfaceDescription.MAOSAlpha * fadeFactor;
                        surfaceData.MAOSBlend.y = surfaceDescription.MAOSAlpha * fadeFactor;
                    #endif

                #endif
            }

			PackedVaryingsToPS Vert(AttributesMesh inputMesh  )
			{
				PackedVaryingsToPS output;

				UNITY_SETUP_INSTANCE_ID(inputMesh);
				UNITY_TRANSFER_INSTANCE_ID(inputMesh, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( output );

				inputMesh.tangentOS = float4( 1, 0, 0, -1);
				inputMesh.normalOS = float3( 0, 1, 0 );

				

				inputMesh.normalOS = inputMesh.normalOS;
				inputMesh.tangentOS = inputMesh.tangentOS;

				float3 positionRWS = TransformObjectToWorld(inputMesh.positionOS);
				float3 normalWS = TransformObjectToWorldNormal(inputMesh.normalOS);
				float4 tangentWS = float4(TransformObjectToWorldDir(inputMesh.tangentOS.xyz), inputMesh.tangentOS.w);

				output.positionCS = TransformWorldToHClip(positionRWS);
				#if defined(ASE_NEEDS_FRAG_RELATIVE_WORLD_POS)
				output.positionRWS = positionRWS;
				#endif

				return output;
			}

			void Frag( PackedVaryingsToPS packedInput,
			#if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_DBUFFER_MESH)
				OUTPUT_DBUFFER(outDBuffer)
			#else
				out float4 outEmissive : SV_Target0
			#endif
			
			)
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(packedInput);
				UNITY_SETUP_INSTANCE_ID(packedInput);

				FragInputs input;
                ZERO_INITIALIZE(FragInputs, input);
                input.tangentToWorld = k_identity3x3;
				#if defined(ASE_NEEDS_FRAG_RELATIVE_WORLD_POS)
				input.positionRWS = packedInput.positionRWS;
				#endif

                input.positionSS = packedInput.positionCS;

				DecalSurfaceData surfaceData;
				float clipValue = 1.0;
				float angleFadeFactor = 1.0;

				PositionInputs posInput;
			#if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_FORWARD_EMISSIVE_PROJECTOR)

				float depth = LoadCameraDepth(input.positionSS.xy);
				posInput = GetPositionInput(input.positionSS.xy, _ScreenSize.zw, depth, UNITY_MATRIX_I_VP, UNITY_MATRIX_V);

				DecalPrepassData material;
				ZERO_INITIALIZE(DecalPrepassData, material);
				if (_EnableDecalLayers)
				{
					uint decalLayerMask = uint(UNITY_ACCESS_INSTANCED_PROP(Decal, _DecalLayerMaskFromDecal).x);

					DecodeFromDecalPrepass(posInput.positionSS, material);

					if ((decalLayerMask & material.renderingLayerMask) == 0)
						clipValue -= 2.0;
				}


				float3 positionDS = TransformWorldToObject(posInput.positionWS);
				positionDS = positionDS * float3(1.0, -1.0, 1.0) + float3(0.5, 0.5, 0.5);
				if (!(all(positionDS.xyz > 0.0f) && all(1.0f - positionDS.xyz > 0.0f)))
				{
					clipValue -= 2.0;
				}

			#ifndef SHADER_API_METAL
				clip(clipValue);
			#else
				if (clipValue > 0.0)
				{
			#endif

				float4x4 normalToWorld = UNITY_ACCESS_INSTANCED_PROP(Decal, _NormalToWorld);
				float2 scale = float2(normalToWorld[3][0], normalToWorld[3][1]);
				float2 offset = float2(normalToWorld[3][2], normalToWorld[3][3]);
				positionDS.xz = positionDS.xz * scale + offset;

				input.texCoord0.xy = positionDS.xz;
				input.texCoord1.xy = positionDS.xz;
				input.texCoord2.xy = positionDS.xz;
				input.texCoord3.xy = positionDS.xz;

				float3 V = GetWorldSpaceNormalizeViewDir(posInput.positionWS);
				if (_EnableDecalLayers)
				{
					float2 angleFade = float2(normalToWorld[1][3], normalToWorld[2][3]);

					if (angleFade.x > 0.0f)
					{
						float3 decalNormal = float3(normalToWorld[0].z, normalToWorld[1].z, normalToWorld[2].z);
                        angleFadeFactor = DecodeAngleFade(dot(material.geomNormalWS, decalNormal), angleFade);
					}
				}

			#else
				posInput = GetPositionInput(input.positionSS.xy, _ScreenSize.zw, input.positionSS.z, input.positionSS.w, input.positionRWS.xyz, uint2(0, 0));
				#if defined(ASE_NEEDS_FRAG_RELATIVE_WORLD_POS)
					float3 V = GetWorldSpaceNormalizeViewDir(input.positionRWS);
				#else
					float3 V = float3(1.0, 1.0, 1.0);
				#endif
			#endif

				float3 positionWS = GetAbsolutePositionWS( posInput.positionWS );
				float3 positionRWS = posInput.positionWS;

				float3 worldTangent = TransformObjectToWorldDir(float3(1, 0, 0));
				float3 worldNormal = TransformObjectToWorldDir(float3(0, 1, 0));
				float3 worldBitangent = TransformObjectToWorldDir(float3(0, 0, 1));

				float4 texCoord0 = input.texCoord0;
				float4 texCoord1 = input.texCoord1;
				float4 texCoord2 = input.texCoord2;
				float4 texCoord3 = input.texCoord3;

				SurfaceDescription surfaceDescription = (SurfaceDescription)0;

				float2 uv_BaseMap = texCoord0.xy * _BaseMap_ST.xy + _BaseMap_ST.zw;
				float4 tex2DNode20 = tex2D( _BaseMap, uv_BaseMap );
				float4 blendOpSrc16 = tex2DNode20;
				float4 blendOpDest16 = ( GlobalTint * saturate( ( GlobalContrast + tex2DNode20.r ) ) );
				float2 uv_DetailMap = texCoord0.xy * _DetailMap_ST.xy + _DetailMap_ST.zw;
				float temp_output_24_0 = ( GlobalAlbedoOpacity * _DetailOpacityOverride );
				float4 temp_cast_0 = ((saturate( ( -temp_output_24_0 + 0.5 ) ) + (tex2D( _DetailMap, uv_DetailMap ).r - 0.0) * (saturate( ( temp_output_24_0 + 0.5 ) ) - saturate( ( -temp_output_24_0 + 0.5 ) )) / (1.0 - 0.0))).xxxx;
				float4 blendOpSrc9 = ( saturate( 2.0f*blendOpDest16*blendOpSrc16 + blendOpDest16*blendOpDest16*(1.0f - 2.0f*blendOpSrc16) ));
				float4 blendOpDest9 = temp_cast_0;
				
				float2 uv_NormalMap = texCoord0.xy * _NormalMap_ST.xy + _NormalMap_ST.zw;
				float2 uv_DetailNormals = texCoord0.xy * _DetailNormals_ST.xy + _DetailNormals_ST.zw;
				float3 unpack34 = UnpackNormalScale( tex2D( _DetailNormals, uv_DetailNormals ), DetailNormalStrength );
				unpack34.z = lerp( 1, unpack34.z, saturate(DetailNormalStrength) );
				
				float2 uv_MaskMap = texCoord0.xy * _MaskMap_ST.xy + _MaskMap_ST.zw;
				float4 tex2DNode27 = tex2D( _MaskMap, uv_MaskMap );
				

				surfaceDescription.BaseColor = ( saturate( (( blendOpDest9 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest9 ) * ( 1.0 - blendOpSrc9 ) ) : ( 2.0 * blendOpDest9 * blendOpSrc9 ) ) )).rgb;
				surfaceDescription.Alpha = tex2DNode20.a;
				surfaceDescription.NormalTS = BlendNormal( UnpackNormalScale( tex2D( _NormalMap, uv_NormalMap ), 1.0f ) , unpack34 );
				surfaceDescription.NormalAlpha = tex2DNode27.b;
				surfaceDescription.Metallic = 0;
				surfaceDescription.Occlusion = saturate( ( tex2DNode27.g + _AmbientOcclusionRemapping ) );
				surfaceDescription.Smoothness = ( tex2DNode27.a * tex2DNode20.a );
				surfaceDescription.MAOSAlpha = tex2DNode27.b;
				surfaceDescription.Emission = float3( 0, 0, 0 );

				GetSurfaceData(surfaceDescription, input, V, posInput, angleFadeFactor, surfaceData);

			#if ((SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_FORWARD_EMISSIVE_PROJECTOR)) && defined(SHADER_API_METAL)
				} // if (clipValue > 0.0)

				clip(clipValue);
			#endif

			#if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_DBUFFER_MESH)
				ENCODE_INTO_DBUFFER(surfaceData, outDBuffer);
			#else
				// Emissive need to be pre-exposed
				outEmissive.rgb = surfaceData.emissive * GetCurrentExposureMultiplier();
				outEmissive.a = 1.0;
			#endif
			}

            ENDHLSL
        }

		
        Pass
		{
			
			Name "DecalProjectorForwardEmissive"
			Tags { "LightMode"="DecalProjectorForwardEmissive" }

			Stencil
			{
				Ref [_DecalStencilRef]
				WriteMask [_DecalStencilWriteMask]
				Comp Always
				Pass Replace
			}


			Cull Front
			ZWrite Off
			ZTest Greater

			Blend 0 SrcAlpha One

			HLSLPROGRAM

            #pragma shader_feature_local_fragment _MATERIAL_AFFECTS_ALBEDO
            #pragma shader_feature_local_fragment _COLORMAP
            #pragma shader_feature_local_fragment _MATERIAL_AFFECTS_NORMAL
            #pragma shader_feature_local_fragment _MATERIAL_AFFECTS_MASKMAP
            #define _MATERIAL_AFFECTS_EMISSION
            #pragma shader_feature_local_fragment _EMISSIVEMAP
            #pragma shader_feature_local_fragment _MASKMAP
            #define ASE_VERSION 19800
            #define ASE_SRP_VERSION 170003


            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/NormalSurfaceGradient.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/Functions.hlsl"

			#define SHADERPASS SHADERPASS_FORWARD_EMISSIVE_PROJECTOR
			#define SUPPORT_GLOBAL_MIP_BIAS

            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/Decal.hlsl"

			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/DecalPrepassBuffer.hlsl"

			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0


            struct AttributesMesh
            {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

      		struct PackedVaryingsToPS
			{
				float4 positionCS : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_RELATIVE_WORLD_POS)
				float3 positionRWS : TEXCOORD0;
				#endif
				
                UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

            CBUFFER_START(UnityPerMaterial)
			float4 _BaseMap_ST;
			float4 _DetailMap_ST;
			float4 _NormalMap_ST;
			float4 _DetailNormals_ST;
			float4 _MaskMap_ST;
			float _DetailOpacityOverride;
			float _AmbientOcclusionRemapping;
			float _DrawOrder;
			float _NormalBlendSrc;
			float _MaskBlendSrc;
			float _DecalBlend;
			int   _DecalMeshBiasType;
			float _DecalMeshDepthBias;
			float _DecalMeshViewBias;
            float _DecalStencilWriteMask;
            float _DecalStencilRef;
            #ifdef _MATERIAL_AFFECTS_ALBEDO
            float _AffectAlbedo;
			#endif
            #ifdef _MATERIAL_AFFECTS_NORMAL
            float _AffectNormal;
			#endif
            #ifdef _MATERIAL_AFFECTS_MASKMAP
            float _AffectAO;
			float _AffectMetal;
            float _AffectSmoothness;
			#endif
            #ifdef _MATERIAL_AFFECTS_EMISSION
            float _AffectEmission;
			#endif
            float _DecalColorMask0;
            float _DecalColorMask1;
            float _DecalColorMask2;
            float _DecalColorMask3;
            CBUFFER_END

			sampler2D _BaseMap;
			float4 GlobalTint;
			float GlobalContrast;
			sampler2D _DetailMap;
			float GlobalAlbedoOpacity;
			sampler2D _NormalMap;
			sampler2D _DetailNormals;
			float DetailNormalStrength;
			sampler2D _MaskMap;


			
            void GetSurfaceData(SurfaceDescription surfaceDescription, FragInputs fragInputs, float3 V, PositionInputs posInput, float angleFadeFactor, out DecalSurfaceData surfaceData)
            {
                #if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_FORWARD_EMISSIVE_PROJECTOR)
                    float4x4 normalToWorld = UNITY_ACCESS_INSTANCED_PROP(Decal, _NormalToWorld);
                    float fadeFactor = clamp(normalToWorld[0][3], 0.0f, 1.0f) * angleFadeFactor;
                    float2 scale = float2(normalToWorld[3][0], normalToWorld[3][1]);
                    float2 offset = float2(normalToWorld[3][2], normalToWorld[3][3]);
                    fragInputs.texCoord0.xy = fragInputs.texCoord0.xy * scale + offset;
                    fragInputs.texCoord1.xy = fragInputs.texCoord1.xy * scale + offset;
                    fragInputs.texCoord2.xy = fragInputs.texCoord2.xy * scale + offset;
                    fragInputs.texCoord3.xy = fragInputs.texCoord3.xy * scale + offset;
                    fragInputs.positionRWS = posInput.positionWS;
                    fragInputs.tangentToWorld[2].xyz = TransformObjectToWorldDir(float3(0, 1, 0));
                    fragInputs.tangentToWorld[1].xyz = TransformObjectToWorldDir(float3(0, 0, 1));
                #else
                    #ifdef LOD_FADE_CROSSFADE
                    LODDitheringTransition(ComputeFadeMaskSeed(V, posInput.positionSS), unity_LODFade.x);
                    #endif

                    float fadeFactor = 1.0;
                #endif

                ZERO_INITIALIZE(DecalSurfaceData, surfaceData);

                #ifdef _MATERIAL_AFFECTS_EMISSION
                    surfaceData.emissive.rgb = surfaceDescription.Emission.rgb * fadeFactor;
                #endif
            }

			PackedVaryingsToPS Vert(AttributesMesh inputMesh  )
			{
				PackedVaryingsToPS output;

				UNITY_SETUP_INSTANCE_ID(inputMesh);
				UNITY_TRANSFER_INSTANCE_ID(inputMesh, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( output );

				inputMesh.tangentOS = float4( 1, 0, 0, -1);
				inputMesh.normalOS = float3( 0, 1, 0 );

				

				inputMesh.normalOS = inputMesh.normalOS;
				inputMesh.tangentOS = inputMesh.tangentOS;

				float3 positionRWS = TransformObjectToWorld(inputMesh.positionOS);
				float3 normalWS = TransformObjectToWorldNormal(inputMesh.normalOS);
				float4 tangentWS = float4(TransformObjectToWorldDir(inputMesh.tangentOS.xyz), inputMesh.tangentOS.w);

				output.positionCS = TransformWorldToHClip(positionRWS);
				#if defined(ASE_NEEDS_FRAG_RELATIVE_WORLD_POS)
				output.positionRWS = positionRWS;
				#endif

				return output;
			}

			void Frag( PackedVaryingsToPS packedInput,
			#if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_DBUFFER_MESH)
				OUTPUT_DBUFFER(outDBuffer)
			#else
				out float4 outEmissive : SV_Target0
			#endif
			
			)
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(packedInput);
				UNITY_SETUP_INSTANCE_ID(packedInput);

				FragInputs input;
                ZERO_INITIALIZE(FragInputs, input);
                input.tangentToWorld = k_identity3x3;
				#if defined(ASE_NEEDS_FRAG_RELATIVE_WORLD_POS)
				input.positionRWS = packedInput.positionRWS;
				#endif

                input.positionSS = packedInput.positionCS;

				DecalSurfaceData surfaceData;
				float clipValue = 1.0;
				float angleFadeFactor = 1.0;

				PositionInputs posInput;
			#if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_FORWARD_EMISSIVE_PROJECTOR)

				float depth = LoadCameraDepth(input.positionSS.xy);
				#if (SHADERPASS == SHADERPASS_FORWARD_EMISSIVE_PROJECTOR) && UNITY_REVERSED_Z
					depth = IsSky(depth) ? UNITY_NEAR_CLIP_VALUE : depth;
				#endif

				posInput = GetPositionInput(input.positionSS.xy, _ScreenSize.zw, depth, UNITY_MATRIX_I_VP, UNITY_MATRIX_V);

				DecalPrepassData material;
				ZERO_INITIALIZE(DecalPrepassData, material);
				if (_EnableDecalLayers)
				{
					uint decalLayerMask = uint(UNITY_ACCESS_INSTANCED_PROP(Decal, _DecalLayerMaskFromDecal).x);

					DecodeFromDecalPrepass(posInput.positionSS, material);

					if ((decalLayerMask & material.renderingLayerMask) == 0)
						clipValue -= 2.0;
				}


				float3 positionDS = TransformWorldToObject(posInput.positionWS);
				positionDS = positionDS * float3(1.0, -1.0, 1.0) + float3(0.5, 0.5, 0.5);
				if (!(all(positionDS.xyz > 0.0f) && all(1.0f - positionDS.xyz > 0.0f)))
				{
					clipValue -= 2.0;
				}

			#ifndef SHADER_API_METAL
				clip(clipValue);
			#else
				if (clipValue > 0.0)
				{
			#endif

				float4x4 normalToWorld = UNITY_ACCESS_INSTANCED_PROP(Decal, _NormalToWorld);
				float2 scale = float2(normalToWorld[3][0], normalToWorld[3][1]);
				float2 offset = float2(normalToWorld[3][2], normalToWorld[3][3]);
				positionDS.xz = positionDS.xz * scale + offset;

				input.texCoord0.xy = positionDS.xz;
				input.texCoord1.xy = positionDS.xz;
				input.texCoord2.xy = positionDS.xz;
				input.texCoord3.xy = positionDS.xz;

				float3 V = GetWorldSpaceNormalizeViewDir(posInput.positionWS);
				if (_EnableDecalLayers)
				{
					float2 angleFade = float2(normalToWorld[1][3], normalToWorld[2][3]);

					if (angleFade.x > 0.0f)
					{
						float3 decalNormal = float3(normalToWorld[0].z, normalToWorld[1].z, normalToWorld[2].z);
                        angleFadeFactor = DecodeAngleFade(dot(material.geomNormalWS, decalNormal), angleFade);
					}
				}

			#else
				posInput = GetPositionInput(input.positionSS.xy, _ScreenSize.zw, input.positionSS.z, input.positionSS.w, input.positionRWS.xyz, uint2(0, 0));
				#if defined(ASE_NEEDS_FRAG_RELATIVE_WORLD_POS)
					float3 V = GetWorldSpaceNormalizeViewDir(input.positionRWS);
				#else
					float3 V = float3(1.0, 1.0, 1.0);
				#endif
			#endif

				float3 positionWS = GetAbsolutePositionWS( posInput.positionWS );
				float3 positionRWS = posInput.positionWS;

				float3 worldTangent = TransformObjectToWorldDir(float3(1, 0, 0));
				float3 worldNormal = TransformObjectToWorldDir(float3(0, 1, 0));
				float3 worldBitangent = TransformObjectToWorldDir(float3(0, 0, 1));

				float4 texCoord0 = input.texCoord0;
				float4 texCoord1 = input.texCoord1;
				float4 texCoord2 = input.texCoord2;
				float4 texCoord3 = input.texCoord3;

				SurfaceDescription surfaceDescription = (SurfaceDescription)0;

				float2 uv_BaseMap = texCoord0.xy * _BaseMap_ST.xy + _BaseMap_ST.zw;
				float4 tex2DNode20 = tex2D( _BaseMap, uv_BaseMap );
				float4 blendOpSrc16 = tex2DNode20;
				float4 blendOpDest16 = ( GlobalTint * saturate( ( GlobalContrast + tex2DNode20.r ) ) );
				float2 uv_DetailMap = texCoord0.xy * _DetailMap_ST.xy + _DetailMap_ST.zw;
				float temp_output_24_0 = ( GlobalAlbedoOpacity * _DetailOpacityOverride );
				float4 temp_cast_0 = ((saturate( ( -temp_output_24_0 + 0.5 ) ) + (tex2D( _DetailMap, uv_DetailMap ).r - 0.0) * (saturate( ( temp_output_24_0 + 0.5 ) ) - saturate( ( -temp_output_24_0 + 0.5 ) )) / (1.0 - 0.0))).xxxx;
				float4 blendOpSrc9 = ( saturate( 2.0f*blendOpDest16*blendOpSrc16 + blendOpDest16*blendOpDest16*(1.0f - 2.0f*blendOpSrc16) ));
				float4 blendOpDest9 = temp_cast_0;
				
				float2 uv_NormalMap = texCoord0.xy * _NormalMap_ST.xy + _NormalMap_ST.zw;
				float2 uv_DetailNormals = texCoord0.xy * _DetailNormals_ST.xy + _DetailNormals_ST.zw;
				float3 unpack34 = UnpackNormalScale( tex2D( _DetailNormals, uv_DetailNormals ), DetailNormalStrength );
				unpack34.z = lerp( 1, unpack34.z, saturate(DetailNormalStrength) );
				
				float2 uv_MaskMap = texCoord0.xy * _MaskMap_ST.xy + _MaskMap_ST.zw;
				float4 tex2DNode27 = tex2D( _MaskMap, uv_MaskMap );
				

				surfaceDescription.BaseColor = ( saturate( (( blendOpDest9 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest9 ) * ( 1.0 - blendOpSrc9 ) ) : ( 2.0 * blendOpDest9 * blendOpSrc9 ) ) )).rgb;
				surfaceDescription.Alpha = tex2DNode20.a;
				surfaceDescription.NormalTS = BlendNormal( UnpackNormalScale( tex2D( _NormalMap, uv_NormalMap ), 1.0f ) , unpack34 );
				surfaceDescription.NormalAlpha = tex2DNode27.b;
				surfaceDescription.Metallic = 0;
				surfaceDescription.Occlusion = saturate( ( tex2DNode27.g + _AmbientOcclusionRemapping ) );
				surfaceDescription.Smoothness = ( tex2DNode27.a * tex2DNode20.a );
				surfaceDescription.MAOSAlpha = tex2DNode27.b;
				surfaceDescription.Emission = float3( 0, 0, 0 );

				GetSurfaceData(surfaceDescription, input, V, posInput, angleFadeFactor, surfaceData);

			#if ((SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_FORWARD_EMISSIVE_PROJECTOR)) && defined(SHADER_API_METAL)
				}

				clip(clipValue);
			#endif

			#if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_DBUFFER_MESH)
				ENCODE_INTO_DBUFFER(surfaceData, outDBuffer);
			#else
				// Emissive need to be pre-exposed
				outEmissive.rgb = surfaceData.emissive * GetCurrentExposureMultiplier();
				outEmissive.a = 1.0;
			#endif
			}

            ENDHLSL
        }

		
        Pass
		{
			
			Name "DBufferMesh"
			Tags { "LightMode"="DBufferMesh" }


			Stencil
			{
				Ref [_DecalStencilRef]
				WriteMask [_DecalStencilWriteMask]
				Comp Always
				Pass Replace
			}


			ZWrite Off
			ZTest LEqual

			Blend 0 SrcAlpha OneMinusSrcAlpha, Zero OneMinusSrcAlpha
			Blend 1 SrcAlpha OneMinusSrcAlpha, Zero OneMinusSrcAlpha
			Blend 2 SrcAlpha OneMinusSrcAlpha, Zero OneMinusSrcAlpha
			Blend 3 Zero OneMinusSrcColor

			ColorMask[_DecalColorMask0]
			ColorMask[_DecalColorMask1] 1
			ColorMask[_DecalColorMask2] 2
			ColorMask[_DecalColorMask3] 3

            HLSLPROGRAM

            #pragma shader_feature_local_fragment _MATERIAL_AFFECTS_ALBEDO
            #pragma shader_feature_local_fragment _COLORMAP
            #pragma shader_feature_local_fragment _MATERIAL_AFFECTS_NORMAL
            #pragma shader_feature_local_fragment _MATERIAL_AFFECTS_MASKMAP
            #define _MATERIAL_AFFECTS_EMISSION
            #pragma shader_feature_local_fragment _EMISSIVEMAP
            #pragma shader_feature_local_fragment _MASKMAP
            #define ASE_VERSION 19800
            #define ASE_SRP_VERSION 170003


            #pragma vertex Vert
            #pragma fragment Frag

			#pragma multi_compile_fragment DECALS_3RT DECALS_4RT
			#pragma multi_compile_fragment _ DECAL_SURFACE_GRADIENT

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/NormalSurfaceGradient.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/Functions.hlsl"

			#define SHADERPASS SHADERPASS_DBUFFER_MESH
			#define SUPPORT_GLOBAL_MIP_BIAS

            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/Decal.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/DecalPrepassBuffer.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/DecalMeshBiasTypeEnum.cs.hlsl"

			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0


            struct AttributesMesh
            {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float4 uv0 : TEXCOORD0;
				
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

			struct PackedVaryingsToPS
			{
				float4 positionCS : SV_POSITION;
                float3 positionRWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float4 tangentWS : TEXCOORD2;
                float4 uv0 : TEXCOORD3;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

            CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            float4 _DetailMap_ST;
            float4 _NormalMap_ST;
            float4 _DetailNormals_ST;
            float4 _MaskMap_ST;
            float _DetailOpacityOverride;
            float _AmbientOcclusionRemapping;
            float _DrawOrder;
			float _NormalBlendSrc;
			float _MaskBlendSrc;
			float _DecalBlend;
			int   _DecalMeshBiasType;
            float _DecalMeshDepthBias;
			float _DecalMeshViewBias;
            float _DecalStencilWriteMask;
            float _DecalStencilRef;
            #ifdef _MATERIAL_AFFECTS_ALBEDO
            float _AffectAlbedo;
			#endif
            #ifdef _MATERIAL_AFFECTS_NORMAL
            float _AffectNormal;
			#endif
            #ifdef _MATERIAL_AFFECTS_MASKMAP
            float _AffectAO;
			float _AffectMetal;
            float _AffectSmoothness;
			#endif
            #ifdef _MATERIAL_AFFECTS_EMISSION
            float _AffectEmission;
			#endif
            float _DecalColorMask0;
            float _DecalColorMask1;
            float _DecalColorMask2;
            float _DecalColorMask3;
            CBUFFER_END

	   		sampler2D _BaseMap;
	   		float4 GlobalTint;
	   		float GlobalContrast;
	   		sampler2D _DetailMap;
	   		float GlobalAlbedoOpacity;
	   		sampler2D _NormalMap;
	   		sampler2D _DetailNormals;
	   		float DetailNormalStrength;
	   		sampler2D _MaskMap;


			
            void GetSurfaceData(SurfaceDescription surfaceDescription, FragInputs fragInputs, float3 V, PositionInputs posInput, float angleFadeFactor, out DecalSurfaceData surfaceData)
            {
                #if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_FORWARD_EMISSIVE_PROJECTOR)
                    float4x4 normalToWorld = UNITY_ACCESS_INSTANCED_PROP(Decal, _NormalToWorld);
                    float fadeFactor = clamp(normalToWorld[0][3], 0.0f, 1.0f) * angleFadeFactor;
                    float2 scale = float2(normalToWorld[3][0], normalToWorld[3][1]);
                    float2 offset = float2(normalToWorld[3][2], normalToWorld[3][3]);
                    fragInputs.texCoord0.xy = fragInputs.texCoord0.xy * scale + offset;
                    fragInputs.texCoord1.xy = fragInputs.texCoord1.xy * scale + offset;
                    fragInputs.texCoord2.xy = fragInputs.texCoord2.xy * scale + offset;
                    fragInputs.texCoord3.xy = fragInputs.texCoord3.xy * scale + offset;
                    fragInputs.positionRWS = posInput.positionWS;
                    fragInputs.tangentToWorld[2].xyz = TransformObjectToWorldDir(float3(0, 1, 0));
                    fragInputs.tangentToWorld[1].xyz = TransformObjectToWorldDir(float3(0, 0, 1));
                #else
                    #ifdef LOD_FADE_CROSSFADE
                    LODDitheringTransition(ComputeFadeMaskSeed(V, posInput.positionSS), unity_LODFade.x);
                    #endif

                    float fadeFactor = 1.0;
                #endif

                ZERO_INITIALIZE(DecalSurfaceData, surfaceData);

                #ifdef _MATERIAL_AFFECTS_EMISSION
                #endif

                #ifdef _MATERIAL_AFFECTS_ALBEDO
                    surfaceData.baseColor.xyz = surfaceDescription.BaseColor;
                    surfaceData.baseColor.w = surfaceDescription.Alpha * fadeFactor;
                #endif

                #ifdef _MATERIAL_AFFECTS_NORMAL
                    #ifdef DECAL_SURFACE_GRADIENT
                        #if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_FORWARD_EMISSIVE_PROJECTOR)
                            float3x3 tangentToWorld = transpose((float3x3)normalToWorld);
                        #else
                            float3x3 tangentToWorld = fragInputs.tangentToWorld;
                        #endif

                        surfaceData.normalWS.xyz = SurfaceGradientFromTangentSpaceNormalAndFromTBN(surfaceDescription.NormalTS.xyz, tangentToWorld[0], tangentToWorld[1]);
                    #else
                        #if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR)
                            surfaceData.normalWS.xyz = mul((float3x3)normalToWorld, surfaceDescription.NormalTS);
                        #elif (SHADERPASS == SHADERPASS_DBUFFER_MESH) || (SHADERPASS == SHADERPASS_FORWARD_PREVIEW)

                            surfaceData.normalWS.xyz = normalize(TransformTangentToWorld(surfaceDescription.NormalTS, fragInputs.tangentToWorld));
                        #endif
                    #endif

                    surfaceData.normalWS.w = surfaceDescription.NormalAlpha * fadeFactor;
                #else
                    #if (SHADERPASS == SHADERPASS_FORWARD_PREVIEW)
                        #ifdef DECAL_SURFACE_GRADIENT
                            surfaceData.normalWS.xyz = float3(0.0, 0.0, 0.0);
                        #else
                            surfaceData.normalWS.xyz = normalize(TransformTangentToWorld(float3(0.0, 0.0, 0.1), fragInputs.tangentToWorld));
                        #endif
                    #endif
                #endif

                #ifdef _MATERIAL_AFFECTS_MASKMAP
                    surfaceData.mask.z = surfaceDescription.Smoothness;
                    surfaceData.mask.w = surfaceDescription.MAOSAlpha * fadeFactor;

                    #ifdef DECALS_4RT
                        surfaceData.mask.x = surfaceDescription.Metallic;
                        surfaceData.mask.y = surfaceDescription.Occlusion;
                        surfaceData.MAOSBlend.x = surfaceDescription.MAOSAlpha * fadeFactor;
                        surfaceData.MAOSBlend.y = surfaceDescription.MAOSAlpha * fadeFactor;
                    #endif

                #endif
            }

			PackedVaryingsToPS Vert(AttributesMesh inputMesh  )
			{
				PackedVaryingsToPS output;

				UNITY_SETUP_INSTANCE_ID(inputMesh);
				UNITY_TRANSFER_INSTANCE_ID(inputMesh, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				inputMesh.tangentOS = float4( 1, 0, 0, -1);
				inputMesh.normalOS = float3( 0, 1, 0 );

				

				inputMesh.normalOS = inputMesh.normalOS;
				inputMesh.tangentOS = inputMesh.tangentOS;

				float3 worldSpaceBias = 0.0f;

				if (_DecalMeshBiasType == DECALMESHDEPTHBIASTYPE_VIEW_BIAS)
				{
					float3 positionRWS = TransformObjectToWorld(inputMesh.positionOS);
					float3 V = GetWorldSpaceNormalizeViewDir(positionRWS);
					worldSpaceBias = V * (_DecalMeshViewBias);
				}

				float3 positionRWS = TransformObjectToWorld(inputMesh.positionOS) + worldSpaceBias;
				float3 normalWS = TransformObjectToWorldNormal(inputMesh.normalOS);
				float4 tangentWS = float4(TransformObjectToWorldDir(inputMesh.tangentOS.xyz), inputMesh.tangentOS.w);

				output.positionRWS.xyz = positionRWS;
				output.positionCS = TransformWorldToHClip(positionRWS);
				output.normalWS.xyz = normalWS;
				output.tangentWS.xyzw = tangentWS;
				output.uv0.xyzw = inputMesh.uv0;

				if (_DecalMeshBiasType == DECALMESHDEPTHBIASTYPE_DEPTH_BIAS)
				{
					#if UNITY_REVERSED_Z
						output.positionCS.z -= _DecalMeshDepthBias;
					#else
						output.positionCS.z += _DecalMeshDepthBias;
					#endif
				}


				return output;
			}

			void Frag(  PackedVaryingsToPS packedInput,
			#if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_DBUFFER_MESH)
				OUTPUT_DBUFFER(outDBuffer)
			#else
				out float4 outEmissive : SV_Target0
			#endif
			
			)
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(packedInput);
				UNITY_SETUP_INSTANCE_ID(packedInput);

                FragInputs input;
                ZERO_INITIALIZE(FragInputs, input);

                input.tangentToWorld = k_identity3x3;
                input.positionSS = packedInput.positionCS;

                input.positionRWS = packedInput.positionRWS.xyz;

                input.tangentToWorld = BuildTangentToWorld(packedInput.tangentWS.xyzw, packedInput.normalWS.xyz);
                input.texCoord0 = packedInput.uv0.xyzw;

				DecalSurfaceData surfaceData;
				float clipValue = 1.0;
				float angleFadeFactor = 1.0;

				PositionInputs posInput;
			#if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_FORWARD_EMISSIVE_PROJECTOR)

				float depth = LoadCameraDepth(input.positionSS.xy);
				posInput = GetPositionInput(input.positionSS.xy, _ScreenSize.zw, depth, UNITY_MATRIX_I_VP, UNITY_MATRIX_V);

				DecalPrepassData material;
				ZERO_INITIALIZE(DecalPrepassData, material);
				if (_EnableDecalLayers)
				{
					uint decalLayerMask = uint(UNITY_ACCESS_INSTANCED_PROP(Decal, _DecalLayerMaskFromDecal).x);

					DecodeFromDecalPrepass(posInput.positionSS, material);

					if ((decalLayerMask & material.renderingLayerMask) == 0)
						clipValue -= 2.0;
				}

				float3 positionDS = TransformWorldToObject(posInput.positionWS);
				positionDS = positionDS * float3(1.0, -1.0, 1.0) + float3(0.5, 0.5, 0.5);
				if (!(all(positionDS.xyz > 0.0f) && all(1.0f - positionDS.xyz > 0.0f)))
				{
					clipValue -= 2.0;
				}

			#ifndef SHADER_API_METAL
				clip(clipValue);
			#else
				if (clipValue > 0.0)
				{
			#endif

				float4x4 normalToWorld = UNITY_ACCESS_INSTANCED_PROP(Decal, _NormalToWorld);
				float2 scale = float2(normalToWorld[3][0], normalToWorld[3][1]);
				float2 offset = float2(normalToWorld[3][2], normalToWorld[3][3]);
				positionDS.xz = positionDS.xz * scale + offset;

				input.texCoord0.xy = positionDS.xz;
				input.texCoord1.xy = positionDS.xz;
				input.texCoord2.xy = positionDS.xz;
				input.texCoord3.xy = positionDS.xz;

				float3 V = GetWorldSpaceNormalizeViewDir(posInput.positionWS);

				if (_EnableDecalLayers)
				{
					float2 angleFade = float2(normalToWorld[1][3], normalToWorld[2][3]);
					if (angleFade.x > 0.0f)
					{
						float3 decalNormal = float3(normalToWorld[0].z, normalToWorld[1].z, normalToWorld[2].z);
                        angleFadeFactor = DecodeAngleFade(dot(material.geomNormalWS, decalNormal), angleFade);
					}
				}

			#else
				posInput = GetPositionInput(input.positionSS.xy, _ScreenSize.zw, input.positionSS.z, input.positionSS.w, input.positionRWS.xyz, uint2(0, 0));
				float3 V = GetWorldSpaceNormalizeViewDir(input.positionRWS);
			#endif

				float3 positionWS = GetAbsolutePositionWS( posInput.positionWS );
				float3 positionRWS = posInput.positionWS;

				float3 worldTangent = TransformObjectToWorldDir(float3(1, 0, 0));
				float3 worldNormal = TransformObjectToWorldDir(float3(0, 1, 0));
				float3 worldBitangent = TransformObjectToWorldDir(float3(0, 0, 1));

				float4 texCoord0 = input.texCoord0;
				float4 texCoord1 = input.texCoord1;
				float4 texCoord2 = input.texCoord2;
				float4 texCoord3 = input.texCoord3;

				SurfaceDescription surfaceDescription = (SurfaceDescription)0;

				float2 uv_BaseMap = texCoord0.xy * _BaseMap_ST.xy + _BaseMap_ST.zw;
				float4 tex2DNode20 = tex2D( _BaseMap, uv_BaseMap );
				float4 blendOpSrc16 = tex2DNode20;
				float4 blendOpDest16 = ( GlobalTint * saturate( ( GlobalContrast + tex2DNode20.r ) ) );
				float2 uv_DetailMap = texCoord0.xy * _DetailMap_ST.xy + _DetailMap_ST.zw;
				float temp_output_24_0 = ( GlobalAlbedoOpacity * _DetailOpacityOverride );
				float4 temp_cast_0 = ((saturate( ( -temp_output_24_0 + 0.5 ) ) + (tex2D( _DetailMap, uv_DetailMap ).r - 0.0) * (saturate( ( temp_output_24_0 + 0.5 ) ) - saturate( ( -temp_output_24_0 + 0.5 ) )) / (1.0 - 0.0))).xxxx;
				float4 blendOpSrc9 = ( saturate( 2.0f*blendOpDest16*blendOpSrc16 + blendOpDest16*blendOpDest16*(1.0f - 2.0f*blendOpSrc16) ));
				float4 blendOpDest9 = temp_cast_0;
				
				float2 uv_NormalMap = texCoord0.xy * _NormalMap_ST.xy + _NormalMap_ST.zw;
				float2 uv_DetailNormals = texCoord0.xy * _DetailNormals_ST.xy + _DetailNormals_ST.zw;
				float3 unpack34 = UnpackNormalScale( tex2D( _DetailNormals, uv_DetailNormals ), DetailNormalStrength );
				unpack34.z = lerp( 1, unpack34.z, saturate(DetailNormalStrength) );
				
				float2 uv_MaskMap = texCoord0.xy * _MaskMap_ST.xy + _MaskMap_ST.zw;
				float4 tex2DNode27 = tex2D( _MaskMap, uv_MaskMap );
				

				surfaceDescription.BaseColor = ( saturate( (( blendOpDest9 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest9 ) * ( 1.0 - blendOpSrc9 ) ) : ( 2.0 * blendOpDest9 * blendOpSrc9 ) ) )).rgb;
				surfaceDescription.Alpha = tex2DNode20.a;
				surfaceDescription.NormalTS = BlendNormal( UnpackNormalScale( tex2D( _NormalMap, uv_NormalMap ), 1.0f ) , unpack34 );
				surfaceDescription.NormalAlpha = tex2DNode27.b;
				surfaceDescription.Metallic = 0;
				surfaceDescription.Occlusion = saturate( ( tex2DNode27.g + _AmbientOcclusionRemapping ) );
				surfaceDescription.Smoothness = ( tex2DNode27.a * tex2DNode20.a );
				surfaceDescription.MAOSAlpha = tex2DNode27.b;
				surfaceDescription.Emission = float3( 0, 0, 0 );

				GetSurfaceData(surfaceDescription, input, V, posInput, angleFadeFactor, surfaceData);

			#if ((SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_FORWARD_EMISSIVE_PROJECTOR)) && defined(SHADER_API_METAL)
				}

				clip(clipValue);
			#endif

			#if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_DBUFFER_MESH)
				ENCODE_INTO_DBUFFER(surfaceData, outDBuffer);
			#else
				outEmissive.rgb = surfaceData.emissive * GetCurrentExposureMultiplier();
				outEmissive.a = 1.0;
			#endif
			}
            ENDHLSL
        }

		
        Pass
		{
			
			Name "DecalMeshForwardEmissive"
			Tags { "LightMode"="DecalMeshForwardEmissive" }

			Stencil
			{
				Ref [_DecalStencilRef]
				WriteMask [_DecalStencilWriteMask]
				Comp Always
				Pass Replace
			}


			ZWrite Off
			ZTest LEqual

			Blend 0 SrcAlpha One

            HLSLPROGRAM

            #pragma shader_feature_local_fragment _MATERIAL_AFFECTS_ALBEDO
            #pragma shader_feature_local_fragment _COLORMAP
            #pragma shader_feature_local_fragment _MATERIAL_AFFECTS_NORMAL
            #pragma shader_feature_local_fragment _MATERIAL_AFFECTS_MASKMAP
            #define _MATERIAL_AFFECTS_EMISSION
            #pragma shader_feature_local_fragment _EMISSIVEMAP
            #pragma shader_feature_local_fragment _MASKMAP
            #define ASE_VERSION 19800
            #define ASE_SRP_VERSION 170003


            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/NormalSurfaceGradient.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/Functions.hlsl"

			#define SHADERPASS SHADERPASS_FORWARD_EMISSIVE_MESH
			#define SUPPORT_GLOBAL_MIP_BIAS

            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/Decal.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/DecalPrepassBuffer.hlsl"

			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0


            struct AttributesMesh
            {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float4 uv0 : TEXCOORD0;
				
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

			struct PackedVaryingsToPS
			{
				float4 positionCS : SV_POSITION;
                float3 positionRWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float4 tangentWS : TEXCOORD2;
                float4 uv0 : TEXCOORD3;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

            CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            float4 _DetailMap_ST;
            float4 _NormalMap_ST;
            float4 _DetailNormals_ST;
            float4 _MaskMap_ST;
            float _DetailOpacityOverride;
            float _AmbientOcclusionRemapping;
            float _DrawOrder;
			float _NormalBlendSrc;
			float _MaskBlendSrc;
			float _DecalBlend;
			int   _DecalMeshBiasType;
            float _DecalMeshDepthBias;
			float _DecalMeshViewBias;
            float _DecalStencilWriteMask;
            float _DecalStencilRef;
            #ifdef _MATERIAL_AFFECTS_ALBEDO
            float _AffectAlbedo;
			#endif
            #ifdef _MATERIAL_AFFECTS_NORMAL
            float _AffectNormal;
			#endif
            #ifdef _MATERIAL_AFFECTS_MASKMAP
            float _AffectAO;
			float _AffectMetal;
            float _AffectSmoothness;
			#endif
            #ifdef _MATERIAL_AFFECTS_EMISSION
            float _AffectEmission;
			#endif
            float _DecalColorMask0;
            float _DecalColorMask1;
            float _DecalColorMask2;
            float _DecalColorMask3;
            CBUFFER_END

	   		sampler2D _BaseMap;
	   		float4 GlobalTint;
	   		float GlobalContrast;
	   		sampler2D _DetailMap;
	   		float GlobalAlbedoOpacity;
	   		sampler2D _NormalMap;
	   		sampler2D _DetailNormals;
	   		float DetailNormalStrength;
	   		sampler2D _MaskMap;


			
			void GetSurfaceData(SurfaceDescription surfaceDescription, FragInputs fragInputs, float3 V, PositionInputs posInput, float angleFadeFactor, out DecalSurfaceData surfaceData)
			{
				#if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_FORWARD_EMISSIVE_PROJECTOR)
					float4x4 normalToWorld = UNITY_ACCESS_INSTANCED_PROP(Decal, _NormalToWorld);
					float fadeFactor = clamp(normalToWorld[0][3], 0.0f, 1.0f) * angleFadeFactor;
					float2 scale = float2(normalToWorld[3][0], normalToWorld[3][1]);
					float2 offset = float2(normalToWorld[3][2], normalToWorld[3][3]);
					fragInputs.texCoord0.xy = fragInputs.texCoord0.xy * scale + offset;
					fragInputs.texCoord1.xy = fragInputs.texCoord1.xy * scale + offset;
					fragInputs.texCoord2.xy = fragInputs.texCoord2.xy * scale + offset;
					fragInputs.texCoord3.xy = fragInputs.texCoord3.xy * scale + offset;
					fragInputs.positionRWS = posInput.positionWS;
					fragInputs.tangentToWorld[2].xyz = TransformObjectToWorldDir(float3(0, 1, 0));
					fragInputs.tangentToWorld[1].xyz = TransformObjectToWorldDir(float3(0, 0, 1));
				#else
					#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition(ComputeFadeMaskSeed(V, posInput.positionSS), unity_LODFade.x);
					#endif

					float fadeFactor = 1.0;
				#endif

				ZERO_INITIALIZE(DecalSurfaceData, surfaceData);

				#ifdef _MATERIAL_AFFECTS_EMISSION
					surfaceData.emissive.rgb = surfaceDescription.Emission.rgb * fadeFactor;
				#endif

				#ifdef _MATERIAL_AFFECTS_ALBEDO
					surfaceData.baseColor.xyz = surfaceDescription.BaseColor;
					surfaceData.baseColor.w = surfaceDescription.Alpha * fadeFactor;
				#endif

				#ifdef _MATERIAL_AFFECTS_NORMAL
					#ifdef DECAL_SURFACE_GRADIENT
						#if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_FORWARD_EMISSIVE_PROJECTOR)
							float3x3 tangentToWorld = transpose((float3x3)normalToWorld);
						#else
							float3x3 tangentToWorld = fragInputs.tangentToWorld;
						#endif

						surfaceData.normalWS.xyz = SurfaceGradientFromTangentSpaceNormalAndFromTBN(surfaceDescription.NormalTS.xyz, tangentToWorld[0], tangentToWorld[1]);
					#else
						#if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR)
							surfaceData.normalWS.xyz = mul((float3x3)normalToWorld, surfaceDescription.NormalTS);
						#elif (SHADERPASS == SHADERPASS_DBUFFER_MESH) || (SHADERPASS == SHADERPASS_FORWARD_PREVIEW)
							surfaceData.normalWS.xyz = normalize(TransformTangentToWorld(surfaceDescription.NormalTS, fragInputs.tangentToWorld));
						#endif
					#endif

					surfaceData.normalWS.w = surfaceDescription.NormalAlpha * fadeFactor;
				#else
					#if (SHADERPASS == SHADERPASS_FORWARD_PREVIEW)
						#ifdef DECAL_SURFACE_GRADIENT
							surfaceData.normalWS.xyz = float3(0.0, 0.0, 0.0);
						#else
							surfaceData.normalWS.xyz = normalize(TransformTangentToWorld(float3(0.0, 0.0, 0.1), fragInputs.tangentToWorld));
						#endif
					#endif
				#endif

				#ifdef _MATERIAL_AFFECTS_MASKMAP
					surfaceData.mask.z = surfaceDescription.Smoothness;
					surfaceData.mask.w = surfaceDescription.MAOSAlpha * fadeFactor;

					#ifdef DECALS_4RT
						surfaceData.mask.x = surfaceDescription.Metallic;
						surfaceData.mask.y = surfaceDescription.Occlusion;
						surfaceData.MAOSBlend.x = surfaceDescription.MAOSAlpha * fadeFactor;
						surfaceData.MAOSBlend.y = surfaceDescription.MAOSAlpha * fadeFactor;
					#endif

				#endif
			}

			PackedVaryingsToPS Vert(AttributesMesh inputMesh  )
			{
				PackedVaryingsToPS output;

				UNITY_SETUP_INSTANCE_ID(inputMesh);
				UNITY_TRANSFER_INSTANCE_ID(inputMesh, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				inputMesh.tangentOS = float4( 1, 0, 0, -1);
				inputMesh.normalOS = float3( 0, 1, 0 );

				

				inputMesh.normalOS = inputMesh.normalOS;
				inputMesh.tangentOS = inputMesh.tangentOS;

				float3 positionRWS = TransformObjectToWorld(inputMesh.positionOS);
				float3 normalWS = TransformObjectToWorldNormal(inputMesh.normalOS);
				float4 tangentWS = float4(TransformObjectToWorldDir(inputMesh.tangentOS.xyz), inputMesh.tangentOS.w);

				output.positionRWS.xyz = positionRWS;
				output.positionCS = TransformWorldToHClip(positionRWS);
				output.normalWS.xyz = normalWS;
				output.tangentWS.xyzw = tangentWS;
				output.uv0.xyzw = inputMesh.uv0;

				return output;
			}

			void Frag(  PackedVaryingsToPS packedInput,
			#if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_DBUFFER_MESH)
				OUTPUT_DBUFFER(outDBuffer)
			#else
				out float4 outEmissive : SV_Target0
			#endif
			
			)
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(packedInput);
				UNITY_SETUP_INSTANCE_ID(packedInput);

                FragInputs input;
                ZERO_INITIALIZE(FragInputs, input);

                input.tangentToWorld = k_identity3x3;
                input.positionSS = packedInput.positionCS;

                input.positionRWS = packedInput.positionRWS.xyz;

                input.tangentToWorld = BuildTangentToWorld(packedInput.tangentWS.xyzw, packedInput.normalWS.xyz);
                input.texCoord0 = packedInput.uv0.xyzw;

				DecalSurfaceData surfaceData;
				float clipValue = 1.0;
				float angleFadeFactor = 1.0;

				PositionInputs posInput;
			#if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_FORWARD_EMISSIVE_PROJECTOR)

				float depth = LoadCameraDepth(input.positionSS.xy);
				posInput = GetPositionInput(input.positionSS.xy, _ScreenSize.zw, depth, UNITY_MATRIX_I_VP, UNITY_MATRIX_V);

				DecalPrepassData material;
				ZERO_INITIALIZE(DecalPrepassData, material);
				if (_EnableDecalLayers)
				{
					uint decalLayerMask = uint(UNITY_ACCESS_INSTANCED_PROP(Decal, _DecalLayerMaskFromDecal).x);

					DecodeFromDecalPrepass(posInput.positionSS, material);

					if ((decalLayerMask & material.renderingLayerMask) == 0)
						clipValue -= 2.0;
				}

				float3 positionDS = TransformWorldToObject(posInput.positionWS);
				positionDS = positionDS * float3(1.0, -1.0, 1.0) + float3(0.5, 0.5, 0.5);
				if (!(all(positionDS.xyz > 0.0f) && all(1.0f - positionDS.xyz > 0.0f)))
				{
					clipValue -= 2.0;
				}

			#ifndef SHADER_API_METAL
				clip(clipValue);
			#else
				if (clipValue > 0.0)
				{
			#endif

				float4x4 normalToWorld = UNITY_ACCESS_INSTANCED_PROP(Decal, _NormalToWorld);
				float2 scale = float2(normalToWorld[3][0], normalToWorld[3][1]);
				float2 offset = float2(normalToWorld[3][2], normalToWorld[3][3]);
				positionDS.xz = positionDS.xz * scale + offset;

				input.texCoord0.xy = positionDS.xz;
				input.texCoord1.xy = positionDS.xz;
				input.texCoord2.xy = positionDS.xz;
				input.texCoord3.xy = positionDS.xz;

				float3 V = GetWorldSpaceNormalizeViewDir(posInput.positionWS);

				if (_EnableDecalLayers)
				{
					float2 angleFade = float2(normalToWorld[1][3], normalToWorld[2][3]);
					if (angleFade.x > 0.0f)
					{
						float3 decalNormal = float3(normalToWorld[0].z, normalToWorld[1].z, normalToWorld[2].z);
                        angleFadeFactor = DecodeAngleFade(dot(material.geomNormalWS, decalNormal), angleFade);
					}
				}

			#else
				posInput = GetPositionInput(input.positionSS.xy, _ScreenSize.zw, input.positionSS.z, input.positionSS.w, input.positionRWS.xyz, uint2(0, 0));
				float3 V = GetWorldSpaceNormalizeViewDir(input.positionRWS);
			#endif

				float3 positionWS = GetAbsolutePositionWS( posInput.positionWS );
				float3 positionRWS = posInput.positionWS;

				float3 worldTangent = TransformObjectToWorldDir(float3(1, 0, 0));
				float3 worldNormal = TransformObjectToWorldDir(float3(0, 1, 0));
				float3 worldBitangent = TransformObjectToWorldDir(float3(0, 0, 1));

				float4 texCoord0 = input.texCoord0;
				float4 texCoord1 = input.texCoord1;
				float4 texCoord2 = input.texCoord2;
				float4 texCoord3 = input.texCoord3;

				SurfaceDescription surfaceDescription = (SurfaceDescription)0;

				float2 uv_BaseMap = texCoord0.xy * _BaseMap_ST.xy + _BaseMap_ST.zw;
				float4 tex2DNode20 = tex2D( _BaseMap, uv_BaseMap );
				float4 blendOpSrc16 = tex2DNode20;
				float4 blendOpDest16 = ( GlobalTint * saturate( ( GlobalContrast + tex2DNode20.r ) ) );
				float2 uv_DetailMap = texCoord0.xy * _DetailMap_ST.xy + _DetailMap_ST.zw;
				float temp_output_24_0 = ( GlobalAlbedoOpacity * _DetailOpacityOverride );
				float4 temp_cast_0 = ((saturate( ( -temp_output_24_0 + 0.5 ) ) + (tex2D( _DetailMap, uv_DetailMap ).r - 0.0) * (saturate( ( temp_output_24_0 + 0.5 ) ) - saturate( ( -temp_output_24_0 + 0.5 ) )) / (1.0 - 0.0))).xxxx;
				float4 blendOpSrc9 = ( saturate( 2.0f*blendOpDest16*blendOpSrc16 + blendOpDest16*blendOpDest16*(1.0f - 2.0f*blendOpSrc16) ));
				float4 blendOpDest9 = temp_cast_0;
				
				float2 uv_NormalMap = texCoord0.xy * _NormalMap_ST.xy + _NormalMap_ST.zw;
				float2 uv_DetailNormals = texCoord0.xy * _DetailNormals_ST.xy + _DetailNormals_ST.zw;
				float3 unpack34 = UnpackNormalScale( tex2D( _DetailNormals, uv_DetailNormals ), DetailNormalStrength );
				unpack34.z = lerp( 1, unpack34.z, saturate(DetailNormalStrength) );
				
				float2 uv_MaskMap = texCoord0.xy * _MaskMap_ST.xy + _MaskMap_ST.zw;
				float4 tex2DNode27 = tex2D( _MaskMap, uv_MaskMap );
				

				surfaceDescription.BaseColor = ( saturate( (( blendOpDest9 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest9 ) * ( 1.0 - blendOpSrc9 ) ) : ( 2.0 * blendOpDest9 * blendOpSrc9 ) ) )).rgb;
				surfaceDescription.Alpha = tex2DNode20.a;
				surfaceDescription.NormalTS = BlendNormal( UnpackNormalScale( tex2D( _NormalMap, uv_NormalMap ), 1.0f ) , unpack34 );
				surfaceDescription.NormalAlpha = tex2DNode27.b;
				surfaceDescription.Metallic = 0;
				surfaceDescription.Occlusion = saturate( ( tex2DNode27.g + _AmbientOcclusionRemapping ) );
				surfaceDescription.Smoothness = ( tex2DNode27.a * tex2DNode20.a );
				surfaceDescription.MAOSAlpha = tex2DNode27.b;
				surfaceDescription.Emission = float3( 0, 0, 0 );

				GetSurfaceData(surfaceDescription, input, V, posInput, angleFadeFactor, surfaceData);

			#if ((SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_FORWARD_EMISSIVE_PROJECTOR)) && defined(SHADER_API_METAL)
				}

				clip(clipValue);
			#endif

			#if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_DBUFFER_MESH)
				ENCODE_INTO_DBUFFER(surfaceData, outDBuffer);
			#else
				outEmissive.rgb = surfaceData.emissive * GetCurrentExposureMultiplier();
				outEmissive.a = 1.0;
			#endif
			}
            ENDHLSL
        }

		
        Pass
		{
			
			Name "ScenePickingPass"
			Tags { "LightMode"="Picking" }

            Cull Back

            HLSLPROGRAM
		    #pragma shader_feature_local_fragment _MATERIAL_AFFECTS_ALBEDO
		    #pragma shader_feature_local_fragment _COLORMAP
		    #pragma shader_feature_local_fragment _MATERIAL_AFFECTS_NORMAL
		    #pragma shader_feature_local_fragment _MATERIAL_AFFECTS_MASKMAP
		    #define _MATERIAL_AFFECTS_EMISSION
		    #pragma shader_feature_local_fragment _EMISSIVEMAP
		    #pragma shader_feature_local_fragment _MASKMAP
		    #define ASE_VERSION 19800
		    #define ASE_SRP_VERSION 170003

		    #pragma exclude_renderers glcore gles gles3 ps4 ps5 
			#pragma editor_sync_compilation

			#pragma vertex Vert
			#pragma fragment Frag

            #define SHADERPASS SHADERPASS_DEPTH_ONLY
            #define SCENEPICKINGPASS 1
			#define SUPPORT_GLOBAL_MIP_BIAS

            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/NormalSurfaceGradient.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"

			// Require _SelectionID variable
            float4 _SelectionID;

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.shadergraph/ShaderGraphLibrary/Functions.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/PickingSpaceTransforms.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/Decal.hlsl"



			

            struct AttributesMesh
			{
				float3 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 tangentOS : TANGENT;
				float4 ase_texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryingsToPS
			{
				float4 positionCS : SV_POSITION;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

            CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            float4 _DetailMap_ST;
            float4 _NormalMap_ST;
            float4 _DetailNormals_ST;
            float4 _MaskMap_ST;
            float _DetailOpacityOverride;
            float _AmbientOcclusionRemapping;
            float _DrawOrder;
			float _NormalBlendSrc;
			float _MaskBlendSrc;
			float _DecalBlend;
			int   _DecalMeshBiasType;
            float _DecalMeshDepthBias;
			float _DecalMeshViewBias;
            float _DecalStencilWriteMask;
            float _DecalStencilRef;
            #ifdef _MATERIAL_AFFECTS_ALBEDO
            float _AffectAlbedo;
			#endif
            #ifdef _MATERIAL_AFFECTS_NORMAL
            float _AffectNormal;
			#endif
            #ifdef _MATERIAL_AFFECTS_MASKMAP
            float _AffectAO;
			float _AffectMetal;
            float _AffectSmoothness;
			#endif
            #ifdef _MATERIAL_AFFECTS_EMISSION
            float _AffectEmission;
			#endif
            float _DecalColorMask0;
            float _DecalColorMask1;
            float _DecalColorMask2;
            float _DecalColorMask3;
            CBUFFER_END

	   		sampler2D _BaseMap;
	   		float4 GlobalTint;
	   		float GlobalContrast;
	   		sampler2D _DetailMap;
	   		float GlobalAlbedoOpacity;


			
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/DecalPrepassBuffer.hlsl"

			PackedVaryingsToPS Vert(AttributesMesh inputMesh )
			{
				PackedVaryingsToPS output;

				UNITY_SETUP_INSTANCE_ID(inputMesh);
				UNITY_TRANSFER_INSTANCE_ID(inputMesh, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				inputMesh.tangentOS = float4( 1, 0, 0, -1);
				inputMesh.normalOS = float3( 0, 1, 0 );

				output.ase_texcoord.xy = inputMesh.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				output.ase_texcoord.zw = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
				float3 defaultVertexValue = inputMesh.positionOS.xyz;
				#else
				float3 defaultVertexValue = float3( 0, 0, 0 );
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
				inputMesh.positionOS.xyz = vertexValue;
				#else
				inputMesh.positionOS.xyz += vertexValue;
				#endif

				float3 positionRWS = TransformObjectToWorld(inputMesh.positionOS) ;
				output.positionCS = TransformWorldToHClip(positionRWS);

				return output;
			}

			void Frag(  PackedVaryingsToPS packedInput,
						out float4 outColor : SV_Target0
						
						)
			{
				float2 uv_BaseMap = packedInput.ase_texcoord.xy * _BaseMap_ST.xy + _BaseMap_ST.zw;
				float4 tex2DNode20 = tex2D( _BaseMap, uv_BaseMap );
				float4 blendOpSrc16 = tex2DNode20;
				float4 blendOpDest16 = ( GlobalTint * saturate( ( GlobalContrast + tex2DNode20.r ) ) );
				float2 uv_DetailMap = packedInput.ase_texcoord.xy * _DetailMap_ST.xy + _DetailMap_ST.zw;
				float temp_output_24_0 = ( GlobalAlbedoOpacity * _DetailOpacityOverride );
				float4 temp_cast_0 = ((saturate( ( -temp_output_24_0 + 0.5 ) ) + (tex2D( _DetailMap, uv_DetailMap ).r - 0.0) * (saturate( ( temp_output_24_0 + 0.5 ) ) - saturate( ( -temp_output_24_0 + 0.5 ) )) / (1.0 - 0.0))).xxxx;
				float4 blendOpSrc9 = ( saturate( 2.0f*blendOpDest16*blendOpSrc16 + blendOpDest16*blendOpDest16*(1.0f - 2.0f*blendOpSrc16) ));
				float4 blendOpDest9 = temp_cast_0;
				

				//This port is needed as templates always require fragment ports to correctly work...this will be discarded by the compiler
				float3 baseColor = ( saturate( (( blendOpDest9 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest9 ) * ( 1.0 - blendOpSrc9 ) ) : ( 2.0 * blendOpDest9 * blendOpSrc9 ) ) )).rgb;
				outColor = _SelectionID;
			}

            ENDHLSL
        }
		
    }
    CustomEditor "Rendering.HighDefinition.DecalShaderGraphGUI"
	
	Fallback Off
}
/*ASEBEGIN
Version=19800
Node;AmplifyShaderEditor.RangedFloatNode;25;-2294.287,149.8207;Inherit;False;Property;_DetailOpacityOverride;Detail Opacity Override;6;0;Create;True;0;0;0;False;0;False;1;0.541;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;26;-2304,64;Inherit;False;Global;GlobalAlbedoOpacity;Global Albedo Opacity;5;0;Create;True;0;0;0;False;0;False;0.3;0.13;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;22;-1618.337,-553.4233;Inherit;False;Global;GlobalContrast;Global Contrast;11;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;24;-1990.286,101.8207;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;20;-1666.021,-383.4874;Inherit;True;Property;_BaseMap;Base Map;1;0;Create;True;0;0;0;False;0;False;-1;None;469287a134d34e64a8daa864f402b1e6;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.NegateNode;10;-1668.252,-7.795705;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;19;-1344.738,-481.4227;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;11;-1637.066,78.33736;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;13;-1502.064,-32.26009;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;18;-1224.738,-475.0226;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;21;-1424.221,-700.221;Inherit;False;Global;GlobalTint;Global Tint;10;0;Create;True;0;0;0;False;0;False;0.4811321,0.4811321,0.4811321,0;0.4607511,0.7165627,0.8773585,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SaturateNode;12;-1500.163,82.71416;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;14;-1365.161,-27.8833;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;-1077.539,-524.6237;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;23;-1182.879,-139.3981;Inherit;True;Property;_DetailMap;Detail Map;3;0;Create;True;0;0;0;False;0;False;-1;d9570d4206b45314ba84dd09a8449e3c;d9570d4206b45314ba84dd09a8449e3c;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.TFHCRemapNode;15;-837.6503,32.65782;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.BlendOpsNode;16;-896.8544,-385.2185;Inherit;False;SoftLight;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.BlendOpsNode;9;-456.9971,-183.3858;Inherit;False;Overlay;True;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;28;-696.4681,812.085;Inherit;False;Global;GlobalSmooth;Global Smooth;9;0;Create;True;0;0;0;False;0;False;1;2.03;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;29;-474.5638,707.8097;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;31;-683.7785,908.4882;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;27;-1047.51,636.334;Inherit;True;Property;_MaskMap;Mask Map;2;0;Create;True;0;0;0;False;0;False;-1;None;987fdc30ff941464daf158ba9d6f078f;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SaturateNode;32;-478.2828,909.9203;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BlendNormalsNode;35;-266.278,111.8598;Inherit;False;0;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;7;-1076.331,212.2389;Inherit;True;Property;_NormalMap;Normal Map;0;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SamplerNode;34;-1073.878,414.975;Inherit;True;Property;_DetailNormals;Detail Normals;4;0;Create;True;0;0;0;False;0;False;-1;4e92277bd12e548419f9ce2417d1a10e;4e92277bd12e548419f9ce2417d1a10e;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.RangedFloatNode;30;-1071.755,930.9483;Inherit;False;Property;_AmbientOcclusionRemapping;Ambient Occlusion Remapping;5;0;Create;True;0;0;0;False;0;False;0.5;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;36;-1483.027,463.233;Inherit;False;Global;DetailNormalStrength;Detail Normal Strength;7;0;Create;True;0;0;0;False;0;False;1;1;-2;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;2;0,0;Float;False;False;-1;2;Rendering.HighDefinition.DecalShaderGraphGUI;0;1;New Amplify Shader;d345501910c196f4a81c9eff8a0a5ad7;True;DecalProjectorForwardEmissive;0;1;DecalProjectorForwardEmissive;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=HDRenderPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;5;True;7;d3d11;metal;vulkan;xboxone;xboxseries;playstation;switch;0;False;True;8;5;False;;1;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;;False;False;False;False;False;False;False;False;False;True;True;0;True;_DecalStencilRef;255;False;;255;True;_DecalStencilWriteMask;7;False;;3;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;2;False;;True;2;False;;False;True;1;LightMode=DecalProjectorForwardEmissive;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;3;0,0;Float;False;False;-1;2;Rendering.HighDefinition.DecalShaderGraphGUI;0;1;New Amplify Shader;d345501910c196f4a81c9eff8a0a5ad7;True;DBufferMesh;0;2;DBufferMesh;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=HDRenderPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;5;True;7;d3d11;metal;vulkan;xboxone;xboxseries;playstation;switch;0;False;True;2;5;False;;10;False;;1;0;False;;10;False;;False;False;True;2;5;False;;10;False;;1;0;False;;10;False;;False;False;True;2;5;False;;10;False;;1;0;False;;10;False;;False;False;True;1;0;False;;6;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;0;True;_DecalStencilRef;255;False;;255;True;_DecalStencilWriteMask;7;False;;3;False;;0;False;;0;False;;7;False;;3;False;;0;False;;0;False;;False;True;2;False;;True;3;False;;False;True;1;LightMode=DBufferMesh;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;4;0,0;Float;False;False;-1;2;Rendering.HighDefinition.DecalShaderGraphGUI;0;1;New Amplify Shader;d345501910c196f4a81c9eff8a0a5ad7;True;DecalMeshForwardEmissive;0;3;DecalMeshForwardEmissive;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=HDRenderPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;5;True;7;d3d11;metal;vulkan;xboxone;xboxseries;playstation;switch;0;False;True;8;5;False;;1;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;0;True;_DecalStencilRef;255;False;;255;True;_DecalStencilWriteMask;7;False;;3;False;;0;False;;0;False;;7;False;;3;False;;0;False;;0;False;;False;True;2;False;;True;3;False;;False;True;1;LightMode=DecalMeshForwardEmissive;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;5;0,0;Float;False;False;-1;2;Rendering.HighDefinition.DecalShaderGraphGUI;0;1;New Amplify Shader;d345501910c196f4a81c9eff8a0a5ad7;True;ScenePickingPass;0;4;ScenePickingPass;1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=HDRenderPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;5;True;7;d3d11;metal;vulkan;xboxone;xboxseries;playstation;switch;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Picking;False;True;7;d3d11;metal;vulkan;xboxone;xboxseries;playstation;switch;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1;0,0;Float;False;True;-1;2;Rendering.HighDefinition.DecalShaderGraphGUI;0;15;Amplify Scripted Decal Shader;d345501910c196f4a81c9eff8a0a5ad7;True;DBufferProjector;0;0;DBufferProjector;11;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=HDRenderPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;5;True;7;d3d11;metal;vulkan;xboxone;xboxseries;playstation;switch;0;False;True;2;5;False;;10;False;;1;0;False;;10;False;;False;False;True;2;5;False;;10;False;;1;0;False;;10;False;;False;False;True;2;5;False;;10;False;;1;0;False;;10;False;;False;False;True;1;0;False;;6;False;;0;1;False;;0;False;;False;False;False;True;1;False;;False;False;False;False;False;False;False;False;False;True;True;0;True;_DecalStencilRef;255;False;;255;True;_DecalStencilWriteMask;7;False;;3;False;;0;False;;0;False;;7;False;;3;False;;0;False;;0;False;;False;True;2;False;;True;2;False;;False;True;1;LightMode=DBufferProjector;False;False;0;;0;0;Standard;7;Affect BaseColor;1;0;Affect Normal;1;0;Affect Metal;1;638447341583157220;Affect AO;1;0;Affect Smoothness;1;638447339590473587;Affect Emission;1;0;Support LOD CrossFade;0;638447341445300585;0;5;True;True;True;True;True;False;;False;0
WireConnection;24;0;26;0
WireConnection;24;1;25;0
WireConnection;10;0;24;0
WireConnection;19;0;22;0
WireConnection;19;1;20;1
WireConnection;11;0;24;0
WireConnection;13;0;10;0
WireConnection;18;0;19;0
WireConnection;12;0;11;0
WireConnection;14;0;13;0
WireConnection;17;0;21;0
WireConnection;17;1;18;0
WireConnection;15;0;23;1
WireConnection;15;3;14;0
WireConnection;15;4;12;0
WireConnection;16;0;20;0
WireConnection;16;1;17;0
WireConnection;9;0;16;0
WireConnection;9;1;15;0
WireConnection;29;0;27;4
WireConnection;29;1;20;4
WireConnection;31;0;27;2
WireConnection;31;1;30;0
WireConnection;32;0;31;0
WireConnection;35;0;7;0
WireConnection;35;1;34;0
WireConnection;34;5;36;0
WireConnection;1;0;9;0
WireConnection;1;1;20;4
WireConnection;1;2;35;0
WireConnection;1;3;27;3
WireConnection;1;5;32;0
WireConnection;1;6;29;0
WireConnection;1;7;27;3
ASEEND*/
//CHKSM=7ED5427330367D0D9F09847D11E6861962899E1E