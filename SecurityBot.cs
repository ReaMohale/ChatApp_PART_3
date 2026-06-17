using System;
using System.Media;
using System.Threading;

namespace ChatApp_Part3
{
    public class SecurityBot
    {
        private string agentName = string.Empty;

        public string AgentName
        {
            get => agentName;
            set => agentName = value;
        }

        public event Action<string>? OnBotOutput;

        public void Launch()
        {
            Console.Title = "🛡 SecureShield Awareness System";
            PrintBanner();
            PlayStartupAudio();
            CollectAgentName();
            GreetAgent();
        }

        public void PrintBanner()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("*=================================================*");
            Console.WriteLine("*======= CYBERSECURITY AWARENESS SYSTEM ==========*");
            Console.WriteLine("*=================================================*");
            Console.WriteLine(@"
 ____                           ____  _     _      _     _ 
/ ___|  ___  ___ _   _ _ __ ___/ ___|| |__ (_) ___| | __| |
\___ \ / _ \/ __| | | | '__/ _ \___ \| '_ \| |/ _ \ |/ _` |
 ___) |  __/ (__| |_| | | |  __/___) | | | | |  __/ | (_| |
|____/ \___|\___|\__,_|_|  \___|____/|_| |_|_|\___|_|\__,_|   
        'Protecting you in the Digital Age'  
");
            Console.ResetColor();
        }

        public void PlayStartupAudio()
        {
            try
            {
                string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "welcome.wav");
                if (System.IO.File.Exists(path))
                {
                    using SoundPlayer player = new SoundPlayer(path);
                    player.Load();
                    player.PlaySync();
                }
            }
            catch { }
        }

        public void CollectAgentName()
        {
            Console.Write("\nEnter your agent name: ");
            agentName = Console.ReadLine() ?? string.Empty;

            while (string.IsNullOrWhiteSpace(agentName))
            {
                Console.Write("Agent name cannot be blank. Try again: ");
                agentName = Console.ReadLine() ?? string.Empty;
            }
        }

        public void GreetAgent()
        {
            StreamText($"\nAgent {agentName}, SecureShield is online and ready.");
            StreamText("Your digital safety is our mission.\n");
        }

        public void StreamText(string message)
        {
            foreach (char ch in message)
            {
                Console.Write(ch);
                Thread.Sleep(12);
            }
            Console.WriteLine();
        }
    }
}