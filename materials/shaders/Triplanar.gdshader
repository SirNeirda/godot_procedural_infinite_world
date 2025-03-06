shader_type spatial;

// Varyings
varying float flatness;
varying vec3 power_normal;
varying vec3 object_normal;
varying vec3 world_normal;
varying vec3 triplanar_pos;

uniform float TopTexScale;
uniform sampler2D TopTex;
uniform vec4 TopColor : source_color;

vec2 hash( vec2 p )
{
	return fract( sin( p * mat2( vec2( 127.1, 311.7 ), vec2( 269.5, 183.3 ) ) ) * 43758.5453 );
}

void vertex(){
	object_normal = NORMAL;
	world_normal = vec3(0.0,1.0,0.0);
	flatness = dot(object_normal,world_normal);
	power_normal = normalize(pow(abs(object_normal),vec3(10.0,10.0,10.0)));
	triplanar_pos = VERTEX*vec3(1.0,-1.0,1.0);
}


vec4 triplanar_texture(sampler2D p_sampler, vec3 p_weights, vec3 p_triplanar_pos, float uv_scale) {
	vec4 samp = vec4(0.0);
	samp += texture(p_sampler, p_triplanar_pos.xy*vec2(uv_scale,uv_scale)) * p_weights.z;
	samp += texture(p_sampler, p_triplanar_pos.xz*vec2(uv_scale,uv_scale)) * p_weights.y;
	samp += texture(p_sampler, p_triplanar_pos.zy * vec2(-1.0,1.0)*vec2(uv_scale,uv_scale)) * p_weights.x;
	return samp;
}


void fragment() {

	vec4 top_tex = triplanar_texture(TopTex, power_normal, triplanar_pos,TopTexScale);
	ALBEDO = top_tex.rgb * TopColor.rgb;

}