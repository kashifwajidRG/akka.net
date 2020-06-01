using System;
using System.Threading;
using Akka.Actor;
using Akka.Configuration;
using Akka.Dispatch;
using RGLoggingAndTracing;

namespace RGLocalActorRefTest
{
    class Program
    {
        static void Main(string[] args)
        {
             // Config config =
             //     // enable clustering
             //     ConfigurationFactory.ParseString("akka.actor.provider=RGLoggingAndTracing.Actor.RGLocalActorRefProvider");
            
            var config = ConfigurationFactory.ParseString(@"
akka {  
    actor.provider = ""RGLoggingAndTracing.Actor.RGLocalActorRefProvider, RGLocalActorRefTest""
}
");
            
            using var system = ActorSystem.Create("RGLocalActorRefTestSystem", config);
            var actor = system.ActorOf(Props.Create( () => new MyActor()), "Actor1");
            actor.Tell(new Message("Kashif"));

            while (true)
            {
                Thread.Sleep(100);
            }
        }
    }


    class MyActor : ReceiveActor
    {
        public MyActor()
        {
            Receive<Message>(cr =>
            {
                Console.WriteLine("Receiving Message in MyActor : Name = {0}, Message =  {1}", Context.Self.Path.Name, cr.Name);
            });
        }
    }

    class Message
    {
        public string Name { get; set; }

        public Message(string name)
        {
            Name = name;
        }
    }
}