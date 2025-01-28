using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace DominoCS.Controllers
{
    [ApiController]
    [Route("user-requests")]
    public class UserRequestController : ControllerBase
    {        
        static Dictionary<string, JObject> requests = new Dictionary<string, JObject>();

        public UserRequestController()
        {
            
        }

        [HttpPost]
        public IActionResult Post([FromBody] JObject req)
        {
            
            if (req == null)
                return BadRequest();
            UserRequest request = null;
            try
            {
                request = req.ToObject<UserRequest>();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            if (request == null)
                return BadRequest();

            if (string.IsNullOrEmpty(request.externalId))
            {                
                request.externalId = Guid.NewGuid().ToString();
                req["externalId"] = request.externalId;
            }

            var validate = request.validate();
            if (!string.IsNullOrEmpty(validate))
            {
                return BadRequest(validate);
            }

            requests[request.externalId] = req;
            return new JsonResult(req);
        }

        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            if (requests.ContainsKey(id))
                return new JsonResult(requests[id]);
            else
                return NotFound();

        }

        [HttpGet("")]
        public IActionResult GetAll()
        {            
            return new JsonResult(requests.Values);         
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            if (requests.ContainsKey(id))
            {
                requests.Remove(id);
                return Ok();
            }
            else
                return NotFound();

        }
    }
}
