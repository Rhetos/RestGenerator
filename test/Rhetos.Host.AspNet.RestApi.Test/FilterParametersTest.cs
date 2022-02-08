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
using Rhetos.Host.AspNet.RestApi.Test.Tools;
using System;
using System.Net;
using System.Threading.Tasks;
using TestApp;
using Xunit;
using Xunit.Abstractions;

namespace Rhetos.Host.AspNet.RestApi.Test
{
    public class FilterParametersTest : IDisposable
    {
        private readonly WebApplicationFactory<Startup> _factory;
        private readonly ITestOutputHelper output;

        public FilterParametersTest(ITestOutputHelper output)
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

        // Generic filter:
        [InlineData("rest/Common/Claim/?filters=[{\"Property\":\"ID\",\"Operation\":\"in\",\"Value\":[\"4a5c23ff-6525-4e17-b848-185e082a6974\",\"0f6618b1-074a-482f-be74-c6e394641209\"]}]")]

        // Specific filter with simplified class name:
        [InlineData("rest/Common/Claim/?filters=[{\"Filter\":\"Guid[]\",\"Value\":[\"4a5c23ff-6525-4e17-b848-185e082a6974\",\"0f6618b1-074a-482f-be74-c6e394641209\"]}]")]
        [InlineData("rest/Common/Claim/?filters=[{\"Filter\":\"System.Guid[]\",\"Value\":[\"4a5c23ff-6525-4e17-b848-185e082a6974\",\"0f6618b1-074a-482f-be74-c6e394641209\"]}]")]
        [InlineData("rest/Common/Claim/?filters=[{\"Filter\":\"System.Collections.Generic.IEnumerable`1[[System.Guid]]\",\"Value\":[\"4a5c23ff-6525-4e17-b848-185e082a6974\",\"0f6618b1-074a-482f-be74-c6e394641209\"]}]")]

        // Legacy filters:
        [InlineData("rest/Common/Claim/?filter=Guid[]&fparam=[\"4a5c23ff-6525-4e17-b848-185e082a6974\",\"0f6618b1-074a-482f-be74-c6e394641209\"]")]
        [InlineData("rest/Common/Claim/?filter=System.Guid[]&fparam=[\"4a5c23ff-6525-4e17-b848-185e082a6974\",\"0f6618b1-074a-482f-be74-c6e394641209\"]")]
        [InlineData("rest/Common/Claim/?filter=System.Collections.Generic.IEnumerable`1[[System.Guid]]&fparam=[\"4a5c23ff-6525-4e17-b848-185e082a6974\",\"0f6618b1-074a-482f-be74-c6e394641209\"]")]

        // Specific filter with parameter name:
        [InlineData("rest/Common/MyClaim/?filters=[{\"Filter\":\"Common.Claim\",\"Value\":{\"ClaimResource\":\"Common.RolePermission\",\"ClaimRight\":\"Read\"}}]")]
        [InlineData("rest/Common/MyClaim/?filters=[{\"Filter\":\"Claim\",\"Value\":{\"ClaimResource\":\"Common.RolePermission\",\"ClaimRight\":\"Read\"}}]")]

        // Deactivatable:
        // (THIS TEST REQUIRES Rhetos.CommonConceptsTest)
        [InlineData("rest/TestDeactivatable/BasicEnt/")]
        [InlineData("rest/TestDeactivatable/BasicEnt/?filters=[{\"Filter\":\"Rhetos.Dom.DefaultConcepts.ActiveItems,%20Rhetos.Dom.DefaultConcepts.Interfaces\"}]")]
        [InlineData("rest/TestDeactivatable/BasicEnt/?filters=[{\"Filter\":\"Rhetos.Dom.DefaultConcepts.ActiveItems\"}]")]
        [InlineData("rest/TestDeactivatable/BasicEnt/?filters=[{\"Filter\":\"ActiveItems\"}]")]

        // DateTime (old MS format):
        [InlineData("rest/TestHistory/Standard/")]
        [InlineData("rest/TestHistory/Standard/?filters=[{\"Filter\":\"System.DateTime\",\"Value\":\"/Date(1544195644420%2B0100)/\"}]")]

        public async Task SupportedFilterParameters(string url)
        {
            var logEntries = new LogEntries();
            var client = _factory
                .WithWebHostBuilder(builder => builder.MonitorLogging(logEntries))
                .CreateClient();
            var response = await client.GetAsync(url);
            string responseContent = await response.Content.ReadAsStringAsync();

            output.WriteLine(responseContent);
            output.WriteLine(string.Join(Environment.NewLine, logEntries));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
