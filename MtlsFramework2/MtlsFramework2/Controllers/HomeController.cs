using System.Web.Http;

namespace MtlsFramework2.Controllers
{
    public class HomeController : ApiController
    {
        [HttpGet]
        [Route("api/Home/GetMessage")]
        public IHttpActionResult GetMessage()
        {
            return Ok("Super Secret Message");
        }
    }
}
