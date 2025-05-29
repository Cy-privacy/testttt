using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace LunarAimbot
{
    public class Program
    {
        private const int SCREEN_WIDTH = Screen.PrimaryScreen.Bounds.Width;
        private const int SCREEN_HEIGHT = Screen.PrimaryScreen.Bounds.Height;
        private const int SCREEN_X = SCREEN_WIDTH / 2;
        private const int SCREEN_Y = SCREEN_HEIGHT / 2;
        private const int AIM_HEIGHT = 10;
        private const int FOV = 350;
        private const float CONFIDENCE = 0.45f;
        private const bool USE_TRIGGER_BOT = true;

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, UIntPtr dwExtraInfo);

        private static bool aimbotEnabled = true;

        public static void Main(string[] args)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(@"
  _    _   _ _   _    _    ____     _     ___ _____ _____ 
 | |  | | | | \ | |  / \  |  _ \   | |   |_ _|_   _| ____|
 | |  | | | |  \| | / _ \ | |_) |  | |    | |  | | |  _|  
 | |__| |_| | |\  |/ ___ \|  _ <   | |___ | |  | | | |___ 
 |_____\___/|_| \_/_/   \_\_| \_\  |_____|___| |_| |_____|
                                                                         
(Neural Network Aimbot)");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("To get full version of Lunar V2, visit https://gannonr.com/lunar OR join the discord: discord.gg/aiaimbot");
            Console.ResetColor();

            SetupConfiguration();
            StartAimbot();
        }

        private static void SetupConfiguration()
        {
            if (!System.IO.File.Exists("config.json"))
            {
                Console.WriteLine("[INFO] In-game X and Y axis sensitivity should be the same");
                float xySens = PromptFloat("X-Axis and Y-Axis Sensitivity (from in-game settings): ");
                float targetingSens = PromptFloat("Targeting Sensitivity (from in-game settings): ");

                var config = new
                {
                    xy_sens = xySens,
                    targeting_sens = targetingSens,
                    xy_scale = 10 / xySens,
                    targeting_scale = 1000 / (targetingSens * xySens)
                };

                string jsonConfig = System.Text.Json.JsonSerializer.Serialize(config);
                System.IO.File.WriteAllText("config.json", jsonConfig);
                Console.WriteLine("[INFO] Sensitivity configuration complete");
            }
        }

        private static float PromptFloat(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                if (float.TryParse(Console.ReadLine(), out float result))
                {
                    return result;
                }
                Console.WriteLine("[!] Invalid Input. Make sure to enter only the number (e.g. 6.9)");
            }
        }

        private static void StartAimbot()
        {
            Console.WriteLine("[INFO] Press F1 to toggle aimbot");
            Console.WriteLine("[INFO] Press F2 to quit");

            // Start keyboard listener
            var keyboardThread = new Thread(() =>
            {
                while (true)
                {
                    if (GetAsyncKeyState(Keys.F1))
                    {
                        aimbotEnabled = !aimbotEnabled;
                        Console.WriteLine($"[!] AIMBOT IS [{(aimbotEnabled ? "ENABLED" : "DISABLED")}]");
                        Thread.Sleep(200);
                    }
                    else if (GetAsyncKeyState(Keys.F2))
                    {
                        Environment.Exit(0);
                    }
                    Thread.Sleep(10);
                }
            });
            keyboardThread.Start();

            // Main aimbot loop
            while (true)
            {
                if (aimbotEnabled)
                {
                    ProcessFrame();
                }
                Thread.Sleep(1);
            }
        }

        private static void ProcessFrame()
        {
            // Capture screen region
            Rectangle captureRegion = new Rectangle(
                SCREEN_X - FOV / 2,
                SCREEN_Y - FOV / 2,
                FOV,
                FOV
            );

            using (Bitmap screenshot = new Bitmap(captureRegion.Width, captureRegion.Height))
            using (Graphics g = Graphics.FromImage(screenshot))
            {
                g.CopyFromScreen(captureRegion.Left, captureRegion.Top, 0, 0, captureRegion.Size);
                
                // Here we would implement the YOLO detection
                // For demonstration, we'll just simulate target detection
                Point target = DetectTarget(screenshot);
                
                if (target.X != -1 && target.Y != -1)
                {
                    MoveMouseToTarget(target.X + captureRegion.Left, target.Y + captureRegion.Top);
                }
            }
        }

        private static Point DetectTarget(Bitmap screenshot)
        {
            // Placeholder for YOLO detection
            // In real implementation, this would use ML.NET or other AI framework
            return new Point(-1, -1);
        }

        private static void MoveMouseToTarget(int targetX, int targetY)
        {
            const uint MOUSEEVENTF_MOVE = 0x0001;
            
            int deltaX = targetX - SCREEN_X;
            int deltaY = targetY - SCREEN_Y;
            
            mouse_event(MOUSEEVENTF_MOVE, deltaX, deltaY, 0, UIntPtr.Zero);
        }

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(Keys vKey);
    }
}