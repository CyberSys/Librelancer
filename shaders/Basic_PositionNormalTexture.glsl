@include(Basic_Features.inc)
@include(Basic_Fragment.inc)

@vertex
@include(includes/lighting.inc) 
@include(includes/camera.inc)
in vec3 vertex_position;
in vec3 vertex_normal;
in vec2 vertex_texture1;

out vec2 out_texcoord;
out vec2 out_texcoord2;
out vec3 out_normal;
out vec3 world_position;
out vec4 out_vertexcolor;
out vec4 view_position;

uniform mat4x4 World;
uniform mat4x4 NormalMatrix;
uniform vec4 MaterialAnim;

void main()
{
	vec4 pos = (ViewProjection * World) * vec4(vertex_position, 1.0);
	gl_Position = pos;
	world_position = (World * vec4(vertex_position,1)).xyz;
	view_position = (View * World) * vec4(vertex_position,1);
	out_normal = (NormalMatrix * vec4(vertex_normal,0)).xyz;
	out_texcoord = vec2(
		(vertex_texture1.x + MaterialAnim.x) * MaterialAnim.z, 
		1. - (vertex_texture1.y + MaterialAnim.y) * MaterialAnim.w
	);
	out_texcoord2 = out_texcoord;
	out_vertexcolor = vec4(1,1,1,1);
    light_vert(world_position, view_position, out_normal);
}
