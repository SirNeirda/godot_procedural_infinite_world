shader_type canvas_item;

uniform float saturation : hint_range(0.0, 2.0) = 1.0;
uniform float opacity : hint_range(0.0, 1.0) = 1.0;

void fragment() {
    vec4 tex = texture(TEXTURE, UV);

    // Convert to grayscale (luminance)
    float gray = dot(tex.rgb, vec3(0.299, 0.587, 0.114));

    // Mix grayscale with original color
    vec3 saturated = mix(vec3(gray), tex.rgb, saturation);

    COLOR = vec4(saturated, tex.a * opacity);
}