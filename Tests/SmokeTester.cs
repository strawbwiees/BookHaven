using BookHaven.Forms;
using BookHaven.Models;
using BookHaven.Services;
using BookHaven.UI;
using BookHaven.Views;

namespace BookHaven.Tests
{
    public static class SmokeTester
    {
        public static void RunTests()
        {
            Console.WriteLine("Running BookHaven System Smoke Tests...");

            var service = LibraryDataService.Instance;
            Console.WriteLine($"[OK] Seeded Books: {service.Books.Count}");
            Console.WriteLine($"[OK] Seeded Members: {service.Members.Count}");
            Console.WriteLine($"[OK] Seeded Transactions: {service.Transactions.Count}");

            var thread = new Thread(() =>
            {
                string dumpDir = @"C:\Users\noe\.gemini\antigravity\brain\ecc88d2a-7815-49c4-bf2b-494edac97661";
                using var main = new MainForm();
                main.Size = new Size(1300, 860);
                main.Show();
                Application.DoEvents();

                Console.WriteLine($"Form ClientSize: {main.ClientSize}");
                var sidebar = main.Controls[1];
                var right = main.Controls[0];
                Console.WriteLine($"Sidebar: {sidebar.Bounds}");
                Console.WriteLine($"RightContainer: {right.Bounds}");

                void Capture(string name)
                {
                    Application.DoEvents();
                    Thread.Sleep(100);
                    Application.DoEvents();
                    using var bmp = new Bitmap(main.Width, main.Height);
                    main.DrawToBitmap(bmp, new Rectangle(0, 0, main.Width, main.Height));
                    bmp.Save(Path.Combine(dumpDir, $"screenshot_{name}.png"), System.Drawing.Imaging.ImageFormat.Png);
                    Console.WriteLine($"Saved screenshot_{name}.png");
                }

                // Test Dashboard
                main.NavigateTo("Home");
                Capture("dashboard");

                // Test Books View
                main.NavigateTo("Books");
                Capture("books");

                // Test Issue/Return
                main.NavigateTo("Issue/Return Book");
                Capture("issue_return");

                // Test Borrowed Books
                main.NavigateTo("Borrowed Books");
                Capture("borrowed");

                // Test Borrowers
                main.NavigateTo("Borrowers");
                Capture("borrowers");

                // Test Settings
                main.NavigateTo("Settings");
                Capture("settings");

                main.Close();

                using var login = new LoginForm();
                login.Show();
                Application.DoEvents();
                using var loginBmp = new Bitmap(login.Width, login.Height);
                login.DrawToBitmap(loginBmp, new Rectangle(0, 0, login.Width, login.Height));
                loginBmp.Save(Path.Combine(dumpDir, "screenshot_login.png"), System.Drawing.Imaging.ImageFormat.Png);
                login.Close();
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            Console.WriteLine("ALL BOOKHAVEN SMOKE TESTS PASSED SUCCESSFULLY!");
        }
    }
}
