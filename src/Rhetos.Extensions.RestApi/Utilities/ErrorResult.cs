using System;

namespace Rhetos.Extensions.RestApi.Utilities
{
    public class ErrorResult
    {
        public string UserMessage { get; }
        public string SystemMessage { get; }

        public ErrorResult(string userMessage, string systemMessage)
        {
            UserMessage = userMessage;
            SystemMessage = systemMessage;
        }
    }
}
