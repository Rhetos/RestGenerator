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

using System;
using System.ComponentModel.Composition;
using Rhetos.Compiler;
using Rhetos.Dsl;
using Rhetos.Dsl.DefaultConcepts;
using Rhetos.Extensibility;
using Rhetos.RestGenerator;

namespace Rhetos.RestGenerator.Plugins
{
    /// <summary>
    /// This is not exported, but called from DataStructureCodeGenerator if exists.
    /// </summary>
    public class WritableOrmDataStructureCodeGenerator
    {      
        private const string ImplementationCodeSnippet = @"
        [OperationContract]
        [WebInvoke(Method = ""POST"", UriTemplate = """", BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public InsertDataResult Insert{0}{1}(System.IO.Stream reqStream)
        {{            
            {0}.{1} entity = null;
            using (StreamReader streamReader = new StreamReader(reqStream))
            {{
                entity = Newtonsoft.Json.JsonConvert.DeserializeObject<{0}.{1}>(streamReader.ReadToEnd());
            }}

            if (Guid.Empty == entity.ID)
                entity.ID = Guid.NewGuid();

            var result = _serviceUtility.InsertData(entity);
            return new InsertDataResult {{ ID = entity.ID }};
        }}

        [OperationContract]
        [WebInvoke(Method = ""PUT"", UriTemplate = """", BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public void Update{0}{1}(System.IO.Stream reqStream)
        {{
            {0}.{1} entity = null;
            using (StreamReader streamReader = new StreamReader(reqStream))
            {{
                entity = Newtonsoft.Json.JsonConvert.DeserializeObject<{0}.{1}>(streamReader.ReadToEnd());
            }}
            if (Guid.Empty == entity.ID)
                throw new Rhetos.LegacyClientException(""Given entity must have valid ID."");

            _serviceUtility.UpdateData(entity);
        }}

        [OperationContract]
        [WebInvoke(Method = ""DELETE"", UriTemplate = ""{{id}}"", BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public void Delete{0}{1}(string id)
        {{
            Guid guid;
            if (!Guid.TryParse(id, out guid))
                throw new Rhetos.LegacyClientException(""Invalid format of GUID parametar 'ID'."");
            var entity = new {0}.{1} {{ ID = guid }};

            _serviceUtility.DeleteData(entity);
        }}

";

        public static void GenerateCode(IConceptInfo conceptInfo, ICodeBuilder codeBuilder)
        {
            DataStructureInfo info = (DataStructureInfo) conceptInfo;

            if (info is IWritableOrmDataStructure)
            {
                codeBuilder.InsertCode(
                    String.Format(ImplementationCodeSnippet, info.Module.Name, info.Name),
                    DataStructureCodeGenerator.AdditionalOperationsTag.Evaluate(info));
            }
        }
    }
}