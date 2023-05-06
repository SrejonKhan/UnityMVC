namespace UnityMVC
{
    public class FailedViewResult : ActionResult 
    {
        public FailedViewResult(string route)
        {
            base.RouteUrl = route;
            base.IsFailed = true;
        }
    }
}