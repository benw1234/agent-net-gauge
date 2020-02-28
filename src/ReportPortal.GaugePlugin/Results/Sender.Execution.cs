﻿using Gauge.Messages;
using ReportPortal.Client.Abstractions.Requests;
using ReportPortal.Shared.Reporter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReportPortal.GaugePlugin.Results
{
    partial class Sender
    {
        private object _lockObj = new object();

        private int _launchesCount;

        private ILaunchReporter _launch;

        public void StartLaunch(ExecutionStartingRequest request)
        {
            lock (_lockObj)
            {
                if (_launch == null)
                {
                    var suiteExecutionResult = request.SuiteResult;

                    var launchReporter = new LaunchReporter(_service, _configuration, null);

                    launchReporter.Start(new StartLaunchRequest
                    {
                        Name = _configuration.GetValue("Launch:Name", suiteExecutionResult.ProjectName),
                        Description = _configuration.GetValue("Launch:Description", string.Empty),
                        Tags = _configuration.GetValues("Launch:Tags", new List<string>()).ToList(),
                        StartTime = DateTime.UtcNow
                    });

                    _launch = launchReporter;
                }

                _launchesCount++;
            }
        }

        public void FinishLaunch(ExecutionEndingRequest request)
        {
            lock (_lockObj)
            {
                _launchesCount--;

                if (_launchesCount == 0)
                {
                    _launch.Finish(new FinishLaunchRequest
                    {
                        EndTime = DateTime.UtcNow
                    });
                }
            }
        }

        public void Sync()
        {
            lock(_lockObj)
            {
                if (_launchesCount == 0)
                {
                    _launch.Sync();
                }
            }
        }
    }
}