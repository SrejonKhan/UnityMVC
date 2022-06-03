namespace UnityMVC
{
    /// <summary>
    /// This attribute indicate MVC Action to invoke when Refresh() is called on ViewResult
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class OnRefreshAttribute : UnityEngine.Scripting.PreserveAttribute // preserve to avoid stripping
    {
        public OnRefreshAttribute()
        {
        }
    }
}
