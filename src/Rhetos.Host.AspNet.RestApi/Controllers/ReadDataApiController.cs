using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Rhetos.Dom.DefaultConcepts;
using Rhetos.Host.AspNet.RestApi.Metadata;
using Rhetos.Host.AspNet.RestApi.Utilities;
using Rhetos.Processing;

namespace Rhetos.Host.AspNet.RestApi.Controllers
{
    // We are using ActionResult<TResult> in each action and return JsonResult to circumvent JsonOutputFormatter bug
    // bug causes Actions which return TResult directly to ignore some serializer settings (e.g. MicrosoftDateTime)
    public class ReadDataApiController<T> : RhetosApiControllerBase<T>
    {
        protected readonly ServiceUtility serviceUtility;
        protected readonly Lazy<Tuple<string, Type>[]> dataStructureParameters;

        public ReadDataApiController(ServiceUtility serviceUtility, ControllerRestInfoRepository controllerRestInfoRepository)
        {
            this.serviceUtility = serviceUtility;
            dataStructureParameters = new Lazy<Tuple<string, Type>[]>(() =>
            {
                var dataStructureInfoMetadata = controllerRestInfoRepository.ControllerConceptInfo[this.GetType()] as DataStructureInfoMetadata;
                if (dataStructureInfoMetadata == null)
                    throw new InvalidOperationException(
                        $"Registered {nameof(ConceptInfoRestMetadata)} for {GetType().Name} should be an instance of {nameof(DataStructureInfoMetadata)}.");
                return dataStructureInfoMetadata.GetParameters();
            });
        }

        /// <remarks>
        /// Obsolete parameters: filter, fparam, genericfilter (use filters), page, psize (use top and skip).
        /// </remarks>
        [HttpGet]
        public ActionResult<RecordsResult<T>> Get(string filter = null, string fparam = null, string genericfilter = null, string filters = null,
            int top = 0, int skip = 0, int page = 0, int psize = 0, string sort = null)
        {
            var data = serviceUtility.GetData<T>(filter, fparam, genericfilter, filters, dataStructureParameters.Value, top, skip, page, psize, sort,
                readRecords: true, readTotalCount: false);
            return new JsonResult(new RecordsResult<T>() { Records = data.Records });
        }

        [Obsolete("Use GetTotalCount instead.")]
        [HttpGet]
        [Route("Count")]
        public ActionResult<CountResult> GetCount(string filter, string fparam, string genericfilter, string filters, string sort)
        {
            var data = serviceUtility.GetData<T>(filter, fparam, genericfilter, filters, dataStructureParameters.Value, 0, 0, 0, 0, sort,
                readRecords: false, readTotalCount: true);
            return new JsonResult(new CountResult {TotalRecords = data.TotalCount });
        }

        /// <remarks>
        /// Obsolete parameters: filter, fparam, genericfilter (use filters).
        /// </remarks>
        [HttpGet]
        [Route("TotalCount")]
        public ActionResult<TotalCountResult> GetTotalCount(string filter, string fparam, string genericfilter, string filters, string sort)
        {
            var data = serviceUtility.GetData<T>(filter, fparam, genericfilter, filters, dataStructureParameters.Value, 0, 0, 0, 0, sort,
                readRecords: false, readTotalCount: true);
            return new JsonResult(new TotalCountResult { TotalCount = data.TotalCount });
        }

        /// <remarks>
        /// Obsolete parameters: filter, fparam, genericfilter (use filters), page, psize (use top and skip).
        /// </remarks>
        [HttpGet]
        [Route("RecordsAndTotalCount")]
        public ActionResult<RecordsAndTotalCountResult<T>> GetRecordsAndTotalCount(string filter, string fparam, string genericfilter, string filters, int top, int skip, int page, int psize,
            string sort)
        {
            var result = serviceUtility.GetData<T>(filter, fparam, genericfilter, filters, dataStructureParameters.Value, top, skip, page, psize, sort,
                readRecords: true, readTotalCount: true);

            return new JsonResult(result);
        }

        [HttpGet]
        [Route("{id}")]
        public ActionResult<T> GetById(string id)
        {
            var result = serviceUtility.GetDataById<T>(id);
            if (result == null)
                throw new LegacyClientException("There is no resource of this type with a given ID.") {HttpStatusCode = HttpStatusCode.NotFound, Severe = false};
            return new JsonResult(result);
        }
    }
}
