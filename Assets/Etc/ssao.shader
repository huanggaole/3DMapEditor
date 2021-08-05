/*
* »·¾³¹âÕÚ±Î
*/

Shader "Unlit/ssao"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [Toggle] _CS ("CS", Range(0, 1)) = 1
        _DepFix ("Depth Fix", Range(-5, 5)) = -0.3
        [Toggle]_DepR ("Depth Real", Range(0, 1)) = 1
        _DForce ("D_Force", Range(0, 4)) = 1.75
        _Limit ("Depth Limit", Range(0, 5)) = 2
        _Dis ("Distance", Range(0, 1)) = 0.07
        _Power ("Power", Range(0, 10)) = 1
        _Sample ("Sample", Range(0, 8)) = 4
        _Seed ("Seed", Float) = 0
        _SamDisPow ("Sample Distance Power", Range(0, 5)) = 2
        _DisDead ("Distance Dead", Float) = 1
        _DarkFix ("Dark Fix", Float) = 0.00002
        [Toggle] _Debug1 ("Debug 1", Range(0, 1)) = 0
        [Toggle] _Debug2 ("Debug 2", Range(0, 1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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
            sampler2D _CameraDepthTexture;
            fixed _CS;
            fixed _DepR;
            float _DepFix;
            float _DForce;
            float _Limit;
            float _Dis;
            float _Power;
            int _Sample;
            float _Seed;
            fixed _Debug1;
            fixed _Debug2;
            float _SamDisPow;
            float _DisDead;
            float _DarkFix;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float rand(fixed2 uv)
            {
                return frac(sin(_Seed + dot(uv, float2(12.9898,78.233))) * 43758.5453123);
            }

            float trueDepth(float d)
            {
                return LinearEyeDepth(d) * pow(10, _DepFix);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float4 col = tex2D(_MainTex, i.uv);
                if (_CS < 0.5) return col;
                float add = 2 / _ScreenParams.x;
                float d = trueDepth(tex2D(_CameraDepthTexture, i.uv).r);
                float d1 = trueDepth(tex2D(_CameraDepthTexture, i.uv + float2(add, 0)).r);
                float d2 = trueDepth(tex2D(_CameraDepthTexture, i.uv + float2(0, add * _ScreenParams.x / _ScreenParams.y)).r);

                float3 nor = -cross(normalize(float3(float2(add, 0), d1 - d)), 
                    normalize(float3(float2(0, add), d2 - d)));

                float dark = 0;
                float base = 0;
                float dis = _Dis;
                if (_DepR > 0.5)
                {
                    dis /= d;
                }
                for (float ii = -_Sample; ii <= _Sample; ii++)
                {
                    for (float jj = -_Sample; jj <= _Sample; jj++)
                    {
                        if (ii == 0 || jj == 0) continue;
                        float2 chk = float2(ii / _Sample, jj / _Sample) * dis;
                        chk *= pow(rand(i.uv + chk), _SamDisPow);
                        float2 fix = float2(1, _ScreenParams.x / _ScreenParams.y);
                        float cd = trueDepth(tex2D(_CameraDepthTexture, i.uv + chk * fix).r);
                        
                        float D = length(float3(chk, cd - d));
                        if (abs(cd - d) < _Limit)
                            dark += pow(max(0.0, dot(nor, normalize(float3(chk, cd - _DarkFix - d)))), _Power) * 
                                1.0 / (1 + D) *
                                (_Limit - abs(cd - d)) / _Limit;
                    }
                }
                dark *= 1 - d * _DisDead;

                if (dark < 0) dark = 0;

                
                
                if (dis < max(1.0 / _ScreenParams.x, 1.0 / _ScreenParams.y)) dark = 0;

                fixed3 norc = nor / 2 + fixed3(0.5, 0.5, 0.5);
                if (_Debug1 > 0.5) return fixed4(norc, 1);
                if (_Debug2 > 0.5) return fixed4(fixed3(1, 1, 1) * (1 - dark / pow(10, _DForce)), 1);
                return fixed4(col.rgb * (1 - dark / pow(10, _DForce)), 1);
            }
            ENDCG
        }
    }
}
