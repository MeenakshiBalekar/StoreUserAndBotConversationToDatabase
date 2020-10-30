using Microsoft.Azure.Documents;
using Microsoft.Bot.Builder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PromptUsersForInput
{
    public class UtteranceLog : IStoreItem
    {
        // A list of things that users have said to the bot  
        public List<string> UtteranceList { get; } = new List<string>();

        public List<Microsoft.Bot.Schema.Attachment> UtteranceListFile { get; } = new List<Microsoft.Bot.Schema.Attachment>();

        // The number of conversational turns that have occurred          
        public int TurnNumber { get; set; } = 0;

        // Create concurrency control where this is used.  
        public string ETag { get; set; } = "*";
    }
}
