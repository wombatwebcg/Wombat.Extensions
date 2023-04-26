using System;
using System.Threading.Tasks;

namespace Wombat
{
    /// <summary>
    /// 拓展方法静态类
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// 异步，按顺序执行第一个方法和第二个方法
        /// </summary>
        /// <param name="firstFunc">第一个方法</param>
        /// <param name="next">下一个方法</param>
        public static void Done(this Action firstFunc, Action next)
        {
            Task firstTask = new Task(() =>
            {
                firstFunc();
            });

            firstTask.Start();
            firstTask.ContinueWith(x => next());
        }

        /// <summary>
        /// 异步，按顺序执行第一个方法和下一个方法
        /// </summary>
        /// <param name="firstFunc">第一个方法</param>
        /// <param name="next">下一个方法</param>
        public static void Done(this Func<object> firstFunc, Action<object> next)
        {
            Task<object> firstTask = new Task<object>(() =>
            {
                return firstFunc();
            });

            firstTask.Start();
            firstTask.ContinueWith(x => next(x.Result));
        }
    }
}
