shader_type spatial;

uniform sampler2D heightmap_tex : source_color;
uniform vec2 uv_scale = vec2(1.0, 1.0);

void fragment() {
    // Apply UV scale
    vec2 scaled_uv = UV * uv_scale;

    // Sample the heightmap
    float h = texture(heightmap_tex, scaled_uv).r;

    // Output
    vec3 final_color = vec3(h);
    float final_alpha = 1.0;
    float final_specular = 0.0;
    float final_roughness = 1.0;

    ALBEDO = final_color;
    ALPHA = final_alpha;
    SPECULAR = final_specular;
    ROUGHNESS = final_roughness;
}