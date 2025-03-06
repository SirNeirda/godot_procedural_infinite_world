shader_type spatial;
render_mode blend_mix, cull_back;

uniform vec4 face_color : source_color = vec4(1.0, 1.0, 1.0, 1.0); // Default white color for faces
uniform vec4 wireframe_color : source_color = vec4(0.0, 0.0, 0.0, 1.0); // Default black color for wireframe
uniform float wireframe_thickness : hint_range(0.0, 1.0) = 0.02; // Thickness of the wireframe

varying vec3 barycentric_coords;

void vertex() {
    // Calculate barycentric coordinates for wireframe
    if (VERTEX_ID % 3 == 0) {
        barycentric_coords = vec3(1.0, 0.0, 0.0);
    } else if (VERTEX_ID % 3 == 1) {
        barycentric_coords = vec3(0.0, 1.0, 0.0);
    } else {
        barycentric_coords = vec3(0.0, 0.0, 1.0);
    }
}

void fragment() {
    // Calculate the distance to the nearest edge using barycentric coordinates
    float edge_distance = min(barycentric_coords.x, min(barycentric_coords.y, barycentric_coords.z));

    // Calculate wireframe factor
    float wire = smoothstep(0.0, wireframe_thickness, edge_distance);

    // Mix face color and wireframe color
    vec4 final_color = mix(wireframe_color, face_color, wire);

    // Set the output color
    ALBEDO = final_color.rgb;
    ALPHA = final_color.a;
}