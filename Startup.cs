// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Microsoft.BotBuilderSamples
{
    public class Startup
    {

        private const string CosmosServiceEndpoint = "https://cosmodbnew.documents.azure.com:443/";
        private const string CosmosDBKey = "IY9E6wBnqqLx7tW1JP89Z767EVbN7SPnbNtp5O3GFEMpXPNLC9pMb2f2NLPUcBzGj95D1dhCA3HJ0As7hOJTrg==";
        private const string CosmosDBDatabaseName = "bot-cosmos-sql-db";
        private const string CosmosDBCollectionName = "bot-storage";
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            var config = new CosmosDbStorageOptions
            {
                AuthKey = CosmosDBKey,
                CollectionId = CosmosDBCollectionName,
                CosmosDBEndpoint = new Uri(CosmosServiceEndpoint),
                DatabaseId = CosmosDBDatabaseName,
            };


            var transcriptMiddleware = new TranscriptLoggerMiddleware(new CosmosTranscriptStore(config));

            services.AddBot<CustomPromptBot>(options =>
            {
                var middleware = options.Middleware;
                middleware.Add(transcriptMiddleware);
            });


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddSingleton(transcriptMiddleware);
            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            // Create the storage we'll be using for User and Conversation state. (Memory is great for testing purposes.)
            services.AddSingleton<IStorage, MemoryStorage>();

            // Create the User state.
            services.AddSingleton<UserState>();

            // Create the Conversation state.
            services.AddSingleton<ConversationState>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, CustomPromptBot>();


        }


     


        //new code:


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();

            //app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
