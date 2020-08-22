﻿using System;
using System.Collections.Generic;
using System.IO;
using Google.Cloud.TextToSpeech.V1;


namespace TextToSpeechTTV
{


    class Config
    {
        //Path for every Config
        private readonly string creds = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Config", "creds.txt");
        private readonly string options = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Config", "options.txt");
        private readonly string blocklist = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Config", "blocklist.txt");
        private readonly string badwords = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Config", "badwords.txt");
        private readonly string usernames = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Config", "usernames.txt");
        private readonly string new_usernames = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Config", "usernames.json");
        private readonly string voicelistfile = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Config", "voicelist.txt");
        private readonly string foldername = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Config");
        public List<string> voicelist;
        public TextToSpeechClient client;

        public Config()
        {
            CreateConfig();


            if (GetOAuth() == "oauth:youroauthkey")
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Your OAuth Key hasn't been set! Can't connect anywhere without setting it.");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("Press any key to quit...");
                Console.ReadKey();
                Environment.Exit(0);
            }
        }

        public object AuthExplicit(string jsonPath)
        {
            System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Config", "gcp.json"));
            try
            {
                client = TextToSpeechClient.Create();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to initialise GCP TTS Client. Set GCP to 'false' in options.txt");
                Console.WriteLine("Press any key to see the exception...");
                Console.ReadKey();
                Console.WriteLine(e);
                Console.WriteLine("Press any key to quit...");
                Console.ReadKey();
                Environment.Exit(0);
            }
            var response = client.ListVoices("");

            List<string> voices = new List<string>();

            foreach (var voice in response.Voices)
            {
                Console.WriteLine($"{voice.Name} ({voice.SsmlGender}); Rate:{voice.NaturalSampleRateHertz} Language codes: {string.Join(", ", voice.LanguageCodes)}");
                voices.Add(voice.Name);
            }
            voicelist = voices;

            File.WriteAllText(voicelistfile, string.Empty);
            File.WriteAllLines(voicelistfile, voices);
            return null;
        }

        private void CreateConfig()
        {


            if (Directory.Exists(foldername))
            {
                if (GetGCP() != "false")
                {
                    AuthExplicit(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Config", "gcp.json"));
                }
                return;
            }
                

            if (!Directory.Exists(foldername))
                Directory.CreateDirectory(foldername);
            if (!File.Exists(options))
                FillOptionsFile();
            if (!File.Exists(blocklist))
                FillBlocklistExamples();
            if (!File.Exists(badwords))
                File.Create(badwords);
            //if (!File.Exists(usernames))
            //    FillUsernamesExamples();
            if (!File.Exists(new_usernames))
                FillNewUsernamesExamples();
            if (!File.Exists(creds))
                FillCredsFile();

            Console.WriteLine("Created config File. Please set up your creds and settings!\nPress any key to quit...");
            Console.ReadKey();
            Environment.Exit(0);
            
        }

        /*private void FillUsernamesExamples()
        {
            File.WriteAllLines(usernames, new string[]
            {
                "drdisrespectlive=doctor disrespect",
                "loltyler1=tyler 1",
                "riot games=rito"
            });
        }*/

        private void FillNewUsernamesExamples()
        {
            File.WriteAllLines(new_usernames, new string[]
            {
            @"{
  ""users"": [
    {
      ""name"": ""youraccount"",
      ""nick"": ""you"",
      ""voice"": ""random""
    },
    {
      ""name"": ""myaccount"",
      ""nick"": ""me"",
      ""voice"": ""fr-CA-Wavenet-B""
    }
  ]
}"
            });
        }
        private void FillBlocklistExamples()
        {
            File.WriteAllLines(blocklist, new string[]
            {
                "nightbot",
                "moobot",
                "phantombot",
                "coebot",
                "deepbot"
            });
        }
        private void FillCredsFile()
        {
            File.WriteAllLines(creds, new string[] {
                "Twitch ID (lowercase):",
                "yourbotname",
                "OAUTH (Twitch TMI):",
                "oauth:youroauthkey",
                "Channel(lowercase):",
                "yourchannelname" });
        }

        private void FillOptionsFile()
        {
            File.WriteAllLines(options, new string[] {
                "Set TTS Voice:",
                "Microsoft David Desktop",
                "Set Message Connector:",
                "said",
                "Maximum allowed Characters, 0 for no limit:",
                "100",
                "Replace swear word with:",
                "beep",
                "Say this, if long Sentence:",
                "to be continued",
                "GCP TTS? (true, wavenet, standard, false)",
                "False",
                "Default GCP Voice: (Select from voicelist.txt, random, random-per-user)",
                "Random"
            });
        }

        public string GetUsername()
        {
            string id = File.ReadAllLines(creds)[1];
            return id;
        }

        public string GetOAuth()
        {
            string password = File.ReadAllLines(creds)[3];
            return password;
        }

        public string GetChannel()
        {
            string channel = File.ReadAllLines(creds)[5];
            return channel;
        }

        public string SetVoice()
        {
            string voice = File.ReadAllLines(options)[1];
            return voice;
        }
        public string SetMessageConnector()
        {
            string say = File.ReadAllLines(options)[3];
            return say;
        }

        public int GetMaxCharacterLength()
        {
            string wordLength = File.ReadAllLines(options)[5];
            int.TryParse(wordLength, out int result);
            return result;
        }

        public string ReplaceSwearWord()
        {
            string antiswear = File.ReadAllLines(options)[7];
            return antiswear;
        }

        public string GetLongMessage()
        {
            string longMessage = File.ReadAllLines(options)[9];
            return longMessage;
        }
        public string GetGCP()
        {
            string gcp = File.ReadAllLines(options)[11].ToLower();
            if (gcp == "true")
            {
                return "true";
            }
            else if (gcp == "wavenet")
            {
                return "wavenet";
            }
            else if (gcp == "standard")
            {
                return "standard";
            }
            else
            {
                return "false";
            }

        }
        public TextToSpeechClient GetGCPClient()
        {
            return client;
        }


    }



}
