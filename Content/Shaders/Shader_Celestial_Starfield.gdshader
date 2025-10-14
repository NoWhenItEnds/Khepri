shader_type canvas_item;

uniform int size;
uniform vec2 positions[2000];
uniform vec4 colours[2000];

varying vec2 pixel_pos;

void vertex() {
    pixel_pos = VERTEX;
}

void fragment() {
    for (int i = 0; i < size; i++) {
		vec2 position = positions[i]; 
		if(distance(position, pixel_pos) < 5.0) {
			COLOR = colours[i];
		}
    }
}