shader_type canvas_item;

uniform int size;
uniform vec3 positions[1000];
uniform vec4 colours[1000];

uniform mat4 PROJECTION_MATRIX;
uniform mat4 VIEW_MATRIX;

void fragment() {
    for (int i = 0; i < size; i++) {
		vec3 position = positions[i]; 
		vec4 viewPosition = VIEW_MATRIX * vec4(position, 1.0);
		vec4 clipPosition = PROJECTION_MATRIX * viewPosition;
		vec3 ndc = clipPosition.xyz / clipPosition.w;
        vec2 targetUV = (ndc.xy * 0.5) + 0.5;
		if(distance(targetUV, SCREEN_UV) < 0.005) {
			COLOR = colours[i];
		}
    }
}