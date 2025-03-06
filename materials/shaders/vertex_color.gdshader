shader_type spatial;

void fragment() {
    // Normalize the green vertex color between 0 and 0.4
    float normalized_green = clamp(COLOR.g / 0.9, 0.1, 1.0);

    // Output the normalized green value as grayscale
    ALBEDO = vec3(VERTEX.g, VERTEX.g, VERTEX.g);
}