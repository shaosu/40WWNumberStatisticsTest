#define MTH // 多线程

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

/// <summary>
/// 非线程安全,不可再开多线程
/// </summary>
public class VirualFileTestMTH
{
    private class Param
    {
        public IVirualFile VFile;

        /// <summary>
        /// 本线程从哪里开始统计
        /// </summary>
        public ulong MyStartPos;

        /// <summary>
        /// 本线程需要统计的个数
        /// </summary>
        public uint MyNumberCount;

        /// <summary>
        /// 多次扫描时使用
        /// </summary>
        public int TagIndex;

        /// <summary>
        /// 总共要统计的数量
        /// </summary>
        public static uint NumberCount;

        public static uint CounterMaxValue;
        public static int Group = 640;
    }

    public long[] 多线程统计(IVirualFile file)
    {
        int PCount = Environment.ProcessorCount;
        Task<long[]>[] tasks = new Task<long[]>[PCount]; // 4
        uint DW = (Param.NumberCount / (uint)PCount);
        for (int i = 0; i < PCount; i++)
        {
            var par = new Param() { VFile = file.DeepCopy() };
            par.MyStartPos = (ulong)i * DW;
            par.MyNumberCount = DW;
            tasks[i] = new Task<long[]>(FFF, par);
        }

        for (int i = 0; i < PCount; i++)
        {
            tasks[i].Start();
        }
        Task.WaitAll(tasks);
        long[] Counter = new long[Param.Group];
        for (int i = 0; i < PCount; i++)
        {
            for (int j = 0; j < Counter.Length; j++)
            {
                Counter[j] += tasks[i].Result[j];
            }
        }
        if (Param.NumberCount % PCount != 0) // 不整除表示有剩下没有统计到
        {
            var par = new Param() { VFile = file.DeepCopy() };
            par.MyStartPos = (ulong)PCount * DW;
            par.MyNumberCount = (uint)(Param.NumberCount - par.MyStartPos);
            var last = FFF(par);
            for (int i = 0; i < Counter.Length; i++)
            {
                Counter[i] += last[i];
            }
        }
        return Counter;
    }

    private long[] FFF(object? a)
    {
        long[] Counter = new long[Param.Group];
        if (a != null)
        {
            Param param = (Param)a;
            uint LineCount = 0;
            param.VFile.Position = param.MyStartPos; // 跳过
            while (LineCount < param.MyNumberCount)
            {
                uint? value = param.VFile.ReadUInt(Param.NumberCount);
                if (value.HasValue)
                {
                    int index = (int)(value / Param.CounterMaxValue);
                    Counter[index]++;
                }
                LineCount++;
            }
            return Counter;
        }
        return Counter;
    }

    private static BitArray bits;
    private static Process proc = Process.GetCurrentProcess();

    public int 多线程区间扫描(IVirualFile file, int TagIndex)
    {
        int PCount = Environment.ProcessorCount;
        Task[] tasks = new Task[PCount]; // 4
        uint DW = (Param.NumberCount / (uint)PCount);

        for (int i = 0; i < PCount; i++)
        {
            var par = new Param() { VFile = file.DeepCopy() };
            par.TagIndex = TagIndex;
            par.MyStartPos = (ulong)i * DW;
            par.MyNumberCount = DW;
            if (OneLoop)
                tasks[i] = new Task(SSSS_OneLoop, par);
            else
                tasks[i] = new Task(SSSS, par);
        }

        for (int i = 0; i < PCount; i++)
        {
            tasks[i].Start();
        }
        Task.WaitAll(tasks);

        if (Param.NumberCount % PCount != 0) // 不整除表示有剩下没有统计到
        {
            var par = new Param() { VFile = file.DeepCopy() };
            par.TagIndex = TagIndex;
            par.MyStartPos = (ulong)PCount * DW;
            par.MyNumberCount = (uint)(Param.NumberCount - par.MyStartPos);
            if (OneLoop)
                SSSS_OneLoop(par);
            else
                SSSS_OneLoop(par);
        }

        // 直接输出
        int NoHave = 0;
        if (OneLoop == false)
        {
            for (int j = 0; j < Param.CounterMaxValue; j++)
            {
                if (bits.Get(j) == false) // false:不存在
                {
                    if (IsMaskMode == false)// 结果太多不输出
                        Console.WriteLine(TagIndex * Param.CounterMaxValue + j);
                    NoHave++;
                }
            }
        }
        return NoHave;
    }

    private void SSSS_OneLoop(object? a)
    {
        if (a != null)
        {
            Param param = (Param)a;
            uint LineCount = 0;
            param.VFile.Position = param.MyStartPos; // 跳过
            while (LineCount < param.MyNumberCount)
            {
                uint? value = param.VFile.ReadUInt(Param.NumberCount);
                if (value.HasValue)
                {
                    int index = (int)(value / Param.CounterMaxValue); // index:区号
                    if (dic.ContainsKey(index))
                    {
                        int pos = (int)(value - Param.CounterMaxValue * index);
                        dic[index].Set(pos, true); // 存在
                    }
                }
                LineCount++;
            }
        }
    }

    private void SSSS(object? a)
    {
        if (a != null)
        {
            Param param = (Param)a;
            uint LineCount = 0;

            param.VFile.Position = param.MyStartPos; // 跳过
            while (LineCount < param.MyNumberCount)
            {
                uint? value = param.VFile.ReadUInt(Param.NumberCount);
                if (value.HasValue)
                {
                    int index = (int)(value / Param.CounterMaxValue); // index:区号
                    if (param.TagIndex == index)
                    {
                        int pos = (int)(value - Param.CounterMaxValue * index);
                        bits.Set(pos, true); // 存在
                    }
                }
                LineCount++;
            }
            // 当前任务的区间遍历完
        }
    }

    private static Dictionary<int, BitArray?>? dic;
    private static bool OneLoop;
    private static bool IsMaskMode;

    public static void UpdateMem(Message msg)
    {
        var mb = proc.WorkingSet64 / 1024F / 1024F;
        msg.WorkSetMemMax = mb > msg.WorkSetMemMax ? mb : msg.WorkSetMemMax;
        mb = proc.PrivateMemorySize64 / 1024F / 1024F;
        msg.PaivateMemMax = mb > msg.PaivateMemMax ? mb : msg.PaivateMemMax;
    }

    /// <summary>
    /// 开始计算
    /// </summary>
    /// <param name="one">使用一次循环:占用空间大,多次循环:占用空间小</param>
    /// <param name="NumberCount">4亿</param>
    /// <param name="Group">64组</param>
    public Message Start(bool one = true, uint NumberCount = 64 * 6250000, int Group = 64, bool mask = false)
    {
        if (Group < Environment.ProcessorCount)
            Group = Environment.ProcessorCount;

        Message msg = new Message();
        msg.多线程模式 = true;
        msg.线程数 = Environment.ProcessorCount;
        msg.单循环 = one;
        msg.NumberCount = NumberCount;
        msg.Group = Group;
        msg.Mask模式 = mask;
        double MB = (double)NumberCount / Group / 8.0 / 1024.0 / 1024.0;
        msg.CompleMinMem = MB;
        msg.CompleMaxnMem = MB;
        if (OneLoop)
            msg.CompleMaxnMem = MB * Group;

        OneLoop = one;
        IsMaskMode = mask;
        Stopwatch sw = new Stopwatch();
        double Times = 0;

        IVirualFile file;
        if (IsMaskMode)
            file = new VirualFileMask((ulong)Group);
        else
            file = new VirualFile(new ulong[] { 1, 99, 324 });

        long[] Counter = new long[Group];
        uint CounterMaxValue = (uint)(NumberCount / Group);// 1024
        if (NumberCount % Group != 0)
        {
            CounterMaxValue += 1;
        }

        Param.Group = Group;
        Param.CounterMaxValue = CounterMaxValue;
        Param.NumberCount = NumberCount;

        #region '      统计过程

        sw.Restart();
        Counter = 多线程统计(file);
        sw.Stop();
        UpdateMem(msg);
        Times += sw.Elapsed.TotalSeconds;
        msg.统计用时 = sw.Elapsed.TotalSeconds;

        #endregion '      统计过程

        Times += 区间统计过程(Group, CounterMaxValue, Counter, sw, file);
        UpdateMem(msg);
        msg.分布区间 = dic.Count;

        #region '      区间扫描过程

        sw.Restart();
        if (OneLoop)
            多线程区间扫描(file, 0);
        else
        {
            //方法1: 开4个任务,每个任务处理一个文件片段,进行Dic.Connt次
            //方法1(msg, file);   //4000W => 331.4667895
            //方法2: 开Dic.Connt个任务,每个任务遍历全部文件
            方法2(msg, file); //4000W => 297.1983686秒
        }

        sw.Stop();
        UpdateMem(msg);
        Times += sw.Elapsed.TotalSeconds;
        msg.查找区间用时 = sw.Elapsed.TotalSeconds;

        #endregion '      区间扫描过程

        if (OneLoop)
        {
            Times += msg.输出用时 = 输出结果(CounterMaxValue, sw, msg);
            UpdateMem(msg);
        }
        msg.总用时 = Times;
        Console.WriteLine($"多线程总用时:{Times}秒");
        return msg;
    }

    private void 方法1(Message msg, IVirualFile file)
    {
        int NoHave = 0;
        bits = new BitArray((int)Param.CounterMaxValue);
        foreach (var item in dic)
        {
            bits.SetAll(false);
            NoHave += 多线程区间扫描(file, item.Key);
        }
        msg.不存在个数 = NoHave;
    }

    private void 方法2(Message msg, IVirualFile file)
    {
        Task<int>[] tasks = new Task<int>[dic.Count]; // 4
        int ti = 0;
        foreach (var kv in dic)
        {
            var par = new 方法2_SSS_Param() { TagIndex = kv.Key, VFile = file.DeepCopy() };
            tasks[ti] = new Task<int>(方法2_SSS, par);
            ti++;
        }
        foreach (var t in tasks)
        {
            t.Start();
        }
        Task.WaitAll(tasks);
        foreach (var t in tasks)
        {
            msg.不存在个数 += t.Result;
        }
    }

    private class 方法2_SSS_Param
    {
        public IVirualFile VFile;

        /// <summary>
        /// 多次扫描时使用
        /// </summary>
        public int TagIndex;
    }

    private int 方法2_SSS(object? obj)
    {
        int NoHave = 0;
        if (obj != null)
        {
            方法2_SSS_Param param = (方法2_SSS_Param)obj;
            var bits = new BitArray((int)Param.CounterMaxValue);
            uint LineCount = 0;
            param.VFile.Position = 0;
            while (LineCount < Param.NumberCount)
            {
                uint? value = param.VFile.ReadUInt(Param.NumberCount);
                if (value.HasValue)
                {
                    int index = (int)(value / Param.CounterMaxValue); // index:区号
                    if (param.TagIndex == index)
                    {
                        int pos = (int)(value - Param.CounterMaxValue * index);
                        bits.Set(pos, true); // 存在
                    }
                }
                LineCount++;
            }
            param.VFile.Position = 0;

            // 直接输出
            for (int j = 0; j < Param.CounterMaxValue; j++)
            {
                if (bits.Get(j) == false) // false:不存在
                {
                    if (IsMaskMode == false)// 结果太多不输出
                        Console.WriteLine(param.TagIndex * Param.CounterMaxValue + j);
                    NoHave++;
                }
            }
            bits = null;
        }
        return NoHave;
    }

    private static double 输出结果(uint CounterMaxValue, Stopwatch sw, Message msg)
    {
        sw.Restart();
        int NoHave = 0;
        foreach (var key in dic.Keys)
        {
            for (int i = 0; i < CounterMaxValue; i++)
            {
                if (dic[key].Get(i) == false) // false:不存在
                {
                    if (IsMaskMode == false)// 结果太多不输出
                        Console.WriteLine(key * CounterMaxValue + i);
                    NoHave++;
                }
            }
            dic[key] = null;
        }
        sw.Stop();
        msg.不存在个数 = NoHave;
        dic.Clear();
        dic = null;
        GC.Collect();
        return sw.Elapsed.TotalSeconds;
    }

    private static double 区间统计过程(int Group, uint CounterMaxValue, long[] Counter, Stopwatch sw, IVirualFile file)
    {
        sw.Restart();
        file.Position = 0;
        dic = new Dictionary<int, BitArray?>();
        for (int i = 0; i < Group; i++)
        {
            if (Counter[i] < CounterMaxValue)
            {
                BitArray bits;
                if (OneLoop)
                    bits = new BitArray((int)CounterMaxValue);
                else
                    bits = null;
                dic.Add(i, bits);
            }
        }
        sw.Stop();
        return sw.Elapsed.TotalSeconds;
    }
}
