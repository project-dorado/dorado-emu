// Managed port of Mark Adler's public-domain puff.c (contrib/puff, zlib),
// extended with preset-dictionary support so MSZIP cabinet blocks — each a
// complete DEFLATE stream that references the previous 32 KiB window — can be
// decoded without native zlib. See docs/container-formats.md.

namespace Dorado.Containers.Internal;

/// <summary>Raised when a DEFLATE stream is malformed or truncated.</summary>
internal sealed class PuffException : Exception
{
    public PuffException(int code, string message)
        : base(message)
    {
        Code = code;
    }

    public int Code { get; }
}

/// <summary>A raw-DEFLATE decoder with an optional preset dictionary.</summary>
internal sealed class Puff
{
    private const int MaxBits = 15;
    private const int MaxLcodes = 286;
    private const int MaxDcodes = 30;
    private const int MaxCodes = MaxLcodes + MaxDcodes;
    private const int FixLcodes = 288;

    private static readonly Huffman FixedLiteral = BuildFixedLiteral();
    private static readonly Huffman FixedDistance = BuildFixedDistance();

    private readonly byte[] _input;
    private readonly int _inputLength;
    private int _inputCount;
    private int _bitBuffer;
    private int _bitCount;

    private byte[] _output;
    private int _outputCount;
    private readonly int _dictionaryLength;

    private Puff(byte[] input, int inputLength, byte[] dictionary, int expectedSize)
    {
        _input = input;
        _inputLength = inputLength;
        _dictionaryLength = dictionary.Length;

        int capacity = Math.Max(64, dictionary.Length + expectedSize);
        _output = new byte[capacity];
        Array.Copy(dictionary, _output, dictionary.Length);
        _outputCount = dictionary.Length;
    }

    /// <summary>Inflates one raw DEFLATE stream whose window is seeded by <paramref name="dictionary"/>.</summary>
    public static byte[] Inflate(byte[] compressed, int compressedLength, byte[] dictionary, int expectedSize)
    {
        var puff = new Puff(compressed, compressedLength, dictionary, expectedSize);
        puff.Run();
        int produced = puff._outputCount - puff._dictionaryLength;
        var result = new byte[produced];
        Array.Copy(puff._output, puff._dictionaryLength, result, 0, produced);
        return result;
    }

    private void Run()
    {
        int last;
        do
        {
            last = Bits(1);
            int type = Bits(2);
            int err = type switch
            {
                0 => Stored(),
                1 => Codes(FixedLiteral, FixedDistance),
                2 => Dynamic(),
                _ => -1,
            };

            if (err != 0)
            {
                throw new PuffException(err, $"DEFLATE error {err} ({Describe(err)}).");
            }
        }
        while (last == 0);
    }

    private static string Describe(int err) => err switch
    {
        -1 => "invalid block type",
        -3 => "too many length or distance codes",
        -4 => "code-length code incomplete",
        -5 => "repeat with no preceding length",
        -6 => "repeat exceeds declared lengths",
        -7 => "invalid literal/length code lengths",
        -8 => "invalid distance code lengths",
        -9 => "missing end-of-block code",
        -10 => "invalid literal/length or distance symbol",
        -11 => "distance too far back",
        _ => "malformed stream",
    };

    private int Bits(int need)
    {
        int val = _bitBuffer;
        while (_bitCount < need)
        {
            if (_inputCount == _inputLength)
            {
                throw new PuffException(2, "Unexpected end of DEFLATE input.");
            }

            val |= _input[_inputCount++] << _bitCount;
            _bitCount += 8;
        }

        _bitBuffer = val >> need;
        _bitCount -= need;
        return val & ((1 << need) - 1);
    }

    private int Stored()
    {
        _bitBuffer = 0;
        _bitCount = 0;

        if (_inputCount + 4 > _inputLength)
        {
            return 2;
        }

        int len = _input[_inputCount++];
        len |= _input[_inputCount++] << 8;
        if (_input[_inputCount++] != (~len & 0xff) || _input[_inputCount++] != ((~len >> 8) & 0xff))
        {
            return -2;
        }

        if (_inputCount + len > _inputLength)
        {
            return 2;
        }

        Ensure(len);
        Array.Copy(_input, _inputCount, _output, _outputCount, len);
        _inputCount += len;
        _outputCount += len;
        return 0;
    }

    private int Codes(Huffman lencode, Huffman distcode)
    {
        int symbol;
        do
        {
            symbol = Decode(lencode);
            if (symbol < 0)
            {
                return symbol;
            }

            if (symbol < 256)
            {
                Ensure(1);
                _output[_outputCount++] = (byte)symbol;
            }
            else if (symbol > 256)
            {
                symbol -= 257;
                if (symbol >= 29)
                {
                    return -10;
                }

                int len = LengthBase[symbol] + Bits(LengthExtra[symbol]);

                symbol = Decode(distcode);
                if (symbol < 0)
                {
                    return symbol;
                }

                int dist = DistanceBase[symbol] + Bits(DistanceExtra[symbol]);
                if (dist > _outputCount)
                {
                    return -11;
                }

                Ensure(len);
                for (int i = 0; i < len; i++)
                {
                    _output[_outputCount] = _output[_outputCount - dist];
                    _outputCount++;
                }
            }
        }
        while (symbol != 256);

        return 0;
    }

    private int Dynamic()
    {
        var lengths = new short[MaxCodes];
        int nlen = Bits(5) + 257;
        int ndist = Bits(5) + 1;
        int ncode = Bits(4) + 4;
        if (nlen > MaxLcodes || ndist > MaxDcodes)
        {
            return -3;
        }

        int index;
        for (index = 0; index < ncode; index++)
        {
            lengths[Order[index]] = (short)Bits(3);
        }

        for (; index < 19; index++)
        {
            lengths[Order[index]] = 0;
        }

        var lencode = new Huffman();
        int err = Construct(lencode, lengths, 19);
        if (err != 0)
        {
            return -4;
        }

        index = 0;
        while (index < nlen + ndist)
        {
            int symbol = Decode(lencode);
            if (symbol < 0)
            {
                return symbol;
            }

            if (symbol < 16)
            {
                lengths[index++] = (short)symbol;
            }
            else
            {
                int len = 0;
                if (symbol == 16)
                {
                    if (index == 0)
                    {
                        return -5;
                    }

                    len = lengths[index - 1];
                    symbol = 3 + Bits(2);
                }
                else if (symbol == 17)
                {
                    symbol = 3 + Bits(3);
                }
                else
                {
                    symbol = 11 + Bits(7);
                }

                if (index + symbol > nlen + ndist)
                {
                    return -6;
                }

                while (symbol-- > 0)
                {
                    lengths[index++] = (short)len;
                }
            }
        }

        if (lengths[256] == 0)
        {
            return -9;
        }

        var distcode = new Huffman();
        err = Construct(lencode, lengths, nlen);
        if (err != 0 && (err < 0 || nlen != lencode.Count[0] + lencode.Count[1]))
        {
            return -7;
        }

        var distLengths = new short[ndist];
        Array.Copy(lengths, nlen, distLengths, 0, ndist);
        err = Construct(distcode, distLengths, ndist);
        if (err != 0 && (err < 0 || ndist != distcode.Count[0] + distcode.Count[1]))
        {
            return -8;
        }

        return Codes(lencode, distcode);
    }

    private int Decode(Huffman h)
    {
        int code = 0;
        int first = 0;
        int index = 0;
        for (int len = 1; len <= MaxBits; len++)
        {
            code |= Bits(1);
            int count = h.Count[len];
            if (code - count < first)
            {
                return h.Symbol[index + (code - first)];
            }

            index += count;
            first += count;
            first <<= 1;
            code <<= 1;
        }

        return -10;
    }

    private static int Construct(Huffman h, short[] lengths, int n)
    {
        for (int len = 0; len <= MaxBits; len++)
        {
            h.Count[len] = 0;
        }

        for (int symbol = 0; symbol < n; symbol++)
        {
            h.Count[lengths[symbol]]++;
        }

        if (h.Count[0] == n)
        {
            return 0;
        }

        int left = 1;
        for (int len = 1; len <= MaxBits; len++)
        {
            left <<= 1;
            left -= h.Count[len];
            if (left < 0)
            {
                return left;
            }
        }

        var offsets = new int[MaxBits + 1];
        offsets[1] = 0;
        for (int len = 1; len < MaxBits; len++)
        {
            offsets[len + 1] = offsets[len] + h.Count[len];
        }

        h.Symbol = new short[n];
        for (int symbol = 0; symbol < n; symbol++)
        {
            if (lengths[symbol] != 0)
            {
                h.Symbol[offsets[lengths[symbol]]++] = (short)symbol;
            }
        }

        return left;
    }

    private void Ensure(int extra)
    {
        if (_outputCount + extra <= _output.Length)
        {
            return;
        }

        int newSize = _output.Length;
        while (newSize < _outputCount + extra)
        {
            newSize *= 2;
        }

        Array.Resize(ref _output, newSize);
    }

    private static Huffman BuildFixedLiteral()
    {
        var lengths = new short[FixLcodes];
        int symbol;
        for (symbol = 0; symbol < 144; symbol++)
        {
            lengths[symbol] = 8;
        }

        for (; symbol < 256; symbol++)
        {
            lengths[symbol] = 9;
        }

        for (; symbol < 280; symbol++)
        {
            lengths[symbol] = 7;
        }

        for (; symbol < FixLcodes; symbol++)
        {
            lengths[symbol] = 8;
        }

        var h = new Huffman();
        Construct(h, lengths, FixLcodes);
        return h;
    }

    private static Huffman BuildFixedDistance()
    {
        var lengths = new short[MaxDcodes];
        Array.Fill(lengths, (short)5);
        var h = new Huffman();
        Construct(h, lengths, MaxDcodes);
        return h;
    }

    private static readonly short[] LengthBase =
    [
        3, 4, 5, 6, 7, 8, 9, 10, 11, 13, 15, 17, 19, 23, 27, 31,
        35, 43, 51, 59, 67, 83, 99, 115, 131, 163, 195, 227, 258,
    ];

    private static readonly short[] LengthExtra =
    [
        0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2,
        3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5, 0,
    ];

    private static readonly short[] DistanceBase =
    [
        1, 2, 3, 4, 5, 7, 9, 13, 17, 25, 33, 49, 65, 97, 129, 193,
        257, 385, 513, 769, 1025, 1537, 2049, 3073, 4097, 6145,
        8193, 12289, 16385, 24577,
    ];

    private static readonly short[] DistanceExtra =
    [
        0, 0, 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6,
        7, 7, 8, 8, 9, 9, 10, 10, 11, 11, 12, 12, 13, 13,
    ];

    private static readonly short[] Order =
    [
        16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15,
    ];

    private sealed class Huffman
    {
        internal readonly short[] Count = new short[MaxBits + 1];
        internal short[] Symbol = Array.Empty<short>();
    }
}
