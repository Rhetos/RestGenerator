using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json.Linq;
using Rhetos;
using Rhetos.Dom;
using Rhetos.Dom.DefaultConcepts;
using Rhetos.Host.AspNet.JsonCommands.Controllers;
using Rhetos.Host.AspNet.JsonCommands.Test.Tools;
using Rhetos.Processing;
using Rhetos.Processing.DefaultCommands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using TestApp;
using Xunit;
using Xunit.Abstractions;

namespace Rhetos.Host.AspNet.JsonCommands.Tests
{
    public class JsonCommandsControllerTests : IDisposable
    {
        private readonly WebApplicationFactory<Startup> _factory;
        private readonly ITestOutputHelper output;

        public JsonCommandsControllerTests(ITestOutputHelper output)
        {
            _factory = new CustomWebApplicationFactory<Startup>();
            this.output = output;
        }

        public void Dispose()
        {
            _factory.Dispose();
            GC.SuppressFinalize(this);
        }

        [Fact]
        public async Task BatchJsonCommands()
        {
            var client = SetupClient();

            Guid guid = Guid.NewGuid();
            Guid guid2 = Guid.NewGuid();
            Guid guid3 = Guid.NewGuid();

            string insertJson = $@"
            [
              {{
                ""Bookstore.Book"": {{
                  ""Insert"": [
                    {{ ""ID"": ""{guid}"", ""Name"": ""__Test__This is a test book"" }},
                    {{ ""ID"": ""{guid2}"", ""Name"": ""__Test__This is the second test book"" }}
                  ]
                }}
              }}
            ]";

            string updateJson = $@"
            [
              {{
                ""Bookstore.Book"": {{
                  ""Delete"": [
                    {{ ""ID"": ""{guid}"" }}
                  ],
                  ""Update"": [
                    {{ ""ID"": ""{guid2}"", ""Name"": ""__Test__Updated name"" }}
                  ],
                  ""Insert"": [
                    {{ ""ID"": ""{guid3}"", ""Name"": ""__Test__This is a another book"" }}
                  ]
                }}
              }}
            ]";

            string deleteJson = $@"
            [
              {{
                ""Bookstore.Book"": {{
                  ""Delete"": [
                    {{ ""ID"": ""{guid2}"" }},
                    {{ ""ID"": ""{guid3}"" }}
                  ],
                }}
              }}
            ]";

            var response = await Execute(insertJson, client);
            Assert.True(response.IsSuccessStatusCode);

            await Execute(updateJson, client);
            Assert.True(response.IsSuccessStatusCode);

            await Execute(deleteJson, client);
            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task FailDoubleInsert()
        {
            var client = SetupClient();

            Guid guid = Guid.NewGuid();
            
            string insertJson = $@"
            [
              {{
                ""Bookstore.Book"": {{
                  ""Insert"": [
                    {{ ""ID"": ""{guid}"", ""Name"": ""__Test__This is a test book"" }},
                    {{ ""ID"": ""{guid}"", ""Name"": ""__Test__This is the second test book"" }}
                  ]
                }}
              }}
            ]";

            var response = await Execute(insertJson, client);
            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.False(response.IsSuccessStatusCode);
            Assert.StartsWith("{\"UserMessage\":\"Operation could not be completed because the request sent to the server was not valid or not properly formatted.\""
                    + ",\"SystemMessage\":\"Inserting a record that already exists in database.", responseContent);
        }


        private HttpClient SetupClient()
        {
            var logEntries = new LogEntries();
            return _factory
                .WithWebHostBuilder(builder => builder.MonitorLogging(logEntries))
                .CreateClient();
        }

        private async Task<HttpResponseMessage> Execute(string json, HttpClient client)
        {
            var content = new StringContent(json);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return await client.PostAsync("jc/write", content);
        }
    }
}
