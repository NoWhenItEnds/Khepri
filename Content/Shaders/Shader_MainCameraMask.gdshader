shader_type canvas_item;
render_mode blend_mix;

void fragment() {
	if(COLOR.r != 0.0 || COLOR.g != 0.0 || COLOR.b != 0.0) {
		discard;
	}
	COLOR.rgb = vec3(0.0, 0.0, 0.0);
}