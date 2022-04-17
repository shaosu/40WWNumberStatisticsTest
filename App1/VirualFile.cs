using System;
using System.Linq;

public interface IVirualFile
{
    public ulong? ReadULong(ulong NumberCount);

    public UInt32? ReadUInt(ulong NumberCount);

    public UInt16? ReadUInt16(ulong NumberCount);

    public ulong Position { get; set; }
    public ulong MaxValue { get; set; }

    public IVirualFile DeepCopy();
}

public class VirualFileMask : IVirualFile
{
    public ulong Mask;
    public ulong Position { get; set; }
    public ulong MaxValue { get; set; }

    public VirualFileMask(ulong m)
    {
        Mask = m;
        MaxValue = ulong.MaxValue;
        Position = 0;
    }

    public IVirualFile DeepCopy()
    {
        VirualFileMask var = new VirualFileMask(Mask);
        var.Position = Position;
        var.MaxValue = MaxValue;
        var.Mask = Mask;
        Position = 0;
        return var;
    }

    public ulong? ReadULong(ulong NumberCount)
    {
        if (Position >= NumberCount)
            return null;
        ulong value = Position;
        Position++;
        if (value % Mask == 0)
        {
            return null;
        }
        return value;
    }

    public UInt32? ReadUInt(ulong NumberCount)
    {
        if (Position >= NumberCount)
            return null;
        ulong value = Position;
        Position++;
        if (value % Mask == 0)
        {
            return null;
        }
        return (UInt32)value;
    }

    public UInt16? ReadUInt16(ulong NumberCount)
    {
        if (Position >= NumberCount)
            return null;
        ulong value = Position;
        Position++;
        if (value % Mask == 0)
        {
            return null;
        }
        return (ushort)value;
    }
}

public class VirualFile : IVirualFile
{
    private ulong[] Miss;
    public ulong Position { get; set; }
    public ulong MaxValue { get; set; }

    public IVirualFile DeepCopy()
    {
        VirualFile var = new VirualFile(Miss);
        var.Position = Position;
        var.MaxValue = MaxValue;
        Position = 0;
        return var;
    }

    public VirualFile(ulong[] miss)
    {
        this.Miss = miss;
    }

    public virtual ulong? ReadULong(ulong NumberCount)
    {
        if (Position >= NumberCount)
            return null;
        ulong value = Position;
        Position++;
        if (Miss.Contains(value))
        {
            return null;
        }
        return value;
    }

    public virtual UInt32? ReadUInt(ulong NumberCount)
    {
        if (Position >= NumberCount)
            return null;
        ulong value = Position;
        Position++;
        if (Miss.Contains(value))
        {
            return null;
        }
        return (UInt32)value;
    }

    public virtual UInt16? ReadUInt16(ulong NumberCount)
    {
        if (Position >= NumberCount)
            return null;
        ulong value = Position;
        Position++;
        if (Miss.Contains(value))
        {
            return null;
        }
        return (ushort)value;
    }
}
