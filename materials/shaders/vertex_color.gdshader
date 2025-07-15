shader_type spatial;

/* If you want it completely self‑lit (no shading), keep `unshaded`.
   Delete `unshaded` if you’d prefer it to pick up scene lights. */
//render_mode unshaded;

void fragment() {
    ALBEDO = COLOR.rgb;   // the vertex colour
}
