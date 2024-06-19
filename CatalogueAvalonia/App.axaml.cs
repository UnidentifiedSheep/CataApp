using System;
using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using CatalogueAvalonia.Core;
using CatalogueAvalonia.Models;
using CatalogueAvalonia.Services.BarcodeServer;
using CatalogueAvalonia.Services.DataBaseAction;
using CatalogueAvalonia.Services.DataStore;
using CatalogueAvalonia.Services.DialogueServices;
using CatalogueAvalonia.ViewModels;
using CatalogueAvalonia.Views;
using CommunityToolkit.Mvvm.Messaging;
using DataBase.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using ILogger = Serilog.ILogger;


namespace CatalogueAvalonia;

public class App : Application
{
    public IHost? GlobalHost { get; private set; }

    private static void CheckDir()
    {
        List<string> dir = ["Data", "Documents", "Logger"];

        foreach (var path in dir)
            Directory.CreateDirectory($"../{path}");
        var parth = Environment.ProcessPath!.Substring(0, Environment.ProcessPath!.LastIndexOf('\\')) + "\\Cache";
        Directory.CreateDirectory(parth);
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        var hostBuilder = CreateHostBuilder();
        var host = hostBuilder.Build();
        GlobalHost = host;


        await GlobalHost.Services.GetRequiredService<DataContext>().Database.EnsureCreatedAsync();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            BindingPlugins.DataValidators.RemoveAt(0);
            desktop.MainWindow = new MainWindow
            {
                DataContext = GlobalHost.Services.GetRequiredService<MainWindowViewModel>()
            };
            desktop.Exit += OnExit;
        }

        DataTemplates.Add(GlobalHost.Services.GetRequiredService<ViewLocator>());
        
        base.OnFrameworkInitializationCompleted();
        await host.StartAsync();
    }

    private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    { 
        DirectoryInfo di = new DirectoryInfo("../Documents");
        foreach (FileInfo file in di.GetFiles())
            file.Delete();
        if (GlobalHost != null)
        {
            if (GlobalHost.Services.GetRequiredService<DataContext>().Database.GetDbConnection() is SqliteConnection conn)
                SqliteConnection.ClearPool(conn);
            if (GlobalHost.Services.GetRequiredService<DataContextDataProvider>().Database.GetDbConnection() is SqliteConnection conn2)
                SqliteConnection.ClearPool(conn2);
            
        }
    }

    private static HostApplicationBuilder CreateHostBuilder()
    {
        CheckDir();

        ILogger log = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.File("../Logger/log.txt", LogEventLevel.Information, rollingInterval: RollingInterval.Day,
                rollOnFileSizeLimit: true)
            .CreateLogger();

        var builder = Host.CreateApplicationBuilder(Environment.GetCommandLineArgs());

        builder.Services.AddSingleton(log);
        builder.Services.AddDbContext<DataContext>(o => o.UseSqlite("DataSource=../Data/data.db"));
        builder.Services.AddDbContext<DataContextDataProvider>(o =>
            o.UseSqlite("DataSource=../Data/data.db").UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
        builder.Services.AddTransient<ViewLocator>();

        builder.Services.AddSingleton<MainWindowViewModel>();
        builder.Services.AddSingleton<CatalogueViewModel>();
        builder.Services.AddSingleton<AgentViewModel>();
        builder.Services.AddSingleton<ZakupkaViewModel>();
        builder.Services.AddSingleton<ProdajaViewModel>();

        builder.Services.AddSingleton<TopModel>();
        builder.Services.AddSingleton<DataStore>();
        builder.Services.AddSingleton<TaskQueue>();
        builder.Services.AddSingleton<Listener>();
        builder.Services.AddSingleton<TcpServer>();
        builder.Services.AddSingleton<IDataBaseProvider, DataBaseProvider>();
        builder.Services.AddSingleton<IDataBaseAction, DataBaseAction>();
        builder.Services.AddSingleton<IDialogueService, DialogueService>();
        builder.Services.AddSingleton<IMessenger, WeakReferenceMessenger>();


        return builder;
    }
}