Shader "EDW/MeshBlending/TerrainHeightMap"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        Cull Back

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            uniform float _TerrainMinHeight;
            uniform float _TerrainMaxHeight;

            struct appdata
            {
                float4 pos : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 normal : NORMAL;
                float heightWS : DEPTH;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.pos);
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.heightWS = mul(unity_ObjectToWorld, v.pos).y;
                
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float height = (i.heightWS - _TerrainMinHeight) / (_TerrainMaxHeight - _TerrainMinHeight);
                //return i.heightWS.xxxx;
                return EncodeDepthNormal(height, normalize(i.normal));
            }
            ENDCG
        }
    }
}
