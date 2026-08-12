namespace Kokkoro.Core.Primitives;

/// <summary>
/// IDisposable 工厂
/// </summary>
public static class Disposable
{
    /// <summary>
    /// 创建一个释放时执行指定操作的 IDisposable
    /// </summary>
    public static IDisposable Create(Action dispose)
    {
        return new ActionDisposable(dispose);
    }

    /// <summary>
    /// 基于委托实现的 IDisposable
    /// </summary>
    /// <param name="dispose">释放时执行的操作</param>
    private sealed class ActionDisposable(Action dispose) : IDisposable
    {
        private Action? _dispose = dispose;

        /// <summary>
        /// 释放资源并执行回调，仅执行一次
        /// </summary>
        public void Dispose()
        {
            Interlocked.Exchange(ref _dispose, null)?.Invoke();
        }
    }
}