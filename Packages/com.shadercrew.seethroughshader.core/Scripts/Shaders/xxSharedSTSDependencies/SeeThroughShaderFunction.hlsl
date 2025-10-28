#ifndef SEETHROUGHSHADER_FUNCTION
#define SEETHROUGHSHADER_FUNCTION


#ifndef UNITY_MATRIX_I_M
        #define UNITY_MATRIX_I_M   unity_WorldToObject
#endif






void DoSeeThroughShading(
                                    float3 l0,
                                    float3 ll0, float3 lll0,
                                    float3 llll0, float4 lllll0,
                                    float llllll0, float lllllll0, float llllllll0, float lllllllll0,
                                    float llllllllll0, float lllllllllll0,
                                    float llllllllllll0,
                                    half lllllllllllll0, half llllllllllllll0,
                                    float4 lllllllllllllll0, float llllllllllllllll0, float lllllllllllllllll0,
                                    float llllllllllllllllll0, float lllllllllllllllllll0, float llllllllllllllllllll0, float lllllllllllllllllllll0,
                                    bool llllllllllllllllllllll0,
                                    float lllllllllllllllllllllll0,
                                    float llllllllllllllllllllllll0, 
                                    float lllllllllllllllllllllllll0,
                                    float llllllllllllllllllllllllll0, float lllllllllllllllllllllllllll0,
                                    float llllllllllllllllllllllllllll0, float lllllllllllllllllllllllllllll0,
                                    float llllllllllllllllllllllllllllll0, float lllllllllllllllllllllllllllllll0,
                                    float l1, float ll1,
                                    float lll1,
                                    float llll1,
                                    float lllll1,
                                    float llllll1,
                                    float lllllll1, float llllllll1,
                                    float lllllllll1, float llllllllll1, float lllllllllll1,
                                    float llllllllllll1, float lllllllllllll1, float llllllllllllll1, float lllllllllllllll1, float llllllllllllllll1, float lllllllllllllllll1,
                                    float llllllllllllllllll1, float lllllllllllllllllll1, float llllllllllllllllllll1, float lllllllllllllllllllll1, float llllllllllllllllllllll1, float lllllllllllllllllllllll1,
                                    float llllllllllllllllllllllll1, float lllllllllllllllllllllllll1,
                                    float llllllllllllllllllllllllll1,
                                    bool lllllllllllllllllllllllllll1,
                                    float llllllllllllllllllllllllllll1, float lllllllllllllllllllllllllllll1, float llllllllllllllllllllllllllllll1, float lllllllllllllllllllllllllllllll1,
                                    float l2, float ll2,
                                    float lll2,
                                    float llll2,
#ifdef USE_UNITY_TEXTURE_2D_TYPE
                                    UnityTexture2D lllll2,
                                    UnityTexture2D llllll2,
                                    UnityTexture2D lllllll2,
#else
                                    sampler2D lllll2,
                                    sampler2D llllll2,
                                    sampler2D lllllll2,
                                    float4 llllllll2,
                                    float4 lllllllll2,
                                    float4 llllllllll2,
#endif
                                    out half3 lllllllllll2,
                                    out half3 llllllllllll2,
                                    out float lllllllllllll2
)
{
    ShaderData d;
    d.worldSpaceNormal = llll0;
    d.worldSpacePosition = lll0;
    float3 llllllllllllll2 = float3(0, 0, 0);
#ifdef _HDRP
        llllllllllllll2 = mul(UNITY_MATRIX_I_M, float4(GetCameraRelativePositionWS(d.worldSpacePosition), 1)).xyz;
#else
    llllllllllllll2 = mul(UNITY_MATRIX_I_M, float4(d.worldSpacePosition, 1)).xyz;
#endif
    Surface o;
    o.Normal = ll0;
    o.Albedo = half3(0, 0, 0) + l0;
    o.Emission = half3(0, 0, 0);
    lllllllllll2 = half3(0, 0, 0);
    llllllllllll2 = half3(0, 0, 0);
    lllllllllllll2 = 1;
    float lllllllllllllll2 = _Time.y;
    if (lllllllllllllllllllllllllll1)
    {
        lllllllllllllll2 = _STSCustomTime;
    }
    bool llllllllllllllll2 = (llllll0 > 0 || lllllll0 == -1 && lllllllllllllll2 - llllllll0 < llllllllllllllllllllllllll1) || (llllll0 >= 0 && lllllll0 == 1);
    bool lllllllllllllllll2 = !llllllllll0 && !lllllllllll0;
    float llllllllllllllllll2 = 0;
    half4 lllllllllllllllllll2 = half4(0, 0, 0, 0);
    if (!llllllllllll0 && (llllllllllllllll2 || lllllllllllllllll2))
    {
        float4 llllllllllllllllllll2 = float4(0, 0, 0, 0);
        float4 lllllllllllllllllllll2 = float4(0, 0, 0, 0);
        float4 llllllllllllllllllllll2 = float4(0, 0, 0, 0);
#ifdef USE_UNITY_TEXTURE_2D_TYPE
        llllllllllllllllllll2 = lllll2.texelSize;
        llllllllllllllllllllll2 = lllllll2.texelSize;
        lllllllllllllllllllll2 = llllll2.texelSize;
#else
        llllllllllllllllllll2 = llllllll2;
        llllllllllllllllllllll2 = llllllllll2;
        lllllllllllllllllllll2 = lllllllll2;
#endif
        if (llllll1 < 0)
        {
            llllll1 = 0;
        }
        half lllllllllllllllllllllll2 = 0;
        if (lllllllllllll0 == 0) 
        {
            if (llllllllllllll0 == 0 || llllllllllllll0 == 1)
            {
                float3 llllllllllllllllllllllll2 = float3(0, 0, 0);
                if (llllllllllllll0 == 0) 
                {
                    llllllllllllllllllllllll2 = llllllllllllll2 / (-1.0 * abs(lllllllllllllllll0));
                }
                else 
                {
                    llllllllllllllllllllllll2 = d.worldSpacePosition / (-1.0 * abs(lllllllllllllllll0));
                }
                if (llllllllllllllllllllllll1)
                {
                    llllllllllllllllllllllll2 = llllllllllllllllllllllll2 + abs(((lllllllllllllll2) * lllllllllllllllllllllllll1));
                }
                float3 lllllllllllllllllllllllll2 = tex2D(lllll2, llllllllllllllllllllllll2.yz).rgb;
                float3 llllllllllllllllllllllllll2 = tex2D(lllll2, llllllllllllllllllllllll2.xz).rgb;
                float3 lllllllllllllllllllllllllll2 = tex2D(lllll2, llllllllllllllllllllllll2.xy).rgb;
                float llllllllllllllllllllllllllll2 = abs(d.worldSpaceNormal.x);
                float lllllllllllllllllllllllllllll2 = abs(d.worldSpaceNormal.z);
                float3 llllllllllllllllllllllllllllll2 = lerp(llllllllllllllllllllllllll2, lllllllllllllllllllllllll2, llllllllllllllllllllllllllll2).rgb;
                float3 lllllllllllllllllllllllllllllll2 = lerp(llllllllllllllllllllllllllllll2, lllllllllllllllllllllllllll2, lllllllllllllllllllllllllllll2).rgb;
                lllllllllllllllllllllll2 = lllllllllllllllllllllllllllllll2.r;
            }
            else if (llllllllllllll0 == 2) 
            {
                float2 l3 = lllll0.xy / max(0.01, lllll0.w);
                if (llllllllllllllllllllllll1)
                {
                    l3 = l3 + abs(((lllllllllllllll2) * lllllllllllllllllllllllll1));
                }
                float2 ll3 = l3 * _ScreenParams.xy;                
                float2 lll3 = frac(ll3 * llllllllllllllllllll2.xy); 
                float llll3 = tex2D(lllll2, lll3 * abs(lllllllllllllllll0)).r;
                lllllllllllllllllllllll2 = llll3;
            }
        }
        else if (lllllllllllll0 == 1) 
        {
            float4 lllll3 = float4(lllll0.xy / max(0.01, lllll0.w), 0, 0);
            float2 llllll3 = lllll3 * _ScreenParams.xy;
            float DITHER_THRESHOLDS[16] =
            {
                1.0 / 17.0, 9.0 / 17.0, 3.0 / 17.0, 11.0 / 17.0,
                13.0 / 17.0, 5.0 / 17.0, 15.0 / 17.0, 7.0 / 17.0,
                4.0 / 17.0, 12.0 / 17.0, 2.0 / 17.0, 10.0 / 17.0,
                16.0 / 17.0, 8.0 / 17.0, 14.0 / 17.0, 6.0 / 17.0
            };
            uint llllllllllllllllllllll7 = (uint(llllll3.x) % 4) * 4 + uint(llllll3.y) % 4;
            lllllllllllllllllllllll2 = DITHER_THRESHOLDS[llllllllllllllllllllll7];
        }
        else 
        {
            lllllllllllllllllllllll2 = 0.5; 
        }
        float3 llllllll3 = UNITY_MATRIX_V[2].xyz;
#ifdef _HDRP
                llllllll3 =  mul(UNITY_MATRIX_M, transpose(mul(UNITY_MATRIX_I_M, UNITY_MATRIX_I_V)) [2]).xyz;
#else
        llllllll3 = mul(UNITY_MATRIX_M, transpose(mul(UNITY_MATRIX_I_M, UNITY_MATRIX_I_V))[2]).xyz;
#endif
        float lllllllll3 = 0;
        float llllllllll3 = 0;
        float lllllllllll3 = 1;
        bool llllllllllll3 = false;
        float lllllllllllll3 = 0;
        float llllllllllllll3 = 0;
        float lllllllllllllll3 = 0;
        float llllllllllllllll3 = 0;
        float lllllllllllllllll3 = 0;
        float llllllllllllllllll3 = 0;
#if defined(_ZONING)  
                if(llllllllllllllllllllllllllll1) {
                    float lllllllllllllllllll3 = 0;
                    for (int z = 0; z < _ZonesDataCount; z++){
                        bool llllllllllllllllllll3 = false;
                        float lllllllllllllllllllll3 = lllllllllllllllllll3;
                        if (_ZDFA[lllllllllllllllllll3 + 1] == 0) { 
#if !_EXCLUDE_ZONEBOXES
                            float llllllllllllllllllllll3 = lllllllllllllllllll3 + 2; 
                            float3 lllllllllllllllllllllll3 = d.worldSpacePosition - float3(_ZDFA[llllllllllllllllllllll3],_ZDFA[llllllllllllllllllllll3+1], _ZDFA[llllllllllllllllllllll3+2]);
                            float3 llllllllllllllllllllllll3 =     float3(_ZDFA[llllllllllllllllllllll3+ 3],_ZDFA[llllllllllllllllllllll3+ 4], _ZDFA[llllllllllllllllllllll3+ 5]);
                            float3 lllllllllllllllllllllllll3 =     float3(_ZDFA[llllllllllllllllllllll3+ 6],_ZDFA[llllllllllllllllllllll3+ 7], _ZDFA[llllllllllllllllllllll3+ 8]);
                            float3 llllllllllllllllllllllllll3 =     float3(_ZDFA[llllllllllllllllllllll3+ 9],_ZDFA[llllllllllllllllllllll3+10], _ZDFA[llllllllllllllllllllll3+11]);
                            float3 lllllllllllllllllllllllllll3 = float3(_ZDFA[llllllllllllllllllllll3+12],_ZDFA[llllllllllllllllllllll3+13], _ZDFA[llllllllllllllllllllll3+14]);
                            float llllllllllllllllllllllllllll3 = abs(dot(lllllllllllllllllllllll3, llllllllllllllllllllllll3));
                            float lllllllllllllllllllllllllllll3 = abs(dot(lllllllllllllllllllllll3, lllllllllllllllllllllllll3));
                            float llllllllllllllllllllllllllllll3 = abs(dot(lllllllllllllllllllllll3, llllllllllllllllllllllllll3));
                            llllllllllllllllllll3 =    llllllllllllllllllllllllllll3 <= lllllllllllllllllllllllllll3.x &&
                                        lllllllllllllllllllllllllllll3 <= lllllllllllllllllllllllllll3.y &&
                                        llllllllllllllllllllllllllllll3 <= lllllllllllllllllllllllllll3.z;
                            if(llllllllllllllllllll3 && llllllllllllllllll1 == 1 && l2) {
                                lllllllllllllll3 = _ZDFA[llllllllllllllllllllll3+1] - _ZDFA[llllllllllllllllllllll3+13];  
                                if(lllllllllllllllllll1 == 0) {                                    
                                    bool lllllllllllllllllllllllllllllll3 = ((lllllllllllllll3 - ll2)  <= llllllllllllllllllll1); 
                                    if(!lllllllllllllllllllllllllllllll3) {
                                        llllllllllllllllllll3 = false;
                                    }
                                }
                            }
                            if(llllllllllllllllllll3) {
                                float l4 = lllllllllllllllllllllllllll3.x - llllllllllllllllllllllllllll3;
                                float ll4 = lllllllllllllllllllllllllll3.y - lllllllllllllllllllllllllllll3;
                                float lll4 = lllllllllllllllllllllllllll3.z - llllllllllllllllllllllllllllll3;
                                float llll4 = min(ll4,l4);
                                llll4 = min(llll4,lll4);
                                llllllllllllll3 = max(llll4,llllllllllllll3);
                                if(llll4<0) {
                                    llllllllllllll3 = 0;
                                }
                            }
                            if (llllllllllllllllllll3)
                            {
                                if (llllllllllll3 == false)
                                {
                                    lllllllllllll3 = _ZDFA[lllllllllllllllllllll3];
                                    llllllllllll3 = true;
                                    llllllllllllllll3 = _ZDFA[lllllllllllllllllllll3 + 17];
                                    llllllllllllllllll3 = _ZDFA[lllllllllllllllllllll3 + 18];
                                    lllllllllllllllll3 = _ZDFA[lllllllllllllllllllll3 + 19];
                                }                     
                            }
#endif        
                            lllllllllllllllllll3 = lllllllllllllllllll3 + 17 + 3;
                        } else if (_ZDFA[lllllllllllllllllll3 + 1] == 1) { 
#if !_EXCLUDE_ZONESPHERES
                            float lllll4 = lllllllllllllllllll3 + 2; 
                            float3 llllll4 = float3(_ZDFA[lllll4], _ZDFA[lllll4 + 1], _ZDFA[lllll4 + 2]);
                            float lllllll4 = _ZDFA[lllll4 + 3];
                            float llllllll4 = distance(d.worldSpacePosition, llllll4);
                            llllllllllllllllllll3 = llllllll4 < lllllll4;
                            if (llllllllllllllllllll3 && llllllllllllllllll1 == 1 && l2)
                            {
                                lllllllllllllll3 = _ZDFA[lllll4 + 1] - _ZDFA[lllll4 + 3];
                                if (lllllllllllllllllll1 == 0)
                                {
                                    bool lllllllllllllllllllllllllllllll3 = ((lllllllllllllll3 - ll2) <= llllllllllllllllllll1);
                                    if (!lllllllllllllllllllllllllllllll3)
                                    {
                                        llllllllllllllllllll3 = false;
                                    }
                                }
                            }
                            if (llllllllllllllllllll3)
                            {
                                if (llllllllllll3 == false)
                                {
                                    lllllllllllll3 = _ZDFA[lllllllllllllllllllll3];
                                    llllllllllll3 = true;
                                    llllllllllllllll3 = _ZDFA[lllllllllllllllllllll3 + 6];
                                    llllllllllllllllll3 = _ZDFA[lllllllllllllllllllll3 + 7];
                                    lllllllllllllllll3 = _ZDFA[lllllllllllllllllllll3 + 8];
                                }
                            }
                            if (llllllllllllllllllll3)
                             {
                                float llll4 = max(0, (lllllll4 - llllllll4));
                                llllllllllllll3 = max(llll4, llllllllllllll3);
                            }
#endif
                            lllllllllllllllllll3 = lllllllllllllllllll3 + 6 + 3;
                        } else if (_ZDFA[lllllllllllllllllll3 + 1] == 2) { 
#if !_EXCLUDE_ZONECYLINDERS
                            float lllllllllll4 = lllllllllllllllllll3 + 2;
                            float3 llllllllllll4 = float3(_ZDFA[lllllllllll4], _ZDFA[lllllllllll4 + 1], _ZDFA[lllllllllll4 + 2]);
                            float3 lllllllllllll4 = float3(_ZDFA[lllllllllll4 + 3], _ZDFA[lllllllllll4 + 4], _ZDFA[lllllllllll4 + 5]);
                            float llllllllllllll4 = dot(d.worldSpacePosition.xyz - llllllllllll4, lllllllllllll4);
                            float lllllllllllllll4 = _ZDFA[lllllllllll4 + 6];
                            float llllllllllllllll4 = _ZDFA[lllllllllll4 + 7];
                            float lllllllllllllllll4 = length((d.worldSpacePosition.xyz - llllllllllll4) - llllllllllllll4 * lllllllllllll4);
                            llllllllllllllllllll3 = (abs(llllllllllllll4) < llllllllllllllll4/2) && (lllllllllllllllll4 < lllllllllllllll4);
                            if (llllllllllllllllllll3)
                            {
                                if (llllllllllll3 == false)
                                {
                                    lllllllllllll3 = _ZDFA[lllllllllllllllllllll3];
                                    llllllllllll3 = true;
                                    llllllllllllllll3 = _ZDFA[lllllllllllllllllllll3 + 10];
                                    llllllllllllllllll3 = _ZDFA[lllllllllllllllllllll3 + 11];
                                    lllllllllllllllll3 = _ZDFA[lllllllllllllllllllll3 + 12];
                                }
                            }
                            if (llllllllllllllllllll3)
                            {
                                float llll4 = max(0, (lllllllllllllll4 - lllllllllllllllll4));
                                llll4 = min(llll4, (llllllllllllllll4/2 - abs(llllllllllllll4)));
                                llllllllllllll3 = max(llll4, llllllllllllll3);
                            }
#endif
                            lllllllllllllllllll3 = lllllllllllllllllll3 + 10 + 3;
                        }
                        else if (_ZDFA[lllllllllllllllllll3 + 1] == 3) { 
#if !_EXCLUDE_ZONECONES
                            float lllllllllll4 = lllllllllllllllllll3 + 2;
                            float3 llllllllllll4 = float3(_ZDFA[lllllllllll4], _ZDFA[lllllllllll4 + 1], _ZDFA[lllllllllll4 + 2]);
                            float3 lllllllllllll4 = float3(_ZDFA[lllllllllll4 + 3], _ZDFA[lllllllllll4 + 4], _ZDFA[lllllllllll4 + 5]);
                            float llllllllllllll4 = dot(d.worldSpacePosition.xyz - llllllllllll4, lllllllllllll4);
                            float lllllllllllllllllllllll4 = _ZDFA[lllllllllll4 + 6];
                            float llllllllllllllllllllllll4 = _ZDFA[lllllllllll4 + 7];
                            float3 lllllllllllllllllllllllll4 = llllllllllll4 + (lllllllllllll4 * llllllllllllllllllllllll4/2); 
                            float llllllllllllllllllllllllll4 = dot(lllllllllllllllllllllllll4 - d.worldSpacePosition.xyz, lllllllllllll4);
                            float lllllllllllllllllllllllllll4 = (llllllllllllllllllllllllll4 / llllllllllllllllllllllll4) * lllllllllllllllllllllll4;
                            float lllllllllllllllll4 = length((lllllllllllllllllllllllll4 - d.worldSpacePosition.xyz) - llllllllllllllllllllllllll4 * lllllllllllll4);        
                            llllllllllllllllllll3 = (abs(llllllllllllll4) < llllllllllllllllllllllll4/2) && (lllllllllllllllll4 < lllllllllllllllllllllllllll4);
                            if (llllllllllllllllllll3)
                            {
                                if (llllllllllll3 == false)
                                {
                                    lllllllllllll3 = _ZDFA[lllllllllllllllllllll3];
                                    llllllllllll3 = true;
                                    llllllllllllllll3 = _ZDFA[lllllllllllllllllllll3 + 10];
                                    llllllllllllllllll3 = _ZDFA[lllllllllllllllllllll3 + 11];
                                    lllllllllllllllll3 = _ZDFA[lllllllllllllllllllll3 + 12];
                                }
                            }
                            if (llllllllllllllllllll3)
                            {
                                float llll4 = max(0, (lllllllllllllllllllllllllll4 - lllllllllllllllll4));
                                llll4 = min(llll4, (llllllllllllllllllllllll4 - llllllllllllllllllllllllll4));
                                llllllllllllll3 = max(llll4, llllllllllllll3);
                            }
#endif
                            lllllllllllllllllll3 = lllllllllllllllllll3 + 10 + 3;
                        }
                        else if (_ZDFA[lllllllllllllllllll3 + 1] == 4) { 
#if !_EXCLUDE_ZONEPLANES
                            float llllllllllllllllllllllllllllll4 = lllllllllllllllllll3 + 2;
                            float3 lllllllllllllllllllllllllllllll4 = float3(_ZDFA[llllllllllllllllllllllllllllll4], _ZDFA[llllllllllllllllllllllllllllll4 + 1], _ZDFA[llllllllllllllllllllllllllllll4 + 2]);
                            float l5 = _ZDFA[llllllllllllllllllllllllllllll4 + 3];       
                            float ll5 = dot(d.worldSpacePosition.xyz, lllllllllllllllllllllllllllllll4.xyz) + l5;
                            llllllllllllllllllll3 = ll5 < 0;
                            if (llllllllllllllllllll3)
                            {
                                if (llllllllllll3 == false)
                                {
                                    lllllllllllll3 = _ZDFA[lllllllllllllllllllll3];
                                    llllllllllll3 = true;
                                    llllllllllllllll3 = _ZDFA[lllllllllllllllllllll3 + 6];
                                    llllllllllllllllll3 = _ZDFA[lllllllllllllllllllll3 + 7];
                                    lllllllllllllllll3 = _ZDFA[lllllllllllllllllllll3 + 8];
                                }
                            }
                            if (llllllllllllllllllll3)
                            {
                                float llll4 = max(0, 0 - ll5);
                                llllllllllllll3 = max(llll4, llllllllllllll3);
                            }
#endif
                            lllllllllllllllllll3 = lllllllllllllllllll3 + 6 + 3;
                        }
                }
            }
#endif
        float llll5 = 0;
        float lllll5 = llllllllllll3;
#if !defined(_PLAYERINDEPENDENT)
#if defined(_ZONING)
                    if(llllllllllll3 && llllllllllllllllll1 == 1 && lllllllllllllllllll1 == 1 && l2) {
                        float llllll5 = 0;
                        bool lllllll5 = false;
                        for (int i = 0; i < _ArrayLength; i++){
                            float llllllll5 = _PlayersDataFloatArray[llllll5+1]; 
                            float3 lllllllll5 = _PlayersPosVectorArray[llllllll5].xyz - _WorldSpaceCameraPos;               
                            if(dot(llllllll3,lllllllll5) <= 0) {       
                                if(!lllllllllllllllll2) {
                                    float llllllllll5 = llllll5 + 3;
                                    float lllllllllll5 = 4;
                                    for (int llllllllllllllll8 = 0; llllllllllllllll8 < _PlayersDataFloatArray[llllll5 + 2]; llllllllllllllll8++){
                                        float llllllllllll5 = _PlayersDataFloatArray[llllllllll5 + llllllllllllllll8 * lllllllllll5 + 2];
                                        if (llllllllllll5 != 0 && llllllllllll5 == lllllllll0) {
                                            float lllllllllllll5 = _PlayersDataFloatArray[llllllllll5 + llllllllllllllll8 * lllllllllll5 ];
                                            float llllllllllllll5 = _PlayersDataFloatArray[llllllllll5 + llllllllllllllll8 * lllllllllll5 + 1];
                                            if ((llllllllllllll5 == -1 && lllllllllllllll2 - lllllllllllll5 < llllllllllllllllllllllllll1 )|| (llllllllllllll5 == 1) ) {
                                                float lllllllllllllll5 = _PlayersPosVectorArray[llllllll5].y+ lllllllllllllllllllll1;
                                                if(lll2) {
                                                    if(i==0) {
                                                        llll5 = lllllllllllllll5;
                                                    } else {
                                                        llll5 = max(llll5,lllllllllllllll5);
                                                    }
                                                }
                                                bool llllllllllllllll5 = lllllllllllllll3 >= lllllllllllllll5 + ll2; 
                                                if(!llllllllllllllll5) {
                                                    lllllll5 = true;
                                                } 
                                            }                        
                                        }
                                    }
                                } else if (llllllll1 == 0 || distance(_PlayersPosVectorArray[llllllll5].xyz, d.worldSpacePosition.xyz) < lllllll1) {
                                    float lllllllllllllll5 = _PlayersPosVectorArray[llllllll5].y+ lllllllllllllllllllll1;
                                    if(lll2) {
                                        if(i==0) {
                                            llll5 = lllllllllllllll5;
                                        } else {
                                            llll5 = max(llll5,lllllllllllllll5);
                                        }
                                    }
                                    bool llllllllllllllll5 = lllllllllllllll3 >= lllllllllllllll5 + ll2; 
                                    if(!llllllllllllllll5) {
                                        lllllll5 = true;
                                    } 
                                }
                                llllll5 = llllll5 + _PlayersDataFloatArray[llllll5 + 2]*4 + 3; 
                                llllll5 = llllll5 + _PlayersDataFloatArray[llllll5]*4 + 1; 
                            }
                        }
                        if(!lllllll5) {
                            llllllllllll3 = false;
                        }
                    }
#endif
        float llllll5 = 0;
        for (int i = 0; i < _ArrayLength; i++)
        {
            float llllllll5 = _PlayersDataFloatArray[llllll5 + 1];
            if (sign(_PlayersPosVectorArray[llllllll5].w) != -1) 
            {
                float3 lllllllll5 = _PlayersPosVectorArray[llllllll5].xyz - _WorldSpaceCameraPos;
                float llllllllllllllllllllll5 = 0;
                float lllllllllll5 = 4;
                if (!lllllllllllllllll2)
                {
                    float llllllllll5 = llllll5 + 3;
                    for (int llllllllllllllll8 = 0; llllllllllllllll8 < _PlayersDataFloatArray[llllll5 + 2]; llllllllllllllll8++)
                    {
                        float llllllllllll5 = _PlayersDataFloatArray[llllllllll5 + llllllllllllllll8 * lllllllllll5 + 2];
                        if (llllllllllll5 != 0 && llllllllllll5 == lllllllll0)
                        {
                            float lllllllllllll5 = _PlayersDataFloatArray[llllllllll5 + llllllllllllllll8 * lllllllllll5];
                            float llllllllllllll5 = _PlayersDataFloatArray[llllllllll5 + llllllllllllllll8 * lllllllllll5 + 1];
                            llllllllllllllllllllll5 = 1;
                            if (llllllllllllll5 != 0 && lllllllllllll5 != 0 && lllllllllllllll2 - lllllllllllll5 < llllllllllllllllllllllllll1)
                            {
                                if (llllllllllllll5 == 1)
                                {
                                    llllllllllllllllllllll5 = ((llllllllllllllllllllllllll1 - (lllllllllllllll2 - lllllllllllll5)) / llllllllllllllllllllllllll1);
                                }
                                else
                                {
                                    llllllllllllllllllllll5 = ((lllllllllllllll2 - lllllllllllll5) / llllllllllllllllllllllllll1);
                                }
                            }
                            else if (llllllllllllll5 == -1)
                            {
                                llllllllllllllllllllll5 = 1;
                            }
                            else if (llllllllllllll5 == 1)
                            {
                                llllllllllllllllllllll5 = 0;
                            }
                            else
                            {
                                llllllllllllllllllllll5 = 1;
                            }
                            llllllllllllllllllllll5 = 1 - llllllllllllllllllllll5;
                        }
                    }
                }
                llllll5 = llllll5 + _PlayersDataFloatArray[llllll5 + 2] * 4 + 3;
                float llllllllllllllllllllllllllll5 = 0;
                float lllllllllllllllllllllllllllll5 = 0;
                float llllllllllllllllllllllllllllll5 = 0;
                float lllllllllllllllllllllllllllllll5 = lllllllllllllllllllllllllllll5;
                bool l6 = distance(_PlayersPosVectorArray[llllllll5].xyz, d.worldSpacePosition) > lllllll1;
                if ((llllllllllllllllllllll5 != 0) || ((!llllllllll0 && !lllllllllll0) && (llllllll1 == 0 || !l6)))
                {
#if defined(_ZONING)
                            if(llllllllllllllllllllllllllll1) {
                                if(llllllllllll3) 
                                {
                                    if(llllllllllllllllllllllllllllll1) {
                                        float llllllllll5 = llllll5 + 1;
                                        for (int llllllllllllllll8 = 0; llllllllllllllll8 < _PlayersDataFloatArray[llllll5]; llllllllllllllll8++){
                                            float llllllllllll5 = _PlayersDataFloatArray[llllllllll5 + llllllllllllllll8 * lllllllllll5 + 2];
                                            if (llllllllllll5 != 0 && llllllllllll5 == lllllllllllll3) {
                                                float lllllllllllll5 = _PlayersDataFloatArray[llllllllll5 + llllllllllllllll8 * lllllllllll5 ];
                                                float llllllllllllll5 = _PlayersDataFloatArray[llllllllll5 + llllllllllllllll8 * lllllllllll5 + 1];
                                                llllllllllllllllllllllllllll5 = 1;
                                                float llllll6 = _PlayersDataFloatArray[llllllllll5 + llllllllllllllll8 * lllllllllll5 + 3];
                                                if( llllllllllllll5!= 0 && lllllllllllll5 != 0 && lllllllllllllll2-lllllllllllll5 < llllll6) {
                                                    if(llllllllllllll5 == 1) {
                                                        llllllllllllllllllllllllllll5 = ((llllll6-(lllllllllllllll2-lllllllllllll5))/llllll6);
                                                    } else {
                                                        llllllllllllllllllllllllllll5 = ((lllllllllllllll2-lllllllllllll5)/llllll6);
                                                    }
                                                } else if(llllllllllllll5 ==-1) {
                                                    llllllllllllllllllllllllllll5 = 1;
                                                } else if(llllllllllllll5 == 1) {
                                                    llllllllllllllllllllllllllll5 = 0;
                                                } else {
                                                    llllllllllllllllllllllllllll5 = 1;
                                                }
                                                llllllllllllllllllllllllllll5 = 1 - llllllllllllllllllllllllllll5;
                                            }
                                            if(lllllllllllllllllllllllllllll1 == 0 && llllllllllllllllllllllllllllll1) {
                                                float lllllll6 = 1 / lllllllllllllllllllllllllllllll1;
                                                if (llllllllllllll3 < lllllllllllllllllllllllllllllll1)  {
                                                    float llllllll6 = ((lllllllllllllllllllllllllllllll1-llllllllllllll3) * lllllll6);
                                                    llllllllllllllllllllllllllll5 =  max(llllllllllllllllllllllllllll5,llllllll6);
                                                }
                                            }
                                        }
                                    } else { 
                                    }
                                } else {
                                }
                            }
#endif
                    if (dot(llllllll3, lllllllll5) <= 0)
                    {
                        if (lllllllllllllllllllllll0 == 2 || lllllllllllllllllllllll0 == 3 || lllllllllllllllllllllll0 == 4 || lllllllllllllllllllllll0 == 5 || lllllllllllllllllllllll0 == 6 || lllllllllllllllllllllll0 == 7)
                        {
                            float4 lllllllll6 = float4(0, 0, 0, 0);
                            float4 llllllllll6 = float4(0, 0, 0, 0);
                            float lllllllllll6 = 0;
                            if (llll1 || lllllllllllllllllllllll0 == 6)
                            {
                                float llllllllllll6 = _ScreenParams.x / _ScreenParams.y;
#ifdef _HDRP
                                        float4 lllllllllllll6 = mul(UNITY_MATRIX_VP, float4(GetCameraRelativePositionWS(_PlayersPosVectorArray[llllllll5].xyz), 1.0));
                                        llllllllll6 = ComputeScreenPos(lllllllllllll6 , _ProjectionParams.x);
#else
                                float4 lllllllllllll6 = mul(UNITY_MATRIX_VP, float4(_PlayersPosVectorArray[llllllll5].xyz, 1.0));
                                llllllllll6 = ComputeScreenPos(lllllllllllll6);
#endif
                                llllllllll6.xy /= llllllllll6.w;
                                llllllllll6.x *= llllllllllll6;
#ifdef _HDRP
                                        float4 lllllllllllllll6 = mul(UNITY_MATRIX_VP, float4(GetCameraRelativePositionWS(d.worldSpacePosition.xyz), 1.0));
                                        lllllllll6 = ComputeScreenPos(lllllllllllllll6 , _ProjectionParams.x);
#else
                                float4 lllllllllllllll6 = mul(UNITY_MATRIX_VP, float4(d.worldSpacePosition.xyz, 1.0));
                                lllllllll6 = ComputeScreenPos(lllllllllllllll6);
#endif
                                lllllllll6.xy /= lllllllll6.w;
                                lllllllll6.x *= llllllllllll6;
#if defined(_DISSOLVEMASK)
                                        if(llll1) {
                                                lllllllllll6 = max(lllllllllllllllllllll2.z,lllllllllllllllllllll2.w);
                                        }
#endif
                            }
                            float3 lllllllllllllllll6 = _PlayersPosVectorArray[llllllll5].xyz;
                            float3 llllllllllllllllll6 = _WorldSpaceCameraPos -  lllllllllllllllll6;
                            float3 lllllllllllllllllll6 = normalize(llllllllllllllllll6);
                            float3 llllllllllllllllllll6 = lllllllllllllllll6 + (llllllllllllllllllllllll0 * lllllllllllllllllll6);
                            float3 lllllllllllllllllllll6 = _WorldSpaceCameraPos - llllllllllllllllllll6;
                            float llllllllllllll4 = dot(d.worldSpacePosition.xyz - llllllllllllllllllll6, lllllllllllllllllll6);
                            float lllllllllllllllllllllll6 = 0;
                            float llllllllllllllllllllllll6 = 0;
                            float2 lllllllllllllllllllllllll6 = float2(0, 0);
                            if (lllllllllllllllllllllll0 == 2 || lllllllllllllllllllllll0 == 3)
                            {
                                lllllllllllllllllllllll6 = llllllllllllllllllllllllll0;
                                float lllllllllllllllll4 = length((d.worldSpacePosition.xyz - llllllllllllllllllll6) - llllllllllllll4 * lllllllllllllllllll6);
                                float llllllllllllllllllllllll4 = length(lllllllllllllllllllll6);
                                float lllllllllllllllllllllll4 = lllllllllllllllllllllllllll0;
                                float lllllllllllllllllllllllllll4 = (llllllllllllll4 / llllllllllllllllllllllll4) * lllllllllllllllllllllll4;
#if _DISSOLVEMASK
                                        float llllllllllllllllllllllllllllll6 = (2*lllllllllllllllllllllllllll4) / lllllllllll6;
                                        float2 lllllllllllllllllllllllllllllll6 = lllllllll6.xy - llllllllll6.xy;
                                        lllllllllllllllllllllllllllllll6 =  normalize(lllllllllllllllllllllllllllllll6)*lllllllllllllllll4;
                                        lllllllllllllllllllllllll6 = lllllllllllllllllllllllllllllll6 /llllllllllllllllllllllllllllll6;
#else
                                float l7 = lllllllllllllllll4 < lllllllllllllllllllllllllll4;
                                if (l7)
                                {
                                    float ll7 = lllllllllllllllll4 / lllllllllllllllllllllllllll4;
                                    llllllllllllllllllllllll6 = ll7;
                                }
                                else
                                {
                                    llllllllllllllllllllllll6 = -1;
                                }
#endif
                            }
                            else if (lllllllllllllllllllllll0 == 4 || lllllllllllllllllllllll0 == 5)
                            {
                                lllllllllllllllllllllll6 = llllllllllllllllllllllllllll0;
                                float lllllllllllllllll4 = length((d.worldSpacePosition.xyz - llllllllllllllllllll6) - llllllllllllll4 * lllllllllllllllllll6);
                                float lllllllllllllll4 = lllllllllllllllllllllllllllll0;
                                float lllll7 = (lllllllllllllllll4 < lllllllllllllll4) && llllllllllllll4 > 0;
#if _DISSOLVEMASK
                                        float llllllllllllllllllllllllllllll6 = (2*lllllllllllllll4) / lllllllllll6;
                                        float2 lllllllllllllllllllllllllllllll6 = lllllllll6.xy - llllllllll6.xy;
                                        lllllllllllllllllllllllllllllll6 =  normalize(lllllllllllllllllllllllllllllll6)*lllllllllllllllll4;
                                        lllllllllllllllllllllllll6 = lllllllllllllllllllllllllllllll6 /llllllllllllllllllllllllllllll6;
#else
                                if (lllll7)
                                {
                                    float ll7 = lllllllllllllllll4 / lllllllllllllll4;
                                    llllllllllllllllllllllll6 = ll7;
                                }
                                else
                                {
                                    llllllllllllllllllllllll6 = -1;
                                }
#endif
                            }
                            else if (lllllllllllllllllllllll0 == 6)
                            {
                                lllllllllllllllllllllll6 = llllllllllllllllllllllllllllll0;
                                float lllllllll7 = length(lllllllllllllllllllll6);
                                float llllllllllll6 = _ScreenParams.x / _ScreenParams.y;
                                float lllllllllll7 = min(1, llllllllllll6);
                                float llllllllllll7 = distance(lllllllll6.xy, llllllllll6.xy) < lllllllllllllllllllllllllllllll0 / lllllllll7 * lllllllllll7;
                                float lllllllllllll7 = (llllllllllll7) && llllllllllllll4 > 0;
#if _DISSOLVEMASK
                                        float llllllllllllll7 = lllllllllllllllllllllllllllllll0/lllllllll7*lllllllllll7;
                                        float llllllllllllllllllllllllllllll6 = (2*llllllllllllll7) / lllllllllll6;
                                        float2 lllllllllllllllllllllllllllllll6 = lllllllll6.xy - llllllllll6.xy;
                                        lllllllllllllllllllllllll6 = lllllllllllllllllllllllllllllll6 /llllllllllllllllllllllllllllll6;
#else
                                if (lllllllllllll7)
                                {
                                    float lllllllllllllllll7 = (distance(lllllllll6.xy, llllllllll6.xy) / (lllllllllllllllllllllllllllllll0 / lllllllll7 * lllllllllll7));
                                    llllllllllllllllllllllll6 = lllllllllllllllll7;
                                }
                                else
                                {
                                    llllllllllllllllllllllll6 = -1;
                                }
#endif
                            }
                            else if (lllllllllllllllllllllll0 == 7)
                            {
#if _OBSTRUCTION_CURVE
                                        lllllllllllllllllllllll6 = l1;
                                        float lllllllllllllllll4 = length((d.worldSpacePosition.xyz  - llllllllllllllllllll6) - llllllllllllll4 * lllllllllllllllllll6);
                                        float lllllllll7 = length(lllllllllllllllllllll6);
                                        float4 llllllllllllllllllll7 = float4(0,0,0,0);
                                        float lllllllllllllllllllll7 = llllllllllllllllllllll2.z;
                                        float llllllllllllllllllllll7 = (llllllllllllll4/lllllllll7) * lllllllllllllllllllll7;
                                        float4 lllllllllllllllllllllll7 = float4(0,0,0,0);
                                        lllllllllllllllllllllll7 = llllllllllllllllllllll2;
                                        float2 llllllllllllllllllllllll7 = (llllllllllllllllllllll7+0.5) * lllllllllllllllllllllll7.xy;
                                            llllllllllllllllllll7 = tex2D(lllllll2, llllllllllllllllllllllll7);
                                        float lllllllllllllllllllllllll7 = llllllllllllllllllll7.r * ll1;
                                        float llllllllllllllllllllllllll7 = (lllllllllllllllll4 < lllllllllllllllllllllllll7) && llllllllllllll4 > 0 ;
#if _DISSOLVEMASK
                                            float llllllllllllllllllllllllllllll6 = (2*lllllllllllllllllllllllll7) / lllllllllll6;
                                            float2 lllllllllllllllllllllllllllllll6 = lllllllll6.xy - llllllllll6.xy;
                                            lllllllllllllllllllllllllllllll6 =  normalize(lllllllllllllllllllllllllllllll6)*lllllllllllllllll4;
                                            lllllllllllllllllllllllll6 = lllllllllllllllllllllllllllllll6 /llllllllllllllllllllllllllllll6;
#else
                                            if(llllllllllllllllllllllllll7){
                                                float ll7 = lllllllllllllllll4/lllllllllllllllllllllllll7;
                                                llllllllllllllllllllllll6 = ll7;
                                            } else {
                                                llllllllllllllllllllllll6 = -1;
                                            }
#endif
#endif
                            }
#if defined(_DISSOLVEMASK)
                                    if(llll1) {
                                        float4 llllllllllllllllllllllllllllll7 = float4(0,0,0,0);
                                        llllllllllllllllllllllllllllll7 = lllllllllllllllllllll2;
                                        float2 lllllllllllllllllllllllllllllll7 = float2(llllllllllllllllllllllllllllll7.z/2,llllllllllllllllllllllllllllll7.w/2);
                                        float2 l8 = lllllllllllllllllllllllllllllll7 + lllllllllllllllllllllllll6;
                                        float2 ll8 = (l8+0.5) * llllllllllllllllllllllllllllll7.xy;
                                        float4 lll8 = float4(0,0,0,0);
                                            lll8 = tex2D(llllll2, ll8);
                                        float llll8 = -1;
                                        if(l8.x <= llllllllllllllllllllllllllllll7.z && l8.x >= 0 && l8.y <= llllllllllllllllllllllllllllll7.w && l8.y >= 0 && lll8.x <= 0 && llllllllllllll4 > 0 ){
                                            float lllll8 = sqrt(pow(llllllllllllllllllllllllllllll7.z,2)+pow(llllllllllllllllllllllllllllll7.w,2))/2;
                                            float llllll8 = 40;
                                            float lllllll8 = lllll8/llllll8;
                                            float llllllll8 = 0;
                                            llll8 = 0;     
                                                for (int i = 0; i < llllll8; i++){
                                                    float2 lllllllll8 = lllllllllllllllllllllllllllllll7 + (lllllllllllllllllllllllll6 + ( normalize(lllllllllllllllllllllllll6)*lllllll8*i));
                                                    float2 llllllllll8 = (lllllllll8+0.5) * llllllllllllllllllllllllllllll7.xy;
                                                    float4 lllllllllll8 = tex2Dlod(llllll2, float4(llllllllll8, 0.0, 0.0)); 
                                                    float2 llllllllllll8 = step(float2(0,0), lllllllll8) - step(float2(llllllllllllllllllllllllllllll7.z,llllllllllllllllllllllllllllll7.w), lllllllll8);
                                                    if(lllllllllll8.x <= 0) {
                                                        llllllll8 +=  (1/llllll8) * (llllllllllll8.x * llllllllllll8.y);
                                                    }                                            
                                                }   
                                            llll8 = 1-llllllll8;  
                                        }         
                                        llllllllllllllllllllllll6 = llll8;
                                    }
#endif
                            if (lll1 <= 1)
                            {
                                if (llllllllllllllllllllllll6 != -1)
                                {
                                    float lllllllllllll8 = max(lll1, 0.00001);
                                    float llllllllllllll8 = 1 - lllllllllllllllllllllll6;
                                    float lllllllllllllll8 = exp(lllllllllllll8 * 6);
                                    float llllllllllllllll8 = llllllllllllllllllllllll6;
                                    float lllllllllllllllll8 = llllllllllllll8 / (lllllllllllll8 / (lllllllllllll8 * llllllllllllll8 - 0.15 * (lllllllllllll8 - llllllllllllll8)));
                                    float llllllllllllllllll8 = ((llllllllllllllll8 - lllllllllllllllll8) / (lllllllllllllll8 * (1 - llllllllllllllll8) + llllllllllllllll8)) + lllllllllllllllll8;
                                    llllllllllllllllll8 = 1 - llllllllllllllllll8;
                                    lllllllllllllllllllllllllllll5 = llllllllllllllllll8 * sign(lllllllllllllllllllllll6);
                                }
                            }
                            else
                            {
                                lllllllllllllllllllllllllllll5 = llllllllllllllllllllllll6;
                            }
                        }
                        if (lllllllllllllllllllllll0 == 1 || lllllllllllllllllllllll0 == 3 || lllllllllllllllllllllll0 == 5)
                        {
                            float lllllllllllllllllll8 = distance(_WorldSpaceCameraPos, _PlayersPosVectorArray[llllllll5].xyz);
                            float llllllllllllllllllll8 = distance(_WorldSpaceCameraPos, d.worldSpacePosition.xyz);
                            float3 lllllllllllllllllllll8 = d.worldSpacePosition.xyz - _PlayersPosVectorArray[llllllll5].xyz;
                            float3 llllllllllllllllllllll8 = d.worldSpaceNormal;
                            float lllllllllllllllllllllll8 = acos(dot(lllllllllllllllllllll8, llllllllllllllllllllll8) / (length(lllllllllllllllllllll8) * length(llllllllllllllllllllll8)));
                            if (lllllllllllllllllllllll8 <= 1.5 && lllllllllllllllllll8 > llllllllllllllllllll8)
                            {
                                float llllllllllllllllllllllll8 = (sqrt((lllllllllllllllllll8 - llllllllllllllllllll8)) * 25 / lllllllllllllllllllllll8) * lllllllllllllllllllllllll0;
                                lllllllllllllllllllllllllllll5 += max(0, log(llllllllllllllllllllllll8 * 0.2));
                            }
                        }
                    }
                    float lllllllllllllllllllllllll8 = lllllllllllllllllllllllllllll5;
                    float llllllllllllllllllllllllll8 = 0;
                    float lllllllllllllllllllllllllll8 = 0;
                    if (lllll1 == 1 && lllllllllllllllllllllllllllll1 == 0 && !llllllllllllllllllllllllllllll1)
                    {
                        lllllllllllllllllllllllllllll5 = min((1 * llllll1), 1);
                        llllllllllllllllllllllllllllll5 = lllllllllllllllllllllllllllll5;
                    }
                    else
                    {
                        lllllllllllllllllllllllllllll5 = min(lllllllllllllllllllllllllllll5 + (1 * llllll1), 1);
                        llllllllllllllllllllllllllllll5 = min((1 * llllll1), 1);
                    }
                    if (llllllllllll3)
                    {
                        if (lllllllllllllllllllllllllllll1 == 1)
                        {
                            float lllllll6 = 1 / lllllllllllllllllllllllllllllll1;
                            if (llllllllllllll3 < lllllllllllllllllllllllllllllll1)
                            {
                                float lllllllllllllllllllllllllllll8 = 1 - ((lllllllllllllllllllllllllllllll1 - llllllllllllll3) * lllllll6);
                                lllllllllllllllllllllllllllll5 = min(lllllllllllllllllllllllllllll5, lllllllllllllllllllllllllllll8);
                                llllllllllllllllllllllllllllll5 = min(llllllllllllllllllllllllllllll5, lllllllllllllllllllllllllllll8);
                            }
                        }
                        else if (lllllllllllllllllllllllllllll1 == 0 && !llllllllllllllllllllllllllllll1)
                        {
                            if (lllll1 == 1)
                            {
                                float llllllllllllllllllllllllllllll8 = ((lllllllllllllllllllllllll8) / lllllllllllllllllllllllllllllll1);
                                if (llllllllllllll3 < lllllllllllllllllllllllllllllll1 && lllllllllllllllllllllllll8 > 0 && saturate(lllllllllllllllllllllllll8) > llllll1)
                                {
                                    float lllllllllllllllllllllllllllll8 = ((lllllllllllllllllllllllllllllll1 - llllllllllllll3) * (llllllllllllllllllllllllllllll8));
                                    lllllllllllllllllllllllll8 = lllllllllllllllllllllllll8 - (lllllllllllllllllllllllllllll8);
                                }
                                else
                                {
                                }
                            }
                            if (llllllllllllll3 < lllllllllllllllllllllllllllllll1)
                            {
                                float lllllll6 = lllllllllllllllllllllllllllll5 / lllllllllllllllllllllllllllllll1;
                                float lllllllllllllllllllllllllllll8 = ((lllllllllllllllllllllllllllllll1 - llllllllllllll3) * lllllll6);
                                lllllllllllllllllllllllllllll5 = max(0, lllllllllllllllllllllllllllll8);
                                float lll9 = llllllllllllllllllllllllllllll5 / lllllllllllllllllllllllllllllll1;
                                float llll9 = ((lllllllllllllllllllllllllllllll1 - llllllllllllll3) * lll9);
                                llllllllllllllllllllllllllllll5 = max(0, llll9);
                                llllllllllllllllllllllllll8 = lllllllllllllllllllllllllllll5;
                                lllllllllllllllllllllllllll8 = llllllllllllllllllllllllllllll5;
                                if (lllll1 == 0 || lllll1 == 1)
                                {
                                    lllllllllllllllllllllllllllll5 = max(lllllllllllllllllllllllll8, lllllllllllllllllllllllllllll8);
                                }
                            }
                            else
                            {
                                lllllllllllllllllllllllllllll5 = 0;
                                llllllllllllllllllllllllllllll5 = 0;
                                llllllllllllllllllllllllll8 = lllllllllllllllllllllllllllll5;
                                lllllllllllllllllllllllllll8 = llllllllllllllllllllllllllllll5;
                                if (lllll1 == 0 || lllll1 == 1)
                                {
                                    lllllllllllllllllllllllllllll5 = max(lllllllllllllllllllllllll8, lllllllllllllllllllllllllllll5);
                                }
                            }
                        }
                    }
                    if (lllllllll1)
                    {
                        float lllll9 = lllllllllllllllllllllllllllll5 / lllllllllll1;
                        float llllll9 = llllllllllllllllllllllllllllll5 / lllllllllll1;
                        float3 lllllllll5 = _PlayersPosVectorArray[llllllll5].xyz - _WorldSpaceCameraPos;
                        float3 llllllll9 = d.worldSpacePosition.xyz - _WorldSpaceCameraPos;
                        float lllllllll9 = dot(llllllll9, normalize(lllllllll5));
                        if (lllllllll9 - llllllllll1 >= length(lllllllll5))
                        {
                            float llllllllll9 = lllllllll9 - llllllllll1 - length(lllllllll5);
                            if (llllllllll9 < 0)
                            {
                                llllllllll9 = 0;
                            }
                            if (llllllllll9 < lllllllllll1)
                            {
                                lllllllllllllllllllllllllllll5 = (lllllllllll1 - llllllllll9) * lllll9;
                                llllllllllllllllllllllllllllll5 = (lllllllllll1 - llllllllll9) * llllll9;
                            }
                            else
                            {
                                lllllllllllllllllllllllllllll5 = 0;
                                llllllllllllllllllllllllllllll5 = 0;
                            }
                        }
                    }
                    if (llllllllllllllllllllllllllll1 && !llllllllllll3)
                    {
                        if (lllllllllllllllllllllllllllll1 == 1)
                        {
                            lllllllllllllllllllllllllllll5 = 0;
                            llllllllllllllllllllllllllllll5 = 0;
                        }
                    }
                    if (llllllllllll1 == 1)
                    {
                        float lllllllllll9 = 0;
                        float llllllllllll9 = 0;
                        if (llllllllllllll1 == 0)
                        {
                            lllllllllll9 = lllllllllllllllllllllllllllll5 / lllllllllllllllll1;
                            llllllllllll9 = llllllllllllllllllllllllllllll5 / lllllllllllllllll1;
                        }
                        else if (llllllllllllll1 == 1)
                        {
                            float lllllllllllll9 = 1 - lllllllllllllllllllllllllllll5;
                            float llllllllllllll9 = 1 - llllllllllllllllllllllllllllll5;
                            if (llllllllllllllllllllllllllll1 && llllllllllll3 && llllllllllllllllllllllllllllll1)
                            {
                                lllllllllllll9 = max(1 - lllllllllllllllllllllllllllll5, 1 - (lllllllllllllllllllllllllllll5 * llllllllllllllllllllllllllll5));
                                llllllllllllll9 = max(1 - llllllllllllllllllllllllllllll5, 1 - (llllllllllllllllllllllllllllll5 * llllllllllllllllllllllllllll5));
                            }
                            lllllllllll9 = lllllllllllll9 / lllllllllllllllll1;
                            llllllllllll9 = llllllllllllll9 / lllllllllllllllll1;
                        }
                        if (lllllllllllll1 == 1)
                        {
                            if (d.worldSpacePosition.y > (_PlayersPosVectorArray[llllllll5].y + llllllllllllllll1))
                            {
                                float llllllllll9 = d.worldSpacePosition.y - (_PlayersPosVectorArray[llllllll5].y + llllllllllllllll1);
                                if (llllllllll9 < 0)
                                {
                                    llllllllll9 = 0;
                                }
                                if (llllllllllllll1 == 0)
                                {
                                    if (llllllllll9 < lllllllllllllllll1)
                                    {
                                        lllllllllllllllllllllllllllll5 = ((lllllllllllllllll1 - llllllllll9) * lllllllllll9);
                                        llllllllllllllllllllllllllllll5 = ((lllllllllllllllll1 - llllllllll9) * llllllllllll9);
                                    }
                                    else
                                    {
                                        lllllllllllllllllllllllllllll5 = 0;
                                        llllllllllllllllllllllllllllll5 = 0;
                                    }
                                }
                                else
                                {
                                    if (llllllllll9 < lllllllllllllllll1)
                                    {
                                        lllllllllllllllllllllllllllll5 = 1 - ((lllllllllllllllll1 - llllllllll9) * lllllllllll9);
                                        llllllllllllllllllllllllllllll5 = 1 - ((lllllllllllllllll1 - llllllllll9) * llllllllllll9);
                                    }
                                    else
                                    {
                                        lllllllllllllllllllllllllllll5 = 1;
                                        llllllllllllllllllllllllllllll5 = 1;
                                    }
                                    llllllllllllllllllllllllllll5 = 1;
                                }
                            }
                        }
                        else
                        {
                            if (d.worldSpacePosition.y > lllllllllllllll1)
                            {
                                float llllllllll9 = d.worldSpacePosition.y - lllllllllllllll1;
                                if (llllllllll9 < 0)
                                {
                                    llllllllll9 = 0;
                                }
                                if (llllllllllllll1 == 0)
                                {
                                    if (llllllllll9 < lllllllllllllllll1)
                                    {
                                        lllllllllllllllllllllllllllll5 = ((lllllllllllllllll1 - llllllllll9) * lllllllllll9);
                                        llllllllllllllllllllllllllllll5 = ((lllllllllllllllll1 - llllllllll9) * llllllllllll9);
                                    }
                                    else
                                    {
                                        lllllllllllllllllllllllllllll5 = 0;
                                        llllllllllllllllllllllllllllll5 = 0;
                                    }
                                }
                                else
                                {
                                    if (llllllllll9 < lllllllllllllllll1)
                                    {
                                        lllllllllllllllllllllllllllll5 = 1 - ((lllllllllllllllll1 - llllllllll9) * lllllllllll9);
                                        llllllllllllllllllllllllllllll5 = 1 - ((lllllllllllllllll1 - llllllllll9) * llllllllllll9);
                                    }
                                    else
                                    {
                                        lllllllllllllllllllllllllllll5 = 1;
                                        llllllllllllllllllllllllllllll5 = 1;
                                    }
                                    llllllllllllllllllllllllllll5 = 1;
                                }
                            }
                        }
                    }
                    float lllllllllllllllll9 = lllllllllllllllllllllllllllll5;
                    float llllllllllllllllll9 = llllllllllllllllllllllllllllll5;
                    if (llllllllllllllllll1 == 1)
                    {
                        float lllllllllllllllllll9 = lllllllllllllllllllllllllllll5 / llllllllllllllllllllll1;
                        float llllllllllllllllllll9 = llllllllllllllllllllllllllllll5 / llllllllllllllllllllll1;
                        if (lllllllllllllllllll1 == 1)
                        {
                            if (d.worldSpacePosition.y < (_PlayersPosVectorArray[llllllll5].y + lllllllllllllllllllll1))
                            {
                                float llllllllll9 = (_PlayersPosVectorArray[llllllll5].y + lllllllllllllllllllll1) - d.worldSpacePosition.y;
                                if (llllllllll9 < 0)
                                {
                                    llllllllll9 = 0;
                                }
                                if (llllllllll9 < llllllllllllllllllllll1)
                                {
                                    lllllllllllllllllllllllllllll5 = (llllllllllllllllllllll1 - llllllllll9) * lllllllllllllllllll9;
                                    llllllllllllllllllllllllllllll5 = (llllllllllllllllllllll1 - llllllllll9) * llllllllllllllllllll9;
                                }
                                else
                                {
                                    lllllllllllllllllllllllllllll5 = 0;
                                    llllllllllllllllllllllllllllll5 = 0;
                                }
                            }
                        }
                        else
                        {
                            if (d.worldSpacePosition.y < llllllllllllllllllll1)
                            {
                                float llllllllll9 = llllllllllllllllllll1 - d.worldSpacePosition.y;
                                if (llllllllll9 < 0)
                                {
                                    llllllllll9 = 0;
                                }
                                if (llllllllll9 < llllllllllllllllllllll1)
                                {
                                    lllllllllllllllllllllllllllll5 = (llllllllllllllllllllll1 - llllllllll9) * lllllllllllllllllll9;
                                    llllllllllllllllllllllllllllll5 = (llllllllllllllllllllll1 - llllllllll9) * llllllllllllllllllll9;
                                }
                                else
                                {
                                    lllllllllllllllllllllllllllll5 = 0;
                                    llllllllllllllllllllllllllllll5 = 0;
                                }
                            }
                        }
                        if (lllllllllllllllllllllll1 == 0) 
                        {
                        }
                        else if (lllllllllllllllllllllll1 == 1) 
                        {
                            if (llllllllllll3)
                            {
                                lllllllllllllllllllllllllllll5 = max(llllllllllllllllllllllllll8, lllllllllllllllllllllllllllll5);
                                llllllllllllllllllllllllllllll5 = max(lllllllllllllllllllllllllll8, llllllllllllllllllllllllllllll5);
                            }
                            else
                            {
                                lllllllllllllllllllllllllllll5 = lllllllllllllllll9;
                                llllllllllllllllllllllllllllll5 = llllllllllllllllll9;
                            }
                        }
                        else if (lllllllllllllllllllllll1 == 2) 
                        {
                            if (llllllllllll3)
                            {
                                lllllllllllllllllllllllllllll5 = min(llllllllllllllllllllllllll8, lllllllllllllllllllllllllllll5);
                                lllllllllllllllllllllllllllll5 = max(lllllllllllllllllllllllll8, lllllllllllllllllllllllllllll5);
                                llllllllllllllllllllllllllllll5 = min(lllllllllllllllllllllllllll8, llllllllllllllllllllllllllllll5);
                            }
                        }
                    }
                    if (!llllllllll0 && !lllllllllll0)
                    {
                        if (llllllll1 == 1 && distance(_PlayersPosVectorArray[llllllll5].xyz, d.worldSpacePosition) > lllllll1)
                        {
                            lllllllllllllllllllllllllllll5 = 0;
                            llllllllllllllllllllllllllllll5 = 0;
                        }
                    }
                }
                llllll5 = llllll5 + _PlayersDataFloatArray[llllll5] * 4 + 1;
                if (llllllllllllllllllllllllllll1 && llllllllllll3 && llllllllllllllllllllllllllllll1)
                {
                    llllllllllllllllllllll5 = llllllllllllllllllllll5 * llllllllllllllllllllllllllll5;
                }
                if (llllllllll0 || lllllllllll0)
                {
                    lllllllllllllllllllllllllllll5 = llllllllllllllllllllll5 * lllllllllllllllllllllllllllll5;
                    llllllllllllllllllllllllllllll5 = llllllllllllllllllllll5 * llllllllllllllllllllllllllllll5;
                }
                else
                {
                    if (llllllllllllllllllllllllllll1)
                    {
                        if (llllllllllll3)
                        {
                            if (llllllllllllllllllllllllllllll1)
                            {
                                lllllllllllllllllllllllllllll5 = llllllllllllllllllllllllllll5 * lllllllllllllllllllllllllllll5;
                                llllllllllllllllllllllllllllll5 = llllllllllllllllllllllllllll5 * llllllllllllllllllllllllllllll5;
                            }
                        }
                        else
                        {
                            if (lllllllllllllllllllllllllllll1 == 1)
                            {
                                lllllllllllllllllllllllllllll5 = llllllllllllllllllllllllllll5 * lllllllllllllllllllllllllllll5;
                                llllllllllllllllllllllllllllll5 = llllllllllllllllllllllllllll5 * llllllllllllllllllllllllllllll5;
                            }
                        }
                    }
                }
                lllllllll3 = max(lllllllll3, lllllllllllllllllllllllllllll5);
                llllllllll3 = max(llllllllll3, llllllllllllllllllllllllllllll5);
            }
            else
            {
                llllll5 = llllll5 + _PlayersDataFloatArray[llllll5 + 2] * 4 + 3;
                llllll5 = llllll5 + _PlayersDataFloatArray[llllll5] * 4 + 1;
            }
        }
#else
        float llllllllllllllllllllll5 = 0;
        if (!lllllllllllllllll2)
        {
            llllllllllllllllllllll5 = 1;
            if (lllllll0 != 0 && llllllll0 != 0 && lllllllllllllll2 - llllllll0 < llllllllllllllllllllllllll1)
            {
                if (lllllll0 == 1)
                {
                    llllllllllllllllllllll5 = ((llllllllllllllllllllllllll1 - (lllllllllllllll2 - llllllll0)) / llllllllllllllllllllllllll1);
                }
                else
                {
                    llllllllllllllllllllll5 = ((lllllllllllllll2 - llllllll0) / llllllllllllllllllllllllll1);
                }
            }
            else if (lllllll0 == -1)
            {
                llllllllllllllllllllll5 = 1;
            }
            else if (lllllll0 == 1)
            {
                llllllllllllllllllllll5 = 0;
            }
            else
            {
                llllllllllllllllllllll5 = 1;
            }
            llllllllllllllllllllll5 = 1 - llllllllllllllllllllll5;
        }
        float lllllllllllllllllllllllllllll5 = 0;
        float llllllllllllllllllllllllllll5 = 0;
        bool l6 = distance(_WorldSpaceCameraPos, d.worldSpacePosition) > lllllll1;
        if ((llllllllllllllllllllll5 != 0) || ((!llllllllll0 && !lllllllllll0) && (llllllll1 == 0 || !l6) ))
        {
#if defined(_ZONING)
                        if(llllllllllllllllllllllllllll1) {
                            if(llllllllllll3) 
                            {
                                if(llllllllllllllllllllllllllllll1) {
                                    float lllllllllllll5 = llllllllllllllll3;
                                    float llllllllllllll5 = llllllllllllllllll3;
                                    llllllllllllllllllllllllllll5 = 1;
                                    float llllll6 = lllllllllllllllll3;
                                    if( llllllllllllll5!= 0 && lllllllllllll5 != 0 && lllllllllllllll2-lllllllllllll5 < llllll6) {
                                        if(llllllllllllll5 == 1) {
                                            llllllllllllllllllllllllllll5 = ((llllll6-(lllllllllllllll2-lllllllllllll5))/llllll6);
                                        } else {
                                            llllllllllllllllllllllllllll5 = ((lllllllllllllll2-lllllllllllll5)/llllll6);
                                        }
                                    } else if(llllllllllllll5 ==-1) {
                                        llllllllllllllllllllllllllll5 = 1;
                                    } else if(llllllllllllll5 == 1) {
                                        llllllllllllllllllllllllllll5 = 0;
                                    } else {
                                        llllllllllllllllllllllllllll5 = 1;
                                    }
                                    llllllllllllllllllllllllllll5 = 1 - llllllllllllllllllllllllllll5;
                                    if(lllllllllllllllllllllllllllll1 == 0 && llllllllllllllllllllllllllllll1) {
                                        float lllllll6 = 1 / lllllllllllllllllllllllllllllll1;
                                        if (llllllllllllll3 < lllllllllllllllllllllllllllllll1)  {
                                            float llllllll6 = ((lllllllllllllllllllllllllllllll1-llllllllllllll3) * lllllll6);
                                            llllllllllllllllllllllllllll5 =  max(llllllllllllllllllllllllllll5,llllllll6);
                                        }
                                    }
                                } else { 
                                }
                            } else {
                            }
                        }
#endif
            lllllllllllllllllllllllllllll5 = min(lllllllllllllllllllllllllllll5 + (1 * llllll1), 1);
            if (llllllllllll3)
            {
                if (lllllllllllllllllllllllllllll1 == 1)
                {
                    float lllllll6 = 1 / lllllllllllllllllllllllllllllll1;
                    if (llllllllllllll3 < lllllllllllllllllllllllllllllll1)
                    {
                        float ll10 = 1 - ((lllllllllllllllllllllllllllllll1 - llllllllllllll3) * lllllll6);
                        lllllllllllllllllllllllllllll5 = min(lllllllllllllllllllllllllllll5, ll10);
                    }
                }
                else if (lllllllllllllllllllllllllllll1 == 0 && !llllllllllllllllllllllllllllll1)
                {
                    float lllllll6 = lllllllllllllllllllllllllllll5 / lllllllllllllllllllllllllllllll1;
                    if (llllllllllllll3 < lllllllllllllllllllllllllllllll1)
                    {
                        float ll10 = ((lllllllllllllllllllllllllllllll1 - llllllllllllll3) * lllllll6);
                        lllllllllllllllllllllllllllll5 = max(0, ll10);
                    }
                    else
                    {
                        lllllllllllllllllllllllllllll5 = 0;
                    }
                }
            }
            if (llllllllllllllllllllllllllll1 && !llllllllllll3)
            {
                if (lllllllllllllllllllllllllllll1 == 1)
                {
                    lllllllllllllllllllllllllllll5 = 0;
                }
            }
            if (llllllllllll1 == 1 && lllllllllllll1 == 0)
            {
                float lllllllllll9 = 0;
                if (llllllllllllll1 == 0)
                {
                    lllllllllll9 = (lllllllllllllllllllllllllllll5) / lllllllllllllllll1;
                }
                else if (llllllllllllll1 == 1)
                {
                    float lllllllllllll9 = 1 - lllllllllllllllllllllllllllll5;
                    if (llllllllllllllllllllllllllll1 && llllllllllll3 && llllllllllllllllllllllllllllll1)
                    {
                        lllllllllllll9 = max(1 - lllllllllllllllllllllllllllll5, 1 - (lllllllllllllllllllllllllllll5 * llllllllllllllllllllllllllll5));
                    }
                    lllllllllll9 = lllllllllllll9 / lllllllllllllllll1;
                }
                if (d.worldSpacePosition.y > lllllllllllllll1)
                {
                    float llllllllll9 = d.worldSpacePosition.y - lllllllllllllll1;
                    if (llllllllll9 < 0)
                    {
                        llllllllll9 = 0;
                    }
                    if (llllllllllllll1 == 0)
                    {
                        if (llllllllll9 < lllllllllllllllll1)
                        {
                            lllllllllllllllllllllllllllll5 = ((lllllllllllllllll1 - llllllllll9) * lllllllllll9);
                        }
                        else
                        {
                            lllllllllllllllllllllllllllll5 = 0;
                        }
                    }
                    else
                    {
                        if (llllllllll9 < lllllllllllllllll1)
                        {
                            lllllllllllllllllllllllllllll5 = 1 - ((lllllllllllllllll1 - llllllllll9) * lllllllllll9);
                        }
                        else
                        {
                            lllllllllllllllllllllllllllll5 = 1;
                        }
                        llllllllllllllllllllllllllll5 = 1;
                    }
                }
            }
            if (llllllllllllllllll1 == 1 && lllllllllllllllllll1 == 0)
            {
                float lllllllllllllllllll9 = lllllllllllllllllllllllllllll5 / llllllllllllllllllllll1;
                if (d.worldSpacePosition.y < llllllllllllllllllll1)
                {
                    float llllllllll9 = llllllllllllllllllll1 - d.worldSpacePosition.y;
                    if (llllllllll9 < 0)
                    {
                        llllllllll9 = 0;
                    }
                    if (llllllllll9 < llllllllllllllllllllll1)
                    {
                        lllllllllllllllllllllllllllll5 = (llllllllllllllllllllll1 - llllllllll9) * lllllllllllllllllll9;
                    }
                    else
                    {
                        lllllllllllllllllllllllllllll5 = 0;
                    }
                }
            }
        }
        if (llllllllllllllllllllllllllll1 && llllllllllll3 && llllllllllllllllllllllllllllll1)
        {
            llllllllllllllllllllll5 = llllllllllllllllllllll5 * llllllllllllllllllllllllllll5;
        }
        if (llllllllll0 || lllllllllll0)
        {
            lllllllllllllllllllllllllllll5 = llllllllllllllllllllll5 * lllllllllllllllllllllllllllll5;
        }
        else
        {
            lllllllllllllllllllllllllllll5 = lllllllllllllllllllllllllllll5;
            if (llllllllllllllllllllllllllll1)
            {
                if (llllllllllll3)
                {
                    if (llllllllllllllllllllllllllllll1)
                    {
                        lllllllllllllllllllllllllllll5 = llllllllllllllllllllllllllll5 * lllllllllllllllllllllllllllll5;
                    }
                }
                else
                {
                    if (lllllllllllllllllllllllllllll1 == 1)
                    {
                        lllllllllllllllllllllllllllll5 = llllllllllllllllllllllllllll5 * lllllllllllllllllllllllllllll5;
                    }
                }
            }
        }
        lllllllll3 = max(lllllllll3, lllllllllllllllllllllllllllll5);
#endif
        float lllllllllllllllllllllllllllllll5 = lllllllll3;
        if (!lll2)
        {
            if (lllllllllllllllllllllllllllllll5 == 1)
            {
                lllllllllllllllllllllllllllllll5 = 10;
            }
            if (!llllllllllllllllllllll0) 
            {
#if defined(UNITY_PASS_SHADOWCASTER) 
#if defined(SHADOWS_DEPTH) 
                if (!any(unity_LightShadowBias))
                {
#if !defined(NO_STS_CLIPPING)
                        clip(lllllllllllllllllllllll2 - lllllllllllllllllllllllllllllll5);
#endif
                    lllllllllllll2 = lllllllllllllllllllllll2 - lllllllllllllllllllllllllllllll5;
                }
                else
                {
                    if(llllllllllllllllllllll0) 
                    {
#if !defined(NO_STS_CLIPPING)
                        clip(lllllllllllllllllllllll2 - lllllllllllllllllllllllllllllll5);
#endif
                        lllllllllllll2 = lllllllllllllllllllllll2 - lllllllllllllllllllllllllllllll5;
                    }
                }
#endif
#else
#if !defined(NO_STS_CLIPPING)
                clip(lllllllllllllllllllllll2 - lllllllllllllllllllllllllllllll5);
#endif
                lllllllllllll2 = lllllllllllllllllllllll2 - lllllllllllllllllllllllllllllll5;
#endif
            }
            else
            {
                if (lllllllllllllllllllllll0 == 6 && llllllllllllllllllllll0)
                {
#if defined(UNITY_PASS_SHADOWCASTER) 
#if defined(SHADOWS_DEPTH) 
                    if (!any(unity_LightShadowBias))
                    {
                    }
                    else
                    {
                        lllllllllllllllllllllllllllllll5 = llllllllll3;
                        if (lllllllllllllllllllllllllllllll5 == 1)
                        {
                            lllllllllllllllllllllllllllllll5 = 10;
                        }                    
                    }                
#endif
#endif
                }
#if !defined(NO_STS_CLIPPING)
                clip(lllllllllllllllllllllll2 - lllllllllllllllllllllllllllllll5);
#endif
                lllllllllllll2 = lllllllllllllllllllllll2 - lllllllllllllllllllllllllllllll5;
            }
            if (lllllllllllllllllllllll2 - lllllllllllllllllllllllllllllll5 < 0)
            {
                lllllllllllll2 = 0;
            }
            else
            {
                lllllllllllll2 = 1;
            }
        }
        if (lll2)
        {
            llllllllllllllllll2 = 1;
            if ((lllllllllllllllllllllll2 - lllllllllllllllllllllllllllllll5) < 0)
            {
                lllllllllllllllllll2 = half4(1, 1, 1, 1);
                o.Emission = 1;
            }
            else
            {
                lllllllllllllllllll2 = half4(0, 0, 0, 1);
            }
            if (lllll5)
            {
                if ((lllllllllllllllllllllll2 - lllllllllllllllllllllllllllllll5) < 0)
                {
                    lllllllllllllllllll2 = half4(0.5, 1, 0.5, 1);
                    o.Emission = 0;
                }
                else
                {
                    lllllllllllllllllll2 = half4(0, 0.1, 0, 1);
                }
            }
            if (llllllllllll3 && llllllllllllllllll1 == 1 && l2)
            {
                float lllllllllll10 = 0;
                if (lllllllllllllllllll1 == 1)
                {
                    llll5 = llll5 + ll2;
                    lllllllllll10 = llll5;
                }
                else
                {
                    lllllllllll10 = llllllllllllllllllll1 + ll2;
                }
                if (d.worldSpacePosition.y > (lllllllllll10 - llll2) && d.worldSpacePosition.y < (lllllllllll10 + llll2))
                {
                    lllllllllllllllllll2 = half4(1, 0, 0, 1);
                }
            }
        }
        else
        {
            half3 llllllllllll10 = lerp(1, lllllllllllllll0, llllllllllllllll0).rgb;
            if (llllllllllllllllllll0)
            {
                lllllllllllllllllllll0 = 0.2 + (lllllllllllllllllllll0 * (0.8 - 0.2));
                o.Emission = o.Emission + min(clamp(llllllllllll10 * clamp(((lllllllllllllllllllllllllllllll5 / lllllllllllllllllllll0) - lllllllllllllllllllllll2), 0, 1), 0, 1) * sqrt(llllllllllllllllll0 * lllllllllllllllllll0), clamp(llllllllllll10 * lllllllllllllllllllllllllllllll5, 0, 1) * sqrt(llllllllllllllllll0 * lllllllllllllllllll0));
            }
            else
            {
                o.Emission = o.Emission + clamp(llllllllllll10 * lllllllllllllllllllllllllllllll5, 0, 1) * sqrt(llllllllllllllllll0 * lllllllllllllllllll0);
            }
        }
    }
    if (llllllllllllllllll2)
    {
        o.Albedo = lllllllllllllllllll2.rgb;
    }
    lllllllllll2 = o.Albedo;
    llllllllllll2 = o.Emission;
    #ifdef _HDRP  
        float lllllllllllll10 = 0;
        float llllllllllllll10 = 0;
    #if SHADEROPTIONS_PRE_EXPOSITION
            llllllllllllll10 =  LOAD_TEXTURE2D(_ExposureTexture, int2(0, 0)).x * _ProbeExposureScale;
    #else
            llllllllllllll10 = _ProbeExposureScale;
    #endif
            float lllllllllllllll10 = 0;
            float llllllllllllllll10 = llllllllllllll10;
            lllllllllllllll10 = rcp(llllllllllllllll10 + (llllllllllllllll10 == 0.0));
            float3 lllllllllllllllll10 = o.Emission * lllllllllllllll10;
            o.Emission = lerp(lllllllllllllllll10, o.Emission, lllllllllllll10);
        llllllllllll2 = o.Emission;
    #endif
}
void DoCrossSection(
                    half llllllllllllllllll10,
                    half4 lllllllllllllllllll10,
                    half llllllllllllllllllll10,
                    sampler2D lllllllllllllllllllll10,
                    float llllllllllllllllllllll10,
                    half lllllllllllllllllllllll10,
                    bool llllllllllllllllllllllll10,
                    float4 lllll0,
                    inout half4 llllllllllllllllllllllllll10
                    )
{
    if (llllllllllllllllll10 == 1)
    {
        if (llllllllllllllllllllllll10 == false)
        {
            if (llllllllllllllllllll10 == 1)
            {
                float2 llllllllll8 = lllll0.xy / lllll0.w;
                if (lllllllllllllllllllllll10 == 1)
                {
                    float4 llllllllllllllllllllllllllll10 = mul(UNITY_MATRIX_M, float4(0, 0, 0, 1));
                    llllllllll8.xy *= distance(_WorldSpaceCameraPos, llllllllllllllllllllllllllll10);
                }
                float llllllllllll6 = _ScreenParams.x / _ScreenParams.y;
                llllllllll8.x *= llllllllllll6;
                half3 llllllllllllllllllllllllllllll10 = tex2D(lllllllllllllllllllll10, llllllllll8 * llllllllllllllllllllll10).rgb;
                llllllllllllllllllllllllll10 = half4(llllllllllllllllllllllllllllll10, 1) * lllllllllllllllllll10;
            }
            else
            {
                llllllllllllllllllllllllll10 = lllllllllllllllllll10;
            }
        }
    }
}


#endif
