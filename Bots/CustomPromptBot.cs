// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DateTime;
using Microsoft.Recognizers.Text.Number;
using PromptUsersForInput;
using System.IO;
using Microsoft.Bot.Connector;
using System.Net;
using Newtonsoft.Json;

namespace Microsoft.BotBuilderSamples
{
    // This IBot implementation can run any type of Dialog. The use of type parameterization is to allows multiple different bots
    // to be run at different endpoints within the same project. This can be achieved by defining distinct Controller types
    // each with dependency on distinct IBot types, this way ASP Dependency Injection can glue everything together without ambiguity.
    public class CustomPromptBot : ActivityHandler 
    {
        private const string CosmosServiceEndpoint = "";
        private const string CosmosDBKey = "";
        private const string CosmosDBDatabaseName = "";
        private const string CosmosDBCollectionName = "";

        private static readonly CosmosDbStorage query = new CosmosDbStorage(new CosmosDbStorageOptions
        {
            AuthKey = CosmosDBKey,
            CollectionId = CosmosDBCollectionName,
            CosmosDBEndpoint = new Uri(CosmosServiceEndpoint),
            DatabaseId = CosmosDBDatabaseName,
        });

        private static Attachment CreateAdaptiveCardAttachment(string filePath)
        {
            var adaptiveCardJson = File.ReadAllText(filePath);
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };
            return adaptiveCardAttachment;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {

            //if (turnContext.Activity == Text)
            
           var utterance = turnContext.Activity.Text;
            // List<Attachment> utterance1[0] = turnContext.Activity.Attachments[0];
            var IsAttachmnet = turnContext.Activity.Attachments;
            var utterance1 = turnContext.Activity.Attachments;
           
            UtteranceLog logItems = null;


            // see if there are previous messages saved in storage.  
            try
            {
                string[] utteranceList = { "UtteranceLog" };
                //Attachment[] utteranceListFile = {  };
                


                logItems = query.ReadAsync<UtteranceLog>(utteranceList).Result?.FirstOrDefault().Value;
            }
            catch (Exception ex)
            {
                // Inform the user an error occured.  
                await turnContext.SendActivityAsync("Sorry, something went wrong reading your stored messages!");
            }

            // If no stored messages were found, create and store a new entry.  
            if (logItems is null)
            {
                // add the current utterance to a new object.  
                logItems = new UtteranceLog();
                logItems.UtteranceList.Add(utterance);
                logItems.UtteranceListFile.Add(utterance1[0]);
                //logItems.UtteranceList.Add("Echo " + utterance);
                // set initial turn counter to 1.  
                logItems.TurnNumber++;

                // Show user new user message. 
                
                await turnContext.SendActivityAsync($"Echo" + turnContext.Activity.Text);

                // Create Dictionary object to hold received user messages.  
                var changes = new Dictionary<string, object>();
                {
                    changes.Add("UtteranceLog", logItems);
                }
                try
                {
                    // Save the user message to your Storage.  
                    await query.WriteAsync(changes, cancellationToken);
                }
                catch
                {
                    // Inform the user an error occured.  
                    await turnContext.SendActivityAsync("Sorry, something went wrong storing your message!");
                }
                
            }
            
            // Else, our Storage already contained saved user messages, add new one to the list.  
            else
            {
                string botResponse = turnContext.SendActivityAsync($"bot Echo the value as " + turnContext.Activity.Text).ToString();
               
               Attachment botResponseFile ;
                // add new message to list of messages to display.  
                
                //push to DB user response
                logItems.UtteranceList.Add(utterance);
                if (utterance1 != null)
                {
                    var cardAttachment = turnContext.SendActivityAsync(MessageFactory.Attachment(utterance1));
                    logItems.UtteranceListFile.Add(utterance1[0]);
                    //await turnContext.SendActivityAsync(MessageFactory.Attachment(utterance1), cancellationToken);
                }
                //push to DB bot response
                logItems.UtteranceList.Add(botResponse);
                //logItems.UtteranceListFile.Add(utterance1[0]);

                //logItems.UtteranceList.Add("Echo new  " + utterance);
                // increment turn counter.  
                logItems.TurnNumber++;

                // show user new list of saved messages.  
                //await turnContext.SendActivityAsync(MessageFactory.Attachment(utterance1), cancellationToken);
                //string bot 
                // Create Dictionary object to hold new list of messages.  
                var changes = new Dictionary<string, object>();
                {
                    //changes.Add(, logItems);
                    changes.Add("UtteranceLog", logItems);
                    
                };

                try
                {
                    // Save new list to your Storage.  
                    await query.WriteAsync(changes, cancellationToken);
                }
                catch(Exception ex)
                {
                    // Inform the user an error occured.  
                    await turnContext.SendActivityAsync("Sorry, something went wrong storing your message!");
                }
            }
        }
    }
}
