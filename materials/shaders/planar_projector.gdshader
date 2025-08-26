shader_type spatial;


uniform sampler2D depth_texture : source_color, hint_depth_texture;
uniform vec4 color1: source_color = vec4(1,1,1,.1);
uniform vec4 color2: source_color = vec4(1,1,1,1);
uniform float threshold = .2;
uniform float fresnel_sharpness = 1.0;

void fragment() {
	float depth = texture(depth_texture, SCREEN_UV).x;
	vec3 ndc = vec3(SCREEN_UV, depth) * 2.0 - 1.0;
	vec4 view = INV_PROJECTION_MATRIX * vec4(ndc, 1.0);
	view.xyz /= view.w;
	float linear_depth = -view.z;

	float object_depth = FRAGCOORD.z;
	vec3 object_ndc = vec3(SCREEN_UV, object_depth) * 2.0 - 1.0;
	vec4 object_view = INV_PROJECTION_MATRIX * vec4(object_ndc, 1.0);
	object_view.xyz /= object_view.w;
	float linear_object_depth = -object_view.z;

	vec3 normal = NORMAL;
	if (!FRONT_FACING) {
		normal = -normal;
	}

	float fresnel = pow(1.0 - abs(dot(VIEW, normal)), fresnel_sharpness);

	if (linear_depth - linear_object_depth > threshold) {
		ALBEDO = color1.rgb;
		ALPHA = clamp(color1.a + fresnel, 0.0, 1.0);
	} else {
		ALBEDO = color2.rgb;
		ALPHA = clamp(color2.a + fresnel, 0.0, 1.0);
	}
}