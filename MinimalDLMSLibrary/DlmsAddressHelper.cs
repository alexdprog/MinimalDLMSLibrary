namespace MinimalDLMS;

/// <summary>
/// Вспомогательные методы для формирования адресов DLMS-сервера.
/// </summary>
public static class DlmsAddressHelper
{
    /// <summary>
    /// Формирует адрес сервера по логическому и физическому адресам.
    /// </summary>
    /// <param name="logicalAddress">Логический адрес (например, 0x1 или 0x10).</param>
    /// <param name="physicalAddress">Физический адрес (например, 127 или 0x3FFF).</param>
    /// <returns>Сформированный адрес сервера.</returns>
    public static int GetServerAddress(int logicalAddress, int physicalAddress)
    {
        return new DlmsAddress(logicalAddress, physicalAddress).ToServerAddress();
    }
}
