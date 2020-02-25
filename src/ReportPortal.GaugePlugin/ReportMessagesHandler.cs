﻿using Gauge.Messages;
using Grpc.Core;
using ReportPortal.GaugePlugin.Results;
using ReportPortal.Shared.Internal.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ReportPortal.GaugePlugin
{
    class ReportMessagesHandler : Reporter.ReporterBase
    {
        private static ITraceLogger TraceLogger = TraceLogManager.GetLogger<ReportMessagesHandler>();

        private Server _server;

        private Sender _sender;

        public ReportMessagesHandler(Server server, Sender sender)
        {
            _server = server;

            _sender = sender;
        }

        public override Task<Empty> NotifyExecutionStarting(ExecutionStartingRequest request, ServerCallContext context)
        {
            try
            {
                TraceLogger.Info($"{nameof(NotifyExecutionStarting)} received");
                TraceLogger.Verbose(Newtonsoft.Json.JsonConvert.SerializeObject(request));

                if (request.SuiteResult != null)
                {
                    _sender.StartLaunch(request);
                }
            }
            catch (Exception exp)
            {
                TraceLogger.Error(exp.ToString());
            }

            return Task.FromResult(new Empty());
        }

        public override Task<Empty> NotifyExecutionEnding(ExecutionEndingRequest request, ServerCallContext context)
        {
            try
            {
                TraceLogger.Info($"{nameof(NotifyExecutionEnding)} received");
                TraceLogger.Verbose(Newtonsoft.Json.JsonConvert.SerializeObject(request));

                if (request.SuiteResult != null)
                {
                    _sender.FinishLaunch(request);

                    Console.Write("Finishing to send results to Report Portal... ");
                    var sw = Stopwatch.StartNew();
                    _sender.Sync();
                    Console.WriteLine($"Successfully sent. Elapsed: {sw.Elapsed}");
                }
            }
            catch (Exception exp)
            {
                TraceLogger.Error(exp.ToString());
            }

            return Task.FromResult(new Empty());
        }

        public override Task<Empty> NotifySpecExecutionStarting(SpecExecutionStartingRequest request, ServerCallContext context)
        {
            try
            {
                TraceLogger.Info($"{nameof(NotifySpecExecutionStarting)} received");
                TraceLogger.Verbose(Newtonsoft.Json.JsonConvert.SerializeObject(request));

                if (request.SpecResult != null)
                {
                    _sender.StartSpec(request);
                }
            }
            catch (Exception exp)
            {
                TraceLogger.Error(exp.ToString());
            }

            return Task.FromResult(new Empty());
        }

        public override Task<Empty> NotifySpecExecutionEnding(SpecExecutionEndingRequest request, ServerCallContext context)
        {
            try
            {
                TraceLogger.Info($"{nameof(NotifySpecExecutionEnding)} received");
                TraceLogger.Verbose(Newtonsoft.Json.JsonConvert.SerializeObject(request));

                if (request.SpecResult != null)
                {
                    _sender.FinishSpec(request);
                }
            }
            catch (Exception exp)
            {
                TraceLogger.Error(exp.ToString());
            }

            return Task.FromResult(new Empty());
        }

        public override Task<Empty> NotifyScenarioExecutionStarting(ScenarioExecutionStartingRequest request, ServerCallContext context)
        {
            try
            {
                TraceLogger.Info($"{nameof(NotifyScenarioExecutionStarting)} received");
                TraceLogger.Verbose(Newtonsoft.Json.JsonConvert.SerializeObject(request));

                if (request.ScenarioResult != null)
                {
                    _sender.StartScenario(request);
                }
            }
            catch (Exception exp)
            {
                TraceLogger.Error(exp.ToString());
            }

            return Task.FromResult(new Empty());
        }

        public override Task<Empty> NotifyScenarioExecutionEnding(ScenarioExecutionEndingRequest request, ServerCallContext context)
        {
            try
            {
                TraceLogger.Info($"{nameof(NotifyScenarioExecutionEnding)} received");
                TraceLogger.Verbose(Newtonsoft.Json.JsonConvert.SerializeObject(request));

                if (request.ScenarioResult != null)
                {
                    _sender.FinishScenario(request);
                }
            }
            catch (Exception exp)
            {
                TraceLogger.Error(exp.ToString());
            }

            return Task.FromResult(new Empty());
        }







        public override Task<Empty> NotifyStepExecutionEnding(StepExecutionEndingRequest request, ServerCallContext context)
        {
            //TraceLogger.Info("NotifyStepExecutionEnding received");
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> NotifyStepExecutionStarting(StepExecutionStartingRequest request, ServerCallContext context)
        {
            //TraceLogger.Info("NotifyStepExecutionStarting received");
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> NotifySuiteResult(SuiteExecutionResult request, ServerCallContext context)
        {
            return Task.FromResult(new Empty());
        }

        public override async Task<Empty> Kill(KillProcessRequest request, ServerCallContext context)
        {
            TraceLogger.Info("Kill received");
            try
            {
                return new Empty();
            }
            finally
            {
                await _server.KillAsync();
            }
        }
    }
}
