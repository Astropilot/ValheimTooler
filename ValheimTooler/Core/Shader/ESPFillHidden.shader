Shader "Custom/ESPFillHidden"
{
    Properties { _Color ("Color", Color) = (1,1,1,0.5) }
    SubShader{
        Tags{ "Queue"="Transparent" "RenderType"="Transparent" }

        // 1) Mask-Pass markiert Pixel, wo das Objekt vorn ist
        Pass{
            Tags{ "LightMode"="Always" }
            ZTest LEqual
            ZWrite Off
            ColorMask 0
            Cull Back
            Stencil { Ref 1 Comp Always Pass Replace }
        }

        // 2) Hidden-Pass: nur wo Objekt hinten liegt UND kein Stencil
        Pass{
            ZTest Greater
            ZWrite Off
            Cull Back
            Blend SrcAlpha OneMinusSrcAlpha
            Stencil { Ref 1 Comp NotEqual }

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
