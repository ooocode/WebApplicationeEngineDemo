using Engine;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WF.Engine.DbModels.EngineModels;
using WF.Engine.DbModels.EngineModels.ExtendModels;
using WF.Engine.DbModels.EngineModels.PostgreSQL.CSharp.Models;


namespace WebApplicationWF.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserTasksController : ControllerBase
    {
        private readonly EngineDbContext engineDbContext;
        private readonly ProcessService.ProcessServiceClient processServiceClient;

        public UserTasksController(EngineDbContext engineDbContext,
            Engine.ProcessService.ProcessServiceClient processServiceClient)
        {
            this.engineDbContext = engineDbContext;
            this.processServiceClient = processServiceClient;
        }



        /// <summary>
        /// 获取任务
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet]
        public List<UserTaskDetail> Get(string userId)
        {
            var list = engineDbContext.ActRuTasks.AsNoTracking()
                .Where(e => e.Assignee == userId)
                .Join(engineDbContext.BusinessForms, e => e.BusinessKey,
                    e => e.BusinessKey, (task, form) => new { task, form })
                .OrderByDescending(e => e.task.CreateDateTimeUtc)
                .ToList();

            var x = list.Select(e => new UserTaskDetail(e.task, e.form)).ToList();

            foreach (var item in x)
            {
                if (item.Form != null)
                {
                    item.Form.FieldItems = item.Form.GetFields().ToList();
                }
            }

            return x;
        }


        /// <summary>
        /// 获取待办任务数量
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("todo-count")]
        public async Task<int> GetTodoCount(string userId)
        {
            var list = await engineDbContext.ActRuTasks.AsNoTracking()
                .CountAsync(e => e.Assignee == userId);

            return list;
        }


        public record UserTaskDetail(ActRuTask Task, BusinessForm? Form);


        /// <summary>
        /// 获取任务详情
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<UserTaskDetail?> GetById(string id)
        {
            var task = await engineDbContext.ActRuTasks.AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);
            if (task == null)
            {
                return null;
            }

            var form = await engineDbContext.BusinessForms.AsNoTracking()
                .FirstOrDefaultAsync(e => e.BusinessKey == task.BusinessKey);
            if (form != null)
            {
                form.FieldItems = form.GetFields().ToList();
            }

            return new UserTaskDetail(task, form);
        }


        public record CompleteReq(string UserTaskId,
            Dictionary<string, string> Variables,
            Dictionary<string, string>? FormFields);


        /// <summary>
        /// 完成任务
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPost("complete")]
        public async Task<ActionResult<string>> Complete([FromBody] CompleteReq value)
        {
            var task = await engineDbContext.ActRuTasks.AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == value.UserTaskId);
            if (task == null)
            {
                return NotFound();
            }

            var body = new CompleteTaskRequest
            {
                TaskId = value.UserTaskId,
            };

            if (value.Variables != null)
            {
                foreach (var item in value.Variables)
                {
                    body.Variables.Add(item.Key,
                        Google.Protobuf.ByteString.CopyFromUtf8(item.Value));
                }
            }


            var form = await engineDbContext.BusinessForms
                .FirstOrDefaultAsync(e => e.BusinessKey == task.BusinessKey);
            if (form != null)
            {
                if (value.FormFields != null)
                {
                    foreach (var item in value.FormFields)
                    {
                        form.SetBusinessField(item.Key, item.Value);
                    }
                }
            }

            try
            {
                await engineDbContext.SaveChangesAsync();
                var res = await processServiceClient.CompleteUserTaskAsync(body);
                return Ok($"完成了任务：{value.UserTaskId}");
            }
            catch (RpcException ex)
            {
                return Problem(ex.Status.Detail);
            }
        }
    }
}
