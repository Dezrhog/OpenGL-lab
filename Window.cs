using System;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

/**
 * Created by AlekseevDA on 10.06.2022
 * Класс, выполняющий работу по отрисовке сцены
 */
namespace ComputerGraphics
{
    public class Window : GameWindow
    {
        //Вершины пирамиды
        private readonly float[] _pyramidVertices =
        {
        //    X      Y      Z
             0.0f,  0.5f,  0.0f,
            -0.5f, -0.5f,  0.5f, // Передняя сторона
             0.5f, -0.5f,  0.5f, 

             0.0f,  0.5f,  0.0f,
            -0.5f, -0.5f,  0.5f, // Левая сторона
            -0.5f, -0.5f, -0.5f,

             0.0f,  0.5f,  0.0f,
            -0.5f, -0.5f, -0.5f, // Задняя сторона
             0.5f, -0.5f, -0.5f,

             0.0f,  0.5f,  0.0f,
             0.5f, -0.5f, -0.5f, // Правая сторона
             0.5f, -0.5f,  0.5f,

        //       Основание
            -0.5f, -0.5f,  0.5f,
             0.5f, -0.5f,  0.5f, // Первый полигон основания
            -0.5f, -0.5f, -0.5f,
             0.5f, -0.5f, -0.5f,
            -0.5f, -0.5f, -0.5f, // Второй полигон основания
             0.5f, -0.5f,  0.5f
        };

        //Цвета вершин пирамиды
        private readonly float[] _pyramidColors =
        {
             0.5f,  0.7f,  0.4f,
             0.5f,  0.7f,  0.4f,
             0.5f,  0.7f,  0.4f,

             0.8f,  0.5f,  0.2f,
             0.8f,  0.5f,  0.2f,
             0.8f,  0.5f,  0.2f,

             0.9f,  0.7f,  0.7f,
             0.9f,  0.7f,  0.7f,
             0.9f,  0.7f,  0.7f,

             0.4f,  0.5f,  0.8f,
             0.4f,  0.5f,  0.8f,
             0.4f,  0.5f,  0.8f,

             0.1f,  0.2f,  0.8f,
             0.1f,  0.2f,  0.8f,
             0.1f,  0.2f,  0.8f,
             0.1f,  0.2f,  0.8f,
             0.1f,  0.2f,  0.8f,
             0.1f,  0.2f,  0.8f
        };

        //Вершины куба
        private readonly float[] _cubeVertices =
        {
        //  Вершины нижней грани
            -0.5f, -0.5f,  0.5f, // [0] Передняя левая
             0.5f, -0.5f,  0.5f, // [1] Передняя правая
            -0.5f, -0.5f, -0.5f, // [2] Задняя левая
             0.5f, -0.5f, -0.5f, // [3] Задняя правая

        //  Вершины верхней грани
            -0.5f,  0.5f,  0.5f, // [4] Передняя левая
             0.5f,  0.5f,  0.5f, // [5] Передняя правая
            -0.5f,  0.5f, -0.5f, // [6] Задняя левая
             0.5f,  0.5f, -0.5f  // [7] Задняя правая
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

            5, 4,
            5, 1,
            5, 7,

            6, 7,
            6, 4,
            6, 2
        };

        //Хендлеры VBO моделей
        private int _vboPyramid;
        private int _vboPyramidColor;
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
            _cubeShader = new Shader("../../../Shaders/cubeShader.vert", "../../../Shaders/cubeShader.frag");

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

                //Получаем адрес атрибута позиции шейдера
                var vertexLocation = _pyramidShader.GetAttribLocation("aPosition");
                //Определяется интерпритация буфера вершин пирамиды
                GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
                //Включается атрибут вершин пирамиды
                GL.EnableVertexAttribArray(vertexLocation);

                //Создаётся, привязывается и заполняется VBO для цветов вершин пирамиды
                _vboPyramidColor = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vboPyramidColor);
                GL.BufferData(BufferTarget.ArrayBuffer, _pyramidColors.Length * sizeof(float), _pyramidColors, BufferUsageHint.StaticDraw);

                //Определяется интерпритация буфера цветов вершин пирамиды
                var vertexColor = _pyramidShader.GetAttribLocation("vertexColor");
                GL.VertexAttribPointer(vertexColor, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
                GL.EnableVertexAttribArray(vertexColor);
            }

            //Тот же процесс проделывается для модели куба, за исключением цветов вершин
            {
                _vboCube = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vboCube);
                GL.BufferData(BufferTarget.ArrayBuffer, _cubeVertices.Length * sizeof(float), _cubeVertices, BufferUsageHint.StaticDraw);

                _vaoCube = GL.GenVertexArray();
                GL.BindVertexArray(_vaoCube);

                var vertexLocation = _cubeShader.GetAttribLocation("aPosition");
                GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
                GL.EnableVertexAttribArray(vertexLocation);
              
                //Создаётся Element Buffer Object, привязывается, и заполняется данными
                _eboCube = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, _eboCube);
                GL.BufferData(BufferTarget.ElementArrayBuffer, _cubeEdges.Length * sizeof(uint), _cubeEdges, BufferUsageHint.StaticDraw);
            }
            //Инициализируется камера
            _camera = new Camera(Vector3.UnitZ * 3, Size.X / (float)Size.Y);

            //Данный параметр привязывает курсор к окну приложения при запуске
            CursorGrabbed = true;
        }

        //Данный метод выпоплняется при кадом рендере нового кадра
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            //Значение времени умножается на 40, для более быстрого вращения
            _time += 40.0 * args.Time;

            //Очищаются буферы цвета и глубины
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            //Включается шейдер пирамиды
            _pyramidShader.Use();

            {
                /*
                 * Заполняются униформы вертексного шейдера ппирамиды.
                 * Это необходимо для перемещения модели относительно позиции камеры.
                 */
                var model = Matrix4.Identity * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(_time));
                _pyramidShader.SetMatrix4("model", model);
                _pyramidShader.SetMatrix4("view", _camera.GetViewMatrix());
                _pyramidShader.SetMatrix4("projection", _camera.GetProjectionMatrix());

                //Привязывается VAO пирамиды
                GL.BindVertexArray(_vaoPyramid);
                //Рисуются треугольники по координатам вершин
                GL.DrawArrays(PrimitiveType.Triangles, 0, _pyramidVertices.Length / 3);
            }

            //Далее тот же процесс для куба
            _cubeShader.Use();

            {
                var model = Matrix4.Identity * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(_time));
                _cubeShader.SetMatrix4("model", model);
                _cubeShader.SetMatrix4("view", _camera.GetViewMatrix());
                _cubeShader.SetMatrix4("projection", _camera.GetProjectionMatrix());
                GL.BindVertexArray(_vaoCube);
                //Следующей строчкой задаётся толщина линий
                GL.LineWidth(2.0f);
                GL.DrawElements(PrimitiveType.Lines, _cubeEdges.Length, DrawElementsType.UnsignedInt, 0);
            }

            //Меняются местами буфер текущего кадра и буфер следующего
            SwapBuffers();

            base.OnRenderFrame(args);
        }

        //Данный метод выполняется в момент обновления кадра
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            /**
             * Проверяется в фокусе ли окно.
             * Если нет - данный метод перестаёт выполняться.
             * Это необходимо для того, чтобы нажатия на клавиатуру не считывались,
             * пока окно приложения не активно
             */
            if (!IsFocused)
            {
                return;
            }

            //Сохраняется текущее состояние клавиатуры
            var input = KeyboardState;

            //При нажатии Escape - закрытие окна
            if (input.IsKeyDown(Keys.Escape))
            {
                Close();
            }

            //Задаются константы скорости и чувствительности камеры
            const float cameraSpeed = 1.5f;
            const float sensitivity = 0.2f;

            //Далее задаются клавишы для управления камерой и изменяется позиция камеры
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

            //Сохраняется состояние мыши
            var mouse = MouseState;

            /**
             * На первом кадре сохраняется
             * текущая позиция мыши.
             * 
             * Затем на каждом кадре считается дельта изменения координат мыши
             * и изменяется pitch(горизонтальный наклон) и yaw(вертикальный наклон) камеры
             */
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

        //При вращении колёсика мыши изменяется угол зрения камеры
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            _camera.Fov -= e.OffsetY;
        }

        //Метод регулирует поведение окна при изменении его размера
        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, Size.X, Size.Y);
            _camera.AspectRatio = Size.X / (float)Size.Y;
        }

        //Данный метод вызывается при закрытии окна и очищает все буферы и шейдеры
        protected override void OnUnload()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            int[] buffers = { _vboPyramid, _vaoPyramid, _vboPyramidColor, _vboCube, _vaoCube };

            GL.DeleteBuffers(5, buffers);

            GL.DeleteProgram(_pyramidShader.Handle);
            GL.DeleteProgram(_cubeShader.Handle);

            _pyramidShader.Dispose();
            _cubeShader.Dispose();

            base.OnUnload();
        }
    }
}