﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using OnlyT.Utils;
using Serilog;

namespace OnlyT
{
   /// <summary>
   /// Interaction logic for App.xaml
   /// </summary>
   public partial class App : Application
   {
      private Mutex _appMutex;
      private readonly string _appString = "OnlyTMeetingTimer";

      protected override void OnStartup(StartupEventArgs e)
      {
         if (AnotherInstanceRunning())
         {
            Shutdown();
         }
         else
         {
            ConfigureLogger();
         }

         if (CommandLineParser.Instance.IsSwitchSet("-nogpu"))
         {
            // disable hardware (GPU) rendering so that it's all done by the CPU...
            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
         }
      }

      private void ConfigureLogger()
      {
         string logsDirectory = FileUtils.GetLogFolder();

         Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.RollingFile(Path.Combine(logsDirectory, "log-{Date}.txt"), retainedFileCountLimit: 28)
            .CreateLogger();

         Log.Logger.Information("==== Launched ====");
      }

      private bool AnotherInstanceRunning()
      {
         _appMutex = new Mutex(true, _appString, out var newInstance);
         return !newInstance;
      }

      protected override void OnExit(ExitEventArgs e)
      {
         _appMutex?.Dispose();
         Log.Logger.Information("==== Exit ====");
      }

   }
}