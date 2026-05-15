using Microsoft.AspNetCore.Http;

namespace Engzly.Application.Common.Bases
{
    public abstract class ResponseHandler
    {
        public ResponseHandler()
        {

        }
        public Response<T> Deleted<T>()
        {
            return new Response<T>()
            {
                StatusCode = (int)System.Net.HttpStatusCode.OK,
                Succeeded = true,
                Message = "Deleted Successfully"
            };
        }
        public Response<T> Success<T>(T entity, string message = "Success", object Meta = null)
        {
            return new Response<T>()
            {
                Data = entity,
                StatusCode = (int)System.Net.HttpStatusCode.OK,
                Succeeded = true,
                Message = message,
                Meta = Meta
            };
        }
        public Response<T> UserExist<T>(T entity, object Meta = null)
        {
            return new Response<T>()
            {
                Data = entity,
                StatusCode = (int)System.Net.HttpStatusCode.OK,
                Succeeded = true,
                Message = "User Exist ",
                Meta = Meta
            };
        }
        public Response<T> Deleted<T>(T entity, object Meta = null)
        {
            return new Response<T>()
            {
                Data = entity,
                StatusCode = (int)System.Net.HttpStatusCode.OK,
                Succeeded = true,
                Message = $" Deleted Successfully",
                Meta = Meta
            };
        }

        public Response<T> Unauthorized<T>()
        {
            return new Response<T>()
            {
                StatusCode = (int)System.Net.HttpStatusCode.Unauthorized,
                Succeeded = true,
                Message = "UnAuthorized"
            };
        }
        public Response<T> Unauthorized<T>(string Message = null)
        {
            return new Response<T>()
            {
                StatusCode = (int)System.Net.HttpStatusCode.Unauthorized,
                Succeeded = true,
                Message = Message == null ? " UnAuthorized " : Message
            };
        }
        public Response<T> BadRequest<T>(string Message = null)
        {
            return new Response<T>()
            {
                StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                Succeeded = false,
                Message = Message == null ? "Bad Request" : Message
            };
        }
        public Response<T> BadRequest<T>(string Message = null, List<string> erorrs = null)
        {
            return new Response<T>()
            {
                StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                Succeeded = false,
                Message = Message == null ? "Bad Request" : Message,
                Errors = erorrs
            };
        }

        public Response<T> NotFound<T>(string message = null)
        {
            return new Response<T>()
            {
                StatusCode = (int)System.Net.HttpStatusCode.NotFound,
                Succeeded = false,
                Message = message == null ? "Not Found" : message
            };
        }
        public Response<T> UnprocessableEntity<T>(string message = null)
        {
            return new Response<T>()
            {
                StatusCode = (int)System.Net.HttpStatusCode.UnprocessableEntity,
                Succeeded = false,
                Message = message == null ? "UnprocessableEntity" : message
            };
        }

        public Response<T> Created<T>(T entity, object Meta = null)
        {
            return new Response<T>()
            {
                Data = entity,
                StatusCode = (int)System.Net.HttpStatusCode.Created,
                Succeeded = true,
                Message = "Created Successfully",
                Meta = Meta
            };
        }


        public Response<T> Gone<T>(string message = null)
        {
            return new Response<T>()
            {
                StatusCode = (int)System.Net.HttpStatusCode.Gone,
                Succeeded = false,
                Message = message == null ? "Task Is Completed Or Canceled " : message
            };
        }
        public Response<T> Forbidden<T>(string message = null)
        {
            return new Response<T>()
            {
                StatusCode = (int)System.Net.HttpStatusCode.Forbidden,
                Succeeded = false,
                Message = message == null ? "task Is Forbidden " : message
            };
        }
        public Response<T> Conflict<T>(string message = null)
        {
            return new Response<T>()
            {
                StatusCode = (int)System.Net.HttpStatusCode.Conflict,
                Succeeded = false,
                Message = message == null ? "There Exist Conflict   " : message
            };
        }


        public Response<T> InternalServerError<T>(string message = null)
        {
            return new Response<T>
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Succeeded = false,
                Message = message ?? "Internal Server Error"
            };
        }

        public Response<T> LoggedOutSuccessful<T>(T entity, object Meta = null)
        {
            return new Response<T>()
            {
                Data = entity,
                StatusCode = (int)System.Net.HttpStatusCode.OK,
                Succeeded = true,
                Message = "Logged Out Successfully",
                Meta = Meta
            };
        }

    }

}
