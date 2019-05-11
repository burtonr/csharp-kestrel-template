using FaasUtils.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OpenFaaS.FunctionSDK;

namespace root
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
	        services.AddFaasUtils();
	        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
	        services.AddScoped<FunctionContext>(FunctionContextFactory.Create);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<FaasMiddleware>();
        }
    }
}
