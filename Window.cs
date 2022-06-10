using System;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;

namespace ComputerGraphics
{
    public class Window : GameWindow
    {
        //Вершины пирамиды
        private readonly float[] _vertices =
        {
            -0.5f, -0.5f, 0.0f, //Треугольник 1
             0.5f, -0.5f, 0.0f,
             0.0f,  0.5f, -0.5f,


        };

        //Хендлеры объектов
        private int _vertexBufferObject;
        private int _vertexArrayObject;

        //Оппределяем шейдер
        private Shader _shader;

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings) 
        { 
        
        }

        //Инициализация OpenGL
        protected override void OnLoad()
        {
            base.OnLoad();

            //Очищаем бекграунд сцены и заполняем его серым цветом
            GL.ClearColor(0.3f, 0.3f, 0.3f, 1.0f);

            //Создаем пустой хендлер для вертексного буффера
            _vertexBufferObject = GL.GenBuffer();

            //Привязываем буфер в OpenGL
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);

            //Загружаем в буфер вершины фигуры
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            //Создаём хендлер для вертексного массива
            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            //Определяем интерпритацию данных буфера 
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

            //Включаем переменную 0 в шейдере
            GL.EnableVertexAttribArray(0);

            _shader = new Shader("../../../Shaders/shader.vert", "../../../Shaders/shader.frag");

            _shader.Use();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            _shader.Use();

            GL.BindVertexArray(_vertexArrayObject);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            var input = KeyboardState;

            if (input.IsKeyDown(Keys.Escape))
            {
                Close();
            }
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, Size.X, Size.Y);
        }

        protected override void OnUnload()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            int[] buffers = { _vertexBufferObject, _vertexArrayObject };

            GL.DeleteBuffers(2, buffers);

            GL.DeleteProgram(_shader.Handle);

            base.OnUnload();
        }
    }
}