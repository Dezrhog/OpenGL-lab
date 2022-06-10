using OpenTK.Mathematics;

namespace ComputerGraphics
{
    public class Camera
    {
        //Векторы направления камеры
        private Vector3 _front = -Vector3.UnitZ;
        private Vector3 _up = Vector3.UnitY;
        private Vector3 _right = Vector3.UnitX;

        //Вращение вокруг оси Х (радианы)
        private float _pitch;
        //Вращение вокруг оси Y (радианы)
        private float _yaw = -MathHelper.PiOver2;
        //Поле зрения камера (радианы)
        private float _fov = MathHelper.PiOver2;

        public Camera(Vector3 position, float aspectRation)
        {
            Position = position;
            AspectRatio = aspectRation;
        }

        //Позиция камеры
        public Vector3 Position { get; set; }
        //Соотношение сторон viewport
        public float AspectRatio { get; set; }

        public Vector3 Front => _front;
        public Vector3 Up => _up;
        public Vector3 Right => _right;

        /**
         * Эта переменная должна хранить значение в радианах.
         * Поэтому в сеттере переменной производится конвертация [градусы -> радианы]
         * А в геттере производится конвертация [радианы -> градусы]
         */
        public float Pitch
        {
            get => MathHelper.RadiansToDegrees(_pitch);
            set
            {
                /** 
                 * Значение угла фиксируется отношением [-90 < angle < 90]
                 * для предотвращения переворачивания камеры "вверх ногами"
                 * и прочих возможных проблем.
                 * p.s. хотя различные космические симуляторы с нулевой гравитацией, например,
                 *      допускают переворачивание камеры "вверх ногами", тут 
                 *      в этом нет необходимости
                 */
                var angle = MathHelper.Clamp(value, -89f, 89f);
                _pitch = MathHelper.DegreesToRadians(angle);
                UpdateVectors();
            }
        }

        public float Yaw
        {
            get => MathHelper.RadiansToDegrees(_yaw);
            set
            {
                _yaw = MathHelper.DegreesToRadians(value);
                UpdateVectors();
            }
        }

        /**
         * Поле зрения (Field of view -> FOV) это вертикальный угол зрения камеры.
         * С помощью этой переменной можно имитировать эффект зума камеры,
         * что можно использовать, например, для прицеливания в видеоиграх.
         */
        public float Fov
        {
            get => MathHelper.RadiansToDegrees(_fov);
            set
            {
                var angle = MathHelper.Clamp(value, 1f, 90f);
                _fov = MathHelper.DegreesToRadians(angle);
            }
        }

        //Метод возвращает матрицу обзора с помощью функции LookAt
        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(Position, Position + _front, _up);
        }

        //Возвращает проекцию матрицы используя метод, похожий на используемый выше
        public Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(_fov, AspectRatio, 0.01f, 100f);
        }

        //Этот метод обновляет направление векторов
        private void UpdateVectors()
        {
            _front.X = MathF.Cos(_pitch) * MathF.Cos(_yaw);
            _front.Y = MathF.Sin(_pitch);
            _front.Z = MathF.Cos(_pitch) * MathF.Sin(_yaw);

            _front = Vector3.Normalize(_front);

            _right = Vector3.Normalize(Vector3.Cross(_front, Vector3.UnitY));
            _up = Vector3.Normalize(Vector3.Cross(_right, _front));
        }
    }
}
