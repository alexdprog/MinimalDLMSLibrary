using Operator.Infrastructure.Enums;
using OperatorApp.Core.Models;
using System;
using System.Data;
using System.Threading.Tasks;

namespace OperatorApp.Core.Interfaces
{
#if STUBS
    public delegate void PortDataReceivedEventHandler(object sender, MessageReceivedEventArgs data);
    /// <summary>
    /// Интерфейс соединения (оптоголовки)
    /// </summary>
    public interface IConnectionDevice
    {
        /// <summary>
        /// Настройка устройства
        /// </summary>
        /// <param name="baudRate">Скорость</param>
        /// <param name="dataBits">Бит данных</param>
        /// <param name="stopBits">Стоп бит</param>
        /// <param name="parity">Четность</param>
        /// <returns></returns>
        Task<bool> SetupDevice(int baudRate, int dataBits, int stopBits, int parity, TypeDeviceEnum Typedev = TypeDeviceEnum.DirectUsb);

        /// <summary>
        /// Открывает соединение
        /// </summary>
        /// <returns></returns>
        Task<bool> OpenDevice();

        /// <summary>
        /// Проверка что порт открыт.
        /// </summary>
        /// <returns></returns>
        Task<bool> IsOpen();

        /// <summary>
        /// Закрывает соединение
        /// </summary>
        /// <returns></returns>
        Task<bool> CloseDevice();

        /// <summary>
        /// Посылка сообщения
        /// </summary>
        /// <param name="data"></param>
        /// <returns>Статус операции</returns>
        Task<bool> WriteData(byte[] data);

        /// <summary>
        /// Чтение ответа
        /// </summary>
        /// <param name="count"></param>
        /// <returns>Полученный набор данных</returns>
        Task<byte[]> ReadData(int count);

        /// <summary>
        /// Чтение всех байт из буфера с ожиданием.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<byte[]> ReadAllAsync(CancellationToken token);

        Task ClearBuffer();

        Task <bool> CheckExistsDevice();

        /// <summary>
        /// Событие получения данных
        /// </summary>
        event PortDataReceivedEventHandler DataReceive;
    }
#endif
}
