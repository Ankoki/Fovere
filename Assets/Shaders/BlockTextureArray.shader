Shader "Custom/BlockTextureArray"
{
    Properties
    {
        _MainTex ("Texture Array", 2DArray) = "" {}
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5

            #include "UnityCG.cginc"

            UNITY_DECLARE_TEX2DARRAY (_MainTex);

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 texData : TEXCOORD1;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 texData : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.texData = v.texData;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 tiledUV = frac(i.uv);
                return UNITY_SAMPLE_TEX2DARRAY(_MainTex, float3(tiledUV, i.texData.z));
            }
            ENDCG
        }
    }
}