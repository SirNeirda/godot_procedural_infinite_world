shader_type canvas_item;

uniform float blur_strength : hint_range(0.0, 2.0, 0.01) = 1.0;
uniform vec3 tint_color : source_color = vec3(0.35, 0.55, 1.0);
uniform float tint_strength : hint_range(0.0, 1.0, 0.01) = 0.3;

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear;

void fragment() {
    vec2 uv = SCREEN_UV;
    vec2 px = SCREEN_PIXEL_SIZE * blur_strength;

    // 4-tap blur (cheap box blur)
    vec3 col = texture(SCREEN_TEXTURE, uv).rgb;
    col += texture(SCREEN_TEXTURE, uv + vec2(px.x, 0.0)).rgb;
    col += texture(SCREEN_TEXTURE, uv - vec2(px.x, 0.0)).rgb;
    col += texture(SCREEN_TEXTURE, uv + vec2(0.0, px.y)).rgb;
    col += texture(SCREEN_TEXTURE, uv - vec2(0.0, px.y)).rgb;
    col *= 0.2;

    // Tint
    col = mix(col, tint_color, tint_strength);

    COLOR = vec4(col, 1.0);
}
