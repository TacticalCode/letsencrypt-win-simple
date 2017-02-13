using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Serilog;

namespace LetsEncrypt.ACME.Simple
{
    public class ManualPlugin : Plugin
    {
        public override string Name => "Manual";

        public override List<Target> GetTargets()
        {
            var result = new List<Target>();

            return result;
        }

        public override List<Target> GetSites()
        {
            var result = new List<Target>();

            return result;
        }

        public override void Install(Target target, string pfxFilename, X509Store store, X509Certificate2 certificate)
        {
            RunScript(false);
        }

        public override void Install(Target target)
        {
            // This method with just the Target paramater is currently only used by Centralized SSL
            RunScript(true);
        }

        public override void Renew(Target target)
        {
            Auto(target);
        }

        public override void PrintMenu()
        {
            if (!String.IsNullOrEmpty(Program.Options.ManualHost))
            {
                var target = new Target()
                {
                    Host = Program.Options.ManualHost,
                    WebRootPath = Program.Options.WebRoot,
                    PluginName = Name
                };
                Auto(target);
                Environment.Exit(0);
            }

            Console.WriteLine(" M: Generate a certificate manually.");
        }

        public override void HandleMenuResponse(string response, List<Target> targets)
        {
            if (response == "m")
            {
                Console.Write("Enter a host name: ");
                var hostName = Console.ReadLine();
                string[] alternativeNames = null;
                List<string> sanList = null;

                if (Program.Options.San)
                {
                    Console.Write("Enter all Alternative Names seperated by a comma ");

                    // Copied from http://stackoverflow.com/a/16638000
                    int BufferSize = 16384;
                    Stream inputStream = Console.OpenStandardInput(BufferSize);
                    Console.SetIn(new StreamReader(inputStream, Console.InputEncoding, false, BufferSize));

                    var sanInput = Console.ReadLine();
                    alternativeNames = sanInput.Split(',');
                    sanList = new List<string>(alternativeNames);
                }

                Console.Write("Enter a site path (the web root of the host for http authentication): ");
                var physicalPath = Console.ReadLine();

                if (sanList == null || sanList.Count <= 100)
                {
                    var target = new Target()
                    {
                        Host = hostName,
                        WebRootPath = physicalPath,
                        PluginName = Name,
                        AlternativeNames = sanList
                    };
                    Auto(target);
                }
                else
                {
                    Console.WriteLine(
                        $" You entered too many hosts for a SAN certificate. Let's Encrypt currently has a maximum of 100 alternative names per certificate.");
                    Log.Error(
                        "You entered too many hosts for a San certificate. Let's Encrypt currently has a maximum of 100 alternative names per certificate.");
                }
            }
        }

        public override void CreateAuthorizationFile(string answerPath, string fileContents)
        {
            Console.WriteLine($" Writing challenge answer to {answerPath}");
            Log.Information("Writing challenge answer to {answerPath}", answerPath);
            var directory = Path.GetDirectoryName(answerPath);
            Directory.CreateDirectory(directory);
            File.WriteAllText(answerPath, fileContents);
        }
    }
}
