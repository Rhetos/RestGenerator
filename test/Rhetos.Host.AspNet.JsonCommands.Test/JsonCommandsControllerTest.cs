using Microsoft.AspNetCore.Mvc.Testing;
using Rhetos.Host.AspNet.JsonCommands.Test.Tools;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using TestApp;
using Xunit;
using Xunit.Abstractions;

namespace Rhetos.Host.AspNet.JsonCommands.Tests
{
    public class JsonCommandsControllerTests : IDisposable, IClassFixture<JsonCommandsTestCleanup>
    {
        private readonly WebApplicationFactory<Startup> _factory;
#pragma warning disable IDE0052 // Remove unread private members
        private readonly JsonCommandsTestCleanup _cleanup; // The 'cleanup' instance is used for its constructor and Dispose method for setup and teardown logic. It is injected by xUnit and not directly used in the test methods.
#pragma warning restore IDE0052 // Remove unread private members

        public JsonCommandsControllerTests(JsonCommandsTestCleanup cleanup)
        {
            _factory = new CustomWebApplicationFactory<Startup>();
            _cleanup = cleanup;
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
