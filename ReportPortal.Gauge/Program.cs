﻿using Gauge.CSharp.Core;
using Gauge.Messages;
using Grpc.Core;
using ReportPortal.Client;
using ReportPortal.Shared.Configuration;
using ReportPortal.Shared.Configuration.Providers;
using ReportPortal.Shared.Internal.Logging;
using ReportPortal.Shared.Reporter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ReportPortal.GaugePlugin
{
    class Program
    {
        private static ITraceLogger TraceLogger = TraceLogManager.GetLogger<Program>();

        private static Dictionary<ExecutionStatus, Client.Models.Status> _statusMap;

        private static IConfiguration Config;

        static Program()
        {
            _statusMap = new Dictionary<ExecutionStatus, Client.Models.Status>
            {
                { ExecutionStatus.Failed, Client.Models.Status.Failed },
                { ExecutionStatus.Notexecuted, Client.Models.Status.Skipped },
                { ExecutionStatus.Passed, Client.Models.Status.Passed },
                { ExecutionStatus.Skipped, Client.Models.Status.Skipped }
            };

            var configPrefix = "RP_";
            var configDelimeter = "_";
            Config = new ConfigurationBuilder()
                .Add(new EnvironmentVariablesConfigurationProvider(configPrefix, configDelimeter, EnvironmentVariableTarget.Process))
                .Add(new EnvironmentVariablesConfigurationProvider(configPrefix, configDelimeter, EnvironmentVariableTarget.User))
                .Add(new EnvironmentVariablesConfigurationProvider(configPrefix, configDelimeter, EnvironmentVariableTarget.Machine))
                .Build();
        }

        public class MyHandler : Gauge.Messages.Reporter.ReporterBase
        {
            private static ITraceLogger TraceLogger = TraceLogManager.GetLogger<MyHandler>();

            private Server _server;

            public MyHandler(Server server)
            {
                _server = server;
            }

            public override Task<Empty> NotifyExecutionStarting(ExecutionStartingRequest request, ServerCallContext context)
            {
                try
                {
                    TraceLogger.Info($"NotifyExecutionStarting received");
                    TraceLogger.Verbose(Newtonsoft.Json.JsonConvert.SerializeObject(request));
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
                    TraceLogger.Info($"NotifyExecutionEnding received");
                    TraceLogger.Verbose(Newtonsoft.Json.JsonConvert.SerializeObject(request));

                    if (request.SuiteResult != null)
                    {
                        Console.Write("Finishing to send results to Report Portal... ");
                        var sw = Stopwatch.StartNew();
                        Console.WriteLine($"Successfully sent. Elapsed: {sw.Elapsed}");
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
                    TraceLogger.Info($"NotifyScenarioExecutionEnding received");
                    TraceLogger.Verbose(Newtonsoft.Json.JsonConvert.SerializeObject(request));
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
                    TraceLogger.Info($"NotifyScenarioExecutionStarting received");
                    TraceLogger.Verbose(Newtonsoft.Json.JsonConvert.SerializeObject(request));
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
                    TraceLogger.Info($"NotifySpecExecutionEnding received");
                    TraceLogger.Verbose(Newtonsoft.Json.JsonConvert.SerializeObject(request));
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
                    TraceLogger.Info($"NotifySpecExecutionStarting received");
                    TraceLogger.Verbose(Newtonsoft.Json.JsonConvert.SerializeObject(request));
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
                //TraceLogger.Info("NotifySuiteResult received");
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
                    //await Task.Delay(5000);
                    await _server.KillAsync();
                }
            }
        }

        static async Task Main(string[] args)
        {
            var server = new Server();

            var s = Reporter.BindService(new MyHandler(server));
            server.Services.Add(s);

            var g_port = server.Ports.Add(new ServerPort("127.0.0.1", 0, ServerCredentials.Insecure));

            Console.Write($"Listening on port:{g_port}");
            server.Start();
            TraceLogger.Info("Server has started.");

            await server.ShutdownTask;






            //var rpUri = new Uri(Config.GetValue<string>("Uri"));
            //var rpProject = Config.GetValue<string>("Project");
            //var rpUuid = Config.GetValue<string>("Uuid");

            //var service = new Service(rpUri, rpProject, rpUuid);
            //var launchReporter = new LaunchReporter(service, Config, null);

            //var tcpClientWrapper = new TcpClientWrapper(Gauge.CSharp.Core.Utils.GaugeApiPort);
            //using (var gaugeConnection = new GaugeConnection(tcpClientWrapper))
            //{
            //    while (gaugeConnection.Connected)
            //    {
            //        try
            //        {
            //            TraceLogger.Verbose($"Reading message...");

            //            var messageBytes = gaugeConnection.ReadBytes();
            //            TraceLogger.Verbose($"Read message. Length: {messageBytes.Count()}");

            //            var message = Message.Parser.ParseFrom(messageBytes.ToArray());
            //            TraceLogger.Verbose($"Received event {message.MessageType}");

            //            //if (message.MessageType == Message.Types.MessageType.SuiteExecutionResult)
            //            //{
            //            //    var suiteExecutionResult = message.SuiteExecutionResult.SuiteResult;

            //            //    var launchStartDateTime = DateTime.UtcNow.AddMilliseconds(-suiteExecutionResult.ExecutionTime);
            //            //    launchReporter.Start(new Client.Requests.StartLaunchRequest
            //            //    {
            //            //        Name = Config.GetValue("Launch:Name", suiteExecutionResult.ProjectName),
            //            //        Description = Config.GetValue("Launch:Description", string.Empty),
            //            //        Tags = Config.GetValues("Launch:Tags", new List<string>()).ToList(),
            //            //        StartTime = launchStartDateTime
            //            //    });

            //            //    foreach (var specResult in suiteExecutionResult.SpecResults)
            //            //    {
            //            //        var specStartTime = launchStartDateTime;
            //            //        var specReporter = launchReporter.StartChildTestReporter(new Client.Requests.StartTestItemRequest
            //            //        {
            //            //            Type = Client.Models.TestItemType.Suite,
            //            //            Name = specResult.ProtoSpec.SpecHeading,
            //            //            Description = string.Join("", specResult.ProtoSpec.Items.Where(i => i.ItemType == ProtoItem.Types.ItemType.Comment).Select(c => c.Comment.Text)),
            //            //            StartTime = specStartTime,
            //            //            Tags = specResult.ProtoSpec.Tags.Select(t => t.ToString()).ToList()
            //            //        });

            //            //        foreach (var scenarioResult in specResult.ProtoSpec.Items.Where(i => i.ItemType == ProtoItem.Types.ItemType.Scenario || i.ItemType == ProtoItem.Types.ItemType.TableDrivenScenario))
            //            //        {
            //            //            ProtoScenario scenario;

            //            //            switch (scenarioResult.ItemType)
            //            //            {
            //            //                case ProtoItem.Types.ItemType.Scenario:
            //            //                    scenario = scenarioResult.Scenario;
            //            //                    break;
            //            //                case ProtoItem.Types.ItemType.TableDrivenScenario:
            //            //                    scenario = scenarioResult.TableDrivenScenario.Scenario;
            //            //                    break;
            //            //                default:
            //            //                    scenario = scenarioResult.Scenario;
            //            //                    break;
            //            //            }

            //            //            var scenarioStartTime = specStartTime;
            //            //            var scenarioReporter = specReporter.StartChildTestReporter(new Client.Requests.StartTestItemRequest
            //            //            {
            //            //                Type = Client.Models.TestItemType.Step,
            //            //                StartTime = scenarioStartTime,
            //            //                Name = scenario.ScenarioHeading,
            //            //                Description = string.Join("", scenario.ScenarioItems.Where(i => i.ItemType == ProtoItem.Types.ItemType.Comment).Select(c => c.Comment.Text)),
            //            //                Tags = scenario.Tags.Select(t => t.ToString()).ToList()
            //            //            });

            //            //            // internal log ("rp_log_enabled" property)
            //            //            if (Config.GetValue("log:enabled", false))
            //            //            {
            //            //                scenarioReporter.Log(new Client.Requests.AddLogItemRequest
            //            //                {
            //            //                    Text = "Spec Result Proto",
            //            //                    Level = Client.Models.LogLevel.Trace,
            //            //                    Time = DateTime.UtcNow,
            //            //                    Attach = new Client.Models.Attach("Spec", "application/json", System.Text.Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(specResult)))
            //            //                });
            //            //                scenarioReporter.Log(new Client.Requests.AddLogItemRequest
            //            //                {
            //            //                    Text = "Scenario Result Proto",
            //            //                    Level = Client.Models.LogLevel.Trace,
            //            //                    Time = DateTime.UtcNow,
            //            //                    Attach = new Client.Models.Attach("Scenario", "application/json", System.Text.Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(scenarioResult)))
            //            //                });
            //            //            }

            //            //            var lastStepStartTime = scenarioStartTime;
            //            //            if (scenario.ScenarioItems != null)
            //            //            {
            //            //                foreach (var stepResult in scenario.ScenarioItems.Where(i => i.ItemType == ProtoItem.Types.ItemType.Step))
            //            //                {
            //            //                    var text = "!!!MARKDOWN_MODE!!!" + stepResult.Step.ActualText;
            //            //                    var stepLogLevel = stepResult.Step.StepExecutionResult.ExecutionResult.Failed ? Client.Models.LogLevel.Error : Client.Models.LogLevel.Info;

            //            //                    // if step argument is table
            //            //                    var tableParameter = stepResult.Step.Fragments.FirstOrDefault(f => f.Parameter?.Table != null)?.Parameter.Table;
            //            //                    if (tableParameter != null)
            //            //                    {
            //            //                        text += Environment.NewLine + Environment.NewLine + "| " + string.Join(" | ", tableParameter.Headers.Cells.ToArray()) + " |";
            //            //                        text += Environment.NewLine + "| " + string.Join(" | ", tableParameter.Headers.Cells.Select(c => "---")) + " |";

            //            //                        foreach (var tableRow in tableParameter.Rows)
            //            //                        {
            //            //                            text += Environment.NewLine + "| " + string.Join(" | ", tableRow.Cells.ToArray()) + " |";
            //            //                        }
            //            //                    }

            //            //                    // if dynamic arguments
            //            //                    var dynamicParameteres = stepResult.Step.Fragments.Where(f => f.FragmentType == Fragment.Types.FragmentType.Parameter && f.Parameter.ParameterType == Parameter.Types.ParameterType.Dynamic).Select(f => f.Parameter);
            //            //                    if (dynamicParameteres.Count() != 0)
            //            //                    {
            //            //                        text += Environment.NewLine;

            //            //                        foreach (var dynamicParameter in dynamicParameteres)
            //            //                        {
            //            //                            text += $"{Environment.NewLine}{dynamicParameter.Name}: {dynamicParameter.Value}";
            //            //                        }
            //            //                    }

            //            //                    if (stepResult.Step.StepExecutionResult.ExecutionResult.Failed)
            //            //                    {
            //            //                        text += $"{Environment.NewLine}{Environment.NewLine}{stepResult.Step.StepExecutionResult.ExecutionResult.ErrorMessage}{Environment.NewLine}{stepResult.Step.StepExecutionResult.ExecutionResult.StackTrace}";
            //            //                    }

            //            //                    scenarioReporter.Log(new Client.Requests.AddLogItemRequest
            //            //                    {
            //            //                        Level = stepLogLevel,
            //            //                        Time = lastStepStartTime,
            //            //                        Text = text
            //            //                    });

            //            //                    if (stepResult.Step.StepExecutionResult.ExecutionResult.ScreenShot?.Length != 0)
            //            //                    {
            //            //                        scenarioReporter.Log(new Client.Requests.AddLogItemRequest
            //            //                        {
            //            //                            Level = Client.Models.LogLevel.Debug,
            //            //                            Time = lastStepStartTime,
            //            //                            Text = "Screenshot",
            //            //                            Attach = new Client.Models.Attach("Screenshot", "image/png", stepResult.Step.StepExecutionResult.ExecutionResult.ScreenShot.ToByteArray())
            //            //                        });
            //            //                    }

            //            //                    lastStepStartTime = lastStepStartTime.AddMilliseconds(stepResult.Step.StepExecutionResult.ExecutionResult.ExecutionTime);
            //            //                }
            //            //            }

            //            //            scenarioReporter.Finish(new Client.Requests.FinishTestItemRequest
            //            //            {
            //            //                EndTime = scenarioStartTime.AddMilliseconds(scenario.ExecutionTime),
            //            //                Status = _statusMap[scenario.ExecutionStatus]
            //            //            });
            //            //        }

            //            //        var specFinishStatus = specResult.Failed ? Client.Models.Status.Failed : Client.Models.Status.Passed;
            //            //        specReporter.Finish(new Client.Requests.FinishTestItemRequest
            //            //        {
            //            //            Status = specFinishStatus,
            //            //            EndTime = specStartTime.AddMilliseconds(specResult.ExecutionTime)
            //            //        });
            //            //    }

            //            //    launchReporter.Finish(new Client.Requests.FinishLaunchRequest
            //            //    {
            //            //        EndTime = DateTime.UtcNow
            //            //    });
            //            //}

            //            if (message.MessageType == Message.Types.MessageType.KillProcessRequest)
            //            {
            //                Console.Write("Finishing to send results to Report Portal... ");
            //                var sw = Stopwatch.StartNew();
            //                launchReporter.Sync();

            //                Console.WriteLine($"Elapsed: {sw.Elapsed}");

            //                return;
            //            }
            //        }
            //        catch (Exception exp)
            //        {
            //            TraceLogger.Error($"Unhandler error: {exp}");
            //        }
            //    }
            //}


        }
    }
}
