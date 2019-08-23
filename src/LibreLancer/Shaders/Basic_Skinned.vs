#pragma include (lighting.inc)
in vec3 vertex_position;
in vec3 vertex_normal;
in vec2 vertex_texture1;
in vec4 vertex_boneweights;
in vec4 vertex_boneids;

out vec2 out_texcoord;
out vec2 out_texcoord2;
out vec3 out_normal;
out vec3 world_position;
out vec4 out_vertexcolor;
out vec4 view_position;

uniform mat4x4 World;
uniform mat4x4 View;
uniform mat4x4 ViewProjection;
uniform mat4x4 NormalMatrix;
uniform vec4 MaterialAnim;
uniform mat4x4 Bones[50];
uniform float FlipNormal;
uniform bool SkinningEnabled;

void main()
{
    mat4 boneTransform =
        Bones[int(vertex_boneids[0])] * vertex_boneweights[0] +
        Bones[int(vertex_boneids[1])] * vertex_boneweights[1] +
        Bones[int(vertex_boneids[2])] * vertex_boneweights[2] +
        Bones[int(vertex_boneids[3])] * vertex_boneweights[3];
    vec3 skinnedPos = (boneTransform * vec4(vertex_position,1)).xyz;    
    if(!SkinningEnabled) skinnedPos = vertex_position;
    vec4 pos = (ViewProjection * World) * vec4(skinnedPos, 1);
    gl_Position = pos;
    world_position = (World * vec4(skinnedPos,1)).xyz;
    view_position = (View * World) * vec4(skinnedPos,1);
    out_normal = (NormalMatrix * vec4(vertex_normal,0)).xyz * FlipNormal;
    out_texcoord = vec2(
        (vertex_texture1.x + MaterialAnim.x) * MaterialAnim.z, 
        1. - (vertex_texture1.y + MaterialAnim.y) * MaterialAnim.w
    );
    out_texcoord2 = out_texcoord;
    out_vertexcolor = vec4(1,1,1,1);
    light_vert(world_position, view_position, out_normal);
}
