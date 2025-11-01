using Renova.Core.Components.Job;

namespace Renova.Core
{
    /// <summary>
    /// 标记为作业类
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class JobAttribute : Attribute
    {
        /// <summary>
        ///  任务描述
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// 作业参数类型
        /// </summary>
        public Type ParametersType { get; }

        /// <summary>
        ///  构造函数
        /// </summary>
        public JobAttribute(string description, Type? parametersType = null)
        {
            Description = description;
            ParametersType = parametersType ?? typeof(JobParameters);

            if (!typeof(JobParameters).IsAssignableFrom(ParametersType))
            {
                throw new ArgumentException(
                    $"参数类型 {ParametersType.FullName} 必须继承自 {nameof(JobParameters)}。",
                    nameof(parametersType));
            }
        }
    }
}
