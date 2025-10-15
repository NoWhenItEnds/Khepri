shader_type canvas_item;

uniform int size;
uniform vec3 positions[2095];
uniform vec4 colours[2095];

varying vec2 pixel_pos;

void vertex() {
    pixel_pos = VERTEX;
}

void fragment() {
    for (int i = 0; i < size; i++) {
		vec2 position = positions[i].xy; 
		float magnitude = positions[i].z;
		if(distance(position, pixel_pos) < magnitude) {
			COLOR = colours[i];
		}
    }
}