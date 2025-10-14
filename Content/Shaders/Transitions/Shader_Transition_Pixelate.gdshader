/* https://godotshaders.com/shader/pixelate-3d-effect/ */

shader_type canvas_item;

uniform float size : hint_range(1.0, 32.0) = 8.0;
uniform sampler2D SCREEN_TEXTURE : hint_screen_texture;

void fragment() {
    vec2 screen_res = vec2(1.0) / SCREEN_PIXEL_SIZE;
    vec2 uv = SCREEN_UV;

    uv = floor(uv * screen_res / size) * size / screen_res;

    COLOR = texture(SCREEN_TEXTURE, uv);
}
