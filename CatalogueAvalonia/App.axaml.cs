using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using CatalogueAvalonia.Core;
using CatalogueAvalonia.Models;
using CatalogueAvalonia.Services.DataBaseAction;
using CatalogueAvalonia.Services.DataStore;
using CatalogueAvalonia.Services.DialogueServices;
using CatalogueAvalonia.ViewModels;
using CatalogueAvalonia.ViewModels.DialogueViewModel;
using CatalogueAvalonia.Views;
using CatalogueAvalonia.Views.DialogueWindows;
using CommunityToolkit.Mvvm.Messaging;
using DataBase.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
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

			builder.Services.AddSingleton<MainWindowViewModel>();
			builder.Services.AddSingleton<CatalogueViewModel>();
			builder.Services.AddSingleton<AgentViewModel>();
			builder.Services.AddSingleton<ZakupkaViewModel>();
			builder.Services.AddSingleton<ProdajaViewModel>();

			builder.Services.AddSingleton<TopModel>();
			builder.Services.AddSingleton<DataStore>();
			builder.Services.AddSingleton<TaskQueue>();
			builder.Services.AddSingleton<IDataBaseProvider, DataBaseProvider>();
			builder.Services.AddSingleton<IDataBaseAction, DataBaseAction>();
			builder.Services.AddSingleton<IDialogueService, DialogueService>();
			builder.Services.AddSingleton<IMessenger, WeakReferenceMessenger>();

			builder.Services.AddDbContext<DataContext>(o => o.UseSqlite("DataSource=C:\\Users\\Shep\\Desktop\\data.db"));

			return builder;
		}
	}
}