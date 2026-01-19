shader_type canvas_item;

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;
uniform float tint_strength : hint_range(0.0, 1.0) = 0.2;

void fragment() {
    vec4 col = texture(SCREEN_TEXTURE, SCREEN_UV);
    vec3 tint = vec3(0.0, 0.2, 0.5); // bluish underwater tint

    COLOR = vec4(mix(col.rgb, tint, tint_strength), col.a);
}
