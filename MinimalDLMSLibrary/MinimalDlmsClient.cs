using OperatorApp.Core.Interfaces;
using System.Diagnostics;
using System.Text;

namespace MinimalDLMS;

/// <summary>
/// Минимальный клиент для чтения ограниченного набора OBIS-кодов по serial.
/// </summary>
public sealed class MinimalDlmsClient
{
    /// <summary>
    /// OBIS-код устройства (Logical Device Name).
    /// </summary>
    public const string DeviceLogicalNameObis = "0.0.42.0.0.255";

    /// <summary>
    /// OBIS-код серийного номера.
    /// </summary>
    public const string SerialNumberObis = "0.0.96.1.0.255";

    private readonly IConnectionDevice _portAdapter;
    private readonly int _clientAddress;

    private int _serverAddress;
    private bool _associationEstablished;
    private DlmsRequestBuilder? _requestBuilder;

    /// <summary>
    /// Server address.
    /// </summary>
    public int ServerAddress
    {
        get => _serverAddress;
        set
        {
            if (_serverAddress == value)
            {
                return;
            }

            _serverAddress = value;
            _associationEstablished = false;
            _requestBuilder = null;
        }
    }

    /// <summary>
    /// Создает экземпляр минимального DLMS-клиента.
    /// </summary>
    /// <param name="portAdapter">Адаптер порта.</param>
    /// <param name="clientAddress">DLMS-адрес клиента. По умолчанию 0x10.</param>
    public MinimalDlmsClient(IConnectionDevice portAdapter, int clientAddress = 0x10)
    {
        _portAdapter = portAdapter;
        _clientAddress = clientAddress;
    }

    /// <summary>
    /// Читает два обязательных OBIS-кода: 0.0.42.0.0.255 и 0.0.96.1.0.255.
    /// </summary>
    /// <param name="timeoutMs">Таймаут чтения ответа в миллисекундах.</param>
    /// <returns>Результаты чтения двух OBIS-кодов.</returns>
    public async Task<IReadOnlyList<DlmsReadResult>> ReadRequiredObisAsync(int timeoutMs)
    {
        var first = await ReadObisAsync(DeviceLogicalNameObis, timeoutMs);
        var second = await ReadObisAsync(SerialNumberObis, timeoutMs);
        return new[] { first, second };
    }

    /// <summary>
    /// Выполняет минимальный GET-запрос для чтения одного OBIS-кода.
    /// </summary>
    /// <param name="obis">OBIS-код в формате A.B.C.D.E.F.</param>
    /// <param name="timeoutMs">Таймаут чтения ответа в миллисекундах.</param>
    /// <returns>Результат чтения.</returns>
    public async Task<DlmsReadResult> ReadObisAsync(string obis, int timeoutMs)
    {
        await EnsureAssociationAsync(timeoutMs);

        var request = GetRequestBuilder().BuildGetRequest(obis);
        Debug.WriteLine("MinDLMS write request: " + BitConverter.ToString(request));

        CancellationTokenSource _cts = new CancellationTokenSource();
        //await _portAdapter.WriteData(request);
        //var response = await _portAdapter.ReadAllAsync(_cts.Token);

        byte[] response = new byte[] { };
        var textValue = TryExtractText(response);
        return new DlmsReadResult(obis, response, textValue);
    }

    /// <summary>
    /// Открывает DLMS-ассоциацию: SNRM -> UA, затем AARQ -> AARE.
    /// </summary>
    /// <param name="timeoutMs">Таймаут обмена в миллисекундах.</param>
    /// <returns>Задача выполнения инициализации.</returns>
    public async Task EnsureAssociationAsync(int timeoutMs)
    {
        if (_associationEstablished)
        {
            return;
        }

        var requestBuilder = GetRequestBuilder();
        requestBuilder.ResetControlSequence();

        var snrm = requestBuilder.BuildSnrmRequest();
        Debug.WriteLine("MinDLMS Snrm request: " + BitConverter.ToString(snrm));
        //await _portAdapter.WriteAsync(snrm);
        //_ = await _portAdapter.ReadAsync(timeoutMs); // UA

        var aarq = requestBuilder.BuildAarqRequest();
        Debug.WriteLine("MinDLMS Aarq request: " + BitConverter.ToString(aarq));
        //await _portAdapter.WriteAsync(aarq);
        //_ = await _portAdapter.ReadAsync(timeoutMs); // AARE

        _associationEstablished = true;
    }

    private DlmsRequestBuilder GetRequestBuilder()
    {
        return _requestBuilder ??= new DlmsRequestBuilder(_serverAddress, _clientAddress);
    }

    private static string? TryExtractText(byte[] response)
    {
        var octetTagIndex = Array.IndexOf(response, (byte)0x09);
        if (octetTagIndex >= 0 && octetTagIndex + 1 < response.Length)
        {
            var len = response[octetTagIndex + 1];
            var start = octetTagIndex + 2;
            if (start + len <= response.Length)
            {
                var bytes = response.Skip(start).Take(len).ToArray();
                return Encoding.ASCII.GetString(bytes);
            }
        }

        return response.Length > 0 ? BitConverter.ToString(response) : null;
    }
}
