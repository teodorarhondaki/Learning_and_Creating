Shader "CharacterTest/OpaqueRadiance"
{
    Properties { _Color ("Warm white", Color) = (1,0.96,0.83,1) }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            fixed4 _Color;
            struct v2f { float4 position:SV_POSITION; float3 normal:TEXCOORD0; };
            v2f vert(appdata_base v) { v2f o; o.position=UnityObjectToClipPos(v.vertex);o.normal=UnityObjectToWorldNormal(v.normal);return o; }
            fixed4 frag(v2f i):SV_Target { float variation=.96+.04*abs(normalize(i.normal).y);return fixed4(_Color.rgb*variation,1); }
            ENDCG
        }
        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
}
