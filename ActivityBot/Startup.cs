using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ActivityBot.Activity.Proxy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Configuration;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ActivityBot
{
    public class Startup
    {
        private ILoggerFactory _loggerFactory;
        private readonly bool _isProduction;
        private readonly HttpClient _httpClient;

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            _isProduction = env.IsProduction();
            Configuration = configuration;
            _httpClient = new HttpClient();

        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddTransient(x => new ActivityProxy(_httpClient, Configuration.GetSection("GetPendingApprovalsUrl")?.Value));

            var secretKey = Configuration.GetSection("botFileSecret")?.Value;
            var botFilePath = Configuration.GetSection("botFilePath")?.Value;
            if (!File.Exists(botFilePath))
            {
                throw new FileNotFoundException($"The .bot configuration file was not found. botFilePath: {botFilePath}");
            }

            // Loads .bot configuration file and adds a singleton that your Bot can access through dependency injection.
            BotConfiguration botConfig = null;
            try
            {
                botConfig = BotConfiguration.Load(botFilePath, secretKey);
            }
            catch
            {
                var msg = @"Error reading bot file. Please ensure you have valid botFilePath and botFileSecret set for your environment.
        - You can find the botFilePath and botFileSecret in the Azure App Service application settings.
        - If you are running this bot locally, consider adding a appsettings.json file with botFilePath and botFileSecret.
        - See https://aka.ms/about-bot-file to learn more about .bot file its use and bot configuration.
        ";
                throw new InvalidOperationException(msg);
            }

            services.AddSingleton(sp => botConfig ?? throw new InvalidOperationException($"The .bot configuration file could not be loaded. botFilePath: {botFilePath}"));

            // Retrieve current endpoint.
            var environment = _isProduction ? "production" : "development";
            var service = botConfig.Services.FirstOrDefault(s => s.Type == "endpoint" && s.Name == environment);
            if (service == null && _isProduction)
            {
                // Attempt to load development environment
                service = botConfig.Services.Where(s => s.Type == "endpoint" && s.Name == "development").FirstOrDefault();
            }

            if (!(service is EndpointService endpointService))
            {
                throw new InvalidOperationException($"The .bot file does not contain an endpoint with name '{environment}'.");
            }

            // Memory Storage is for local bot debugging only. When the bot
            // is restarted, everything stored in memory will be gone.
            IStorage dataStore = new MemoryStorage();

            // For production bots use the Azure Blob or
            // Azure CosmosDB storage providers. For the Azure
            // based storage providers, add the Microsoft.Bot.Builder.Azure
            // Nuget package to your solution. That package is found at:
            // https://www.nuget.org/packages/Microsoft.Bot.Builder.Azure/
            // Un-comment the following lines to use Azure Blob Storage
            // // Storage configuration name or ID from the .bot file.
            // const string StorageConfigurationId = "<STORAGE-NAME-OR-ID-FROM-BOT-FILE>";
            // var blobConfig = botConfig.FindServiceByNameOrId(StorageConfigurationId);
            // if (!(blobConfig is BlobStorageService blobStorageConfig))
            // {
            //    throw new InvalidOperationException($"The .bot file does not contain an blob storage with name '{StorageConfigurationId}'.");
            // }
            // // Default container name.
            // const string DefaultBotContainer = "<DEFAULT-CONTAINER>";
            // var storageContainer = string.IsNullOrWhiteSpace(blobStorageConfig.Container) ? DefaultBotContainer : blobStorageConfig.Container;
            // IStorage dataStore = new Microsoft.Bot.Builder.Azure.AzureBlobStorage(blobStorageConfig.ConnectionString, storageContainer);

            // Create and add conversation state.
            var conversationState = new ConversationState(dataStore);
            services.AddSingleton(conversationState);

            services.AddBot<Activity.ActivityBot>(options =>
            {
                options.CredentialProvider = new SimpleCredentialProvider(endpointService.AppId, endpointService.AppPassword);

                // Catches any errors that occur during a conversation turn and logs them to currently
                // configured ILogger.
                ILogger logger = _loggerFactory.CreateLogger<Activity.ActivityBot>();

                options.OnTurnError = async (context, exception) =>
                {
                    logger.LogError($"Exception caught : {exception}");
                    await context.SendActivityAsync("Sorry, it looks like something went wrong.");
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;

            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}

            //app.UseMvc();
            app//.UseDefaultFiles()
                //.UseStaticFiles()
                .UseBotFramework();

        }
    }
}
