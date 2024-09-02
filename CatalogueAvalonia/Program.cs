using Avalonia;
using System;
using System.Threading;
using Avalonia.Controls;
using CatalogueAvalonia.Core;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using OfficeOpenXml;
using QuestPDF;
using QuestPDF.Infrastructure;
using Serilog;
using Serilog.Events;

namespace CatalogueAvalonia
{
    internal sealed class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.

        private static Mutex mutex = new Mutex(true, "c533298572f02ab76227a31bb04faaf6");
        [STAThread]
        public static void Main(string[] args)
        {
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                try
                {
                    Settings.License = LicenseType.Community;
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
                    mutex.ReleaseMutex();
                }
                catch (Exception e)
                {
                    ILogger log = new LoggerConfiguration()
                        .Enrich.FromLogContext()
                        .WriteTo.File("../Logger/FATALlog.txt", LogEventLevel.Information, rollingInterval: RollingInterval.Day,
                            rollOnFileSizeLimit: true)
                        .CreateLogger();
                    log.Error(e, "Fatal error");
                }
            }
            else
            {
                Win32.MessageBox(new IntPtr(0), "Приложение уже запущенно.", "?", 0);
            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}