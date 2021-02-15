﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    /// <summary>
    /// This class will add a new event listener and capture events from the Microsoft.IdentityModel.EventSource module.
    /// Using the the event tracing for Windows (ETW) system. 
    /// 
    /// An event listener represents the target for all events generated by an event source (EventSource object). 
    /// When a new event listener is created and added, it is logically attached to all event sources in that application domain.
    /// 
    /// References
    /// * System.Diagnostics.Tracing Namespace
    ///   https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.tracing?view=net-5.0
    /// * Create documentation that shows how to collect log messages
    ///   https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/issues/1452
    /// * Microsoft.IdentityModel Wiki
    ///   https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/wiki
    ///   
    /// Written by Tore Nestenius for the IdentityServer in production training class 
    /// https://www.edument.se/en/product/identityserver-in-production
    /// </summary>
    public class IdentityModelEventListener : EventListener
    {
        /// <summary>
        /// Occurs when an event source (EventSource object) is attached to the dispatcher.
        /// </summary>
        /// <param name="eventSource"></param>
        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            base.OnEventSourceCreated(eventSource);

            Console.WriteLine("EventSource found: " + eventSource.Name);

            //Only log events from this source
            if (eventSource.Name == "Microsoft.IdentityModel.EventSource")
            {
                Console.WriteLine("Listener attached to the source " + eventSource.Name);
                EnableEvents(eventSource, System.Diagnostics.Tracing.EventLevel.Verbose, EventKeywords.All);
            }
        }

        /// <summary>
        /// Occurs when an event has been written by an event source (EventSource object) for which the event listener has enabled events.
        /// </summary>
        /// <param name="eventData"></param>
        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            if (eventData != null && eventData.Payload != null && eventData.Payload.Count > 0)
            {
                //Map .NET EventLevel to Serilog logging levels
                Serilog.Events.LogEventLevel level = Serilog.Events.LogEventLevel.Information;
                switch (eventData!.Level)
                {
                    case EventLevel.Critical:
                        level = Serilog.Events.LogEventLevel.Fatal;
                        break;
                    case EventLevel.Error:
                        level = Serilog.Events.LogEventLevel.Error;
                        break;
                    case EventLevel.Informational:
                        level = Serilog.Events.LogEventLevel.Information;
                        break;
                    case EventLevel.Warning:
                        level = Serilog.Events.LogEventLevel.Warning;
                        break;
                    case EventLevel.Verbose:
                        level = Serilog.Events.LogEventLevel.Verbose;
                        break;
                }

                //Send the event to the console 
                Console.WriteLine("#### Event:" + eventData?.Payload[0]?.ToString() ?? "Unknown");

                //Debugger.Break();  //For debugging purposes

                //Send the event to the log system
                Serilog.Log.Logger
                        .ForContext("SourceContext", "Microsoft.IdentityModel.EventSource")
                        .ForContext("Message", eventData?.Payload[0]?.ToString() ?? "Unknown")
                        .Write(level, "ETW-event: from " + "Microsoft.IdentityModel.EventSource");
            }
        }
    }

}
