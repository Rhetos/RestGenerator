/*
    Copyright (C) 2014 Omega software d.o.o.

    This file is part of Rhetos.

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using Rhetos.Host.AspNet.JsonCommands.Test.Tools;
using System;
using System.Linq;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using TestAction;
using TestApp;
using Xunit;
using Xunit.Abstractions;

namespace Rhetos.Host.AspNet.JsonCommands.Test
{
    public class ErrorReportingTest : IDisposable
    {
        // TODO: It seems that new RhetosHost is created for each test method run, which hinders test performance.
        private readonly WebApplicationFactory<Startup> _factory;
        private readonly ITestOutputHelper output;

        public ErrorReportingTest(ITestOutputHelper output)
        {
            _factory = new CustomWebApplicationFactory<Startup>();
            this.output = output;
        }

        public void Dispose()
        {
            _factory.Dispose();
            GC.SuppressFinalize(this);
        }

        [Theory]
        [InlineData("test1", "test2",
            @"400 {""UserMessage"":""test1"",""SystemMessage"":""test2""}",
            "[Trace] Rhetos.Host.AspNet.JsonCommands.Filters.ApiExceptionFilter:|Rhetos.UserException: test1|SystemMessage: test2")]
        [InlineData("test1", null,
            @"400 {""UserMessage"":""test1"",""SystemMessage"":null}",
            "[Trace] Rhetos.Host.AspNet.JsonCommands.Filters.ApiExceptionFilter:|Rhetos.UserException: test1")]
        [InlineData(null, null,
            @"400 {""UserMessage"":""Exception of type 'Rhetos.UserException' was thrown."",""SystemMessage"":null}",
            "[Trace] Rhetos.Host.AspNet.JsonCommands.Filters.ApiExceptionFilter:|Rhetos.UserException: Exception of type 'Rhetos.UserException' was thrown.")]
        public async Task UserExceptionResponse(string testUserMessage, string testSystemMessage, string expectedResponse, string expectedLogPatterns)
        {
            var logEntries = new LogEntries();
            var client = _factory
                .WithWebHostBuilder(builder => builder.MonitorLogging(logEntries, LogLevel.Trace))
                .CreateClient();
            var requestData = new ReturnUserError { TestUserMessage = testUserMessage, TestSystemMessage = testSystemMessage };
            var response = await client.PostAsync("rest/TestAction/ReturnUserError", JsonContent.Create(requestData));
            string responseContent = await response.Content.ReadAsStringAsync();

            Assert.Equal<object>(expectedResponse, $"{(int)response.StatusCode} {responseContent}");

            output.WriteLine(string.Join(Environment.NewLine, logEntries.Where(e => e.Message.Contains("Exception"))));
            string apiExceptionLog = logEntries.Select(e => e.ToString()).Where(e => e.Contains("ApiExceptionFilter")).Single();
            foreach (var pattern in expectedLogPatterns.Split('|'))
                Assert.Contains(pattern, apiExceptionLog);
        }

        [Fact]
        public async Task LocalizedUserException()
        {
            var logEntries = new LogEntries();
            var client = _factory
                .WithWebHostBuilder(builder => builder.MonitorLogging(logEntries, LogLevel.Trace))
                .CreateClient();
            var requestData = new ReturnLocalizedUserError { TestUserMessage = "TestErrorMessage {0}", MessageParameterCount = 1 };
            var response = await client.PostAsync("rest/TestAction/ReturnLocalizedUserError", JsonContent.Create(requestData));
            string responseContent = await response.Content.ReadAsStringAsync();

            Assert.Equal(@"400 {""UserMessage"":""TestErrorMessage 1000"",""SystemMessage"":null}", $"{(int)response.StatusCode} {responseContent}");

            output.WriteLine(string.Join(Environment.NewLine, logEntries.Where(e => e.Message.Contains("Exception"))));
            string[] exceptedLogPatterns = new[]
            {
                "[Trace] Rhetos.Host.AspNet.JsonCommands.Filters.ApiExceptionFilter:",
                "Rhetos.UserException: TestErrorMessage 1000",
            };
            Assert.Equal(1, logEntries.Select(e => e.ToString()).Count(
                // The command summary is not reported by ProcessingEngine for UserExceptions to improved performance.
                entry => exceptedLogPatterns.All(pattern => entry.Contains(pattern))));
        }

        [Fact]
        public async Task LocalizedUserExceptionInvalidFormat()
        {
            var logEntries = new LogEntries();
            var client = _factory
                .WithWebHostBuilder(builder => builder.MonitorLogging(logEntries, LogLevel.Trace))
                .CreateClient();
            var requestData = new ReturnLocalizedUserError { TestUserMessage = "TestErrorMessage {0} {1}", MessageParameterCount = 1 };
            var response = await client.PostAsync("rest/TestAction/ReturnLocalizedUserError", JsonContent.Create(requestData));
            string responseContent = await response.Content.ReadAsStringAsync();

            Assert.StartsWith(
                @"500 {""UserMessage"":null,""SystemMessage"":""Internal server error occurred. See server log for more information. (ArgumentException, ",
                $"{(int)response.StatusCode} {responseContent}");

            Assert.DoesNotContain(@"TestErrorMessage", $"{(int)response.StatusCode} {responseContent}");
            Assert.DoesNotContain(@"1000", $"{(int)response.StatusCode} {responseContent}");

            output.WriteLine(string.Join(Environment.NewLine, logEntries.Where(e => e.Message.Contains("Exception"))));
            string[] exceptedLogPatterns = new[]
            {
                "[Error] Rhetos.Host.AspNet.JsonCommands.Filters.ApiExceptionFilter",
                "System.ArgumentException: Invalid error message format. Message: \"TestErrorMessage {0} {1}\", Parameters: \"1000\". Index (zero based) must be greater than or equal to zero and less than the size of the argument list.",
            };
            Assert.Equal(1, logEntries.Select(e => e.ToString()).Count(
                // The command summary is not reported by ProcessingEngine for UserExceptions to improved performance.
                entry => exceptedLogPatterns.All(pattern => entry.Contains(pattern))));
        }

        [Fact]
        public async Task ClientExceptionResponse()
        {
            var logEntries = new LogEntries();
            var client = _factory
                .WithWebHostBuilder(builder => builder.MonitorLogging(logEntries))
                .CreateClient();
            var response = await client.PostAsync("rest/TestAction/ReturnClientError", JsonContent.Create(new { }));
            string responseContent = await response.Content.ReadAsStringAsync();

            Assert.Equal<object>(
                "400 {\"UserMessage\":\"Operation could not be completed because the request sent to the server was not valid or not properly formatted.\""
                    + ",\"SystemMessage\":\"test exception\"}",
                $"{(int)response.StatusCode} {responseContent}");

            output.WriteLine(string.Join(Environment.NewLine, logEntries));
            string[] exceptedLogPatterns = new[] {
                "[Information] Rhetos.Host.AspNet.JsonCommands.Filters.ApiExceptionFilter:",
                "Rhetos.ClientException: test exception",
                "Command: ExecuteActionCommandInfo TestAction.ReturnClientError" };
            Assert.Equal(1, logEntries.Select(e => e.ToString()).Count(
                entry => exceptedLogPatterns.All(pattern => entry.Contains(pattern))));
        }

        [Fact]
        public async Task ServerExceptionResponse()
        {
            var logEntries = new LogEntries();
            var client = _factory
                .WithWebHostBuilder(builder => builder.MonitorLogging(logEntries))
                .CreateClient();

            Guid[] ids = new Guid[] {
                new Guid("15a1b223-aa2b-448a-9ddd-b4384188c489"),
                new Guid("25a1b223-aa2b-448a-9ddd-b4384188c489") };
            var response = await client.GetAsync(
                $"rest/TestAction/ReturnServerError/?filters=[{{\"Filter\":\"Guid[]\",\"Value\":{JsonSerializer.Serialize(ids)}}}]");
            string responseContent = await response.Content.ReadAsStringAsync();

            Assert.StartsWith
                ("500 {\"UserMessage\":null,\"SystemMessage\":\"Internal server error occurred. See server log for more information. (ArgumentException, " + DateTime.Now.ToString("yyyy-MM-dd"),
                $"{(int)response.StatusCode} {responseContent}");

            output.WriteLine(string.Join(Environment.NewLine, logEntries));
            string[] exceptedLogPatterns = new[] {
                "[Error] Rhetos.Host.AspNet.JsonCommands.Filters.ApiExceptionFilter:",
                "System.ArgumentException: test exception",
                "Command: ReadCommandInfo TestAction.ReturnServerError records, filters: Guid[] \"2 items: 15a1b223-aa2b-448a-9ddd-b4384188c489 ...\"" };
            Assert.Equal(1, logEntries.Select(e => e.ToString()).Count(
                entry => exceptedLogPatterns.All(pattern => entry.Contains(pattern))));
        }

        [Fact]
        public async Task InvalidWebRequestFormatResponse()
        {
            var logEntries = new LogEntries();
            var client = _factory
                .WithWebHostBuilder(builder => builder.MonitorLogging(logEntries))
                .CreateClient();

            var response = await client.GetAsync("rest/TestAction/ReturnServerError/?filters=[{0}");
            string responseContent = await response.Content.ReadAsStringAsync();

            Assert.Equal<object>(
                "400 {\"UserMessage\":\"Operation could not be completed because the request sent to the server was not valid or not properly formatted.\""
                    + ",\"SystemMessage\":\"The provided filter parameter has invalid JSON format. See server log for more information.\"}",
                $"{(int)response.StatusCode} {responseContent}");

            output.WriteLine(string.Join(Environment.NewLine, logEntries));
            string[] exceptedLogPatterns = new[] {
                "[Information]",
                "Rhetos.ClientException: The provided filter",
                "Invalid JavaScript property identifier character: }. Path '[0]', line 1, position 3.",
                "Filter parameter: '[{0}'.",
                // The command summary is not reported by ProcessingEngine, because the ClientException occurred before the command was constructed.
            };
            Assert.Equal(1, logEntries.Select(e => e.ToString()).Count(
                entry => exceptedLogPatterns.All(pattern => entry.Contains(pattern))));
        }
    }
}
