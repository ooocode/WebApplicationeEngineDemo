using Snowflake.Core;
using WebApplicationeEngineDemo.Events;
using WF.Engine.DbModels;

var builder = WebApplication.CreateBuilder(args);


var dbConnectString = "server=127.0.0.1;" +
        "database=Test-Engine-0321-2;" +
        "user id=postgres;password=12345678";
var autoCreateDb = true;
var engineGrpcAddress = string.Empty;

//������ݿ������ĺ͹���������ӿ�
builder.Services.AddEngineDbContextAndGrpcClient(dbConnectString,
    autoCreateDb,
    engineGrpcAddress);

//����������ص�
builder.Services.AddEngineUserTaskJobs(TimeSpan.FromSeconds(60), true);
builder.Services.AddEngineUserTaskJobsEvent<MyCompleteUserTaskEvent>();

//���long��id������
builder.Services.AddSingleton(s => new IdWorker(1, 1));


builder.Services.AddControllers();
builder.Services.AddOpenApiDocument();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();
app.UseOpenApi();
app.UseReDoc(conf=>conf.Path = "/redoc");

app.Run();
