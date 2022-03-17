namespace UnityMVC
{
    /// <summary>
    /// This attribute indicate MVC Action to invoke when particual model is instanced
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class InvokeAttribute : UnityEngine.Scripting.PreserveAttribute // preserve to avoid stripping
    {
        public InvokeAttribute()
        {
        }
    }
}
