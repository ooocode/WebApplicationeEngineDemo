using Snowflake.Core;
using WebApplicationeEngineDemo.Events;
using WF.Engine.DbModels;

var builder = WebApplication.CreateBuilder(args);


var dbConnectString = "server=127.0.0.1;" +
        "database=Test-Engine-0321-2;" +
        "user id=postgres;password=12345678";
var autoCreateDb = true;
var engineGrpcAddress = string.Empty;

//添加数据库上下文和工作流引擎接口
builder.Services.AddEngineDbContextAndGrpcClient(dbConnectString,
    autoCreateDb,
    engineGrpcAddress);

//添加完成任务回调
builder.Services.AddEngineUserTaskJobs(TimeSpan.FromSeconds(60), true);
builder.Services.AddEngineUserTaskJobsEvent<MyCompleteUserTaskEvent>();

//添加long型id生成器
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
