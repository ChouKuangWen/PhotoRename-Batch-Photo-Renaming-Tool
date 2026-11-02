namespace PhotoRename.App;

static class Program
{
    [STAThread]
    static void Main()
    {
        try
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
        catch (Exception ex)
        {
            Console.WriteLine("=== Application Startup Exception ===");
            Console.WriteLine($"Exception Type: {ex.GetType().FullName}");
            Console.WriteLine($"Message: {ex.Message}");
            Console.WriteLine($"Stack Trace:\n{ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"\nInner Exception Type: {ex.InnerException.GetType().FullName}");
                Console.WriteLine($"Inner Message: {ex.InnerException.Message}");
                Console.WriteLine($"Inner Stack Trace:\n{ex.InnerException.StackTrace}");
            }
            
            // 尝试通过 MessageBox 显示
            try
            {
                MessageBox.Show(
                    $"Application startup failed!\n\n" +
                    $"Exception Type: {ex.GetType().FullName}\n" +
                    $"Message: {ex.Message}\n\n" +
                    $"Stack Trace:\n{ex.StackTrace}",
                    "Critical Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch
            {
                // 如果 MessageBox 也失败，忽略
            }
        }
    }
}
