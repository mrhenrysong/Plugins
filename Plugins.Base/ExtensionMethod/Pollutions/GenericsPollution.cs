using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Plugins.Base.ExtensionMethod.Pollutions
{
    /// <summary>
    /// 泛型的扩展方法包，用于给泛型附加扩展方法
    /// </summary>
    public static class GenericsPollution
    {
        /// <summary>
        /// 往List增加数据，当前list中没有数据才会添加
        /// </summary>
        /// <typeparam name="T">输入序列中的元素的类型</typeparam>
        /// <param name="array">输入序列中的元素列表</param>
        /// <param name="data">输入的元素</param>
        public static void ArrayAddData<T>(this List<T> array, T data)
        {
            if (array != null && !array.Contains(data))
            {
                array.Add(data);
            }
        }

        /// <summary>
        /// 将一个对象的属性值映射到另一个对象
        /// </summary>
        /// <typeparam name="R">目标对象类型</typeparam>
        /// <typeparam name="T">原始对象类型</typeparam>
        /// <param name="model">目标对象</param>
        /// <param name="model1">原始对象</param>
        /// <returns></returns>
        public static R Mapping<R, T>(this T model, R model1)
        {

            // 获取目标对象model1的所有属性
            foreach (PropertyInfo targetProperty in model1.GetType().GetProperties())
            {
                // 获取源对象model的属性
                PropertyInfo sourceProperty = typeof(T).GetProperty(targetProperty.Name);

                // 如果源对象也有这个属性，且目标属性是可写的
                if (sourceProperty != null && targetProperty.CanWrite)
                {
                    try
                    {
                        // 检查属性类型是否匹配，如果不匹配，可以尝试转换
                        if (sourceProperty.PropertyType == targetProperty.PropertyType)
                        {
                            // 如果类型相同，直接赋值
                            targetProperty.SetValue(model1, sourceProperty.GetValue(model));
                        }
                        else if (targetProperty.PropertyType.IsAssignableFrom(sourceProperty.PropertyType))
                        {
                            // 如果目标类型可以从源类型转换，执行转换
                            targetProperty.SetValue(model1, Convert.ChangeType(sourceProperty.GetValue(model), targetProperty.PropertyType));
                        }
                        else
                        {
                            Debug.WriteLine($"属性类型不匹配: {sourceProperty.Name}");
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine($"错误映射属性 {sourceProperty.Name}: {e.Message}");
                    }
                }
            }
            return model1;
        }

        /// <summary>
        /// 通过使用指定的 System.Collections.Generic.IEqualityComparer<T,K> 对值进行比较产生任意类型两个序列的差集。
        /// </summary>
        /// <typeparam name="T">输入序列中的元素的类型。</typeparam>
        /// <typeparam name="K">输入对比序列中的元素的类型。</typeparam>
        /// <param name="source"> 一个 System.Collections.Generic.IEnumerable<T>，将返回其也不在 second 中的元素。</param>
        /// <param name="ContrastSource"> 一个 System.Collections.Generic.IEnumerable<K>，如果它的元素也出现在第一个序列中，则将导致从返回的序列中移除这些元素。</param>
        /// <param name="EqualityComparer"> 用于比较值的 System.Collections.Generic.IEqualityComparer<T,K></param>
        /// <returns> 包含两个序列元素的差集的序列。</returns>
        public static List<T> Except<T, K>(this List<T> source, List<K> ContrastSource, IEqualityComparer<T, K> EqualityComparer)
        {
            List<T> list = new List<T>();
            source.ForEach(i =>
            {
                bool Flag = true;
                ContrastSource.ForEach(j =>
                {
                    if (EqualityComparer.Equals(i, j))
                    {
                        Flag = false;
                    }
                });
                if (Flag)
                {
                    list.Add(i);
                }
            });
            return list;
        }

        /// <summary>
        /// 通过使用指定的 System.Collections.Generic.IEqualityComparer<T,K> 对值进行比较产生任意类型两个序列的补集。
        /// </summary>
        /// <typeparam name="T">输入序列中的元素的类型。</typeparam>
        /// <typeparam name="K">输入对比序列中的元素的类型。</typeparam>
        /// <param name="source"> 一个 System.Collections.Generic.IEnumerable<T>，将返回其也不在 second 中的元素。</param>
        /// <param name="ContrastSource"> 一个 System.Collections.Generic.IEnumerable<K>，如果它的元素也出现在第一个序列中，则将导致从返回的序列中移除这些元素。</param>
        /// <param name="EqualityComparer"> 用于比较值的 System.Collections.Generic.IEqualityComparer<T,K></param>
        /// <returns> 包含两个序列元素的补集的序列。</returns>
        public static List<T> Complement<T, K>(this List<T> source, List<K> ContrastSource, IEqualityComparer<T, K> EqualityComparer)
        {
            List<T> list = new List<T>();

            source.ForEach(i =>
            {
                bool Flag = true;
                ContrastSource.ForEach(j =>
                {
                    if (EqualityComparer.Equals(i, j))
                    {
                        Flag = false;
                    }
                });
                if (Flag)
                {
                    list.Add(i);
                }
            });
            return list;
        }

        /// <summary>
        ///  返回当前数组的拷贝(其实和拷贝没关系- -)
        /// </summary>
        /// <typeparam name="T">数组里的数据类型</typeparam>
        /// <param name="source">数据源</param>
        /// <returns></returns>
        public static List<T> CopyList<T>(this IList<T> source) => (from T item in source
                                                                    select item).ToList();

        /// <summary>
        /// 用于对比及返回两个数组相交的项目
        /// </summary>
        /// <typeparam name="T">数组内部数据类型</typeparam>
        /// <param name="source">原始数组</param>
        /// <param name="ContrastSource">对比数组</param>
        /// <param name="Item">ref 第一个相交项</param>
        /// <returns>是否有相交</returns>
        public static bool Contains<T>(this IList<T> source, IList<T> ContrastSource, ref T Item) where T : class
        {
            foreach (T item in from T item in ContrastSource
                               from T iteMc2 in source
                               where item.GetHashCode() == iteMc2.GetHashCode()
                               select item)
            {
                Item = item;
                return true;
            }

            Item = null;
            return false;
        }

        /// <summary>
        /// 判断当前项目是否存在当前数组里
        /// </summary>
        /// <typeparam name="T">数组类型</typeparam>
        /// <typeparam name="K">对比的数据类型</typeparam>
        /// <param name="source">数组</param>
        /// <param name="ContrastSource">对比的数据</param>
        /// <param name="EqualityComparer">用于对比的策略</param>
        /// <returns>返回是否存在</returns>
        public static bool Contains<T, K>(this IList<T> source, K ContrastSource, IEqualityComparer<T, K> EqualityComparer, out T V, ref int IndexOf) where T : class
        {
            bool Flag = false;
            V = null;
            foreach (T item in from T item in source
                               where EqualityComparer.Equals(item, ContrastSource)
                               select item)
            {
                Flag = true;
                V = item;
                IndexOf = source.IndexOf(item);
            }

            return Flag;
        }
        /// <summary>
        /// 判断当前数据是否为null
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="Source">需要判断的数据</param>
        /// <param name="NewSource">如果为null则返回这个参</param>
        /// <returns>返回NewSource参</returns>
        public static T Nullable<T>(this T Source, T NewSource) where T : class => Source ?? NewSource;

        /// <summary>
        /// 对IEnumerable泛型类进行污染,循环访问IEnumerable泛型类里的内容,将每个结果发送给指定的方法
        /// </summary>
        /// <typeparam name="T">类型标示符</typeparam>
        /// <param name="Source">IEnumerable泛型数组</param>
        /// <param name="ForEach">将值发送给指定的方法,该方法接受一个传入参数且不返回值,如:Console.WriteLine()</param>
        public static void ForEach<T>(this IEnumerable<T> Source, Action<T> ForEach)
        {
            foreach (T item in Source)
            {
                ForEach(item);
            }
        }

        /// <summary>
        /// 对IEnumerable泛型类进行污染,循环访问IEnumerable泛型类里的内容,将每个结果发送给指定的方法
        /// </summary>
        /// <typeparam name="T">类型标示符</typeparam>
        /// <param name="Source">IEnumerable泛型数组</param>
        /// <param name="ForEach">将值发送给指定的方法,该方法接受一个传入参数且不返回值,如:Console.WriteLine()</param>
        public static void ForEach<T, R>(this IEnumerable<T> Source, Action<T, R> ForEach)
        {
            for (int i = 0; i < Source.Count(); i++)
            {
                ForEach(Source.ElementAt(i), (dynamic)i);
            }
        }

        /// <summary>
        /// 根据条件循环访问IEnumerable泛型类的内容,将满足条件的结果发送给指定的方法,不满足条件的结果直接返回
        /// </summary>
        /// <typeparam name="T">类型标示符</typeparam>
        /// <typeparam name="R">返回值的类型标示符</typeparam>
        /// <param name="Source">IEnumerable泛型数组</param>
        /// <param name="ForEach">将值发送给指定的方法,该方法接受一个传入参数且不返回值,如:Console.WriteLine()</param>
        /// <param name="Condition">用于对比的判断类型</param>
        /// <returns>List数组</returns>
        public static IEnumerable<R> ForEach<T, R>(this IEnumerable<T> Source, Func<T, R> ForEach, dynamic Condition)
        {
            List<R> list = new List<R>();
            Source.ForEach(i =>
            {
                Dictionary<Predicate<bool>, Action> dic = new Dictionary<Predicate<bool>, Action>()
                {
                    { x => x == true,new Action(()=> list.Add(ForEach(i)))},
                    {x => x == false,new Action(()=>list.Add((R)(object)i))}
                };
                dic.Where(j => j.Key(i.GetHashCode() == Condition.GetHashCode())).ForEach(k => k.Value());
            });
            return list;
        }

        /// <summary>
        /// 对IEnumerable泛型类进行污染,循环访问IEnumerable泛型类里的内容,将每个结果发送给指定的方法,并返回指定方法运行后的结果
        /// </summary>
        /// <typeparam name="T">类型标示符</typeparam>
        /// <typeparam name="R">返回值的类型标示符</typeparam>
        /// <param name="Source">IEnumerable泛型数组</param>
        /// <param name="ForEach">将值发送给指定的方法,该方法接受一个传入参数且不返回值,如:Console.WriteLine()</param>
        /// <returns>方法执行后的运行结果列表</returns>
        public static IEnumerable<R> ForEach<T, R>(this IEnumerable<T> Source, Func<T, R> ForEach)
        {
            List<R> list = new List<R>();
            Source.ForEach(i => list.Add(ForEach(i)));
            return list;
        }

        /// <summary>
        /// 对IEnumerable泛型类进行污染,循环访问IEnumerable泛型类里的内容,将每个结果发送给指定的方法
        /// </summary>
        /// <typeparam name="T">类型标示符</typeparam>
        /// <param name="Source">IEnumerable泛型数组</param>
        /// <param name="ForEach">将值发送给指定的方法,该方法接受一个传入参数且不返回值,如:Console.WriteLine()</param>
        public static void ForEach<T>(this IEnumerable Source, Action<T> ForEach)
        {
            Source.Cast<T>().ForEach(i => ForEach(i));
        }

        /// <summary>
        /// 基于谓词筛选值序列.
        /// </summary>
        /// <typeparam name="T">序列的类型</typeparam>
        /// <param name="_Source">用于筛选的序列</param>
        /// <param name="_Predicate">用于测试每个元素是否满足条件的函数</param>
        /// <returns></returns>
        public static IEnumerable<T> Where<T>(this IEnumerable _Source, Predicate<T> _Predicate) where T : class
        {
            List<T> list = new List<T>();
            foreach (object item in _Source)
            {
                if (item is T t)
                {
                    if (_Predicate(t))
                    {
                        list.Add(t);
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// 连接IEnumerable全部值
        /// </summary>
        /// <param name="ie">数据</param>
        /// <returns></returns>
        public static string Link16(this IEnumerable ie)
        {
            string str = null;
            foreach (object item in ie)
            {
                str += Convert.ToByte(item).ToString("X2");
            }
            return str;
        }

        /// <summary>
        /// 连接IEnumerable全部值
        /// </summary>
        /// <param name="ie">数据</param>
        /// <returns></returns>
        public static string Link(this IEnumerable ie)
        {
            string str = null;
            foreach (object item in ie)
            {
                str += Convert.ToInt32(item);
            }
            return str;
        }

        /// <summary>
        /// 一个对象转化为一个只包含该对象的 List<T>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static List<T> ToSingletonList<T>(this T obj)
        {
            return new List<T>()
            {
                obj
            };
        }

        /// <summary>
        /// 一个对象转化为一个只包含该对象的 T[]
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T[] ToSingletonArray<T>(this T obj)
        {
            return new[] { obj };
        }

    }
}
