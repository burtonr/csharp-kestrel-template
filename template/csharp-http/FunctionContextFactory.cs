using System;
using System.Collections.Specialized;
using System.IO;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using OpenFaaS.FunctionSDK;

namespace root
{
	public static class FunctionContextFactory
	{
		public static FunctionContext Create(IServiceProvider provider)
		{
			// Ideally this should use constructor injection in FunctionContext instead...
			var context = provider.GetRequiredService<IHttpContextAccessor>().HttpContext;

			// extract the request information
			var requestBody = getRequest(context);
			var requestHeaders = getHeaders(context);
			var requestQueries = getQuery(context);

			// set the context to pass to the function
			return new FunctionContext
			{
				Body = requestBody,
				Headers = requestHeaders,
				Method = new HttpMethod(context.Request.Method),
				QueryString = requestQueries
			};
		}

		private static object getRequest(HttpContext context)
		{
			StreamReader reader = new StreamReader(context.Request.Body);
			string sBody = reader.ReadToEnd();
			object body = sBody;

			// If the request is JSON, convert it to a JObject for easy deserialization in the handler
			if (sBody.Length > 0 && sBody[0] == '{')
			{
				body = JsonConvert.DeserializeObject(sBody);
			}

			return body;
		}

		private static NameValueCollection getHeaders(HttpContext context)
		{
			var results = new NameValueCollection();
			var headers = context.Request.Headers;
			foreach (var h in headers)
			{
				results.Add(h.Key, string.Join('|', h.Value));
			}
			return results;
		}

		private static NameValueCollection getQuery(HttpContext context)
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
