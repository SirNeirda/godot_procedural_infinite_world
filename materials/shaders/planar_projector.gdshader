// Partly based on https://godotshaders.com/shader/distortion-bubble/

shader_type spatial;

render_mode unshaded, cull_disabled;

uniform sampler2D depth_texture : source_color, hint_depth_texture;
uniform sampler2D screen_texture : hint_screen_texture, filter_linear;

uniform sampler2D distortion_texture_x : source_color;
uniform sampler2D distortion_texture_y : source_color;

uniform vec4 color1 : source_color = vec4(1,1,1,.1);
uniform vec4 color2 : source_color = vec4(1,1,1,1);

uniform float threshold = 0.2;
uniform float fresnel_sharpness = 1.0;

uniform float distortion_strength = 0.02;
uniform float distortion_speed = 0.5;
uniform float distortion_scale = 1.0;


// HELPERS

vec2 get_distorted_screen_uv(vec2 uv, float fresnel) {
	vec2 noise_uv = uv * distortion_scale + TIME * distortion_speed;

	float noise_x = texture(distortion_texture_x, noise_uv).r * 2.0 - 1.0;
	float noise_y = texture(distortion_texture_y, noise_uv).r * 2.0 - 1.0;

	return uv + vec2(noise_x, noise_y) * distortion_strength * fresnel;
}

vec3 get_distorted_screen_color(vec2 uv, float fresnel) {
	return texture(screen_texture, get_distorted_screen_uv(uv, fresnel)).rgb;
}


// FRAGMENT

void fragment() {
	// Scene depth 
	float depth = texture(depth_texture, SCREEN_UV).x;
	vec3 ndc = vec3(SCREEN_UV, depth) * 2.0 - 1.0;
	vec4 view = INV_PROJECTION_MATRIX * vec4(ndc, 1.0);
	view.xyz /= view.w;
	float linear_depth = -view.z;

	//  Object depth 
	float object_depth = FRAGCOORD.z;
	vec3 object_ndc = vec3(SCREEN_UV, object_depth) * 2.0 - 1.0;
	vec4 object_view = INV_PROJECTION_MATRIX * vec4(object_ndc, 1.0);
	object_view.xyz /= object_view.w;
	float linear_object_depth = -object_view.z;

	//  Normal + Fresnel 
	vec3 normal = FRONT_FACING ? NORMAL : -NORMAL;
	float fresnel = pow(1.0 - abs(dot(VIEW, normal)), fresnel_sharpness);
	
	fresnel = max(fresnel, 0.2);

	//  Distorted background 
	vec3 refracted_color = get_distorted_screen_color(SCREEN_UV, fresnel);

	// Final blending 
	if (abs(linear_depth - linear_object_depth) > threshold) {
		ALBEDO = mix(refracted_color, color1.rgb, color1.a);
		ALPHA  = clamp(color1.a + fresnel, 0.0, 1.0);
	} else {
		ALBEDO = mix(refracted_color, color2.rgb, color2.a);
		ALPHA  = clamp(color2.a + fresnel, 0.0, 1.0);
	}
}
