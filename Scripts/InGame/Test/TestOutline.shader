Shader "Custom/TestOutline"
{
	Properties
	{
		[PerRendererData]
	    _MainTex("Albedo (RGB)", 2D) = "white" {}

		[Header(Life)]
		_Color("_Main Color", Color) = (0.2,1,0.2,1)
		_Steps("_Steps", Float) = 1
		_Percent("_Percent", Float) = 1

		[Header(Damages)]
		_DamagesColor("Damages color", Color) = (1,1,0,1)
		_DamagesPercent("Damages Percent", Float) = 0


		[Header(Border)]
		_BorderColor("Border color", Color) = (0.1,0.1,0.1,1)
		_BorderWidth("Border width", Float) = 1
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0


		_ImageSize("Image Size", Vector) = (100, 100, 0, 0)

		//_MainTex2("Albedo (RGB)", 2D) = "white" {}
		_OutLineWidth("width", float) = 1.2//      
	}

		SubShader
		{
			Tags
			{
				"Queue" = "Transparent"
				"IgnoreProjector" = "True"
				"RenderType" = "Transparent"
				"PreviewType" = "Plane"
				"CanUseSpriteAtlas" = "True"
			}

			Cull Off
			Lighting Off
			ZWrite Off
			Blend One OneMinusSrcAlpha

			

			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				struct appdata
				{
					float4 vertex:POSITION;
					float2 uv:TEXCOORD0;
				};

				struct v2f
				{
					float2 uv :TEXCOORD0;
					float4 vertex:SV_POSITION;
				};

				float _OutLineWidth;//    

				v2f vert(appdata v)
				{
					v2f o;

					//    xy
					//v.vertex.xy *= 1.1;
					v.vertex.xy *= _OutLineWidth;//    
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = v.uv;
					return o;
				}

				sampler2D _MainTex;
				fixed4 _BorderColor;

				fixed4 frag(v2f i) :SV_Target
				{
					//fixed4 col = tex2D(_MainTex, i.uv);
					return _BorderColor;
				}
				ENDCG
			}

			Pass
			{
				ZTest Always
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				struct appdata
				{
					float4 vertex:POSITION;
					float2 uv:TEXCOORD0;
				};

				struct v2f
				{
					float2 uv :TEXCOORD0;
					float4 vertex:SV_POSITION;
				};


				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = v.uv;
					return o;
				}

				sampler2D _MainTex;

				fixed4 frag(v2f i) :SV_Target
				{
					fixed4 col = tex2D(_MainTex, i.uv);
				//return fixed4(0, 0, 1, 1);//    ，                
				return col;
			}
		ENDCG
		}

			Pass
			{
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile _ PIXELSNAP_ON
				#include "UnityCG.cginc"

				struct appdata_t
				{
					float4 vertex   : POSITION;
					float4 color    : COLOR;
					float2 texcoord : TEXCOORD0;
				};

				struct v2f
				{
					float4 vertex   : SV_POSITION;
					fixed4 color : COLOR;
					half2 texcoord  : TEXCOORD0;
				};

				fixed4 _Color;
				half _Steps;
				half _Percent;

				fixed4 _DamagesColor;
				half _DamagesPercent;

				fixed4 _BorderColor;
				half _BorderWidth;

				//v2f를 반환하는 함수 vert
				v2f vert(appdata_t IN)
				{
					//들어온 IN을 가지고 
					v2f OUT; // v2f 구조체 OUT을 생성
					OUT.vertex = UnityObjectToClipPos(IN.vertex); //
					OUT.texcoord = IN.texcoord;
					#ifdef PIXELSNAP_ON
					OUT.vertex = UnityPixelSnap(OUT.vertex);
					#endif

					return OUT;
				}

				sampler2D _MainTex;
				float4 _ImageSize;

				fixed4 frag(v2f IN) : SV_Target
				{
					fixed4 c = tex2D(_MainTex, IN.texcoord);

					if (IN.texcoord.x > _Percent + _DamagesPercent)
					{
					   c.a = 0;
					}
					else
					{
						if (IN.texcoord.x > _Percent)
						   c *= _DamagesColor;
						else
						{
						   if ((IN.texcoord.x * _ImageSize.x) % (_ImageSize.x / _Steps) < _BorderWidth)
							   c *= _BorderColor;
						   else if (IN.texcoord.y * _ImageSize.y < _BorderWidth)
							   c *= _BorderColor;
						   else
							   c *= _Color;
						}
					}

					c.rgb *= c.a;
					return c;
				}
			ENDCG
			}

		}

		SubShader{

			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				struct appdata
				{
					float4 vertex:POSITION;
					float2 uv:TEXCOORD0;
				};

				struct v2f
				{
					float2 uv :TEXCOORD0;
					float4 vertex:SV_POSITION;
				};

				float _OutLineWidth;//    

				v2f vert(appdata v)
				{
					v2f o;

					//    xy
					//v.vertex.xy *= 1.1;
					v.vertex.xy *= _OutLineWidth;//    
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = v.uv;
					return o;
				}

				sampler2D _MainTex;
				fixed4 _BorderColor;

				fixed4 frag(v2f i) :SV_Target
				{
					//fixed4 col = tex2D(_MainTex, i.uv);
					return _BorderColor;
				}
				ENDCG
			}


			Pass
			{
				ZTest Always
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				struct appdata
				{
					float4 vertex:POSITION;
					float2 uv:TEXCOORD0;
				};

				struct v2f
				{
					float2 uv :TEXCOORD0;
					float4 vertex:SV_POSITION;
				};


				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = v.uv;
					return o;
				}

				sampler2D _MainTex;

				fixed4 frag(v2f i) :SV_Target
				{
					fixed4 col = tex2D(_MainTex, i.uv);
				//return fixed4(0, 0, 1, 1);//    ，                
				return col;
			}
		ENDCG
		}


		}
	FallBack "Diffuse"
}
