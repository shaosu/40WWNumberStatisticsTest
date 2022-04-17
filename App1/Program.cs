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

    public static void Main(string[] argc)
    {
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
