//#define   UseBitConverter
using System;
using System.Collections;
using System.Collections.Generic;

public class RealFileTest
{
    public void Start()
    {
        const string Path = @"Test.txt"; // G:\
        const uint NumberCount = 64 * 100;// 4亿= 64 * 6250000
        const int Group = 64;
        long[] Counter = new long[Group];
        uint CounterMaxValue = (NumberCount / Group);// 64的整数倍时不加+
        if (NumberCount % Group != 0)
        {
            CounterMaxValue += 1;
        }

        int DateByteCount = sizeof(uint);
        byte[] data = new byte[DateByteCount];
        if (System.IO.File.Exists(Path) == false) // 产生文件,需保证数字值小于总数量
        {
            var wf = System.IO.File.OpenWrite(Path);
            var t1 = System.Threading.Tasks.Task.Run(async () =>
            {
                for (uint i = 0; i <= NumberCount; i++)
                {
#if UseBitConverter
                     data[0] = (byte)(i & 0xFF);
                     data[1] = (byte)(i >> 8);
                     data[2] = (byte)(i >> 16);
                     data[3] = (byte)(i >> 24);
#else
                    data = System.BitConverter.GetBytes(i);
#endif
                    if (i == 1 || i == 99 || i == 324) continue;
                    await wf.WriteAsync(data, 0, DateByteCount);
                }
            });
            t1.Wait();
            wf.Close();
        }

        var file = System.IO.File.OpenRead(Path);

        int c = file.Read(data, 0, DateByteCount);
        uint LineCount = 0;
        while (c >= DateByteCount && LineCount < NumberCount) // 统计
        {
#if UseBitConverter
            uint value = (uint)(data[1] << 8 | data[0]);
            value |= (uint)((data[2] << 16 | data[3] << 24));
#else
            uint value = System.BitConverter.ToUInt32(data);
#endif
            int index = (int)(value / CounterMaxValue);
            Counter[index]++;
            c = file.Read(data, 0, DateByteCount);
            LineCount++;
        }

        file.Position = 0;
        Dictionary<int, BitArray?>? dic = new Dictionary<int, BitArray?>();
        for (int i = 0; i < Group; i++) // 区间统计
        {
            if (Counter[i] < CounterMaxValue)
            {
                BitArray bits = new BitArray((int)CounterMaxValue);// 可先为Null,再在后面再申请空间
                dic.Add(i, bits);
            }
        }

        LineCount = 0;
        c = file.Read(data, 0, DateByteCount);
        while (c >= DateByteCount && LineCount < NumberCount) // 区间扫描
        {
#if UseBitConverter
            uint value = (uint)(data[1] << 8 | data[0]);
            value |= (uint)((data[2] << 16 | data[3] << 24));
#else
            uint value = System.BitConverter.ToUInt32(data);
#endif
            int index = (int)(value / CounterMaxValue); // index:区号
            if (dic.ContainsKey(index))
            {
                int pos = (int)(value - CounterMaxValue * index);
                dic[index].Set(pos, true); // 存在
            }
            c = file.Read(data, 0, DateByteCount);
            LineCount++;
        }
        file.Position = 0;
        file.Close();

        int NoHave = 0;
        foreach (var key in dic.Keys) // 输出
        {
            for (int i = 0; i < CounterMaxValue; i++)
            {
                if (dic[key].Get(i) == false) // false:不存在
                {
                    System.Console.WriteLine(key * CounterMaxValue + i);
                    NoHave++;
                }
            }
            dic[key] = null;
        }
        System.Console.WriteLine($"不存的数分布在:{dic.Count}个区间");
        System.Console.WriteLine($"不存在个数:{NoHave}");
        dic.Clear();
        dic = null;
        GC.Collect();
        Console.ReadLine();
        Console.ReadLine();
        Console.ReadLine();
    }
}
