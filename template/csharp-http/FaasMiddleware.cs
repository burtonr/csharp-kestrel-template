using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FaasUtils;
using Function;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using OpenFaaS.FunctionSDK;

namespace root
{
	public class FaasMiddleware
	{
		private readonly Func<FunctionHandler, IServiceProvider, Task<object>> _function;
		private readonly FunctionHandler _instance;

		public FaasMiddleware(RequestDelegate next, IServiceProvider services, IFunctionExpressionTreeBuilder funcBuilder)
		{
			_instance = ActivatorUtilities.CreateInstance<FunctionHandler>(services);
			_function = funcBuilder.CreateLambda<FunctionHandler>();
		}

		public async Task InvokeAsync(HttpContext context, FunctionContext fnContext, IServiceProvider services)
		{
			try
			{
				// execute the function
				var result = (FunctionResponse) await _function(_instance, services);

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
		}
	}
}
