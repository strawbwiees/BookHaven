using System.IO;
using System.Text.Json;
using BookHaven.Forms;
using BookHaven.Services;
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

        var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
        if (File.Exists(configPath))
        {
            var json = File.ReadAllText(configPath);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.TryGetProperty("ConnectionStrings", out var cs) && cs.TryGetProperty("BookHaven", out var conn))
            {
                var connStr = conn.GetString();
                if (!string.IsNullOrEmpty(connStr))
                    Database.Configure(connStr);
            }
        }

        Application.Run(new LoginForm());
    }    
}
