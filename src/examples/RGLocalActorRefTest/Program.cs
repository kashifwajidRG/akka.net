using System;
using System.Globalization;
using System.IO;
using System.Threading;
using Akka.Actor;
using Akka.Configuration;
using Akka.Dispatch;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Core.Sampling.Local;
using Microsoft.Extensions.Configuration;
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
            // //Starting the AWS Xray Recorder
             var (configAws, recorder) = Load();
             AWSXRayRecorder.InitializeInstance(configAws, recorder); // pass IConfiguration object that reads appsettings.json file
            // //--------
            
            
            using var system = ActorSystem.Create("RGLocalActorRefTestSystem", config);
            var Mainactor = system.ActorOf(Props.Create( () => new ControllerActor()), "MainActor");

            for (int i = 0; i < 2; i++)
            {
                var actor = system.ActorOf(Props.Create( () => new WorkerActor(Mainactor)), "Actor" + i.ToString());
                Mainactor.Tell(new ControlMessage("Created", actor.Path.Name));
                Thread.Sleep(100);
                actor.Tell(new Message("Message " + i.ToString()));
                Thread.Sleep(100);
            }

            while (true)
            {
                Thread.Sleep(100);
            }
        }
        
        public static (IConfiguration, AWSXRayRecorder) Load()
        {   
            Console.WriteLine(Directory.GetCurrentDirectory());
            Console.WriteLine(Environment.CurrentDirectory);
            string targetJsonFile = Path.Combine(Environment.CurrentDirectory, $"Config/appsettings.json");
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "Config"))
                .AddJsonFile(targetJsonFile)
                .Build();
            
            string samplingRuleJsonFile = Path.Combine(Environment.CurrentDirectory, $"Config/sampling-rules.json");
            
            var recorder = new AWSXRayRecorderBuilder().WithSamplingStrategy(new LocalizedSamplingStrategy(samplingRuleJsonFile)).Build();


            return (configuration, recorder);
        }
    }


    class WorkerActor : ReceiveActor
    {
        private IActorRef _mainActor;
        public WorkerActor(IActorRef mainActor)
        {
            _mainActor = mainActor;
            Receive<Message>(cr =>
            {
                Console.WriteLine("Receiving Message in WorkerActor : Name = {0}, Message =  {1}", Context.Self.Path.Name, cr.Name);
                _mainActor.Tell(new ControlMessage("Received", Self.Path.Name));
            });
        }
    }

    class ControllerActor : ReceiveActor
    {
        public ControllerActor()
        {
            Receive<ControlMessage>(cr =>
            {
                Console.WriteLine("Receiving ControlMessage in ControllerActor : MessageType = {0}, From =  {1}", cr.Messagetype, cr.ActorName);
            });
        }
    }

    class ControlMessage
    {
        public string Messagetype { get; set; }
        public string ActorName { get; set; }

        public ControlMessage(string messagetype, string actorName)
        {
            Messagetype = messagetype;
            ActorName = actorName;
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