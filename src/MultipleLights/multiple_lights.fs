#version 330 core
out vec4 FragColor;

struct Material {
	sampler2D diffuse;
	sampler2D specular;
	float shininess;
};

struct DirLight {
	vec3 direction;
	vec3 ambient;
	vec3 diffuse;
	vec3 specular;
};

struct PointLight {
	vec3 position;
	vec3 ambient;
	vec3 diffuse;
	vec3 specular;
	float constant;
	float linear;
	float quadratic;
};

struct SpotLight {
	vec3 position;
	vec3 direction;

	vec3 ambient;
	vec3 diffuse;
	vec3 specular;

	float constant;
	float linear;
	float quadratic;

	float cutOff;
	float outerCutOff;
};

#define NUM_POINTS_LIGHT 4

in vec3 FragPos;
in vec3 Normal;
in vec2 TexCoords;

uniform DirLight dirLight;
uniform PointLight pointLights[NUM_POINTS_LIGHT];
uniform SpotLight spotLight;

uniform vec3 viewPos;
uniform Material material;

vec3 CalcDirLight(DirLight light, vec3 normal, vec3 viewDir){
	vec3 lightDir = normalize(-light.direction);
	float diff = max(dot(lightDir, normal), 0.0);
	vec3 reflectDir = reflect(-lightDir, normal);
	float spec = pow(max(dot(reflectDir, viewDir), 0.0), material.shininess);

	vec3 ambient = light.ambient * texture(material.diffuse, TexCoords).rgb;
	vec3 diffuse = diff * light.diffuse * texture(material.diffuse, TexCoords).rgb;
	vec3 specular = spec * light.specular * texture(material.specular, TexCoords).rgb;

	return ambient + diffuse + specular;
}

vec3 CalcPointLight(PointLight light, vec3 normal, vec3 viewDir, vec3 fragPos){
	vec3 lightDir = normalize(light.position - fragPos);
	float diff = max(dot(lightDir, normal), 0.0);
	vec3 reflectDir = reflect(-lightDir, normal);
	float spec = pow(max(dot(reflectDir, viewDir), 0.0), material.shininess);

	float distance = length(light.position - fragPos);
	float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * distance * distance);

	vec3 ambient = light.ambient * texture(material.diffuse, TexCoords).rgb;
	vec3 diffuse = diff * light.diffuse * texture(material.diffuse, TexCoords).rgb;
	vec3 specular = spec * light.specular * texture(material.specular, TexCoords).rgb;

	return (ambient + diffuse + specular) * attenuation;
}

vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 viewDir, vec3 fragPos){
	vec3 lightDir = normalize(light.position - fragPos);

	float diff = max(dot(lightDir, normal), 0.0);
	vec3 reflectDir = reflect(-lightDir, normal);
	float spec = pow(max(dot(reflectDir, viewDir), 0.0), material.shininess);

	float distance = length(light.position - fragPos);
	float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * distance * distance);

	float theta = dot(lightDir, normalize(-light.direction));
	float epsilon = light.cutOff - light.outerCutOff;
	float intensity = clamp((theta - light.outerCutOff) / epsilon, 0.0, 1.0);

	vec3 ambient = light.ambient * texture(material.diffuse, TexCoords).rgb;
	vec3 diffuse = diff * light.diffuse * texture(material.diffuse, TexCoords).rgb;
	vec3 specular = spec * light.specular * texture(material.specular, TexCoords).rgb;

	return (ambient + diffuse + specular) * attenuation * intensity;
}

void main()
{
    vec3 norm = normalize(Normal);
    vec3 viewDir = normalize(viewPos - FragPos);
	vec3 result = vec3(0.0);

	// directional light
	result = CalcDirLight(dirLight, norm, viewDir);

	//point light
	for(int i=0; i<NUM_POINTS_LIGHT; ++i){
		result += CalcPointLight(pointLights[i], norm, viewDir, FragPos);
	}

	//spot light
	result += CalcSpotLight(spotLight, norm, viewDir, FragPos);


	FragColor = vec4(result, 1.0);

}