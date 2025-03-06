shader_type spatial;

uniform vec3 water_color: source_color;
uniform float water_color_ratio: hint_range(0.0, 1.0, 0.1) = 0.5;
uniform float beers_law: hint_range(0.0, 20.0, 0.1) = 3.0;

uniform sampler2D water_noise_1;
uniform sampler2D water_noise_2;

uniform sampler2D caustics_noise;
uniform sampler2D foam_noise;

uniform float normal_strength: hint_range(0.0, 2.0, 0.1) = 0.5;
uniform float roughness: hint_range(0.0, 1.0, 0.1) = 0.1;

uniform float time_scale: hint_range(0.0, 10.0, 0.1) = 5.0;
uniform float uv_scale: hint_range(0.1, 5.0, 0.1) = 1.0;

uniform float depth_distance: hint_range(0.0, 10.0, 0.1) = 1.0;
uniform float caustics_power: hint_range(0.0, 1.0, 0.1) = 1.0;
uniform float caustics_distance: hint_range(0.0, 1.0, 0.05) = 0.3;
uniform float foam_distance: hint_range(0.0, 0.1, 0.01) = 0.04;

uniform sampler2D depth_texture: hint_depth_texture;
uniform sampler2D screen_texture: hint_screen_texture;

uniform float wave_power: hint_range(0.0, 0.4, 0.02) = 0.2;
uniform float wave_frequency: hint_range(0.0, 5.0, 0.1) = 1.0;

varying vec4 world_uv;


void vertex() {
	world_uv = MODEL_MATRIX * vec4(VERTEX, 1.0);
	float offset = cos(0.5*world_uv.x + 0.5*world_uv.z) * sin(TIME * wave_frequency);
	offset += sin(0.8*world_uv.z + 0.8*world_uv.y) * cos(1.5 * TIME * wave_frequency);
	VERTEX.y += wave_power * offset;
}

void fragment() {
	float depth_r = textureLod(depth_texture, SCREEN_UV, 0.0).r;
	vec4 world = INV_PROJECTION_MATRIX * vec4(SCREEN_UV * 2.0 - 1.0, depth_r, 1.0);
	world.xyz /= world.w;

	vec2 uv = world_uv.xz * uv_scale;
	float fac1 = 25.0;
	float fac2 = 0.01;
	uv.x += sin(TIME * time_scale + (uv.x + uv.y) * fac1) * fac2;
	uv.y += cos(TIME * time_scale + (uv.x - uv.y) * fac1) * fac2;

	vec2 suv = SCREEN_UV;

	float depth_blend = smoothstep(world.z + depth_distance, world.z, VERTEX.z);
	depth_blend = exp(depth_blend * -beers_law);  // dark: shallow, bright: deep
	vec3 screen = textureLod(screen_texture, suv, 0.0).rgb;
	screen = mix(screen, water_color, water_color_ratio);
	vec3 screen_depth = mix(screen, water_color, depth_blend);
	vec2 uv_caustics = 1.7 * uv;
	vec3 caustics = pow(texture(caustics_noise, uv_caustics).rgb, vec3(3.0));
	float caustics_level = 1.0 - smoothstep(depth_blend, 0.0, caustics_distance);
	caustics_level = pow(caustics_level, 0.6);
	caustics = vec3(caustics_level) * caustics;

	float foam_level = 1.0 - smoothstep(depth_blend, 0.0, foam_distance);
	foam_level = pow(foam_level, 3.0);

	vec3 foam = texture(foam_noise, uv).rgb;
	foam = pow(foam-0.2, vec3(0.5)) * vec3(foam_level);
	foam = clamp(foam, 0.0, 1.0);

	ALBEDO = screen_depth + caustics_power*caustics + foam;
	vec3 normal1 = texture(water_noise_1, 1.0*uv).rgb;
	vec3 normal2 = texture(water_noise_2, 1.0*uv).rgb;
	NORMAL_MAP = mix(normal1, normal2, (sin(TIME * time_scale) + 1.0) / 2.0);
	NORMAL *= normal_strength;

	ROUGHNESS = roughness;
}

//void light() {
	// Called for every pixel for every light affecting the material.
	// Uncomment to replace the default light processing function with this one.
//}