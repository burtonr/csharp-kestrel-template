﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Function;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using OpenFaaS.FunctionSDK;

namespace root
{
	public class FaasMiddleware
	{
		public FaasMiddleware(RequestDelegate next)
		{
		}

		public async Task InvokeAsync(HttpContext context, FunctionContext fnContext)
		{
			var functionHandler = new FunctionHandler();
			try
			{
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
		}
	}
}
