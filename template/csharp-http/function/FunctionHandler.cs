using System.Collections.Specialized;
using System.Threading.Tasks;
using OpenFaaS.FunctionSDK;

namespace Function
{
    /// <summary>
    /// This is only here to demonstrate that custom request objects can be used
    /// Ideally, this type would be included from your specific NuGet package,
    /// not directly in the function
    /// </summary>
    public class SampleRequest
    {
        public string Name { get; set; }
    }


    /// <summary>
    /// Similar to the SampleRequest class above, there is the ability to use
    /// a custom response object. Again, ideally this would be included from a
    /// NuGet package and shared with the calling application
    /// </summary>
    public class SampleResponse
    {
        public string Response { get; set; }
    }

    public class FunctionHandler
    {
		// Sample: use a custom request type by taking an object as an argument
		//public Task<FunctionResponse> InvokeAsync(SampleRequest input)
		public Task<FunctionResponse> InvokeAsync(string input)
        {
            // Sample: Set custom headers
            var resultHeaders = new NameValueCollection
            {
                {"X-OpenFaaS-Function", "csharp-kestrel"},
            };
            
            var result = new FunctionResponse {
                // Sample: Return plain text, or a custom object as the body
                // Body = $"Hello from OpenFaaS + Kestrel, {fnContext.Body}!",
                Body = new SampleResponse
                {
                    Response = $"Hello from OpenFaaS + Kestrel, {input}"
                },
                Headers = resultHeaders
            };

            return Task.FromResult(result);
        }
    }
}
