Shader "Custom/HitEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        
        // 受击效果参数
        _HitColor ("Hit Color", Color) = (1,1,1,1)
        _HitAmount ("Hit Amount", Range(0, 1)) = 0
        _HitFade ("Hit Fade Speed", Range(0.1, 10)) = 5
        
        // 冰冻效果参数
        _FrozenColor ("Frozen Color", Color) = (0.5,0.8,1,1)
        _FrozenAmount ("Frozen Amount", Range(0, 1)) = 0
        _IceBrightness ("Ice Brightness", Range(0.1, 2)) = 1.2
        _FrozenFade ("Frozen Fade Speed", Range(0.1, 10)) = 2
    }
    
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            
            // 受击效果
            float4 _HitColor;
            float _HitAmount;
            float _HitFade;
            
            // 冰冻效果
            float4 _FrozenColor;
            float _FrozenAmount;
            float _IceBrightness;
            float _FrozenFade;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                
                // 受击效果叠加
                if (_HitAmount > 0)
                {
                    col.rgb = lerp(col.rgb, _HitColor.rgb, _HitAmount);
                }
                
                // 冰冻效果叠加
                if (_FrozenAmount > 0)
                {
                    // 降低饱和度，增加蓝色调
                    float luminance = dot(col.rgb, float3(0.299, 0.587, 0.114));
                    fixed3 desaturated = lerp(col.rgb, luminance, 0.7);
                    fixed3 frozenColor = lerp(desaturated, _FrozenColor.rgb, _FrozenAmount * 0.6);
                    col.rgb = lerp(col.rgb, frozenColor * _IceBrightness, _FrozenAmount);
                    
                    // 添加冰冻高光效果
                    col.rgb += _FrozenColor.rgb * _FrozenAmount * 0.3;
                }
                
                return col;
            }
            ENDCG
        }
    }
}