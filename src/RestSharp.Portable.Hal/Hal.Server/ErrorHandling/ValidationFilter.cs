using System.Web.Http.Filters;

namespace Hal.ErrorHandling
{
    public class ValidationFilter : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            var exception = actionExecutedContext.Exception;

            if (exception == null) return;

            if (exception is ValidationFailedException)
            {
                (exception as ValidationFailedException).SetResponse(actionExecutedContext);
            }
        }
    }
}