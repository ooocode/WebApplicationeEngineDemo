using Engine;
using Microsoft.AspNetCore.Mvc;
using Snowflake.Core;
using WF.Engine.DbModels.EngineModels;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApplicationWF.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlowsController : ControllerBase
    {
        private readonly ProcessService.ProcessServiceClient processServiceClient;
        private readonly EngineDbContext engineDbContext;
        private readonly IdWorker idWorker;

        public FlowsController(
            Engine.ProcessService.ProcessServiceClient processServiceClient,
            EngineDbContext engineDbContext,
            IdWorker idWorker)
        {
            this.processServiceClient = processServiceClient;
            this.engineDbContext = engineDbContext;
            this.idWorker = idWorker;
        }


        public record StartProcessReq(
            string ProcessDefinitionKey,
            string UserId,
            Dictionary<string, string> KeyValues);

        // POST api/<FlowsController>
        [HttpPost("start")]
        public async Task<ActionResult<StartProcessReply>> Post([FromBody] StartProcessReq value)
        {
            var req = new StartProcessRequest
            {
                AuthenticatedUserId = value.UserId,
                BusinessKey = Guid.NewGuid().ToString(),
                ProcessDefinitionKey = value.ProcessDefinitionKey
            };

            if (value.KeyValues != null)
            {
                foreach (var item in value.KeyValues)
                {
                    req.Variables.Add(item.Key,
                        Google.Protobuf.ByteString.CopyFromUtf8(item.Value));
                }
            }

            var form = new WF.Engine.DbModels.EngineModels.ExtendModels.BusinessForm
            {
                Id = idWorker.NextId(),
                BusinessKey = req.BusinessKey,
                CreateDateTime = DateTime.UtcNow,
                DrafterUserName = req.AuthenticatedUserId,
            };

            engineDbContext.BusinessForms.Add(form);

            await engineDbContext.SaveChangesAsync();

            StartProcessReply res = await processServiceClient.StartProcessAsync(req);
            return res;
        }
    }
}
