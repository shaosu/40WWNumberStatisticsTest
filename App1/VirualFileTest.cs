using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
/// 非线程安全,不可再开多线程
/// </summary>
public class VirualFileTest
{
    private static Dictionary<int, BitArray?>? dic;

    private static bool OneLoop;
    private static bool IsMaskMode;
    private static Process proc = Process.GetCurrentProcess();

    public void UpdateMem(Message msg)
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
    public Message Start(bool one = true, uint NumberCount = 64 * 6250000, int Group = 640, bool mask = false)
    {
        if (Group < Environment.ProcessorCount)
            Group = Environment.ProcessorCount;

        Message msg = new Message();
        msg.多线程模式 = false;
        msg.线程数 = 1;
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

        Times += msg.统计用时 = 统计过程(NumberCount, CounterMaxValue, Counter, sw, file);
        UpdateMem(msg);
        Times += 区间统计过程(Group, CounterMaxValue, Counter, sw, file);
        msg.分布区间 = dic.Count;

        UpdateMem(msg);
        Times += msg.查找区间用时 = 区间扫描过程(NumberCount, CounterMaxValue, Group, sw, file, msg);
        UpdateMem(msg);
        if (OneLoop)
        {
            Times += msg.输出用时 = 输出结果(CounterMaxValue, sw, msg);
            UpdateMem(msg);
        }
        msg.总用时 = Times;
        Console.WriteLine($"单线程总用时:{Times}秒");
        return msg;
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

    private static double 区间扫描过程(uint NumberCount, uint CounterMaxValue, int Group, Stopwatch sw, IVirualFile file, Message msg)
    {
        sw.Restart();
        uint LineCount;
        if (OneLoop)
        {
            LineCount = 0;
            while (LineCount < NumberCount)
            {
                uint? value = file.ReadUInt(NumberCount);
                if (value.HasValue)
                {
                    int index = (int)(value / CounterMaxValue); // index:区号
                    if (dic.ContainsKey(index))
                    {
                        int pos = (int)(value - CounterMaxValue * index);
                        dic[index].Set(pos, true); // 存在
                    }
                }
                LineCount++;
            }
            file.Position = 0;
        }
        else
        {
            BitArray bits;
            int NoHave = 0;
            bits = new BitArray((int)CounterMaxValue);
            for (int i = 0; i < Group; i++)
            {
                if (dic.ContainsKey(i))
                {
                    var Tagindex = i;
                    bits.SetAll(false);
                    LineCount = 0;
                    while (LineCount < NumberCount)
                    {
                        uint? value = file.ReadUInt(NumberCount);
                        if (value.HasValue)
                        {
                            int index = (int)(value / CounterMaxValue); // index:区号
                            if (Tagindex == index)
                            {
                                int pos = (int)(value - CounterMaxValue * index);
                                bits.Set(pos, true); // 存在
                            }
                        }
                        LineCount++;
                    }
                    file.Position = 0;

                    // 直接输出
                    for (int j = 0; j < CounterMaxValue; j++)
                    {
                        if (bits.Get(j) == false) // false:不存在
                        {
                            if (IsMaskMode == false)// 结果太多不输出
                                Console.WriteLine(Tagindex * CounterMaxValue + j);
                            NoHave++;
                        }
                    }
                }
            }
            msg.不存在个数 = NoHave;
        }
        sw.Stop();
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

    private static double 统计过程(uint NumberCount, uint CounterMaxValue, long[] Counter, Stopwatch sw, IVirualFile file)
    {
        sw.Restart();
        uint LineCount = 0;
        while (LineCount < NumberCount)
        {
            uint? value = file.ReadUInt(NumberCount);
            if (value.HasValue)
            {
                int index = (int)(value / CounterMaxValue);
                Counter[index]++;
            }
            LineCount++;
        }
        sw.Stop();  // 248 249 250 249   =>1 99 324 999
        return sw.Elapsed.TotalSeconds;
    }
}
