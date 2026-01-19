shader_type canvas_item;

uniform float strength  : hint_range(0.0, 0.1, 0.001) = 0.08;
uniform vec2  center    = vec2(0.5, 0.5);
uniform float radius    : hint_range(0.0, 1.0, 0.001) = 0.25;

uniform float aberration : hint_range(0.0, 1.0, 0.001) = 0.425;
uniform float width      : hint_range(0.0, 0.1, 0.0001) = 0.04;
uniform float feather    : hint_range(0.0, 1.0, 0.001) = 0.135;

uniform vec3  tint_color    : source_color = vec3(0.35, 0.55, 1.0);
uniform float tint_strength : hint_range(0.0, 1.0, 0.01) = 0.3;

// New: wavelength absorption (water removes red)
uniform vec3  absorb_color     = vec3(1.0, 0.0, 0.0); 
uniform float absorb_strength  : hint_range(0.0, 2.0, 0.01) = 0.8;

// New: underwater normal-map displacement
uniform sampler2D normal_map : source_color;
uniform vec2 normal_move_speed = vec2(0.5, 0.5);

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;

void fragment() {
    vec2 st = SCREEN_UV;

    // ==== Underwater Normal Map Distortion 
    vec2 nm_uv = UV + TIME * normal_move_speed;
    vec3 nm = texture(normal_map, nm_uv).rgb;
    nm = nm * 2.0 - 1.0;                 // remap to -1..1
    vec2 nm_offset = nm.xy * 0.015;      // distortion intensity (adjustable)
    st += nm_offset;                     // apply distortion globally


    // Original vignette
    float aspect_ratio = SCREEN_PIXEL_SIZE.y / SCREEN_PIXEL_SIZE.x;
    vec2 scaled_st = (st - vec2(0.0, 0.5)) / vec2(1.0, aspect_ratio) + vec2(0.0, 0.5);

    vec2 dist_center = scaled_st - center;
    float mask = (1.0 - smoothstep(radius - feather, radius, length(dist_center)))
               * smoothstep(radius - width - feather, radius - width, length(dist_center));

    vec2 offset = normalize(dist_center) * strength * mask;
    vec2 biased_st = scaled_st - offset;

    vec2 abber_vec = offset * aberration * mask;
    vec2 final_st = st * (1.0 - mask) + biased_st * mask;


    // Chromatic Aberration Sampling
    vec4 red   = texture(SCREEN_TEXTURE, final_st + abber_vec);
    vec4 blue  = texture(SCREEN_TEXTURE, final_st - abber_vec);
    vec4 ori   = texture(SCREEN_TEXTURE, final_st);

    vec4 chroma_col = vec4(red.r, ori.g, blue.b, 1.0);


    // Tinting (bluish haze) 
    vec3 tinted = mix(chroma_col.rgb, tint_color, tint_strength);


    //  Wavelength absorption (we remove the reds because that is how water behaves) 
    // absorb_color defines which wavelengths disappear.
    // absorb_strength defines how fast.
    vec3 absorbed = tinted * (1.0 - absorb_color * absorb_strength);


    COLOR = vec4(absorbed, 1.0);
}
