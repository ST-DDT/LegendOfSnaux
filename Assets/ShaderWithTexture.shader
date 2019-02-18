// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Custom/ShaderWithTexture"
{
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Texture", 2D) = "white" {}
		_NormalMap("Normal Map", 2D) = "bump" {}
		_SpecColor("Specular Color", Color) = (1,1,1,1)
		_Shininess ("Shininess", Float)=10
		_RimColor ("Rim Color", Color) = (1,1,1,1)
		_RimPower ("Rim Power", Range(0.1, 10))=3
	}

		SubShader{
			Pass {
				Tags {"LightMode" = "ForwardBase"}

				CGPROGRAM
				#pragma vertex vert	
				#pragma fragment frag
				#pragma exclude_renderers flash

				//user defined variables
				uniform float4 _Color;
				uniform sampler2D _MainTex;
				uniform float4 _MainTex_ST;
				uniform sampler2D _NormalMap;
				uniform float4 _NormalMap_ST;
				uniform float4 _SpecColor;
				uniform float4 _RimColor;
				uniform float _Shininess;
				uniform float _RimPower;

				//units defined variables
				uniform float4 _LightColor0;
				uniform float4 _WorldSpacesCameraPos;

				//base input struct
				struct vertexInput {
					float4 vertex : POSITION;
					float3 normal : NORMAL;
					float4 texcoord : TEXCOORD0;
					float4 tangent : TANGENT;
				};
				struct vertexOutput {
					float4 pos : SV_POSITION;
					float4 tex : TEXCOORD0;
					float4 posWorld : TEXCOORD1;
					float3 normalWorld : TEXCOORD2;
					float3 tangentWorld : TEXCOORD3;
					float3 binormalWorld : TEXCOORD4;
				};

				//vertex function
				vertexOutput vert(vertexInput v) {
					vertexOutput o;

					o.normalWorld = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject).xyz);
					o.tangentWorld = normalize(mul(unity_ObjectToWorld, v.tangent).xyz);
					o.binormalWorld = normalize(cross(o.normalWorld, o.tangentWorld)* v.tangent.w);

					o.posWorld = mul(unity_ObjectToWorld, v.vertex);
					o.pos = UnityObjectToClipPos(v.vertex);
					o.tex = v.texcoord;

					return o;
				}

				//fregmant function
				float4 frag(vertexOutput i) :COLOR
				{
					float3 viewDirection = normalize(_WorldSpacesCameraPos.xyz - i.posWorld.xyz);
					float3 lightDirection;
					float atten;

					if (_WorldSpaceLightPos0.w == 0.0) { //directional light
						atten = 1;
						lightDirection = normalize(_WorldSpaceLightPos0.xyz);
					}
					else {
						float3 fragmentToLightSource = _WorldSpaceLightPos0.xyz - i.posWorld.xyz;
						float distanze = length(fragmentToLightSource);
						atten = 1 / distanze;
						lightDirection = normalize(fragmentToLightSource);
					}

					//Texture Maps
					float4 tex = tex2D(_MainTex, i.tex.xy * _MainTex_ST.xy * _MainTex_ST.zw);
					float4 texN = tex2D(_NormalMap, i.tex.xy * _NormalMap_ST.xy * _NormalMap_ST.zw);

					//unpackNormal function
					float3 localCoords = float3 (2 * texN.ag - float2(1, 2), 0);
					localCoords.z = 1 - 0.5* dot(localCoords, localCoords);

					//normal transponse matrix
					float3x3 local2WorldTransponse =float3x3 (
						i.tangentWorld,
						i.binormalWorld,
						i.normalWorld
						);

					//calculate normal direction
					float3 normalDirection = normalize(mul(localCoords, local2WorldTransponse));

					//lighting
					float3 diffuseReflection = atten * _LightColor0.xyz*saturate(dot(normalDirection, lightDirection));
					float3 specularReflection = diffuseReflection * _SpecColor.xyz* pow(saturate(dot(reflect(-lightDirection, normalDirection),viewDirection)), _Shininess);

					//Rim Lighting
					float rim = 1 - saturate(dot(viewDirection, normalDirection));
					float3 rimLighting = saturate(dot(normalDirection, lightDirection)*_RimColor.xyz*_LightColor0.xyz*pow(rim, _RimPower));

					float3 lightFinal = UNITY_LIGHTMODEL_AMBIENT.xyz+diffuseReflection+specularReflection+ rimLighting;

				return float4(tex.xyz*lightFinal*_Color.xyz, 1);
				}

				ENDCG
			}
	}

//		Fallback "Diffuse"
}
