using System.Net.Http;
using ActivityBot.Activity.Handlers;
using ActivityBot.Activity.Models;
using ActivityBot.Activity.Proxy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
            var activityProxyConfiguration = new ActivityProxyConfiguration
            {
                GetPendingApprovalsUrl = Configuration.GetSection("GetPendingApprovalsUrl")?.Value,
                ApproveUrl = Configuration.GetSection("ApproveUrl")?.Value,
                ManuallyTriggerWeeklyActivityUrl = Configuration.GetSection("ManuallyTriggerWeeklyActivityUrl")?.Value
            };
            services.AddSingleton(activityProxyConfiguration);

            services.AddHttpClient(ActivityProxy.HTTP_CLIENT_NAME);
            services.AddTransient<ActivityProxy>();

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

            var notificationState = new NotificationState(dataStore);
            services.AddSingleton(notificationState);

            var microsoftBotApplicationId = Configuration.GetSection("MicrosoftBotApplicationId")?.Value;
            var microsoftBotApplicationPassword = Configuration.GetSection("MicrosoftBotApplicationPassword")?.Value;
            services.AddBot<Activity.ActivityBot>(options =>
            {
                options.CredentialProvider = new SimpleCredentialProvider(microsoftBotApplicationId, microsoftBotApplicationPassword);

                // Catches any errors that occur during a conversation turn and logs them to currently
                // configured ILogger.
                ILogger logger = _loggerFactory.CreateLogger<Activity.ActivityBot>();

                options.OnTurnError = async (context, exception) =>
                {
                    logger.LogError($"Exception caught : {exception}");
                    await context.SendActivityAsync("Sorry, it looks like something went wrong.");
                };
            });

            services.AddSingleton(new Configuration
            {
                AppId = string.IsNullOrEmpty(microsoftBotApplicationId) ? "LocalEmulator" : microsoftBotApplicationId
            });

            services.AddTransient<BroadcastCallbackFactory>();
            services.AddTransient<BroadcastHandler>();
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
