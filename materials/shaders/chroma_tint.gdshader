shader_type canvas_item;

uniform float strength  : hint_range(0.0, 0.1, 0.001) = 0.08;
uniform vec2  center    = vec2(0.5, 0.5);
uniform float radius    : hint_range(0.0, 1.0, 0.001) = 0.25;

uniform float aberration : hint_range(0.0, 1.0, 0.001) = 0.425;
uniform float width      : hint_range(0.0, 0.1, 0.0001) = 0.04;
uniform float feather    : hint_range(0.0, 1.0, 0.001) = 0.135;

/* ── NEW WHOLE‑SCREEN TINT ────────────────────────────────────── */
uniform vec3  tint_color    : source_color = vec3(0.35, 0.55, 1.0); // light‑blue haze
uniform float tint_strength : hint_range(0.0, 1.0, 0.01) = 0.3;     // 0 = none, 1 = solid

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;

void fragment() {
    vec2 st            = SCREEN_UV;

    float aspect_ratio = SCREEN_PIXEL_SIZE.y / SCREEN_PIXEL_SIZE.x;
    vec2  scaled_st    = (st - vec2(0.0, 0.5)) / vec2(1.0, aspect_ratio) + vec2(0.0, 0.5);

    vec2  dist_center  = scaled_st - center;
    float mask         = (1.0 - smoothstep(radius - feather, radius, length(dist_center)))
                       * smoothstep(radius - width - feather, radius - width, length(dist_center));

    vec2  offset       = normalize(dist_center) * strength * mask;
    vec2  biased_st    = scaled_st - offset;

    vec2  abber_vec    = offset * aberration * mask;
    vec2  final_st     = st * (1.0 - mask) + biased_st * mask;

    /* — sample the three colour channels — */
    vec4  red          = texture(SCREEN_TEXTURE, final_st + abber_vec);
    vec4  blue         = texture(SCREEN_TEXTURE, final_st - abber_vec);
    vec4  ori          = texture(SCREEN_TEXTURE, final_st);

    vec4  chroma_col   = vec4(red.r, ori.g, blue.b, 1.0);

    vec3 tinted_rgb    = mix(chroma_col.rgb, tint_color, tint_strength);

    COLOR = vec4(tinted_rgb, 1.0);
}
