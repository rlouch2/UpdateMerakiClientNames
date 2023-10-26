// See https://aka.ms/new-console-template for more information
//Console.WriteLine("Hello, World!");


using Google.Apis.Auth.OAuth2;
using Google.Apis.Discovery.v1.Data;
using Google.Apis.Discovery.v1;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using UpdateMerakiClientNames._Code.Admin;
using Google.Apis.Admin.Directory.directory_v1.Data;
using Google.Apis.Admin.Directory.directory_v1;
using Meraki.Api.Data;
using System.Net;
using Meraki.Api;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;


SetupStaticLogger();

Run().Wait();

async Task Run()
{
    Log.Information("Beginning device sync to Meraki from Google Workspace");
    var builder = new ConfigurationBuilder()
        .AddJsonFile($"appsettings.json", optional: true, reloadOnChange: true);


    var config = builder.Build();

    Log.Debug("Authorizing Google API");
    UpdateMerakiClientNames.Objects.GoogleAuth googleAuth = config.GetSection("GoogleAPI").Get<UpdateMerakiClientNames.Objects.GoogleAuth>();
    UpdateMerakiClientNames.Objects.MerakiAuth merakiAuth = config.GetSection("MerakiAPI").Get<UpdateMerakiClientNames.Objects.MerakiAuth>();

    Chromebooks chromebooks = new Chromebooks(googleAuth, merakiAuth);
    await chromebooks.UpdateMerakiClientNameAsync();
}

static void SetupStaticLogger()
{
    var configuration = new ConfigurationBuilder()
        .AddJsonFile($"appsettings.json")
        .Build();

    Log.Logger = (ILogger)new LoggerConfiguration()
     .ReadFrom.Configuration(configuration)
     .CreateLogger();
}