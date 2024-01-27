Shader "Unlit/libretro_crt_geom"
{
    Properties
    {
        [MainTexture] _MainTex ("MainTex", 2D) = "black" {}

        CRTgamma ("Target Gamma", Float) = 2.4
        monitorgamma ("Monitor Gamma", Float) = 2.2
        d ("Distance", Float) = 1.5
        [Toggle(ENABLE_CURVATURE)] _Curvature("Curvature", Float) = 1.0
        R ("Curvature Radius", Float) = 2.0
        cornersize ("Corner Size", Float) = 0.03
        cornersmooth ("Corner Smoothness", Float) = 1000.0
        x_tilt ("Horizontal Tilt", Float) = 0.0
        y_tilt ("Vertical Tilt", Float) = 0.0
        overscan_x ("Horiz. Overscan %", Float) = 100.0
        overscan_y ("Vert. Overscan %", Float) = 100.0
        DOTMASK ("Dot Mask Toggle", Float) = 0.3
        SHARPER ("Sharpness", Float) = 1.0
        scanline_weight ("Scanline Weight", Float) = 0.3
        lum ("Luminance Boost", Float) = 0.0
        [Toggle(ENABLE_INTERLACING)] _Interlacing ("Interlacing", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile __ ENABLE_CURVATURE
            #pragma multi_compile __ ENABLE_INTERLACING

            #include "UnityCG.cginc"

            // Comment the next line to disable interpolation in linear gamma (and gain speed).
            #define LINEAR_PROCESSING

            // Enable 3x oversampling of the beam profile; improves moire effect caused by scanlines+curvature
            #define OVERSAMPLE

            // Use the older, purely gaussian beam profile; uncomment for speed
            //#define USEGAUSSIAN

            #define PI 3.141592653589
            #define FIX(c) max(abs(c), 1e-5);
            #define mod(x,y) (x - y * trunc(x/y))

            #ifdef LINEAR_PROCESSING
            #   define TEX2D(c) pow(tex2D(_MainTex, (c)), float4(CRTgamma,CRTgamma,CRTgamma,CRTgamma))
            #else
            #   define TEX2D(c) tex2D(_MainTex, (c))
            #endif

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 sin_cos_angle : TEXCOORD1;
                float3 stretch : TEXCOORD2;
                float2 TextureSize : TEXCOORD3;
                float2 ilfac : TEXCOORD4;
                float2 one : TEXCOORD5;
                float2 mod_factor : TEXCOORD6;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 sin_cos_angle : TEXCOORD1;
                float3 stretch : TEXCOORD2;
                float2 TextureSize : TEXCOOR3;
                float2 ilfac : TEXCOORD4;
                float2 one : TEXCOORD5;
                float2 mod_factor : TEXCOORD6;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _MainTex_TexelSize;
            float CRTgamma;
            float monitorgamma;
            float d;
            float _Curvature;
            float R;
            float cornersize;
            float cornersmooth;
            float x_tilt;
            float y_tilt;
            float overscan_x;
            float overscan_y;
            float DOTMASK;
            float SHARPER;
            float scanline_weight;
            float lum;
            float _Interlacing;

            static float2 aspect = float2(1.0, 0.75);

            float intersect(float2 xy, float4 sin_cos_angle)
            {
                    float A = dot(xy,xy)+d*d;
                    float B = 2.0*(R*(dot(xy,sin_cos_angle.xy)-d*sin_cos_angle.zw.x*sin_cos_angle.zw.y)-d*d);
                    float C = d*d + 2.0*R*d*sin_cos_angle.zw.x*sin_cos_angle.zw.y;
                    return (-B-sqrt(B*B-4.0*A*C))/(2.0*A);
            }

            float2 bkwtrans(float2 xy, float4 sin_cos_angle)
            {
                    float c = intersect(xy, sin_cos_angle);
                    float2 point_ = float2(c,c)*xy;
                    point_ -= float2(-R,-R)*sin_cos_angle.xy;
                    point_ /= float2(R,R);
                    float2 tang = sin_cos_angle.xy/sin_cos_angle.zw;
                    float2 poc = point_/sin_cos_angle.zw;
                    float A = dot(tang,tang)+1.0;
                    float B = -2.0*dot(poc,tang);
                    float C = dot(poc,poc)-1.0;
                    float a = (-B+sqrt(B*B-4.0*A*C))/(2.0*A);
                    float2 uv = (point_-a*sin_cos_angle.xy)/sin_cos_angle.zw;
                    float r = FIX(R*acos(a));
                    return uv*r/sin(r/R);
            }

            float2 fwtrans(float2 uv, float4 sin_cos_angle)
            {
                    float r = FIX(sqrt(dot(uv,uv)));
                    uv *= sin(r/R)/r;
                    float x = 1.0-cos(r/R);
                    float D = d/R + x*sin_cos_angle.z*sin_cos_angle.w+dot(uv,sin_cos_angle.xy);
                    return d*(uv*sin_cos_angle.zw-x*sin_cos_angle.xy)/D;
            }

            float3 maxscale(float4 sin_cos_angle)
            {
                    float2 c = bkwtrans(-R * sin_cos_angle.xy / (1.0 + R/d*sin_cos_angle.z*sin_cos_angle.w), sin_cos_angle);
                    float2 a = float2(0.5,0.5)*aspect;
                    float2 lo = float2(fwtrans(float2(-a.x,c.y), sin_cos_angle).x,
                                 fwtrans(float2(c.x,-a.y), sin_cos_angle).y)/aspect;
                    float2 hi = float2(fwtrans(float2(+a.x,c.y), sin_cos_angle).x,
                                 fwtrans(float2(c.x,+a.y), sin_cos_angle).y)/aspect;
                    return float3((hi+lo)*aspect*0.5,max(hi.x-lo.x,hi.y-lo.y));
            }

            float4 scanlineWeights(float distance, float4 color)
            {
                    // "wid" controls the width of the scanline beam, for each RGB
                    // channel The "weights" lines basically specify the formula
                    // that gives you the profile of the beam, i.e. the intensity as
                    // a function of distance from the vertical center of the
                    // scanline. In this case, it is gaussian if width=2, and
                    // becomes nongaussian for larger widths. Ideally this should
                    // be normalized so that the integral across the beam is
                    // independent of its width. That is, for a narrower beam
                    // "weights" should have a higher peak at the center of the
                    // scanline than for a wider beam.
            #ifdef USEGAUSSIAN
                    float4 wid = 0.3 + 0.1 * pow(color, float4(3.0, 3.0, 3.0, 3.0));
                    float v = distance / (wid * scanline_weight/0.3);
                    float4 weights = float4(v,v,v,v);
                    return (lum + 0.4) * exp(-weights * weights) / wid;
            #else
                    float4 wid = 2.0 + 2.0 * pow(color, float4(4.0, 4.0, 4.0, 4.0));
                    float v = distance / scanline_weight;
                    float4 weights = float4(v,v,v,v);
                    return (lum + 1.4) * exp(-pow(weights * rsqrt(0.5 * wid), wid)) / (0.6 + 0.2 * wid);
            #endif
            }

            float4 crt_geom(float2 texture_size,
                            float2 video_size,
                            float2 output_size,
                            float frame_count,
                            float4 sin_cos_angle,
                            float3 stretch,
                            float2 ilfac,
                            float2 one,
                            float mod_factor,
                            float2 TextureSize,
                            float2 texCoord,
                            float4 tex)
            {
                // Here's a helpful diagram to keep in mind while trying to
                // understand the code:
                //
                //  |      |      |      |      |
                // -------------------------------
                //  |      |      |      |      |
                //  |  01  |  11  |  21  |  31  | <-- current scanline
                //  |      | @    |      |      |
                // -------------------------------
                //  |      |      |      |      |
                //  |  02  |  12  |  22  |  32  | <-- next scanline
                //  |      |      |      |      |
                // -------------------------------
                //  |      |      |      |      |
                //
                // Each character-cell represents a pixel on the output
                // surface, "@" represents the current pixel (always somewhere
                // in the bottom half of the current scan-line, or the top-half
                // of the next scanline). The grid of lines represents the
                // edges of the texels of the underlying texture.

                // Texture coordinates of the texel containing the active pixel.
                float2 xy;
#ifdef ENABLE_CURVATURE
                float2 cd = texCoord;
                cd *= texture_size / video_size;
                cd = (cd-float2(0.5, 0.5))*aspect*stretch.z+stretch.xy;
                xy =  (bkwtrans(cd, sin_cos_angle)/float2(overscan_x / 100.0, overscan_y / 100.0)/aspect+float2(0.5, 0.5)) * video_size / texture_size;
#else
                xy = texCoord;
#endif
                float2 cd2 = xy;
                cd2 *= texture_size / video_size;
                cd2 = (cd2 - float2(0.5, 0.5)) * float2(overscan_x / 100.0, overscan_y / 100.0) + float2(0.5, 0.5);
                cd2 = min(cd2, float2(1.0, 1.0)-cd2) * aspect;
                float2 cdist = float2(cornersize, cornersize);
                cd2 = (cdist - min(cd2,cdist));
                float dist = sqrt(dot(cd2,cd2));
                float cval = clamp((cdist.x-dist)*cornersmooth,0.0, 1.0);

                float2 xy2 = ((xy * TextureSize/video_size-float2(0.5, 0.5))*float2(1.0,1.0)+float2(0.5, 0.5))*video_size/TextureSize;
                // Of all the pixels that are mapped onto the texel we are
                // currently rendering, which pixel are we currently rendering?
                float2 ilfloat = float2(0.0, ilfac.y > 1.5 ? mod(float(frame_count), 2.0) : 0.0);

                float2 ratio_scale = (xy * TextureSize - float2(0.5, 0.5) + ilfloat) / ilfac;

                float2 uv_ratio = frac(ratio_scale);

                // Snap to the center of the underlying texel.

                xy = (floor(ratio_scale) * ilfac + float2(0.5, 0.5) - ilfloat) / TextureSize;

                // Calculate Lanczos scaling coefficients describing the effect
                // of various neighbour texels in a scanline on the current
                // pixel.
                float4 coeffs = PI * float4(1.0 + uv_ratio.x, uv_ratio.x, 1.0 - uv_ratio.x, 2.0 - uv_ratio.x);

                // Prevent division by zero.
                coeffs = FIX(coeffs);

                // Lanczos2 kernel.
                coeffs = 2.0 * sin(coeffs) * sin(coeffs / 2.0) / (coeffs * coeffs);

                // Normalize.
                coeffs /= dot(coeffs, float4(1.0, 1.0, 1.0, 1.0));

                // Calculate the effective colour of the current and next
                // scanlines at the horizontal location of the current pixel,
                // using the Lanczos coefficients above.
                float4 col  = clamp(mul(coeffs, float4x4(TEX2D(xy + float2(-one.x, 0.0)), TEX2D(xy), TEX2D(xy + float2(one.x, 0.0)), TEX2D(xy + float2(2.0 * one.x, 0.0)))), 0.0, 1.0);
                float4 col2 = clamp(mul(coeffs, float4x4(TEX2D(xy + float2(-one.x, one.y)), TEX2D(xy + float2(0.0, one.y)), TEX2D(xy + one), TEX2D(xy + float2(2.0 * one.x, one.y)))), 0.0, 1.0);

#ifndef LINEAR_PROCESSING
                col  = pow(col , CRTgamma);
                col2 = pow(col2, CRTgamma);
#endif
                // Calculate the influence of the current and next scanlines on
                // the current pixel.
                float4 weights  = scanlineWeights(uv_ratio.y, col);
                float4 weights2 = scanlineWeights(1.0 - uv_ratio.y, col2);
#ifdef OVERSAMPLE
                float filter = video_size.y / output_size.y;
                uv_ratio.y = uv_ratio.y+1.0/3.0*filter;
                weights    = (weights+scanlineWeights(uv_ratio.y, col))/3.0;
                weights2   = (weights2+scanlineWeights(abs(1.0-uv_ratio.y), col2))/3.0;
                uv_ratio.y = uv_ratio.y-2.0/3.0*filter;
                weights    = weights + scanlineWeights(abs(uv_ratio.y), col) / 3.0;
                weights2   = weights2 + scanlineWeights(abs(1.0 - uv_ratio.y), col2) / 3.0;
#endif
                float3 mul_res = (col * weights + col2 * weights2).rgb;
                mul_res *= float3(cval, cval, cval);

                // dot-mask emulation:
                // Output pixels are alternately tinted green and magenta.
                float3 dotMaskWeights = lerp(float3(1.0, 1.0 - DOTMASK, 1.0), float3(1.0 - DOTMASK, 1.0, 1.0 - DOTMASK), floor(mod(mod_factor, 2.0)));
                mul_res *= dotMaskWeights;

                // Convert the image gamma for display on our output device.
                mul_res = pow(mul_res, float3(1.0 / monitorgamma, 1.0 / monitorgamma, 1.0 / monitorgamma));

                // Color the texel.
                return float4(mul_res, 1.0);
            }

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv     = TRANSFORM_TEX(v.uv, _MainTex);

                // Precalculate a bunch of useful values we'll need in the fragment
                // shader.
                float2 sinangle = sin(float2(x_tilt, y_tilt));
                float2 cosangle = cos(float2(x_tilt, y_tilt));
                o.sin_cos_angle = float4(sinangle.x, sinangle.y, cosangle.x, cosangle.y);
                o.stretch = maxscale(o.sin_cos_angle);
                o.TextureSize = float2(SHARPER * _MainTex_TexelSize.z, _MainTex_TexelSize.w);
#ifdef ENABLE_INTERLACING
                o.ilfac = float2(1.0, clamp(floor(_MainTex_TexelSize.w / 200.0), 1.0, 2.0));
#else
                o.ilfac = float2(1.0, clamp(floor(_MainTex_TexelSize.w), 1.0, 2.0));
#endif
                // The size of one texel, in texture-coordinates.
                o.one = o.ilfac / o.TextureSize;

                // Resulting X pixel-coordinate of the pixel we're drawing.
                o.mod_factor = o.uv.x * _MainTex_TexelSize.z;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 size = float2(_MainTex_TexelSize.z, _MainTex_TexelSize.w);
                float2 sizeOut = float2(_MainTex_TexelSize.z * 100, _MainTex_TexelSize.w * 100);
                return crt_geom(size,
                                size,
                                sizeOut,
                                0,
                                i.sin_cos_angle,
                                i.stretch,
                                i.ilfac,
                                i.one,
                                i.mod_factor,
                                size,
                                i.uv,
                                tex2D(_MainTex, i.uv));
            }
            ENDHLSL
        }
    }
}
