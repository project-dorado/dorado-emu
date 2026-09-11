using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Net;

/// <summary>Reads XNA-typed values from an in-memory packet.</summary>
public class PacketReader : BinaryReader
{
    public PacketReader()
        : this(0)
    {
    }

    public PacketReader(int capacity)
        : base(new MemoryStream(capacity))
    {
    }

    public int Length => (int)BaseStream.Length;

    public int Position
    {
        get => (int)BaseStream.Position;
        set => BaseStream.Position = value;
    }

    public Vector2 ReadVector2() => new(ReadSingle(), ReadSingle());

    public Vector3 ReadVector3() => new(ReadSingle(), ReadSingle(), ReadSingle());

    public Vector4 ReadVector4() => new(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());

    public Matrix ReadMatrix() =>
        new(
            ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle(),
            ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle(),
            ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle(),
            ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());

    public Quaternion ReadQuaternion() => new(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());

    public Color ReadColor() => new(ReadUInt32());

    public override float ReadSingle() => base.ReadSingle();

    public override double ReadDouble() => base.ReadDouble();
}

/// <summary>Writes XNA-typed values to an in-memory packet.</summary>
public class PacketWriter : BinaryWriter
{
    public PacketWriter()
        : this(0)
    {
    }

    public PacketWriter(int capacity)
        : base(new MemoryStream(capacity))
    {
    }

    public int Length => (int)BaseStream.Length;

    public int Position
    {
        get => (int)BaseStream.Position;
        set => BaseStream.Position = value;
    }

    public void Write(Vector2 value)
    {
        Write(value.X);
        Write(value.Y);
    }

    public void Write(Vector3 value)
    {
        Write(value.X);
        Write(value.Y);
        Write(value.Z);
    }

    public void Write(Vector4 value)
    {
        Write(value.X);
        Write(value.Y);
        Write(value.Z);
        Write(value.W);
    }

    public void Write(Matrix value)
    {
        Write(value.M11);
        Write(value.M12);
        Write(value.M13);
        Write(value.M14);
        Write(value.M21);
        Write(value.M22);
        Write(value.M23);
        Write(value.M24);
        Write(value.M31);
        Write(value.M32);
        Write(value.M33);
        Write(value.M34);
        Write(value.M41);
        Write(value.M42);
        Write(value.M43);
        Write(value.M44);
    }

    public void Write(Quaternion value)
    {
        Write(value.X);
        Write(value.Y);
        Write(value.Z);
        Write(value.W);
    }

    public void Write(Color value) => Write(value.PackedValue);

    public override void Write(float value) => base.Write(value);

    public override void Write(double value) => base.Write(value);
}
