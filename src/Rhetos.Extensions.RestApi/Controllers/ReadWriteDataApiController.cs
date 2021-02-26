using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Rhetos.Dom.DefaultConcepts;
using Rhetos.Extensions.RestApi.Metadata;
using Rhetos.Extensions.RestApi.Utilities;
using Rhetos.Processing;

namespace Rhetos.Extensions.RestApi.Controllers
{
    // We are using ActionResult<TResult> in each action and return JsonResult to circumvent JsonOutputFormatter bug
    // bug causes Actions which return TResult directly to ignore some serializer settings (e.g. MicrosoftDateTime)
    public class ReadWriteDataApiController<T> : ReadDataApiController<T>
    {
        public ReadWriteDataApiController(ServiceUtility serviceUtility, ControllerRestInfoRepository controllerRestInfoRepository)
            : base(serviceUtility, controllerRestInfoRepository)
        {
        }

        [HttpPost]
        public ActionResult<ProcessingResult> Insert([FromBody]T item)
        {
            /*
            // TODO: How to check this??
            if (!RestServiceMetadata.WritableDataStructures.Contains(typeof(TDataStructure).FullName))
                throw new ClientException($"Invalid request: '{typeof(TDataStructure).FullName}' is not writable.");
            */

            if (item == null)
                throw new ClientException("Invalid request: Missing the record data. The data should be provided in the request message body.");

            var entity = item as IEntity;

            if (Guid.Empty == entity.ID)
                entity.ID = Guid.NewGuid();
            
            serviceUtility.InsertData(item);
            return new JsonResult(new InsertDataResult() { ID = entity.ID});
        }

        [HttpPut]
        [Route("{id}")]
        public void Update(string id, [FromBody] T item)
        {
            /*
            if (!RestServiceMetadata.WritableDataStructures.Contains(typeof(TDataStructure).FullName))
                throw new Rhetos.ClientException($"Invalid request: '{{typeof(TDataStructure).FullName}}' is not writable.");
            */

            if (item == null)
                throw new ClientException("Invalid request: Missing the record data. The data should be provided in the request message body.");

            if (!Guid.TryParse(id, out Guid guid))
                throw new LegacyClientException("Invalid format of GUID parameter 'ID'.");

            var entity = item as IEntity;

            if (Guid.Empty == entity.ID)
                entity.ID = guid;
            if (guid != entity.ID)
                throw new LegacyClientException("Given entity ID is not equal to resource ID from URI.");

            serviceUtility.UpdateData(item);
        }

        [HttpDelete]
        [Route("{id}")]
        public void Delete(string id)
        {
            /*
            if (!RestServiceMetadata.WritableDataStructures.Contains(typeof(TDataStructure).FullName))
                throw new Rhetos.ClientException($""Invalid request: '{{typeof(TDataStructure).FullName}}' is not writable."");
            */
            if (!Guid.TryParse(id, out Guid guid))
                throw new LegacyClientException("Invalid format of GUID parameter 'ID'.");
            
            var item = Activator.CreateInstance<T>();
            (item as IEntity).ID = guid;

            serviceUtility.DeleteData(item);
        }
    }
}
