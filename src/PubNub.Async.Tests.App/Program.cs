using System;
using System.Threading.Tasks;
using Autofac;
using Newtonsoft.Json;
using PubNub.Async.Autofac;
using PubNub.Async.Extensions;
using PubNub.Async.Services.Subscribe;
using PubNub.Async.Tests.App.Properties;

namespace PubNub.Async.Tests.App
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Configuring PubNub.Async...");

            var authKey = Guid.NewGuid();
            Console.WriteLine($"Auth key: {authKey}");

            var builder = new ContainerBuilder();
            builder.RegisterModule<PubNubAsyncModule>();
            builder.Build();

            Console.WriteLine("Please stand by.");
            PubNub.Configure(c =>
            {
                c.PublishKey = Settings.Default.PamPublishKey;
                c.SubscribeKey = Settings.Default.PamSubKey;
                c.SecretKey = Settings.Default.PamSecKey;
                c.CipherKey = Settings.Default.CipherKey;
                c.AuthenticationKey = "console1";
            });
            
            Console.WriteLine($"Subscribing to {Settings.Default.Channel}1");
            var subscribeResult1 = $"{Settings.Default.Channel}1"
                .Secured()
                .Encrypted()
                .Subscribe<Message>(Handler1)
                .Result;

            Console.WriteLine($"Subscribing to {Settings.Default.Channel}2");
            var subscribeResult2 = $"{Settings.Default.Channel}2"
                .SecuredWith("console2")
                .Subscribe<Message>(Handler2)
                .Result;

            Console.WriteLine($"Subscribing redundant to {Settings.Default.Channel}2");
            var subscribeResult3 = $"{Settings.Default.Channel}2"
                .SecuredWith("console2")
                .Subscribe<Message>(Handler3)
                .Result;

            if (!subscribeResult1.Success || !subscribeResult2.Success || !subscribeResult3.Success)
            {
                Console.Error.WriteLine("Something went wrong");
                Console.WriteLine("Press any key to exit.");
                Console.Read();
                Environment.Exit(1);
            }

            Console.CancelKeyPress += ConsoleCancelHandler;
            Console.WriteLine("Success.  Send messages or input CTRL+C to cancel.");

            Task.Run(async () =>
            {
                while (true)
                {
                    var input = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(input))
                    {
                        var pubResponse1 = await $"{Settings.Default.Channel}1".Publish(new Message {Text = input});
                        if (!pubResponse1.Success)
                        {
                            Console.Error.WriteLine("Publish failed");
                            Console.Error.WriteLine(JsonConvert.SerializeObject(pubResponse1, Formatting.Indented));
                            Console.WriteLine("Press any key to exit.");
                            Console.Read();
                            Environment.Exit(1);
                        }
                        var pubResponse2 = await $"{Settings.Default.Channel}2".Publish(new Message { Text = input });
                        if (!pubResponse2.Success)
                        {
                            Console.Error.WriteLine("Publish failed");
                            Console.Error.WriteLine(JsonConvert.SerializeObject(pubResponse2, Formatting.Indented));
                            Console.WriteLine("Press any key to exit.");
                            Console.Read();
                            Environment.Exit(1);
                        }
                    }
                }
            }).Wait();
        }
        
        private static void ConsoleCancelHandler(object sender, ConsoleCancelEventArgs consoleCancelEventArgs)
        {
            Environment.Exit(0);
        }

        private static async Task Handler1(MessageReceivedEventArgs<Message> args)
        {
            var priorForeground = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkGreen;

            Console.WriteLine($"{args.Message.Text} [{args.Sent}]");

            Console.ForegroundColor = priorForeground;
        }

        private static async Task Handler2(MessageReceivedEventArgs<Message> args)
        {
            var priorForeground = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Blue;

            Console.WriteLine($"{args.Message.Text} [{args.Sent}]");

            Console.ForegroundColor = priorForeground;
        }

        private static async Task Handler3(MessageReceivedEventArgs<Message> args)
        {
            var priorForeground = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine($"{args.Message.Text} [{args.Sent}]");

            Console.ForegroundColor = priorForeground;
        }
    }

    class Message
    {
        public string Text { get; set; }
    }
}
