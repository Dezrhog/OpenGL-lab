using System;
using System.Collections.Generic;
using System.IO;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

/**
 * Created by AlekseevDA on 10.06.2022
 * Класс, предназначенный для создания шейдеров, написанных на GLSL
 */
namespace ComputerGraphics
{
    public class Shader
    {
        public readonly int Handle;

        private readonly Dictionary<string, int> _uniformLocations;

        public Shader(string vertPath, string fragPath)
        {
            //Загружается вертексный шейдер
            var shaderSource = File.ReadAllText(vertPath);

            //Создаётся пустой вертексный шейдер
            var vertexShader = GL.CreateShader(ShaderType.VertexShader);

            //Пустой шейдер инициализируется из загруженного источника GLSL
            GL.ShaderSource(vertexShader, shaderSource);

            //Компилируется шейдер
            CompileShader(vertexShader);

            //Те же действия для фрагментного шейдера
            shaderSource = File.ReadAllText(fragPath);
            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, shaderSource);
            CompileShader(fragmentShader);

            //Далее необходимо смержить шейдеры в shader program, которая может быть использована OpenGL
            Handle = GL.CreateProgram();

            //Присоединяем оба шейдера
            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);

            //Затем линкуем их
            LinkProgram(Handle);

            //Отсоединяем шейдеры и удаляем их, очищая тем самым ресурсы
            GL.DetachShader(Handle, vertexShader);
            GL.DetachShader(Handle, fragmentShader);
            GL.DeleteShader(fragmentShader);
            GL.DeleteShader(vertexShader);

            //Далее создаётся кэш для формы шейдера
            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);

            _uniformLocations = new Dictionary<string, int>();

            for (var i = 0; i < numberOfUniforms; i++)
            {
                var key = GL.GetActiveUniform(Handle, i, out _, out _);

                var location = GL.GetUniformLocation(Handle, key);

                _uniformLocations.Add(key, location);
            }
        }

        /**
         * Метод-обёртка для GL.CompileShader,
         * написанный с целью возможности отладки
         * компиляции шейдеров
         */
        public static void CompileShader(int shader)
        {
            //Попытка скомпилировать шейдер
            GL.CompileShader(shader);

            //Проверка ошибок компиляции
            GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);
            if (code != (int)All.True)
            {
                var infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"Ошибка компиляции шейдера({shader}).\n\n{infoLog}");
            }
        }

        /**
         * Метод-обёртка для GL.LinkProgram,
         * написанный с целью возможности отладки
         * линковки программ
         */
        public static void LinkProgram(int program)
        {
            //Линкуем программу
            GL.LinkProgram(program);

            //Проверка ошибок линковки
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var code);
            if (code != (int)All.True)
            {
                var infoLog = GL.GetProgramInfoLog(program);
                throw new Exception($"Ошибка линковки программы({program}).\n\n{infoLog}");
            }
        }

        /**
         * Обёртка функции включения шейдерной программы
         */
        public void Use()
        {
            GL.UseProgram(Handle);
        }


        public int GetAttribLocation(string attribName)
        {
            return GL.GetAttribLocation(Handle, attribName);
        }

        public void SetInt(string name, int data)
        {
            GL.UseProgram(Handle);
            GL.Uniform1(_uniformLocations[name], data);
        }

        public void SetFloat(string name, float data)
        {
            GL.UseProgram(Handle);
            GL.Uniform1(_uniformLocations[name], data);
        }

        public void SetMatrix4(string name, Matrix4 data)
        {
            GL.UseProgram(Handle);
            GL.UniformMatrix4(_uniformLocations[name], true, ref data);
        }

        public void SetVector3(string name, Vector3 data)
        {
            GL.UseProgram(Handle);
            GL.Uniform3(_uniformLocations[name], data);
        }
    }
}
