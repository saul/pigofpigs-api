namespace PigOfPigs

open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open PigOfPigs.Data
open Serilog
open Serilog.Events

module Program =

    let createHostBuilder args =
        Host
            .CreateDefaultBuilder(args)
            .UseSerilog()
            .ConfigureWebHostDefaults(fun webBuilder -> webBuilder.UseStartup<Startup>() |> ignore)

    let createDbIfNotExists (host : IHost) =
        use scope = host.Services.CreateScope()
        let services = scope.ServiceProvider
        let context = services.GetRequiredService<PigContext>()
        DbInitializer.ensureCreated context

    [<EntryPoint>]
    let main args =
        Log.Logger <-
            LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger()

        try
            try
                let host = createHostBuilder(args).Build()
                createDbIfNotExists host
                host.Run()
                0
            with exc ->
                Log.Fatal(exc, "Host terminated unexpectedly")
                1
        finally
            Log.CloseAndFlush()
