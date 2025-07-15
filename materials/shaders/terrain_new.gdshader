shader_type spatial;
render_mode depth_draw_opaque, cull_back,
            diffuse_burley, specular_schlick_ggx;

/* ────────  Layer 0  ──────── */
uniform sampler2D albedo0 : source_color;
uniform sampler2D normal0 : hint_normal;
uniform sampler2D rough0;          // R channel = roughness

/* ────────  Layer 1  ──────── */
uniform sampler2D albedo1 : source_color;
uniform sampler2D normal1 : hint_normal;
uniform sampler2D rough1;

/* ────────  Layer 2  ──────── */
uniform sampler2D albedo2 : source_color;
uniform sampler2D normal2 : hint_normal;
uniform sampler2D rough2;

/* ────────  Mapping & global controls  ──────── */
/* Exactly 1 per mask row; change these to rewire rgb layers */
uniform vec3 channel_mask0 = vec3(1.0, 0.0, 0.0);   // default: R
uniform vec3 channel_mask1 = vec3(0.0, 1.0, 0.0);   // default: G
uniform vec3 channel_mask2 = vec3(0.0, 0.0, 1.0);   // default: B

uniform float uv_scale        : hint_range(0.1, 32.0) = 4.0;
uniform float normal_strength : hint_range(0.0,  2.0) = 1.0;
uniform float rough_strength  : hint_range(0.0,  2.0) = 1.0;

uniform bool  invert_normal_y = false;   // flip green ”
uniform bool  invert_roughness = false;  // set true if black = rough

/* ───────────────────────────────────────────── */

void fragment() {
    /* 1.  Compute weights */
    vec3 vtx = COLOR.rgb;
    vec3 w;
    w.x = dot(vtx, channel_mask0);   // weight for layer0
    w.y = dot(vtx, channel_mask1);   // weight for layer1
    w.z = dot(vtx, channel_mask2);   // weight for layer2

    float sum = max(w.x + w.y + w.z, 0.001);
    w /= sum;                        // renormalise

    /* 2.  UV tiling */
    vec2 uv = UV * uv_scale;

    /* 3.  Albedo */
    vec3 a0 = texture(albedo0, uv).rgb;
    vec3 a1 = texture(albedo1, uv).rgb;
    vec3 a2 = texture(albedo2, uv).rgb;
    ALBEDO = a0 * w.x + a1 * w.y + a2 * w.z;

    /* ── 4.  NORMALS  ──────────────────────────────────────────────── */
vec3 base_N = NORMAL;                          // geometric normal (world)

mat3 tbn = mat3( normalize(TANGENT),
                 normalize(BINORMAL),         // BINORMAL in older builds
                 base_N );

vec3 ts0 = texture(normal0, uv).xyz * 2.0 - 1.0;   // tangentspace
vec3 ts1 = texture(normal1, uv).xyz * 2.0 - 1.0;
vec3 ts2 = texture(normal2, uv).xyz * 2.0 - 1.0;

if (invert_normal_y) { ts0.y = -ts0.y; ts1.y = -ts1.y; ts2.y = -ts2.y; }

vec3 ws0 = normalize(tbn * ts0);               // to world space
vec3 ws1 = normalize(tbn * ts1);
vec3 ws2 = normalize(tbn * ts2);

vec3 blended_ws = normalize(ws0 * w.x + ws1 * w.y + ws2 * w.z);

/* Spherically blend with the geometric normal so vector stays */
vec3 final_N = normalize( mix(base_N, blended_ws, normal_strength) );
NORMAL = final_N;

    /* 5.  Roughness  */
    float r0 = texture(rough0, uv).r;
    float r1 = texture(rough1, uv).r;
    float r2 = texture(rough2, uv).r;
    float rough = r0 * w.x + r1 * w.y + r2 * w.z;
    if (invert_roughness) rough = 1.0 - rough;
    ROUGHNESS = clamp(rough, 0.0, 1.0);//ROUGHNESS = clamp(rough * rough_strength, 0.04, 1.0);  // 0.04 - never 0

    /* 6.  Specular is just a constant */
    SPECULAR = 0.5;
}
