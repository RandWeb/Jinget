﻿using Jinget.Core.Utilities;
using Jinget.Handlers.ExternalServiceHandlers.ServiceHandler;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Jinget.Handlers.ExternalServiceHandlers.DefaultServiceHandler
{
    public class JingetServiceHandler<TResponseModel> : ServiceHandler<JingetServiceHandlerEvents<TResponseModel>> where TResponseModel : class, new()
    {
        public JingetServiceHandler(string baseUri, bool ignoreSslErrors = false, Dictionary<string, string> headers = null) : base(baseUri, ignoreSslErrors, headers) { }

        private async Task<TResponseModel> ProcessTask(Func<Task<HttpResponseMessage>> task)
        {
            TResponseModel responseModel = null;
            try
            {
                var response = await task();
                Events.OnServiceCalled(response);
                response.EnsureSuccessStatusCode();

                string rawResponse = await response.Content.ReadAsStringAsync();
                Events.OnRawResponseReceived(rawResponse);

                switch (response.Content.Headers.ContentType.MediaType)
                {
                    case MediaTypeNames.Application.Json:
                        responseModel = JsonConvert.DeserializeObject<TResponseModel>(rawResponse);
                        break;
                    case MediaTypeNames.Application.Xml:
                    case MediaTypeNames.Text.Xml:
                        responseModel = XmlUtility.DeserializeXmlDescendantsFirst<TResponseModel>(rawResponse);
                        break;
                    default:
                        break;
                }

                Events.OnResponseDeserialized(responseModel);
            }
            catch (Exception ex)
            {
                Events.OnExceptionOccurred(ex);
            }
            return responseModel;

        }
        public async Task<TResponseModel> GetAsync(string url, Dictionary<string, string> headers = null)
            => await ProcessTask(async () => await HttpClientFactory.GetAsync(url, headers));

        public async Task<TResponseModel> PostAsync(string url, object content = null, Dictionary<string, string> headers = null)
            => await ProcessTask(async () => await HttpClientFactory.PostAsync(url, content, headers));

        public async Task<TResponseModel> SendAsync(HttpRequestMessage message) => await ProcessTask(async () => await HttpClientFactory.SendAsync(message));
    }
}