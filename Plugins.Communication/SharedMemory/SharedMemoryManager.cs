using System.IO.MemoryMappedFiles;
using System.Threading;

namespace Plugins.Communication.SharedMemory
{
    public class SharedMemoryManager
    {
        private static SharedMemoryManager instance;
        // 定义一个标识确保线程同步
        private static readonly object locker = new object();

        public static SharedMemoryManager GetInstance()
        {
            if (instance == null)
            {
                lock (locker)
                {
                    if (instance == null)
                    {
                        instance = new SharedMemoryManager();
                    }
                }
            }

            return instance;
        }

        private MemoryMappedFile memoryMappedFileMaster;
        public MemoryMappedFile MemoryMappedFileMaster
        {
            get
            {
                return memoryMappedFileMaster;
            }
            set
            {
                if (value != memoryMappedFileMaster)
                {
                    memoryMappedFileMaster = value;
                }
            }
        }

        private Mutex myMutex;
        /// <summary>
        /// 操作互斥锁
        /// </summary>
        public Mutex MyMutex
        {
            get
            {
                return myMutex;
            }
            set
            {
                if (value != myMutex)
                {
                    myMutex = value;
                }
            }
        }

        private MemoryMappedViewAccessor accessor;
        public MemoryMappedViewAccessor Accessor
        {
            get
            {
                return accessor;
            }
            set
            {
                if (value != accessor)
                {
                    accessor = value;
                }
            }
        }

        /// <summary>
        /// 默认开辟128M的buffer共享内存空间
        /// </summary>
        private int DefaultBufferSize = 1024 * 1024 * 128;

        /// <summary>
        /// 默认使用的共享内存名称
        /// </summary>
        private string DefaultShareName = "SharedMemory";

        /// <summary>
        /// 默认互斥锁的名称
        /// </summary>
        private string DefaultMutexName = "MyMutex";

        public long HaveWrite;

        private SharedMemoryManager()
        { }

        /// <summary>
        /// 按默认参数初始化
        /// </summary>
        public void Init()
        {
            InitSharedMemeory();
        }

        /// <summary>
        /// 按所填的参数初始化
        /// </summary>
        /// <param name="defaultBufferSize">buffer共享内存空间</param>
        /// <param name="defaultShareName">共享内存名称</param>
        /// <param name="defaultMutexName">互斥锁的名称</param>
        public void Init(int bufferSize, string shareName, string mutexName)
        {
            //赋值
            DefaultBufferSize = bufferSize;
            DefaultShareName = shareName;
            DefaultMutexName = mutexName;

            InitSharedMemeory();
        }

        private void InitSharedMemeory()
        {
            MemoryMappedFileMaster = MemoryMappedFile.CreateOrOpen(DefaultShareName, DefaultBufferSize, MemoryMappedFileAccess.ReadWrite);
            Accessor = MemoryMappedFileMaster.CreateViewAccessor();

            MyMutex = new Mutex(false, DefaultMutexName);
        }

        /// <summary>
        /// 写入double数据进共享内存
        /// </summary>
        /// <param name="position">指定要写入的起始位置（偏移量），表示写入操作开始的内存位置。这里的位置是以字节为单位的偏移量。</param>
        /// <param name="sourceArray">要写入非托管内存的源数组。</param>
        /// <param name="offset">在源数组中写入操作的起始位置（偏移量）。表示从数组的哪个索引开始进行写入操作。</param>
        /// <param name="count">要写入的元素数量，表示要写入的元素个数。</param>
        public void WriteArrayToMemory(long position, double[] sourceArray, int offset = 0, int count = 0)
        {
            if (MyMutex != null && Accessor != null)
            {
                //MyMutex.WaitOne();

                if (count == 0)
                {
                    count = sourceArray.Length;
                }
                Accessor.WriteArray(position, sourceArray, offset, count);

                HaveWrite = count;

            }
            else
            {
                throw new System.Exception($"WriteArrayToMemory Error, MyMutex Or Accessor is Null");
            }
        }

        /// <summary>
        /// 写入byte数据进共享内存
        /// </summary>
        /// <param name="position">指定要写入的起始位置（偏移量），表示写入操作开始的内存位置。这里的位置是以字节为单位的偏移量。</param>
        /// <param name="sourceArray">要写入非托管内存的源数组。</param>
        /// <param name="offset">在源数组中写入操作的起始位置（偏移量）。表示从数组的哪个索引开始进行写入操作。</param>
        /// <param name="count">要写入的元素数量，表示要写入的元素个数。</param>
        /// <exception cref="System.Exception"></exception>
        public void WriteArrayToMemory(long position, byte[] sourceArray, int offset = 0, int count = 0)
        {
            if (MyMutex != null && Accessor != null)
            {
                MyMutex.WaitOne();

                if (count == 0)
                {
                    count = sourceArray.Length;
                }
                Accessor.WriteArray(position, sourceArray, offset, count);

                HaveWrite = count;

                MyMutex.ReleaseMutex();
            }
            else
            {
                throw new System.Exception($"WriteArrayToMemory Error, MyMutex Or Accessor is Null");
            }
        }

        public void WriteToMemory(long position, double source)
        {
            accessor.Write(position, source);
        }

        public byte[] ReadFromMemory(long position, int count)
        {
            byte[] values = new byte[count];

            Accessor.ReadArray(position, values, 0, count);

            return values;
        }

        public void CloseSharedMemory()
        {
            // 释放托管资源
            if (accessor != null)
            {
                accessor.Dispose();
                accessor = null;
            }

            if (memoryMappedFileMaster != null)
            {
                memoryMappedFileMaster.Dispose();
                memoryMappedFileMaster = null;
            }

            if (myMutex != null)
            {
                myMutex.Close();
                myMutex = null;
            }
        }
    }
}
