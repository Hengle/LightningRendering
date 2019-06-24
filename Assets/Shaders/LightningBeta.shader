Shader "Custom/LightningBeta"
{
    Properties
    {
		_LightningColor("Lightning Color", Color) = (1, 1, 1, 1)
		_BranchBrightExp("BranchBrightExp", Range(0, 2)) = 1.0
		_BranchBrightnessScale("BranchBrightnessScale", Range(0, 2)) = 1.0
		_LightningBrightness("LightningBrightness", Range(0, 50)) = 20.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
		Blend One One

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

			uniform half _BoltBrightnessScale;
			uniform half _BoltProgress;
			uniform half _BranchInProcess;
			uniform half _BranchOutProcess;
			uniform half _BranchBrightExp;
			uniform half _BranchBrightnessScale;
			uniform half _LightningBrightness;
			uniform half4 _LightningColor;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
				float4 color : COLOR;
				float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
				float2 vColor : TEXCOORD1;
				float3 normal : TEXCOORD2;
            };

            v2f vert (appdata v)
            {
				const half BoltThicknessScale = 0.4;
				const half BranchThicknessScale = 0.24;
                v2f o;
				float3 worldNormal = UnityObjectToWorldNormal(v.normal);
				float3 worldOffset = 0.0;
				half constOne = 1.0;
				float worldScaleY = length(float3(unity_ObjectToWorld[0].y, unity_ObjectToWorld[1].y, unity_ObjectToWorld[2].y));
				if (v.uv.x > constOne)
				{
					worldOffset = worldNormal * BoltThicknessScale * worldScaleY / 100.0;
				}

				else
				{
					worldOffset = worldNormal * BranchThicknessScale * worldScaleY / 100.0;
				}
				float4 worldPos = mul(unity_ObjectToWorld, v.vertex) + float4(worldOffset, 0.0);
                o.vertex = mul(UNITY_MATRIX_VP, worldPos);
                o.uv = v.uv;
				o.vColor = v.color.rg;
				o.normal = worldNormal;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				half4 outColor = 0.0;
				half4 lightningColor = _LightningBrightness * _LightningColor;
				half lightningCoef = 0.0;
				half constOne = 1.0;
				if (i.uv.x > constOne)
				{
					lightningCoef = _BoltBrightnessScale * (1.0 - smoothstep(saturate(_BoltProgress), saturate(_BoltProgress + 0.01), i.uv.y));
				}
				else
				{
					half branchIn = 1.0 - smoothstep(saturate(_BranchInProcess - _BranchInProcess), saturate(_BranchInProcess), sqrt(i.uv.y));
					half branchOut = 1.0 - smoothstep(saturate(_BranchOutProcess - _BranchOutProcess), saturate(_BranchOutProcess), sqrt(i.uv.y));
					half lightningBranchBrightness = pow(i.vColor.x, _BranchBrightExp) * i.vColor.y;
					lightningBranchBrightness *= saturate(branchIn - branchOut) * _BranchBrightnessScale;
					lightningCoef = lightningBranchBrightness;
				}
				outColor = lightningCoef * lightningColor;

				return outColor;
            }
            ENDCG
        }
    }
}
