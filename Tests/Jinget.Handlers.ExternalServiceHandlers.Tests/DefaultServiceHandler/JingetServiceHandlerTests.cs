﻿using Jinget.Handlers.ExternalServiceHandlers.Tests.DefaultServiceHandler.SampleType;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
namespace Jinget.Handlers.ExternalServiceHandlers.DefaultServiceHandler.Tests
{
    [TestClass()]
    public class JingetServiceHandlerTests
    {
        [TestMethod()]
        public async Task should_call_get_restapi()
        {
            var jingetServiceHandler = new JingetServiceHandler<List<SampleGetResponse>>("https://jsonplaceholder.typicode.com");
            jingetServiceHandler.Events.ServiceCalled += (object sender, HttpResponseMessage e) =>
            {
                Assert.IsTrue(e.IsSuccessStatusCode);
            };
            jingetServiceHandler.Events.RawResponseReceived += (object sender, string e) =>
            {
                Assert.IsFalse(e == "");
            };
            jingetServiceHandler.Events.ExceptionOccurred += (object sender, Exception e) =>
            {
                Assert.IsTrue(e is null);
            };
            jingetServiceHandler.Events.ResponseDeserialized += (object sender, List<SampleGetResponse> e) =>
            {
                Assert.IsFalse(e is null);
            };

            var result = await jingetServiceHandler.GetAsync("users");

            Assert.IsFalse(result is null);
        }

        [TestMethod()]
        public async Task should_call_post_restapi()
        {
            var jingetServiceHandler = new JingetServiceHandler<SamplePostResponse>("https://jsonplaceholder.typicode.com", true);
            jingetServiceHandler.Events.ServiceCalled += (object sender, HttpResponseMessage e) =>
            {
                Assert.IsTrue(e.IsSuccessStatusCode);
            };
            jingetServiceHandler.Events.RawResponseReceived += (object sender, string e) =>
            {
                Assert.IsFalse(e == "");
            };
            jingetServiceHandler.Events.ExceptionOccurred += (object sender, Exception e) =>
            {
                Assert.IsTrue(e is null);
            };
            jingetServiceHandler.Events.ResponseDeserialized += (object sender, SamplePostResponse e) =>
            {
                Assert.IsFalse(e is null);
            };
            var result = await jingetServiceHandler
                .PostAsync("posts",
                new
                {
                    title = "foo",
                    body = "bar",
                    userId = 1,
                },
                new Dictionary<string, string>
                {
                    {"Content-type","application/json; charset=UTF-8" }
                });

            Assert.IsFalse(result is null);
        }

        [TestMethod()]
        public async Task should_call_send_restapi()
        {
            var jingetServiceHandler = new JingetServiceHandler<SamplePutResponse>("https://jsonplaceholder.typicode.com", true);
            jingetServiceHandler.Events.ServiceCalled += (object sender, HttpResponseMessage e) =>
            {
                Assert.IsTrue(e.IsSuccessStatusCode);
            };
            jingetServiceHandler.Events.RawResponseReceived += (object sender, string e) =>
            {
                Assert.IsFalse(e == "");
            };
            jingetServiceHandler.Events.ExceptionOccurred += (object sender, Exception e) =>
            {
                Assert.IsTrue(e is null);
            };
            jingetServiceHandler.Events.ResponseDeserialized += (object sender, SamplePutResponse e) =>
            {
                Assert.IsFalse(e is null);
            };

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri("https://jsonplaceholder.typicode.com/posts/1"),
                Method = HttpMethod.Put,
                Content = new StringContent(JsonConvert.SerializeObject(new
                {
                    id = 1,
                    title = "foo",
                    body = "bar",
                    userId = 1,
                }))
            };
            request.Headers.TryAddWithoutValidation("Content-type", "application/json; charset=UTF-8");

            var result = await jingetServiceHandler.SendAsync(request);

            Assert.IsFalse(result is null);
        }

        [TestMethod()]
        public async Task should_call_get_soap()
        {
            var jingetServiceHandler = new JingetServiceHandler<AddResponse>("http://www.dneonline.com/calculator.asmx");
            jingetServiceHandler.Events.ServiceCalled += (object sender, HttpResponseMessage e) =>
            {
                Assert.IsTrue(e.IsSuccessStatusCode);
            };
            jingetServiceHandler.Events.RawResponseReceived += (object sender, string e) =>
            {
                Assert.IsFalse(e == "");
            };
            jingetServiceHandler.Events.ExceptionOccurred += (object sender, Exception e) =>
            {
                Assert.IsTrue(e is null);
            };
            jingetServiceHandler.Events.ResponseDeserialized += (object sender, AddResponse e) =>
            {
                Assert.IsFalse(e is null);
            };

            var (envelope, request) = new SampleSOAPRequest().CreateEnvelope();
            envelope.Body.Add = new SampleSOAPRequest.SampleSOAPGet { intA = 1, intB = 2 };

            var result = await jingetServiceHandler.PostAsync(envelope.ToString(), new Dictionary<string, string>
            {
                {"Content-Type","text/xml" },
                {"SOAPAction","http://tempuri.org/Add" }
            });

            Assert.IsFalse(result is null);
            Assert.AreEqual(3, result.AddResult);
        }

        //[TestMethod]
        public async Task should_post_multipart_formdata()
        {
            var jingetServiceHandler = new JingetServiceHandler<SamplePostResponse>("https://localhost:7027/api/upload", true);
            jingetServiceHandler.Events.ServiceCalled += (object sender, HttpResponseMessage e) =>
            {
                Assert.IsTrue(e.IsSuccessStatusCode);
            };
            jingetServiceHandler.Events.ResponseDeserialized += (object sender, SamplePostResponse e) =>
            {
                Assert.IsFalse(e is null);
            };

            List<FileInfo> files = new() {
                new FileInfo("Sample Upload File1.txt") ,
                new FileInfo("Sample Upload File2.txt")
            };

            var response = await jingetServiceHandler.UploadFileAsync("something", files);

            Assert.IsFalse(string.IsNullOrWhiteSpace(response.Status));
        }
    }
}
#pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).