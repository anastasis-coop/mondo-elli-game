Shader "Lit/SpecialFX/Light Visible"
{
    Properties
    {
        _MainTex ("Albedo Texture", 2D) = "white" {}
        _TintColor("Tint Color", Color) = (1,1,1,1)
        _CutoutThresh("Cutout Threshold", Float) = 1
    }

    SubShader
    {
        Tags { "LightMode"="ForwardBase" "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc" // for UnityObjectToWorldNormal
            #include "UnityLightingCommon.cginc" // for _LightColor0

            struct vert_in {
                float4 pos : POSITION;
                float4 normal : NORMAL;
                half2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                half4 atten_uv : TEXCOORD2;
                half4 world_pos : TEXCOORD3;
                fixed4 diff : COLOR0; // diffuse lighting color
                float4 pos : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _TintColor;
            float _CutoutThresh;

            float atten_uv (const float light_atten, const float3 _4_light_pos, const float3 world_pos) : SV_Target {
                const float range = (0.005 * sqrt(1000000 - light_atten)) / sqrt(light_atten);
                return distance(_4_light_pos, world_pos) / range;
            }
         
            float atten (float atten_uv) : SV_Target {
                return saturate(1.0 / (1.0 + 25.0 * atten_uv * atten_uv) * saturate((1 - atten_uv) * 5.0));
            }
         
            float attenTex (sampler2D _LightTextureB, float _attenUV) : SV_Target {
                return tex2D(_LightTextureB, (_attenUV * _attenUV).xx).UNITY_ATTEN_CHANNEL;
            }

            v2f vert (vert_in v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.pos);
                o.uv = v.uv;
                // get vertex normal in world space
                half3 worldNormal = UnityObjectToWorldNormal(v.normal);
                // dot product between normal and light direction for
                // standard diffuse (Lambert) lighting
                o.diff = _LightColor0 * max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));

                o.world_pos = mul(unity_ObjectToWorld, v.pos);
                
                o.atten_uv.x = atten_uv(unity_4LightAtten0.x, float3(unity_4LightPosX0.x, unity_4LightPosY0.x, unity_4LightPosZ0.x), o.world_pos.xyz);
                o.atten_uv.y = atten_uv(unity_4LightAtten0.y, float3(unity_4LightPosX0.y, unity_4LightPosY0.y, unity_4LightPosZ0.y), o.world_pos.xyz);
                o.atten_uv.z = atten_uv(unity_4LightAtten0.z, float3(unity_4LightPosX0.z, unity_4LightPosY0.z, unity_4LightPosZ0.z), o.world_pos.xyz);
                o.atten_uv.w = atten_uv(unity_4LightAtten0.w, float3(unity_4LightPosX0.w, unity_4LightPosY0.w, unity_4LightPosZ0.w), o.world_pos.xyz);
                
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample texture
                fixed4 col = i.diff;

                float4 attenuation;
                //float _atten.x = attenTex(_LightTextureB0, f.attenUV.x);
                attenuation.x = atten(i.atten_uv.x);
                //attenuation.y = atten(i.atten_uv.y);
                //attenuation.z = atten(i.atten_uv.z);
                //attenuation.w = atten(i.atten_uv.w);

                float4 dist_factor;
                dist_factor.x = 1 / distance(float3(unity_4LightPosX0.x, unity_4LightPosY0.x, unity_4LightPosZ0.x), i.world_pos.xyz) * attenuation.x;
                //dist_factor.y = 1 / distance(float3(unity_4LightPosX0.y, unity_4LightPosY0.y, unity_4LightPosZ0.y), i.world_pos.xyz) * attenuation.y;
                //dist_factor.z = 1 / distance(float3(unity_4LightPosX0.z, unity_4LightPosY0.z, unity_4LightPosZ0.z), i.world_pos.xyz) * attenuation.z;
                //dist_factor.w = 1 / distance(float3(unity_4LightPosX0.w, unity_4LightPosY0.w, unity_4LightPosZ0.w), i.world_pos.xyz) * attenuation.w;

                fixed3 contribution = unity_LightColor[0].rgb * dist_factor.x;
                //col.rgb += unity_LightColor[1].rgb * dist_factor.y;
                //col.rgb += unity_LightColor[2].rgb * dist_factor.z;
                //col.rgb += unity_LightColor[3].rgb * dist_factor.w;

                col.rgb += contribution;
                col *= tex2D(_MainTex, i.uv);
                fixed alpha = min(contribution / _CutoutThresh, 1);                
                return fixed4(col.x, col.y, col.z, alpha);
            }
            ENDCG
        }
    }
}