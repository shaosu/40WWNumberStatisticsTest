using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

public class Program
{
    public static string TestLog = "TestLog.csv";

    private static uint GetNumberCount(string[] argc)
    {
        uint NumberCount = 64 * 6250000; // 4亿
        if (argc.Length >= 1)
        {
            NumberCount = 10000;
            uint v;
            if (uint.TryParse(argc[0], out v))
            {
                v = v > 400000 ? 400000 : v;
                if (v < 1) v = 1;
            }
            else
                v = 1;
            NumberCount *= v;
        }
        else
        {
            NumberCount = 64 * 6250000; // 4亿
        }
        return NumberCount;
    }

    private static int GetGroupCount(string[] argc)
    {
        int Group = 64;
        if (argc.Length >= 2)
        {
            int v;
            if (int.TryParse(argc[0], out v))
            {
                if (v > 640) v = 640;
                if (v < 4) v = 4;
                Group = v;
            }
            else
                Group = 4;
        }
        else
        {
            Group = 64; // 4亿
        }
        return Group;
    }

    static ulong CCC = 0;
    static System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
    private static void Move(string Src, string Tag)
    {
        CCC++;
        //Console.WriteLine($"{Src}->{Tag}");
        if (CCC % 100000000 == 0)
        {
            Console.WriteLine($"CCC:{CCC} ,{CCC/(ulong)sw.ElapsedMilliseconds}次/毫秒");
            sw.Stop();
            //Console.ReadLine();
            sw.Start();
        }
    }

    /// <summary>
    /// 汉诺塔
    /// I5-7500 CPU @ 3.40GHz 单线程
    /// 64位Release    CCC:4300000000 ,242349次/毫秒
    /// 64位Debug      CCC:500000000  ,74404次/毫秒
    /// AnyCPU,Debug    CCC:300000000 ,75719次/毫秒
    /// AnyCPU,Release CCC:1000000000 ,245398次/毫秒
    /// </summary>
    /// <param name="n"></param>
    /// <param name="A">Src</param>
    /// <param name="B">Help:空的</param>
    /// <param name="C">Tag</param>
    private static void HNT(int n, string A, string B, string C)
    {
        if (n == 1)
        {
            Move(A, C);
        }
        else
        {
            HNT(n - 1, A, C, B);
            Move(A, C);
            HNT(n - 1, B, A, C);
        }
    }

    public static void Main(string[] argc)
    {
        int n = 64;
        if (argc.Length >= 1)
        {
            if (int.TryParse(argc[0], out n))
            {
                if (n <= 1)
                    n = 1;
            }
        }
        sw.Start();
        HNT(n, "A", "B", "C");
        return;

        if (File.Exists(TestLog) == false)
        {
            File.AppendAllLines(TestLog, new string[1] { Message.GetHeaderString() });
        }
        uint NumberCount = GetNumberCount(argc);
        int Group = GetGroupCount(argc);
        //VirualFileTestMTH mth = new VirualFileTestMTH();
        //VirualFileTest signal = new VirualFileTest();
        //Message msg1 = mth.Start(false, 4000 * 10000, 640, true);
        //Console.WriteLine("   按任意键退出!");
        //Console.ReadLine();
        //return;

        Console.WriteLine("Step 1");
        StartALL(true, NumberCount, Group, true);
        Console.WriteLine("Step 2");
        StartALL(false, NumberCount, Group, true);
        Console.WriteLine("Step 3");
        StartALL(false, NumberCount, Group, false);
        Console.WriteLine("Step 4");
        StartALL(true, NumberCount, Group, false);

        Console.WriteLine("   按任意键退出!");
        Console.ReadLine();
    }

    public static void StartALL(bool one, uint NumberCount, int Group, bool mask)
    {
        List<string> logs = new List<string>();

        VirualFileTestMTH mth = new VirualFileTestMTH();
        VirualFileTest signal = new VirualFileTest();
        Message msg1 = mth.Start(one, NumberCount, Group, mask);
        Console.WriteLine();
        Message msg2 = signal.Start(one, NumberCount, Group, mask);

        logs.Add(msg1.ToString());
        logs.Add(msg2.ToString());
        File.AppendAllLines(TestLog, logs);
        Thread.Sleep(500);
    }
}
