using Renova;
using Renova.Core.Components.Autofac;
using Renova.Core.Components.Serilog;
using Serilog;


try
{
    Log.Logger = SerilogConfigurator.Init();

    Log.Information("""

     ____                                  _       _           _       
    |  _ \ ___ _ __   _____   ____ _      / \   __| |_ __ ___ |_|_ __  
    | |_) / _ \ '_ \ / _ \ \ / / _` |    / _ \ / _` | '_ ` _ \| | '_  \ 
    |  _ <  __/ | | | (_) \ V / (_| |_  / ___ \ (_| | | | | | | | | | |
    |_| \_\___|_| |_|\___/ \_/ \__,__/ /_/   \_\__,_|_| |_| |_|_|_| |_|   {Version} 

    {description}

    """, "Version: 1.0", "������ͬһ���ọ́�ÿһ����ֵ����ϧ�����۷��껹����գ�δ���ܻ������Ŭ������������");

    Log.Information($"Renova.Admin ��Run! ");
    var builder = WebApplication.CreateBuilder(args);
    Log.Information($"��ǰ������������-��{builder.Environment.EnvironmentName}��");
    Log.Information($"��ǰ����������ַ-��{builder.Configuration["App:SelfUrl"]}��");
    builder.WebHost.UseUrls(builder.Configuration["App:SelfUrl"]!);
    builder.Host.UseSerilog();
    builder.Host.UseAutofac();
    builder.AddServices();
    var app = builder.Build();
    app.UseMiddlewares();
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Renova.Admin ����ʧ��");
}
finally
{
    Log.CloseAndFlush();
}
