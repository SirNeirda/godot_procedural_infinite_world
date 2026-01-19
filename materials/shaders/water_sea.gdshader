shader_type spatial;

render_mode cull_disabled, shadows_disabled;

// Textures
uniform sampler2D heightmap_tex : source_color;//,filter_nearest;
uniform sampler2D normal_map : source_color;

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;

uniform vec2 uv_scale = vec2(1.0, 1.0);
uniform vec2 uv_offset = vec2(0.0, 0.0);
uniform vec2 heightmap_tex_crop_by_pixels = vec2(2.0, 2.0);

// Colors & alpha

uniform vec3 shallow_color : source_color = vec3(0.08, 0.95, 0.85);
uniform vec3 deep_color    : source_color = vec3(0.00, 0.24, 0.55);
uniform float shallow_alpha = 0.18;
uniform float deep_alpha    = 0.85;

uniform float shallow_grad_min = 0.0;
uniform float shallow_grad_max = 1.0;

// Waves
uniform float wave_strength = 0.05;
uniform float wave_height = 0.05;
uniform float wave_scale = 1.0;
uniform float wave_frequency = 2.0;
uniform float wave_speed = 0.4;

// Moving noise
uniform float noise_scale = 2.0;
uniform float noise_strength = 0.02;
uniform vec2 noise_flow_direction = vec2(0.3, 0.2);
uniform float noise_flow_speed = 0.05;
uniform float shallow_noise_attenuation = 1.0;


// Normal map
uniform bool use_normal_map = true;
uniform float normal_strength = 0.8;
uniform vec2 normal_flow_direction = vec2(0.2, 0.1);
uniform float normal_flow_speed = 0.05;

// Fresnel / rim
uniform float rim_strength = 0.3;
uniform float rim_power = 1.5;
uniform vec3 rim_color : source_color = vec3(1.0, 1.0, 1.0);

// NEW: Screen-space reflection rim
uniform bool use_screen_reflection_rim = false;
uniform float screen_reflection_strength = 1.0;
uniform float screen_reflection_normal_influence = 1.0;

// Specular / roughness
uniform float specular_strength = 0.5;
uniform float roughness = 0.32;

// Lens distortion
//uniform float lens_strength = 0.02;
//uniform float lens_scale = 10.0;
//uniform float lens_speed = 0.1;

float moving_noise(vec2 uv) {
    float t = dot(uv, vec2(1.0, 1.0)) * 3.0 + TIME * 1.0;
    return sin(uv.x * 10.0 + t) * cos(uv.y * 10.0 + t);
}


// Vertex. Abandoned for now. 
/*void vertex() {
    float x = fract(UV.x * wave_scale) * TAU;
    float y = fract(UV.y * wave_scale) * TAU;

    float wave = sin(x + TIME * wave_speed) * cos(y + TIME * wave_speed);
    VERTEX.y += wave * wave_height;
}*/

// Pixel cropper helper. We need to crop pixels in the shader because otherwise we get edge bleeding. FWI heightmaps generated contain extra pixels for proper LOD integration, which are also useful for the water heighmap so it can generate proper filter mapping.
vec2 get_cropped_uv(sampler2D tex, vec2 uv, vec2 crop_px) {
    // Get texture size in pixels
    vec2 tex_size = vec2(textureSize(tex, 0));

    // Convert crop in pixels → crop in UV
    vec2 crop_uv = crop_px / tex_size;

    // Compute cropped min/max UV
    vec2 uv_min = crop_uv;
    vec2 uv_max = 1.0 - crop_uv;

    // Remap (0..1) → (uv_min..uv_max)
    return uv_min + (uv_max - uv_min) * uv;
}


// Fragment
void fragment() {
	bool is_front = FRONT_FACING; //We differentiate between front (up the surface) and down. 
	
	 // Compute cropped UV first
    vec2 base_uv = UV * uv_scale + uv_offset;
	
    vec2 uv = get_cropped_uv(heightmap_tex, base_uv, heightmap_tex_crop_by_pixels);
	float depth = 1.0 - texture(heightmap_tex, uv).r;
	
	
	

	depth = 1.0 - texture(heightmap_tex, uv).r;//float depth = 1.0 - texture_nearest(heightmap_tex, base_uv, true).r; // false => clamp edges
depth = clamp(depth, 0.0, 1.0);

    //vec2 distorted_uv = lens_distortion(UV, depth);

    vec3 water_color = mix(shallow_color, deep_color, depth);
	//if (!is_front) {water_color = deep_color;}
    float shallow_grad = smoothstep(shallow_grad_min, shallow_grad_max, 1.0 - depth);
    float water_alpha = mix(shallow_alpha * shallow_grad, deep_alpha, depth);
	//if (!is_front) {water_alpha = deep_alpha;}


    // Normal
    vec3 n = NORMAL;
    if (use_normal_map) {
        float shallow_factor = 1.0 - depth;
        float attenuated_strength = mix(normal_strength, 0.0, shallow_factor);

        vec3 nmap = texture(normal_map, base_uv + normal_flow_direction * TIME * normal_flow_speed).rgb * 2.0 - 1.0;
        n = normalize(mix(n, nmap, attenuated_strength));
    }

    // Fresnel
    vec3 view_dir = normalize(VIEW);
    float fresnel = pow(1.0 - max(dot(n, view_dir), 0.0), rim_power);
    float rim = fresnel * rim_strength;


    // Pseudo Screen-space reflection
    vec3 rim_final_color = rim_color;

    if (use_screen_reflection_rim) {
        float water_line = SCREEN_UV.y;
        float mirrored_y = water_line - (SCREEN_UV.y - water_line);

        vec2 refl_uv = vec2(SCREEN_UV.x, mirrored_y);
        refl_uv = clamp(refl_uv, vec2(0.0), vec2(1.0));

        vec3 screen_refl = texture(SCREEN_TEXTURE, refl_uv).rgb;

        float n_factor = pow(1.0 - dot(n, view_dir), screen_reflection_normal_influence);

        rim_final_color = mix(rim_color, screen_refl, n_factor * screen_reflection_strength);
    }

    // Final Output
    ALBEDO = water_color;
    SPECULAR = specular_strength;
    ROUGHNESS = roughness;
    EMISSION = rim_final_color * rim;
    NORMAL = n;
    ALPHA = water_alpha;
}
