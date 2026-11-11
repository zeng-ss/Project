Shader "MK/Toon/URP/Standard/Simple" {
	Properties {
		[Enum(MK.Toon.Surface)] _Surface ("", Float) = 0
		_Blend ("", Float) = 0
		[Toggle] _AlphaClipping ("", Float) = 0
		[Enum(MK.Toon.RenderFace)] _RenderFace ("", Float) = 2
		_AlbedoColor ("", Vector) = (1,1,1,1)
		[Enum(UV0,0,UV7,7)] _DetailUVSet ("", Float) = 0
		_AlphaCutoff ("", Range(0, 1)) = 0.5
		_AlbedoMap ("", 2D) = "white" {}
		_AlbedoMapIntensity ("", Range(0, 1)) = 1
		[MKToonColorRGB] _SpecularColor ("", Vector) = (1,1,1,1)
		_SpecularMap ("", 2D) = "white" {}
		_Smoothness ("", Range(0, 1)) = 0.5
		_NormalMapIntensity ("", Float) = 1
		[Normal] _NormalMap ("", 2D) = "bump" {}
		_EmissionColor ("", Vector) = (0,0,0,1)
		_EmissionMap ("", 2D) = "black" {}
		[Enum(MK.Toon.Light)] _Light ("", Float) = 1
		_DiffuseRamp ("", 2D) = "grey" {}
		_DiffuseSmoothness ("", Range(0, 1)) = 0
		_DiffuseThresholdOffset ("", Range(0, 1)) = 0.25
		_SpecularRamp ("", 2D) = "grey" {}
		_SpecularSmoothness ("", Range(0, 1)) = 0
		_SpecularThresholdOffset ("", Range(0, 1)) = 0.25
		_RimRamp ("", 2D) = "grey" {}
		_RimSmoothness ("", Range(0, 1)) = 0.5
		_RimThresholdOffset ("", Range(0, 1)) = 0.25
		[MKToonLightBands] _LightBands ("", Range(2, 12)) = 4
		_LightBandsScale ("", Range(0, 1)) = 0.5
		_LightThreshold ("", Range(0, 1)) = 0.5
		_AdditionalLightsThreshold ("", Range(0, 1)) = 0.5
		_AdditionalLightsFalloff ("", Range(0, 1)) = 1
		_ThresholdMap ("", 2D) = "gray" {}
		_ThresholdMapScale ("", Float) = 1
		_GoochRampIntensity ("", Range(0, 1)) = 0.5
		_GoochRamp ("", 2D) = "white" {}
		_GoochBrightColor ("", Vector) = (1,1,1,1)
		_GoochDarkColor ("", Vector) = (0,0,0,1)
		_GoochDarkRemapMin ("", Range(0, 1)) = 0
		_GoochDarkRemapMax ("", Range(0, 1)) = 1
		_Contrast ("", Float) = 1
		_Hue ("", Range(0, 1)) = 0
		[MKToonSaturation] _Saturation ("", Float) = 1
		[MKToonBrightness] _Brightness ("", Float) = 1
		[Enum(MK.Toon.Iridescence)] _Iridescence ("", Float) = 0
		_IridescenceRamp ("", 2D) = "white" {}
		_IridescenceSize ("", Range(0, 5)) = 1
		_IridescenceColor ("", Vector) = (1,1,1,0.5)
		_IridescenceSmoothness ("", Range(0, 1)) = 0.5
		_IridescenceThresholdOffset ("", Range(0, 1)) = 0
		[Enum(MK.Toon.Rim)] _Rim ("", Float) = 0
		_RimSize ("", Range(0, 5)) = 1
		_RimColor ("", Vector) = (1,1,1,1)
		_RimBrightColor ("", Vector) = (1,1,1,1)
		_RimDarkColor ("", Vector) = (0,0,0,1)
		[Enum(MK.Toon.ColorGrading)] _ColorGrading ("", Float) = 0
		[Toggle] _VertexAnimationStutter ("", Float) = 0
		[Enum(MK.Toon.VertexAnimation)] _VertexAnimation ("", Float) = 0
		_VertexAnimationIntensity ("", Range(0, 1)) = 0.05
		_VertexAnimationMap ("", 2D) = "white" {}
		_NoiseMap ("", 2D) = "white" {}
		[MKToonVector3Drawer] _VertexAnimationFrequency ("", Vector) = (2.5,2.5,2.5,1)
		[Enum(MK.Toon.Dissolve)] _Dissolve ("", Float) = 0
		_DissolveMapScale ("", Float) = 1
		_DissolveMap ("", 2D) = "white" {}
		_DissolveAmount ("", Range(0, 1)) = 0.5
		_DissolveBorderSize ("", Range(0, 1)) = 0.25
		_DissolveBorderRamp ("", 2D) = "white" {}
		[HDR] _DissolveBorderColor ("", Vector) = (1,1,1,1)
		[Enum(MK.Toon.Artistic)] _Artistic ("", Float) = 0
		[Enum(MK.Toon.ArtisticProjection)] _ArtisticProjection ("", Float) = 0
		_ArtisticFrequency ("", Range(1, 10)) = 1
		_DrawnMapScale ("", Float) = 1
		_DrawnMap ("", 2D) = "white" {}
		_DrawnClampMin ("", Range(0, 1)) = 0
		_DrawnClampMax ("", Range(0, 1)) = 1
		_HatchingMapScale ("", Float) = 1
		_HatchingBrightMap ("", 2D) = "white" {}
		_HatchingDarkMap ("", 2D) = "Black" {}
		_SketchMapScale ("", Float) = 1
		_SketchMap ("", 2D) = "black" {}
		_ArtisticShadowFilter ("", Range(0, 0.5)) = 0
		[Enum(MK.Toon.BlendFactor)] [HideInInspector] _BlendSrc ("", Float) = 1
		[Enum(MK.Toon.BlendFactor)] [HideInInspector] _BlendDst ("", Float) = 0
		[Enum(MK.Toon.BlendFactor)] [HideInInspector] _BlendSrcAlpha ("", Float) = 1
		[Enum(MK.Toon.BlendFactor)] [HideInInspector] _BlendDstAlpha ("", Float) = 0
		[Enum(MK.Toon.ZWrite)] _ZWrite ("", Float) = 1
		[Enum(MK.Toon.ZTest)] _ZTest ("", Float) = 4
		[Toggle] _WrappedLighting ("", Float) = 1
		_IndirectFade ("", Range(0, 1)) = 1
		[Toggle] _ReceiveShadows ("", Float) = 1
		[Enum(MK.Toon.SpecularSimple)] _Specular ("", Float) = 0
		[MKToonSpecularIntensity] _SpecularIntensity ("", Float) = 1
		[Enum(MK.Toon.EnvironmentReflectionSimple)] _EnvironmentReflections ("", Float) = 1
		[MKToonRenderPriority] _RenderPriority ("", Range(-50, 50)) = 0
		[Enum(MK.Toon.Stencil)] _Stencil ("", Float) = 1
		[MKToonStencilRef] _StencilRef ("", Range(0, 255)) = 0
		[MKToonStencilReadMask] _StencilReadMask ("", Range(0, 255)) = 255
		[MKToonStencilWriteMask] _StencilWriteMask ("", Range(0, 255)) = 255
		[Enum(MK.Toon.StencilComparison)] _StencilComp ("", Float) = 8
		[Enum(MK.Toon.StencilOperation)] _StencilPass ("", Float) = 0
		[Enum(MK.Toon.StencilOperation)] _StencilFail ("", Float) = 0
		[Enum(MK.Toon.StencilOperation)] _StencilZFail ("", Float) = 0
		[HideInInspector] _AddPrecomputedVelocity ("_AddPrecomputedVelocity", Float) = 0
		[HideInInspector] _Initialized ("", Float) = 0
		[HideInInspector] _OptionsTab ("", Float) = 1
		[HideInInspector] _InputTab ("", Float) = 1
		[HideInInspector] _StylizeTab ("", Float) = 0
		[HideInInspector] _AdvancedTab ("", Float) = 0
		[Toggle] [HideInInspector] _AlembicMotionVectors ("_AlembicMotionVectors", Float) = 0
		[HideInInspector] _Cutoff ("", Range(0, 1)) = 0.5
		[HideInInspector] _MainTex ("", 2D) = "white" {}
		[HideInInspector] _Color ("", Vector) = (1,1,1,1)
		[HideInInspector] [NoScaleOffset] unity_Lightmaps ("unity_Lightmaps", 2DArray) = "" {}
		[HideInInspector] [NoScaleOffset] unity_LightmapsInd ("unity_LightmapsInd", 2DArray) = "" {}
		[HideInInspector] [NoScaleOffset] unity_ShadowMasks ("unity_ShadowMasks", 2DArray) = "" {}
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			float4x4 unity_ObjectToWorld;
			float4x4 unity_MatrixVP;
			float4 _MainTex_ST;

			struct Vertex_Stage_Input
			{
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct Vertex_Stage_Output
			{
				float2 uv : TEXCOORD0;
				float4 pos : SV_POSITION;
			};

			Vertex_Stage_Output vert(Vertex_Stage_Input input)
			{
				Vertex_Stage_Output output;
				output.uv = (input.uv.xy * _MainTex_ST.xy) + _MainTex_ST.zw;
				output.pos = mul(unity_MatrixVP, mul(unity_ObjectToWorld, input.pos));
				return output;
			}

			Texture2D<float4> _MainTex;
			SamplerState sampler_MainTex;
			float4 _Color;

			struct Fragment_Stage_Input
			{
				float2 uv : TEXCOORD0;
			};

			float4 frag(Fragment_Stage_Input input) : SV_TARGET
			{
				return _MainTex.Sample(sampler_MainTex, input.uv.xy) * _Color;
			}

			ENDHLSL
		}
	}
	//CustomEditor "MK.Toon.Editor.URP.StandardSimpleEditor"
}