Shader "Custom/Lightning"
{
    Properties
    {
        _LightningColor ("Lightning Color", Color) = (1, 1, 1, 1)
        _BranchBrightExp ("BranchBrightExp", Range(0, 2)) = 1.0
        _BranchBrightnessScale ("BranchBrightnessScale", Range(0, 2)) = 1.0
        _LightningBrightness ("LightningBrightness", Range(0, 50)) = 20.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0
	    #pragma surface surf Lambert vertex:vert
	  

		struct Input
	    {
			float2 texcoord;
			float2 texcoord1;
			float2 color;
	    };

	    void vert(inout appdata_full v)
	    {
	    }

		uniform half _BoltBrightnessScale;
		uniform half _BoltProgress;
		uniform half _BranchInProcess;
		uniform half _BranchOutProcess;
		uniform half _BranchBrightExp;
		uniform half _BranchBrightnessScale;
		uniform half _LightningBrightness;
		uniform half4 _LightningColor;

        void surf (Input IN, inout SurfaceOutput o)
        {
			half4 outColor = 0.0;
			half4 lightningColor = _LightningBrightness * _LightningColor;
			half lightningCoef = 0.0;
			half constOne = 1.0;
			if (IN.texcoord.x > constOne)
			{
				lightningCoef = _BoltBrightnessScale * (1.0 - smoothstep(_BoltProgress, _BoltProgress + 0.01, 1.0 - IN.texcoord.y));
			}
			else 
			{
				half branchIn = 1.0 - smoothstep(_BranchInProcess, _BranchInProcess + 0.01, 1.0 - IN.texcoord.y);
				half branchOut = 1.0 - smoothstep(_BranchOutProcess - 0.1, _BranchOutProcess, 1.0 - IN.texcoord.y);
				half lightningBrightness = pow(IN.color.r, _BranchBrightExp) * IN.color.g;
				lightningBrightness *= (branchIn - branchOut) * _BranchBrightnessScale;
				lightningCoef = lightningBrightness;
			}
			outColor = lightningCoef * lightningColor;
            o.Emission = IN.color.y;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
