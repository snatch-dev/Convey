using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;
using Convey.Persistence.OpenStack.OCS.Http;
using Convey.Persistence.OpenStack.OCS.OcsTypes;
using Convey.Persistence.OpenStack.OCS.OcsTypes.Definition;
using Convey.Persistence.OpenStack.OCS.OpenStack.Responses;
using Convey.Persistence.OpenStack.OCS.RequestHandler;
using Newtonsoft.Json;

namespace Convey.Persistence.OpenStack.OCS.Client
{
    internal class OcsClient : IOcsClient
    {
        private readonly IRequestHandler _requestHandler;
        private readonly OcsOptions _ocsOptions;
       
        private OcsClient() { }
        public OcsClient(IRequestHandler requestHandler, OcsOptions ocsOptions)
        {
            _requestHandler = requestHandler;
            _ocsOptions = ocsOptions;
        }

        public async Task<IOperationResult<byte[]>> GetObjectAsByteArray(params string[] relativePath)
        {
            var result = await _requestHandler.Send(builder => builder
                    .WithMethod(HttpMethod.Get)
                    .WithHeader("Content-Type", MediaTypeNames.Application.Octet)
                    .WithRelativeUrl(GetPath(relativePath)));

            var validationResult = ValidateHttpResult(result);

            return validationResult == OperationStatus.Success ?
                new OperationResult<byte[]>(validationResult, await result.Content.ReadAsByteArrayAsync()) :
                new OperationResult<byte[]>(validationResult);
        }

        public async Task<IOperationResult<Stream>> GetObjectAsStream(params string[] relativePath)
        {
            var result = await _requestHandler.Send(builder => builder
                    .WithMethod(HttpMethod.Get)
                    .WithHeader("Content-Type", MediaTypeNames.Application.Octet)
                    .WithRelativeUrl(GetPath(relativePath)));

            var validationResult = ValidateHttpResult(result);

            return validationResult == OperationStatus.Success ?
                new OperationResult<Stream>(validationResult, await result.Content.ReadAsStreamAsync()) :
                new OperationResult<Stream>(validationResult);
        }

        public async Task<IOperationResult<string>> GetObjectAsBase64String(params string[] relativePath)
        {
            var result = await _requestHandler.Send(builder => builder
                    .WithMethod(HttpMethod.Get)
                    .WithRelativeUrl(GetPath(relativePath)));

            var validationResult = ValidateHttpResult(result);

            return validationResult == OperationStatus.Success ?
                new OperationResult<string>(validationResult, Convert.ToBase64String(await result.Content.ReadAsByteArrayAsync())) :
                new OperationResult<string>(validationResult);
        }

        public async Task<IOperationResult<List<OcsObjectMetadata>>> GetDirectoryList(params string[] relativePath)
        {
            var result = await _requestHandler.Send(builder => builder
                    .WithRelativeUrl(GetPath(relativePath))
                    .WithMethod(HttpMethod.Get));

            var validationResult = ValidateHttpResult(result);

            if (validationResult != OperationStatus.Success)
            {
                return new OperationResult<List<OcsObjectMetadata>>(validationResult);
            }

            var openStackResponse = JsonConvert.DeserializeObject<List<ObjectMetadata>>(await result.Content.ReadAsStringAsync());
            return new OperationResult<List<OcsObjectMetadata>>(validationResult, ObjectMapper.Map(openStackResponse).ToList());
        }

        public async Task<IOperationResult> UploadAsStream(Stream stream, params string[] relativePath)
        {
            var result = await _requestHandler.Send(builder => builder
                    .WithRelativeUrl(GetPath(relativePath))
                    .WithHeader("Content-Type", MediaTypeNames.Application.Octet)
                    .WithMethod(HttpMethod.Put)
                    .WithStreamContent(stream));

            return new OperationResult(ValidateHttpResult(result));
        }

        public async Task<IOperationResult> UploadAsBase64String(string base64String, params string[] relativePath)
        {
            var result = await _requestHandler.Send(builder => builder
                    .WithRelativeUrl(GetPath(relativePath))
                    .WithHeader("Content-Type", MediaTypeNames.Application.Octet)
                    .WithMethod(HttpMethod.Put)
                    .WithStreamContent(new MemoryStream(Convert.FromBase64String(base64String))));

            return new OperationResult(ValidateHttpResult(result));
        }

        public async Task<IOperationResult> CopyInternally(string relativeSourcePath, string relativeDestinationPath)
        {
            var result = await _requestHandler.Send(builder => builder
                    .WithHeader("Destination", $"{_ocsOptions.RootDirectory}/{relativeDestinationPath}")
                    .WithRelativeUrl(GetPath(relativeSourcePath))
                    .WithMethod(HttpMethodExtended.Copy));

            return new OperationResult(ValidateHttpResult(result));
        }

        public async Task<IOperationResult> Delete(params string[] relativePath)
        {
            var result = await _requestHandler.Send(builder => builder
                    .WithRelativeUrl(GetPath(relativePath))
                    .WithMethod(HttpMethod.Delete));

            return new OperationResult(ValidateHttpResult(result));
        }

        private OperationStatus ValidateHttpResult(HttpRequestResult result)
        {
            if (result.IsSuccess)
            {
                return OperationStatus.Success;
            }

            if (result.Exception is null)
            {
                switch (result.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        return OperationStatus.NotFound;
                }
            }

            return OperationStatus.Failed;
        }

        private string GetPath(string[] relativePath)
            => $"/{_ocsOptions.ProjectRelativeUrl}/{_ocsOptions.RootDirectory}/{string.Join('/', relativePath)}";

        private string GetPath(string relativePath)
            => $"/{_ocsOptions.ProjectRelativeUrl}/{_ocsOptions.RootDirectory}/{relativePath}";
    }
}
