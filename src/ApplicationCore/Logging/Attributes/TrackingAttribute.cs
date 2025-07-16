using Microsoft.AspNetCore.Mvc.Filters;

namespace ApplicationCore.Logging.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class TrackingAttribute : ActionFilterAttribute
{
    public bool StartingPoint { get; set; }

    public TrackingAttribute(bool startingPoint = false)
    {
        StartingPoint = startingPoint;
    }
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!StartingPoint)
        {
            var controller = context.Controller as ITrackingLogger;
            if (controller == null)
                throw new InvalidOperationException("Controller must implement ITrackingLogger to use TrackingAttribute.");

            var trackingId = controller.TrackingId;
            var requuest = context.HttpContext.Request;
            var requestTrackingId = requuest.Headers["X-TRACKING"].ToString();
            if (!string.IsNullOrEmpty(requestTrackingId))
            {
                if (Guid.TryParse(requestTrackingId, out var parsedTrackingId))
                {
                    controller.TrackingId = parsedTrackingId;
                }
                else
                {
                    throw new InvalidOperationException("Invalid tracking ID format in request header.");
                }
            }
        }

      
        base.OnActionExecuting(context); ;
    }

    public override void OnActionExecuted(ActionExecutedContext context)
    {
        var controller = context.Controller as ITrackingLogger;
        if (controller == null)
            throw new InvalidOperationException("Controller must implement ITrackingLogger to use TrackingAttribute.");

        var response = context.HttpContext.Response;
        response.Headers["X-TRACKING"] = controller.TrackingId.ToString();
    }
}
