using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using CatalogueAvalonia.Models;
using CatalogueAvalonia.Services.DataBaseAction;
using CatalogueAvalonia.Services.DataStore;
using CatalogueAvalonia.ViewModels;
using CatalogueAvalonia.Views;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Reflection.Emit;

namespace CatalogueAvalonia
{
	public partial class App : Application
	{
		public IHost? GlobalHost { get; private set; }
		public override void Initialize()
		{
			AvaloniaXamlLoader.Load(this);
		}
		public override async void OnFrameworkInitializationCompleted()
		{
			var hostBuilder = CreateHostBuilder();
			var host = hostBuilder.Build();
			GlobalHost = host;

			if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
			{
				// Line below is needed to remove Avalonia data validation.
				// Without this line you will get duplicate validations from both Avalonia and CT
				BindingPlugins.DataValidators.RemoveAt(0);
				desktop.MainWindow = new MainWindow
				{
					DataContext = GlobalHost.Services.GetRequiredService<MainWindowViewModel>()
				};
			}
			DataTemplates.Add(GlobalHost.Services.GetRequiredService<ViewLocator>());

			base.OnFrameworkInitializationCompleted();
			await host.StartAsync();
		}

		private static HostApplicationBuilder CreateHostBuilder()
		{
			var builder = Host.CreateApplicationBuilder(Environment.GetCommandLineArgs());

			builder.Services.AddTransient<ViewLocator>();

			builder.Services.AddSingleton<MainWindow>();
			builder.Services.AddSingleton<CatalogueView>();
			
			builder.Services.AddSingleton<MainWindowViewModel>();
			builder.Services.AddSingleton<CatalogueViewModel>();

			builder.Services.AddSingleton<TopModel>();
			builder.Services.AddSingleton<DataStore>();
			builder.Services.AddSingleton<IDataBaseProvider, DataBaseProvider>();
			builder.Services.AddSingleton<IMessenger, WeakReferenceMessenger>();

			return builder;
		}
	}
}