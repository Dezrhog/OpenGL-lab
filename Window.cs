using System;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace ComputerGraphics
{
    public class Window : GameWindow
    {
        //Вершины фигуры
        private readonly float[] _vertices =
        {
             0.5f,  0.5f, 0.0f, 
             0.5f, -0.5f, 0.0f,
            -0.5f, -0.5f, 0.0f,
            -0.5f,  0.5f, 0.0f
        };

        /**
         * Данная переменная представляет собой
         * как бы массив указателей на вершины 
         * полигонов (треугольников) фигуры.
         * 
         * Он необходим для того, чтобы иметь возможность
         * переиспользовать вершины.
         */
        private readonly uint[] _indices =
        {
            0, 1, 3,
            1, 2, 3
        };

        //Хендлеры объектов
        private int _vertexBufferObject;
        private int _vertexArrayObject;
        private int _elementBufferObject;

        //Определяем шейдер и камеру
        private Shader _shader;
        private Camera _camera;

        //Вспомогательные переменные для камеры и вращения фигуры
        private bool _firstMove = true;
        private Vector2 _lastPos;
        private double _time;

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

            GL.Enable(EnableCap.DepthTest);

            _shader = new Shader("../../../Shaders/shader.vert", "../../../Shaders/shader.frag");

            _shader.Use();

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            //Создаем пустой хендлер для вертексного буффера
            _vertexBufferObject = GL.GenBuffer();
            //Привязываем буфер
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            //Загружаем в буфер вершины фигуры
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            //Создаём хендлер для вертексного массива
            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            //Определяем интерпритацию данных вершин для буфера
            GL.VertexAttribPointer(_shader.GetAttribLocation("aPosition"), 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

            //Включаем атрибут вершины 0
            GL.EnableVertexAttribArray(0);


            //Создаём элементный буффер, привязываем его, и заполняем его данными
            _elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

            _camera = new Camera(Vector3.UnitZ * 3, Size.X / (float)Size.Y);

            CursorGrabbed = true;
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            _time += 4.0 * args.Time;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _shader.Use();

            var model = Matrix4.Identity * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(_time));
            _shader.SetMatrix4("model", model);
            _shader.SetMatrix4("view", _camera.GetViewMatrix());
            _shader.SetMatrix4("projection", _camera.GetProjectionMatrix());

            GL.BindVertexArray(_vertexArrayObject);

            GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            if (!IsFocused)
            {
                return;
            }

            var input = KeyboardState;

            if (input.IsKeyDown(Keys.Escape))
            {
                Close();
            }

            const float cameraSpeed = 1.5f;
            const float sensitivity = 0.2f;

            if (input.IsKeyDown(Keys.W))
            {
                _camera.Position += _camera.Front * cameraSpeed * (float)e.Time;
            }
            if (input.IsKeyDown(Keys.S))
            {
                _camera.Position -= _camera.Front * cameraSpeed * (float)e.Time;
            }
            if (input.IsKeyDown(Keys.A))
            {
                _camera.Position -= _camera.Right * cameraSpeed * (float)e.Time;
            }
            if (input.IsKeyDown(Keys.D))
            {
                _camera.Position += _camera.Right * cameraSpeed * (float)e.Time;
            }
            if (input.IsKeyDown(Keys.Space))
            {
                _camera.Position += _camera.Up * cameraSpeed * (float)e.Time;
            }
            if (input.IsKeyDown(Keys.LeftShift))
            {
                _camera.Position -= _camera.Up * cameraSpeed * (float)e.Time;
            }

            var mouse = MouseState;

            if (_firstMove)
            {
                _lastPos = new Vector2(mouse.X, mouse.Y);
                _firstMove = false;
            } else
            {
                var deltaX = mouse.X - _lastPos.X;
                var deltaY = mouse.Y - _lastPos.Y;
                _lastPos = new Vector2(mouse.X, mouse.Y);

                _camera.Yaw += deltaX * sensitivity;
                _camera.Pitch -= deltaY * sensitivity;
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            _camera.Fov -= e.OffsetY;
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, Size.X, Size.Y);
            _camera.AspectRatio = Size.X / (float)Size.Y;
        }

        protected override void OnUnload()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            int[] buffers = { _vertexBufferObject, _vertexArrayObject };

            GL.DeleteBuffers(2, buffers);

            GL.DeleteProgram(_shader.Handle);

            _shader.Dispose();

            base.OnUnload();
        }
    }
}