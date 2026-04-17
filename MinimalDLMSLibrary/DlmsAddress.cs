namespace MinimalDLMS;

/// <summary>
/// Представление DLMS-адреса устройства (логический + физический)
/// и преобразования в форматы, используемые в Gurux.
/// </summary>
public sealed class DlmsAddress
{
    public int Logical { get; }
    public int Physical { get; }

    public DlmsAddress(int logical, int physical)
    {
        if (logical < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(logical), "Логический адрес должен быть неотрицательным.");
        }

        if (physical < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(physical), "Физический адрес должен быть неотрицательным.");
        }

        Logical = logical;
        Physical = physical;
    }

    /// <summary>
    /// Формирует server address по правилам Gurux:
    /// short (7+7) или extended (14+14).
    /// </summary>
    public int ToServerAddress()
    {
        if (Logical <= 0x7F && Physical <= 0x7F)
        {
            return (Logical << 7) | Physical;
        }

        if (Logical <= 0x3FFF && Physical <= 0x3FFF)
        {
            return (Logical << 14) | Physical;
        }

        throw new ArgumentOutOfRangeException(nameof(Physical), "Поддерживаются только 7-битные и 14-битные логические/физические адреса.");
    }

    /// <summary>
    /// Кодирует server address в HDLC address field (как в Gurux: по 7 бит на байт, последний байт с LSB=1).
    /// </summary>
    public byte[] ToHdlcBytes()
    {
        return EncodeHdlcAddress(ToServerAddress());
    }

    internal static byte[] EncodeHdlcAddress(int address)
    {
        if (address < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(address), "Адрес не может быть отрицательным.");
        }

        if (address <= 0x7F)
        {
            return new[] { (byte)((address << 1) | 0x01) };
        }

        if (address <= 0x3FFF)
        {
            return new[]
            {
                (byte)(((address >> 7) & 0x7F) << 1),
                (byte)(((address & 0x7F) << 1) | 0x01)
            };
        }

        if (address <= 0x1FFFFF)
        {
            return new[]
            {
                (byte)(((address >> 14) & 0x7F) << 1),
                (byte)(((address >> 7) & 0x7F) << 1),
                (byte)(((address & 0x7F) << 1) | 0x01)
            };
        }

        if (address <= 0x0FFFFFFF)
        {
            return new[]
            {
                (byte)(((address >> 21) & 0x7F) << 1),
                (byte)(((address >> 14) & 0x7F) << 1),
                (byte)(((address >> 7) & 0x7F) << 1),
                (byte)(((address & 0x7F) << 1) | 0x01)
            };
        }

        throw new ArgumentOutOfRangeException(nameof(address), "Поддерживаются HDLC-адреса до 28 бит.");
    }
}
