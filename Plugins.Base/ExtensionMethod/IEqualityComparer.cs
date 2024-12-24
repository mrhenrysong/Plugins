namespace Plugins.Base.ExtensionMethod
{
    /// <summary>
    /// 定义方法以支持对象的相等比较。
    /// </summary>
    /// <typeparam name="T">要比较的对象的类型。</typeparam>
    /// <typeparam name="K">要比较的对象的类型。</typeparam>
    public interface IEqualityComparer<T, K>
    {
        /// <summary>
        ///  确定指定的对象是否相等。
        /// </summary>
        /// <param name="x">要比较的第一个类型为 T 的对象。</param>
        /// <param name="y">要比较的第二个类型为 K 的对象。</param>
        /// <returns> 如果指定的对象相等，则为 true；否则为 false。</returns>
        bool Equals(T x, K y);

        /// <summary>
        /// 返回指定对象的哈希代码。
        /// </summary>
        /// <param name="obj">System.Object，将为其返回哈希代码。</param>
        /// <returns>指定对象的哈希代码。</returns>
        int GetHashCode(T obj);
    }
}
