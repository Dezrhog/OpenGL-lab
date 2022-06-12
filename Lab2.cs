using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;

/**
 * Created by AlekseevDA on 10.06.2022
 * Класс, предназначенный для инициализации и запуска
 * окна приложения
 */
namespace ComputerGraphics
{
    public static class Lab2
    {
        private static void Main()
        {
            var nativeWindowSettings = new NativeWindowSettings()
            {
                Size = new Vector2i(800, 600),
                Title = "Lab2 - OpenGL"
            };

            using (var nativeWindow = new Window(GameWindowSettings.Default, nativeWindowSettings))
            {
                nativeWindow.Run();
            }
        }
    }
}
