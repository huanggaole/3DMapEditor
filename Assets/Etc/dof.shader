/*
* æ∞…Ó
*/

Shader "Unlit/dof"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [Toggle] _CS ("CS", Range(0, 1)) = 1
        _Sample ("Sample", Range(0, 16)) = 4
        [Toggle] _HV ("H&V", Range(0, 1)) = 0
        _Dis ("Size", Range(0, 0.1)) = 0.05
        _Seed ("Seed", Float) = 0
        _Scale ("Scale", Float) = 1
        _DepthM ("Depth Move", Range(0, 10)) = 1
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
            sampler2D _CameraDepthTexture;
            float4 _MainTex_ST;
            int _Sample;
            float _HV;
            fixed _Dis;
            float _CS;
            float _Seed;
            float _Scale;
            float _DepthM;

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

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed dis = _Dis;
                
                float base = 1;
                float4 col = tex2D(_MainTex, i.uv);
                if (_CS < 0.5) return col;
                float md = LinearEyeDepth(tex2D(_CameraDepthTexture, fixed2(0.5, 0.5)).r);
                float cd = LinearEyeDepth(tex2D(_CameraDepthTexture, i.uv).r);

                for (float ii = -_Sample; ii <= _Sample; ii++)
                {
                    fixed2 chk;
                    if (ii == 0)
                    {
                        continue;
                    }
                    
                    if (_HV > 0.5)
                    {
                        chk = fixed2(0, ii / _Sample * _ScreenParams.x / _ScreenParams.y);
                    }
                    else
                    {
                        chk = fixed2(ii / _Sample, 0);
                    }
                    
                    fixed sd = rand(i.uv + chk);
                    chk *= dis;

                    float pd = LinearEyeDepth(tex2D(_CameraDepthTexture, i.uv + chk).r);
                    if (pd > cd) pd = cd;

                    fixed p = (cos(3.14 * ii / _Sample * sd) + 1) * abs(pd - md) * _Scale / pd / pow(md, _DepthM);
                    col += tex2D(_MainTex, i.uv + chk) * p;
                    base += p;
                }

                return col / base;
            }
            ENDCG
        }
    }
}
