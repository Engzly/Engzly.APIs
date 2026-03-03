using System.Net;
using Engzly.Application.Common.Bases;
using Microsoft.AspNetCore.Mvc;

namespace Engzly.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        protected IActionResult Resolve<T>(Response<T> response)
        {
            switch (response.StatusCode)
            {
                case (int)HttpStatusCode.OK:
                    return Ok(response);
                case (int)HttpStatusCode.Created:
                    return Created(string.Empty, response);
                case (int)HttpStatusCode.Unauthorized:
                    return Unauthorized(response);
                case (int)HttpStatusCode.BadRequest:
                    return BadRequest(response);
                case (int)HttpStatusCode.NotFound:
                    return NotFound(response);
                case (int)HttpStatusCode.Accepted:
                    return Accepted(string.Empty, response);
                case (int)HttpStatusCode.UnprocessableEntity:
                    return UnprocessableEntity(response);
                default:
                    return BadRequest(response);
            }
        }
    }
}
