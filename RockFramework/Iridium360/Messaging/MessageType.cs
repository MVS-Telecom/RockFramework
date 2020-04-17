namespace Rock.Iridium360.Messaging
{
    using System;

    public enum MessageType : byte
    {
        /// <summary>
        /// пустое
        /// </summary>
        Empty = 0,

        /// <summary>
        /// Ping/pong
        /// </summary>
        Ping = 1,

        /// <summary>
        ///  пакет байт
        /// </summary>
        Payload = 2,

        /// <summary>
        /// произвольны йтекст
        /// </summary>
        FreeText = 3,

        /// <summary>
        ///  Сообщение в чате
        /// </summary>
        ChatMessage = 4,

        /// <summary>
        ///  Погода
        /// </summary>
        Weather = 5,

    }
}

