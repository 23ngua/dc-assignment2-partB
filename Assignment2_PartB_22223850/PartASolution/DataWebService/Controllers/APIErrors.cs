using DataWebService.Models;

namespace DataWebService.Controllers
{
    /* Builds the error body that controllers send back. The controller chooses the status code. */
    public static class APIErrors
    {
        // 400 - Malformed request or failed field validation
        public static APIError BadRequest(string code, string message, Dictionary<string, string>? details = null)
        {
            APIError error = new APIError();
            error.Error = code;
            error.Message = message;
            error.Details = details;
            return error;
        }

        // 404 - Resource does not exist
        public static APIError NotFound(string code, string message)
        {
            APIError error = new APIError();
            error.Error = code;
            error.Message = message;
            return error;
        }

        // 409 - Conflict, such as a duplicate username or email
        public static APIError Conflict(string code, string message)
        {
            APIError error = new APIError();
            error.Error = code;
            error.Message = message;
            return error;
        }

        // 422 - Valid request that breaks a business rule, such as insufficient funds
        public static APIError Unprocessable(string code, string message)
        {
            APIError error = new APIError();
            error.Error = code;
            error.Message = message;
            return error;
        }

        // 500 - Unexpected failure — never return a stack trace or raw database message
        public static APIError ServerError()
        {
            APIError error = new APIError();
            error.Error = "INTERNAL_ERROR";
            error.Message = "An unexpected error occurred.";
            return error;
        }
    }
}