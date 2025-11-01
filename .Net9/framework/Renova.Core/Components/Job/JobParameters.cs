using NewLife;
using Newtonsoft.Json;

namespace Renova.Core.Components.Job
{
    /// <summary>
    /// 作业参数基类
    /// </summary>
    public class JobParameters
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="value"></param>
        public virtual void Initialize(string value)
        {
            if (GetType() != typeof(JobParameters) && !value.IsNullOrEmpty())
            {
                JsonConvert.PopulateObject(value, this);
            }
        }
    }
}
