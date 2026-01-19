shader_type spatial;
render_mode blend_mix, depth_draw_opaque, cull_back, world_vertex_coords;

// === COLOR TINT ===
uniform vec3  shallow_color : source_color = vec3(0.02, 0.45, 0.55);
uniform vec3  deep_color    : source_color = vec3(0.00, 0.10, 0.28);
uniform vec3  sky_color     : source_color = vec3(0.55, 0.67, 0.82);

// Height-based fake depth fade
uniform float water_level = 0.0;                    // Y world position of surface
uniform float depth_fade_distance : hint_range(0.1, 50.0) = 10.0;

// === NORMALS / WAVES ===
uniform sampler2D normal_map : hint_normal, repeat_enable;
uniform float normal_strength : hint_range(0.0, 2.0) = 0.6;
uniform vec2  normal_speed1 = vec2( 0.04,  0.02);
uniform vec2  normal_speed2 = vec2(-0.03,  0.03);
uniform float wave_height   : hint_range(0.0, 2.0) = 0.1;

// === UV TILING ===
uniform float repeat_size : hint_range(0.1, 512.0) = 8.0;

// === REFLECTION ===
uniform float fresnel_power : hint_range(1.0, 8.0) = 5.0;

// === TRANSPARENCY / RIM ===
uniform float transparency_shallow : hint_range(0.0, 1.0) = 0.1;
uniform float transparency_deep    : hint_range(0.0, 1.0) = 0.9;
uniform vec3  rim_color  : source_color = vec3(1.0);
uniform float rim_width  : hint_range(0.0, 0.5) = 0.15;
uniform float rim_softness : hint_range(0.0, 1.0) = 0.05;

varying vec2 world_uv;
varying vec3 world_pos; // world-space position passed to fragment

// === VERTEX ===
void vertex() {
    // displacement / waves
    vec3 p = VERTEX;
    vec2 disp_uv = p.xz / repeat_size + TIME * normal_speed1;
    p.y += (texture(normal_map, disp_uv).r - 0.5) * wave_height;
    VERTEX = p;

    // world UV for normal sampling
    world_uv = p.xz / repeat_size;

    // compute world-space position and pass to fragment
    // WORLD_MATRIX is available; multiply by vec4(VERTEX, 1.0)
    world_pos = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
}

// === FRAGMENT ===
void fragment() {
    // Blend two moving normals for fake wave distortion
    vec3 n1 = texture(normal_map, world_uv + TIME * normal_speed1).rgb * 2.0 - 1.0;
    vec3 n2 = texture(normal_map, world_uv + TIME * normal_speed2).rgb * 2.0 - 1.0;
    vec3 Nn = normalize(NORMAL + (n1 + n2) * 0.5 * normal_strength);
    NORMAL = Nn;

    // --- Fake depth based on world-space height ---
    float height_diff = abs(world_pos.y - water_level);
    float depth_mix = clamp(height_diff / depth_fade_distance, 0.0, 1.0);

    // --- Tint & reflection ---
    vec3 tint = mix(shallow_color, deep_color, depth_mix);
    float fres = pow(1.0 - max(dot(Nn, VIEW), 0.0), fresnel_power);
    vec3 base = mix(tint, sky_color, fres);

    // --- Transparency & rim light ---
    ALPHA = mix(transparency_shallow, transparency_deep, depth_mix);

    // Rim = subtle highlight near surface transition
    float rim = smoothstep(rim_width + rim_softness, rim_width, depth_mix);
    ALBEDO = mix(base, rim_color, rim);

    METALLIC  = 0.0;
    ROUGHNESS = 0.05;
    SPECULAR  = 0.08;
}
