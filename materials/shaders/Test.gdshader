shader_type spatial;

uniform sampler2D albedo_texture1 : source_color;
uniform sampler2D albedo_texture2 : source_color;
uniform sampler2D albedo_texture3 : source_color;
uniform sampler2D albedo_texture4 : source_color;

uniform float blur_amount : hint_range(0.0, 0.1) = 0.05;
uniform vec2 uv_scale = vec2(1.0, 1.0);

void fragment() {
    vec2 scaled_uv = UV * uv_scale;

    vec4 tex1 = texture(albedo_texture1, scaled_uv);
    vec4 tex2 = texture(albedo_texture2, scaled_uv);
    vec4 tex3 = texture(albedo_texture3, scaled_uv);
    vec4 tex4 = texture(albedo_texture4, scaled_uv);

    // Normalize the green value to a range of 0.0 to 1.0
    float normalized_green = clamp(COLOR.g / 0.4, 0.0, 1.0);

    // Determine the mix factors based on the green value
    float mix_factor1 = smoothstep(0.0, 0.1 + blur_amount, normalized_green);
    float mix_factor2 = smoothstep(0.1 - blur_amount, 0.2 + blur_amount, normalized_green);
    float mix_factor3 = smoothstep(0.2 - blur_amount, 0.3 + blur_amount, normalized_green);
    float mix_factor4 = smoothstep(0.3 - blur_amount, 0.4, normalized_green);

    // Calculate the final color by blending textures
    vec4 mixed_texture = mix(tex1, tex2, mix_factor1);
    mixed_texture = mix(mixed_texture, tex3, mix_factor2);
    mixed_texture = mix(mixed_texture, tex4, mix_factor3);

    ALBEDO = mixed_texture.rgb;
}