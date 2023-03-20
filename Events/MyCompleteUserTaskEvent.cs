using WF.Engine.DbModels.Contract;
using WF.Engine.DbModels.EngineModels;
using WF.Engine.DbModels.EngineModels.ExtendModels;

namespace WebApplicationeEngineDemo.Events
{
    public class MyCompleteUserTaskEvent : ICompleteUserTaskEvent
    {
        private readonly ILogger<MyCompleteUserTaskEvent> logger;

        public MyCompleteUserTaskEvent(ILogger<MyCompleteUserTaskEvent> logger)
        {
            this.logger = logger;
        }
        public Task ExecuteAsync(CompletedUserTask task, EngineDbContext engineDbContext, CancellationToken cancellationToken)
        {
            logger.LogInformation($"{DateTimeOffset.Now} 完成任务 {task.Id}");
            logger.LogInformation($"{task.CoreAttach}");
            return Task.CompletedTask;
        }
    }
}
