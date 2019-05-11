using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Function;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using OpenFaaS.FunctionSDK;

namespace root
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Run(async (context) =>
            {
                var functionHandler = new FunctionHandler();
                try
                {
                    // extract the request information
                    var requestBody = getRequest(context);
                    var requestHeaders = getHeaders(context);
                    var requestQueries = getQuery(context);

                    // set the context to pass to the function
                    var fnContext = new FunctionContext
                    {
                        Body = requestBody,
                        Headers = requestHeaders,
                        Method = new HttpMethod(context.Request.Method),
                        QueryString = requestQueries
                    };

                    // execute the function
                    var result = await functionHandler.Handle(fnContext);
                    
                    // set the response from the FunctionResponse
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = result.StatusCode.ToStatusInt();

                    // if headers were set, convert to the proper type and set the response
                    if (result.Headers != null && result.Headers.Count > 0)
                    {
                        foreach (var head in result.Headers.AllKeys)
                        {
                            context.Response.Headers.Add(new KeyValuePair<string, StringValues>(head, result.Headers[head]));
                        }
                    }

                    // set the response body. Assume object if it's not a stream
                    if (result.Body.GetType() == typeof(Stream))
                    {
                        context.Response.Body = (Stream)result.Body;
                    }
                    else
                    {
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(result.Body));
                    }
                }
                catch (Exception ex)
                {
                    // if there's an error, return appropriate response code and the exception message
                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "application/json";                    
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(new { Error = ex.Message }));
                }
            });
        }

        private object getRequest(HttpContext context)
        {
            StreamReader reader = new StreamReader(context.Request.Body);
            string sBody = reader.ReadToEnd();
            object body = sBody;

            // If the request is JSON, convert it to a JObject for easy deserialization in the handler
            if (sBody[0] == '{')
            {
                body = JsonConvert.DeserializeObject(sBody);
            }

            return body;
        }

        private NameValueCollection getHeaders(HttpContext context)
        {
            var results = new NameValueCollection();
            var headers = context.Request.Headers;
            foreach (var h in headers)
            {
                results.Add(h.Key, string.Join('|', h.Value));
            }
            return results;
        }

        private NameValueCollection getQuery(HttpContext context)
        {
            var queries = new NameValueCollection();
            var reqQuery = context.Request.Query;

            foreach (var q in reqQuery)
            {
                queries.Add(q.Key, string.Join('|', q.Value));
            }
            return queries;
        }
    }
}
