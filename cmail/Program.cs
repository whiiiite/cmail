using System.Net.Mail;
using System.Net;
using System.Text.Json;

namespace cmail
{
    public class Config
    {
        public string? SenderEmail { get; set; }
        public string? SenderPassword { get; set; }
        public string? SmtpHost { get; set; }
        public int? SmtpPort { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }
        public bool? EnableSsl { get; set; }
        public bool? IsBodyHtml { get; set; }
        public List<string> SendTo { get; set; }
    }

    internal class Program
    {
        readonly static string configFile =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "cmail", "config.json");

        static void Main(string[] args)
        {
            if (args[0] == "help")
            {
                PrintHelp();
                return;
            }

            string dir = Path.GetDirectoryName(configFile);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (!File.Exists(configFile))
            {
                File.Create(configFile).Close();
                return;
            }

            Config? config = null;
            try
            {
                config = JsonSerializer.Deserialize<Config>(File.ReadAllText(configFile));
            }
            catch(Exception ex)
            {
                WriteLineYellow("Warning: Config file serialization failed. Start arguments research.");
            }

            config ??= new Config();


            if (!args.Contains("-h") && !args.Contains("--host"))
            {
                WriteLineRed("Error: There is no host. Use [-h <your email server host>]");
                WriteLineGreen("[!]Tip: write 'cmail help' for see help");
                return;
            }

            if (!args.Contains("-b") && !args.Contains("--body"))
            {
                WriteLineRed("Error: There is no body of email to send. Use [-b <your body>]");
                WriteLineGreen("[!]Tip: write 'cmail help' for see help");
                return;
            }

            if (!args.Contains("-s") && !args.Contains("--sender"))
            {
                WriteLineRed("Error: There is no sender of email. Use [-s <sender email>]");
                WriteLineGreen("[!]Tip: write 'cmail help' for see help");
                return;
            }

            if (!args.Contains("-t") && !args.Contains("--sendto"))
            {
                WriteLineRed(
                    "Error: There is no sendto of email. Use [-s <send to email like: email0,email1,email2>]");
                WriteLineGreen("[!]Tip: write 'cmail help' for see help");
                return;
            }

            try
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == "-h" || args[i] == "--host")
                    {
                        WriteLineGreen("Proccess: Read host");
                        if (args.Length > i + 1)
                        {
                            config.SmtpHost = args[i + 1];
                        } 
                        else
                        {
                            throw new ArgumentException("Value for host was not passed");
                        }
                    }

                    if (args[i] == "-p" || args[i] == "--port")
                    {
                        WriteLineGreen("Proccess: Read port");
                        if (args.Length > i + 1)
                        {
                            config.SmtpPort = int.Parse(args[i + 1]);
                        }
                        else
                        {
                            throw new ArgumentException("Value for port was not passed");
                        }
                    }

                    if (args[i] == "-s" || args[i] == "--sender")
                    {
                        WriteLineGreen("Proccess: Read sender");
                        if (args.Length > i + 1)
                        {
                            config.SenderEmail = args[i + 1];
                        }
                        else
                        {
                            throw new ArgumentException("Value for sender was not passed");
                        }
                    }

                    if (args[i] == "-w" || args[i] == "--password")
                    {
                        WriteLineGreen("Proccess: Read password");
                        if (args.Length > i + 1 && !args[i + 1].StartsWith("-"))
                        {
                            config.SenderPassword = args[i + 1];
                        }
                        else
                        {
                            throw new ArgumentException("Value for password was not passed");
                        }
                    }

                    if (args[i] == "-b" || args[i] == "--body")
                    {
                        WriteLineGreen("Proccess: Read body");
                        if (args.Length > i + 1)
                        {
                            config.Body = args[i + 1];
                        }
                        else
                        {
                            throw new ArgumentException("Value for body was not passed");
                        }
                    }

                    if (args[i] == "-u" || args[i] == "--subject")
                    {
                        WriteLineGreen("Proccess: Read subject");
                        if (args.Length > i + 1)
                        {
                            config.Subject = args[i + 1];
                        }
                        else
                        {
                            throw new ArgumentException("Value for subject was not passed");
                        }
                    }

                    if (args[i] == "-e" || args[i] == "--enablessl")
                    {
                        WriteLineGreen("Proccess: Read enablessl");
                        if (args.Length > i + 1)
                        {
                            config.EnableSsl = bool.Parse(args[i + 1]);
                        }
                        else
                        {
                            throw new ArgumentException("Value for enablessl was not passed");
                        }
                    }

                    if (args[i] == "-i" || args[i] == "--isbodyhtml")
                    {
                        WriteLineGreen("Proccess: Read isbodyhtml");
                        if (args.Length > i + 1)
                        {
                            config.IsBodyHtml = bool.Parse(args[i + 1]);
                        }
                        else
                        {
                            throw new ArgumentException("Value for isbodyhtml was not passed");
                        }
                    }

                    if (args[i] == "-t" || args[i] == "--sendto")
                    {
                        WriteLineGreen("Proccess: Read sendto");
                        if (args.Length > i + 1)
                        {
                            config.SendTo = new List<string>();
                            foreach (var email in args[i + 1].Split(','))
                            {
                                config.SendTo.Add(email.Trim());
                            }
                        }
                        else
                        {
                            throw new ArgumentException("Value for sendto was not passed");
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                WriteLineRed("Error: " + ex.Message);
                WriteLineGreen("[!]Tip: write 'cmail help' for see help");
                return;
            }

            var smtpServer = config.SmtpHost;
            var smtpPort = config.SmtpPort;
            var senderEmail = config.SenderEmail;
            var senderPassword = config.SenderPassword;
            var sendTo = config.SendTo;
            var subject = config.Subject;
            var body = config.Body;
            var isBodyHTML = config.IsBodyHtml;
            var enableSsl = config.EnableSsl;

            var mail = new MailMessage();
            mail.From = new MailAddress(senderEmail);
            foreach (var item in sendTo)
            {
                mail.To.Add(item);
            }
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = isBodyHTML ?? false;

            System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient
            {
                Host = smtpServer,
                Port = smtpPort ?? 587,
                EnableSsl = enableSsl ?? false,
                UseDefaultCredentials = false,
                DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(senderEmail, senderPassword)
            };

            Console.WriteLine("Data: ");
            Console.WriteLine(smtpServer);
            Console.WriteLine(smtpPort);
            Console.WriteLine(senderEmail);
            Console.WriteLine(subject);
            Console.WriteLine(body);
            Console.WriteLine(isBodyHTML);
            Console.WriteLine(senderPassword);
            Console.WriteLine(enableSsl);
            Console.WriteLine(sendTo[0]);

            try
            {
                client.Send(mail);
                WriteLineGreen("\n--Mail was sent successfuly");
            }
            catch (Exception ex)
            {
                WriteLineRed($"Error while sending message: {ex.Message}");
            }
            finally
            {
                mail.Dispose();
                client.Dispose();
            }
        }


        private static void WriteLineRed(string line)
        {
            ConsoleColor prev = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(line);
            Console.ForegroundColor = prev;
        }

        private static void WriteLineYellow(string line)
        {
            ConsoleColor prev = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(line);
            Console.ForegroundColor = prev;
        }

        private static void WriteLineGreen(string line)
        {
            ConsoleColor prev = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(line);
            Console.ForegroundColor = prev;
        }


        private static void PrintHelp()
        {
            Console.WriteLine("-h <host name of smtp server> - host of email service");
            Console.WriteLine("-p <port of smtp server> - pprt of email server. integer");
            Console.WriteLine("-s <sender address> - email address of sender of email");
            Console.WriteLine("-w <password of sender> - password of sender of email");
            Console.WriteLine("-b <email body> - body of email");
            Console.WriteLine("-u <subject of email> - email's subject");
            Console.WriteLine("-e <true or false> - enable ssl or not");
            Console.WriteLine("-i <true or false> - is body is html format or not");
            Console.WriteLine(@"-t <email0,email1,email2...> or <email> - 
send to option take single email or list separated by comma ',' (recommended without spaces between)");
        }
    }
}

// vial actj xilw wqff