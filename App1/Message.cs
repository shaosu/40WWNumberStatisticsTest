using System.Text;

public class Message
{
    public bool 多线程模式 { get; set; }
    public int 线程数 { get; set; }
    public bool 单循环 { get; set; }
    public uint NumberCount { get; set; }
    public int Group { get; set; }
    public bool Mask模式 { get; set; }

    /// <summary>
    /// 预计最小使用内存MB
    /// </summary>
    public double CompleMinMem { get; set; }
    public double CompleMaxnMem { get; set; }
    /// <summary>
    /// 实际使用的:MB
    /// </summary>
    public double PaivateMemMax { get; set; }
    /// <summary>
    /// 实际使用的:MB
    /// </summary>
    public double WorkSetMemMax { get; set; }

    public int 分布区间 { get; set; }
    public int 不存在个数 { get; set; }
    public double 统计用时 { get; set; }
    public double 查找区间用时 { get; set; }
    public double 输出用时 { get; set; }
    public double 总用时 { get; set; }
    public override string ToString()
    {
        string Num;
        if (NumberCount >= 10000 * 10000)
        {
            Num = (NumberCount / 100000000.0).ToString("F1") + "WW";
        }
        else
        {
            Num = (NumberCount / 10000.0).ToString("F1") + "W";
        }

        StringBuilder sb = new StringBuilder();
        sb.Append(多线程模式);
        sb.Append(",");
        sb.Append(线程数);
        sb.Append(",");
        sb.Append(单循环);
        sb.Append(",");
        sb.Append(Num);
        sb.Append(",");
        sb.Append(Group);
        sb.Append(",");
        sb.Append(Mask模式);
        sb.Append(",");
        sb.Append(CompleMinMem);
        sb.Append(",");
        sb.Append(CompleMaxnMem);
        sb.Append(",");
        sb.Append(PaivateMemMax);
        sb.Append(",");
        sb.Append(WorkSetMemMax);
        sb.Append(",");
        sb.Append(分布区间);
        sb.Append(",");
        sb.Append(不存在个数);
        sb.Append(",");
        sb.Append(统计用时);
        sb.Append(",");
        sb.Append(查找区间用时);
        sb.Append(",");
        sb.Append(输出用时);
        sb.Append(",");
        sb.Append(总用时);
        sb.Append(",");
        return sb.ToString();
    }

    public static string GetHeaderString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("多线程模式");
        sb.Append(",");
        sb.Append("线程数");
        sb.Append(",");
        sb.Append(nameof(单循环));
        sb.Append(",");
        sb.Append("Num");
        sb.Append(",");
        sb.Append("Group");
        sb.Append(",");
        sb.Append("Mask模式");
        sb.Append(",");
        sb.Append("CompleMinMem");
        sb.Append(",");
        sb.Append("CompleMaxnMem");
        sb.Append(",");
        sb.Append("PaivateMemMax");
        sb.Append(",");
        sb.Append("WorkSetMemMax");
        sb.Append(",");
        sb.Append("分布区间");
        sb.Append(",");
        sb.Append("不存在个数");
        sb.Append(",");
        sb.Append("统计用时");
        sb.Append(",");
        sb.Append("查找区间用时");
        sb.Append(",");
        sb.Append("输出用时");
        sb.Append(",");
        sb.Append("总用时/秒");
        sb.Append(",");
        return sb.ToString();
    }

}
