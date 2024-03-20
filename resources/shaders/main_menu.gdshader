shader_type canvas_item;

//uniform float U_time = 0.2;
uniform sampler2D u_noise_x;
uniform sampler2D u_noise_y;
uniform float u_time;
uniform float u_scale;

void fragment() {
    float time = floor(TIME / u_time) * u_time;
	//time = sin(time);
	
	vec2 uv = UV + time;
	float xoff = texture(u_noise_x, fract(uv)).r;
	float yoff = texture(u_noise_y, fract(uv)).r;
	vec2 off = vec2(xoff, yoff) * u_scale;

	COLOR = texture(TEXTURE, fract(UV + off));
	//COLOR = texture(u_noise_x, fract(UV));
	
	//COLOR = vec4(time, time, time, 1);
	//COLOR = texture(u_noise, UV);
    
    // Get the color of the displaced pixel
    //vec3 color = texture(TEXTURE, displacedCoord).rgb;
    
    // Output the color
    //COLOR = vec4(color, 1.0);
}