Shader "Custom/ESPFillVisible"
{
    Properties { _Color ("Color", Color) = (1,0,0,0.1) }
    SubShader{
        Tags{ "Queue"="Transparent" "RenderType"="Transparent" }
        ZWrite Off
        ZTest LEqual
        Cull Back
        Blend SrcAlpha OneMinusSrcAlpha

        Pass{
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;
            struct v2f { float4 pos:SV_POSITION; };
            v2f vert(appdata_base v){ v2f o; o.pos = UnityObjectToClipPos(v.vertex); return o; }
            fixed4 frag(v2f i):SV_Target { return _Color; }

            ENDCG
        }
    }
}
