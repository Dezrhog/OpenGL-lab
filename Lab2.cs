using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;

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
