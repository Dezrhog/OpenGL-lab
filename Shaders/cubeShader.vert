﻿#version 330 core

in vec3 aPosition;

void main(void)
{
    gl_Position = vec4(aPosition, 1.0);
}