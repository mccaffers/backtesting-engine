using Webserver.Api.Hubs;

namespace Webserver.Api;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();

        services.AddSignalR().AddMessagePackProtocol();

        services.AddCors(options =>
        {
            options.AddPolicy("ClientPermission", pb =>
                pb.AllowAnyHeader()
                    .AllowAnyMethod()
                    .SetIsOriginAllowed(_ => true)
            );
        });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }



        app.UseHttpsRedirection();
        
    
        app.UseRouting();
        app.UseCors("ClientPermission");
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            
            endpoints.MapControllers();
            endpoints.MapHub<ChatHub>("/hubs/chat");

        });

        
    }
}