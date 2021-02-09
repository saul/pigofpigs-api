namespace PigOfPigs

open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.HttpOverrides
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open PigOfPigs.Data
open Microsoft.EntityFrameworkCore

type Startup(configuration: IConfiguration) =
    member _.Configuration = configuration

    // This method gets called by the runtime. Use this method to add services to the container.
    member _.ConfigureServices(services: IServiceCollection) : unit =
        services.AddControllers() |> ignore

        services.AddDbContext<PigContext>(fun options ->
            options.UseSqlite(configuration.GetConnectionString("PigContext")) |> ignore
        ) |> ignore

        services.AddDatabaseDeveloperPageExceptionFilter() |> ignore

        services.AddCors(fun options ->
            options.AddDefaultPolicy(fun builder ->
                builder
                    .WithOrigins("http://localhost:3000", "https://www.pigofpigs.com")
                    .WithHeaders("Content-Type")
                    .WithMethods("GET", "POST")
                    .AllowCredentials()
                |> ignore
            )
        ) |> ignore

        services
            .AddAuthentication(fun options ->
                options.DefaultScheme <- CookieAuthenticationDefaults.AuthenticationScheme
            )
            .AddCookie(fun options ->
                options.Cookie.SameSite <- SameSiteMode.None
                options.LoginPath <- PathString.op_Implicit "/account/google-login"
            )
            .AddGoogle(fun options ->
                let googleAuthNSection = configuration.GetSection("Authentication:Google")
                options.ClientId <- googleAuthNSection.["ClientId"]
                options.ClientSecret <- googleAuthNSection.["ClientSecret"]
            )
        |> ignore

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    member _.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) : unit =
        if env.IsDevelopment() then
            app.UseDeveloperExceptionPage() |> ignore

        ForwardedHeadersOptions(
            ForwardedHeaders=(ForwardedHeaders.XForwardedFor ||| ForwardedHeaders.XForwardedProto)
        )
        |> app.UseForwardedHeaders
        |> ignore

        app
            .UseRouting()
            .UseCors()
            .UseAuthentication()
            .UseAuthorization()
            .UseEndpoints(fun endpoints -> endpoints.MapControllers() |> ignore)
        |> ignore
