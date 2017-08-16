#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aNormal;

out vec3 FragPos;
out vec3 Normal;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
    FragPos = vec3(view * model * vec4(aPos, 1.0));
    //Normal = vec3(view * vec4(aNormal, 1.0));  
	//Normal = mat3(view) * aNormal;
	Normal = mat3(transpose(inverse(view * model))) * aNormal;
    
    gl_Position = projection * view * vec4(FragPos, 1.0);
}