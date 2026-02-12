

try
{
    Log.Logger = SerilogConfigurator.CreateBootstrapLogger();

    Log.Information("""                      

     ____                                  _       _           _       
    |  _ \ ___ _ __   _____   ____ _      / \   __| |_ __ ___ |_|_ __  
    | |_) / _ \ '_ \ / _ \ \ / / _` |    / _ \ / _` | '_ ` _ \| | '_  \ 
    |  _ <  __/ | | | (_) \ V / (_| |_  / ___ \ (_| | | | | | | | | | |
    |_| \_\___|_| |_|\___/ \_/ \__,__/ /_/   \_\__,_|_| |_| |_|_|_| |_|   {Version} 

    {description}

    """, "Version: 1.0", "生命如同一场旅程，每一步都值得珍惜；无论风雨还是晴空，未来总会因你的努力而更加美好");

    Log.Information($"Renova.Admin ，Run! ");
    var builder = WebApplication.CreateBuilder(args).AddDynamicJsonFiles();
    Log.Information($"当前主机启动环境-【{builder.Environment.EnvironmentName}】");
    Log.Information($"当前主机启动地址-【{builder.Configuration["App:SelfUrl"]}】");
    builder.WebHost.UseUrls(builder.Configuration["App:SelfUrl"]!);
    builder.Host.UseConfiguredSerilog();
    builder.Host.UseAutofac();
    builder.AddServices();
    var app = builder.Build();
    app.UseMiddlewares();
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Renova.Admin 启动失败");
}
finally
{
    Log.CloseAndFlush();
}
