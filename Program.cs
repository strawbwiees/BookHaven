using BookHaven.Forms;
using BookHaven.Tests;

namespace BookHaven;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "--test")
        {
            SmokeTester.RunTests();
            return;
        }

        ApplicationConfiguration.Initialize();
        Application.Run(new LoginForm());
    }    
}
