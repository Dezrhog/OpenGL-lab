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
        //Вершины пирамиды
        private readonly float[] _pyramidVertices =
        {
        //    X      Y      Z
             0.0f,  0.5f, -0.5f,
            -0.5f, -0.5f,  0.0f, // Передняя сторона
             0.5f, -0.5f,  0.0f, 

             0.0f,  0.5f, -0.5f,
            -0.5f, -0.5f,  0.0f, // Левая сторона
            -0.5f, -0.5f, -1.0f,

             0.0f,  0.5f, -0.5f,
            -0.5f, -0.5f, -1.0f, // Задняя сторона
             0.5f, -0.5f, -1.0f,

             0.0f,  0.5f, -0.5f,
             0.5f, -0.5f, -1.0f, // Правая сторона
             0.5f, -0.5f,  0.0f,

        //       Основание
            -0.5f, -0.5f,  0.0f,
             0.5f, -0.5f,  0.0f, // Первый полигон основания
            -0.5f, -0.5f, -1.0f,
             0.5f, -0.5f, -1.0f,
            -0.5f, -0.5f, -1.0f, // Второй полигон основания
             0.5f, -0.5f,  0.0f
        };

        //Вершины куба
        private readonly float[] _cubeVertices =
        {
        //  Вершины нижней грани
            -0.5f, -0.5f,  0.0f, // [0] Передняя левая
             0.5f, -0.5f,  0.0f, // [1] Передняя правая
            -0.5f, -0.5f, -1.0f, // [2] Задняя левая
             0.5f, -0.5f, -1.0f, // [3] Задняя правая

        //  Вершины верхней грани
            -0.5f,  0.5f,  0.0f, // [4] Передняя левая
             0.5f,  0.5f,  0.0f, // [5] Передняя правая
            -0.5f,  0.5f, -1.0f, // [6] Задняя левая
             0.5f,  0.5f, -1.0f  // [7] Задняя правая
        };

        /**
         * Данная переменная представляет собой
         * как бы массив указателей на вершины,
         * составляющие рёбра куба
         * 
         * Этот массив будет использоваться
         * для заполнения Element Buffer Array
         * для модели куба
         */
        private readonly uint[] _cubeEdges =
        {
            0, 1,
            0, 2,
            0, 4,

            3, 2,
            3, 1,
            3, 7,

            5, 6,
            5, 4,
            5, 1,

            6, 7,
            6, 4,
            6, 2
        };

        //Хендлеры VBO моделей
        private int _vboPyramid;
        private int _vboCube;

        //Хендлеры VAO моделей
        private int _vaoPyramid;
        private int _vaoCube;

        //Хендлер EBO куба
        private int _eboCube;

        //Шейдеры моделей
        private Shader _pyramidShader;
        private Shader _cubeShader;

        //Камера
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

            //Очищение бекграунда сцены и заполнение его серым цветом
            GL.ClearColor(0.3f, 0.3f, 0.3f, 1.0f);

            /**
             * Включение данной функции
             * позволяет избежать наслоения
             * расположенных сзади полигонов
             * на расположенные спереди.
             * 
             * То есть фактически включается
             * глубина изображения.
             */
            GL.Enable(EnableCap.DepthTest);

            //Инициализация шейдеров
            _pyramidShader = new Shader("../../../Shaders/shader.vert", "../../../Shaders/shader.frag");
            _cubeShader = new Shader("../../../Shaders/cubeShader.vert", "../../../Shaders/shader.frag");

            {
                //Создаётся Vertex Buffer Object для пирамиды
                _vboPyramid = GL.GenBuffer();
                //Привязывается буфер
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vboPyramid);
                //В буфер загружаются вершины пирамиды
                GL.BufferData(BufferTarget.ArrayBuffer, _pyramidVertices.Length * sizeof(float), _pyramidVertices, BufferUsageHint.StaticDraw);

                //Создается и привязывается Vertex Array Object для пирамиды
                _vaoPyramid = GL.GenVertexArray();
                GL.BindVertexArray(_vaoPyramid);

                //Определяется интерпритация вершин пирамиды для буфера
                var vertexLocation = _pyramidShader.GetAttribLocation("aPosition");
                GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

                //Включается атрибут вершин пирамиды
                GL.EnableVertexAttribArray(vertexLocation);
            }

            {
                _vboCube = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vboCube);
                GL.BufferData(BufferTarget.ArrayBuffer, _cubeVertices.Length * sizeof(float), _cubeVertices, BufferUsageHint.StaticDraw);

                _vaoCube = GL.GenVertexArray();
                GL.BindVertexArray(_vaoCube);

                var vertexLocation = _cubeShader.GetAttribLocation("aPosition");
                GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

                //Создаётся Element Buffer Object, привязывается, и заполняется данными
                _eboCube = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, _eboCube);
                GL.BufferData(BufferTarget.ElementArrayBuffer, _cubeEdges.Length * sizeof(uint), _cubeEdges, BufferUsageHint.StaticDraw);
            }
            _camera = new Camera(Vector3.UnitZ * 3, Size.X / (float)Size.Y);

            CursorGrabbed = true;
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            _time += 4.0 * args.Time;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _pyramidShader.Use();

            var model = Matrix4.Identity * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(1));//_time));
            _pyramidShader.SetMatrix4("model", model);
            _pyramidShader.SetMatrix4("view", _camera.GetViewMatrix());
            _pyramidShader.SetMatrix4("projection", _camera.GetProjectionMatrix());

            GL.BindVertexArray(_vaoPyramid);
            //GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
            GL.DrawArrays(PrimitiveType.Triangles, 0, _pyramidVertices.Length / 3);

            _cubeShader.Use();
            GL.BindVertexArray(_vaoCube);
            //GL.LineWidth(2.0f);
            GL.DrawElements(PrimitiveType.Lines, _cubeEdges.Length, DrawElementsType.UnsignedInt, 0);
            
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

            int[] buffers = { _vboPyramid, _vaoPyramid };

            GL.DeleteBuffers(2, buffers);

            GL.DeleteProgram(_pyramidShader.Handle);

            _pyramidShader.Dispose();

            base.OnUnload();
        }
    }
}