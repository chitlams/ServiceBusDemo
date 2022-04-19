using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using SBShared.Models;

namespace SBReceiver
{
    class Program
    {
        const string connectionString = "Endpoint=sb://sudhakarchitlamservicebus.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SqtOrdnZ8rl+5XSw4WeXmpA/GiqxmRZXkEiL7tIkZMY=";
        const string queueName = "personqueue";
        static IQueueClient queueClient;

        static async Task Main(string[] args)
        {
            queueClient = new QueueClient(connectionString, queueName);

            var messageHandlerOptions = new MessageHandlerOptions(exceptionReceivedHandler)
            {
                MaxConcurrentCalls = 1,
                AutoComplete = false // Mark messages as not complete. As long as it reads then it completes.

            };

            queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions); //message handler is the method that accepts messages when new messages comes in,

            Console.ReadLine();

            await queueClient.CloseAsync();

            //Here output will be like all the messages will display in console from azure service bus that we have created from SBSender and then immedialy it deletes.
            //Here we can set both SBReciver and SBSender as startup projects and run both(we can set this in settings). Two windows will open.
            // In one Sender window we type and submit the message and in other Receiver window message will be recieved. (Ex: Person Received : Sudhakar Chitlam)
        }

        private static async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            var jsonString = Encoding.UTF8.GetString(message.Body);
            PersonModel person = JsonSerializer.Deserialize<PersonModel>(jsonString);
            Console.WriteLine($"Person Received:{person.FirstName} {person.LastName}");

            await queueClient.CompleteAsync(message.SystemProperties.LockToken); //item has lock token, its gonna take that remove that message to complete. once its gone and its gone.
        }

        private static Task exceptionReceivedHandler(ExceptionReceivedEventArgs arg)
        {
            Console.WriteLine($"Message handler exception: {arg.Exception }");
            return Task.CompletedTask;
        }
    }
}
