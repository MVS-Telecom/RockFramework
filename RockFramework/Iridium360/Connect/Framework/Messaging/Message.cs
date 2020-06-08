using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Iridium360.Connect.Framework.Messaging
{
    public abstract class Message
    {
        private static Dictionary<MessageType, System.Type> knownMOTypes = new Dictionary<MessageType, System.Type>();
        private static Dictionary<MessageType, System.Type> knownMTTypes = new Dictionary<MessageType, System.Type>();

        private const byte SIGNATURE = 0x12;


        public static bool CheckSignature(byte[] bytes)
        {
            return CheckSignature(bytes[0]);
        }

        public static bool CheckSignature(byte _byte)
        {
            return _byte != SIGNATURE;
        }


        protected Message()
        {
        }

        protected abstract void pack(BinaryBitWriter writer);
        public byte[] Pack()
        {
            byte[] buffer;
            byte[] buffer3;
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryBitWriter writer = new BinaryBitWriter((Stream)stream))
                {
                    this.pack(writer);
                    writer.Flush();
                    buffer = stream.GetBuffer();
                    Array.Resize<byte>(ref buffer, (int)stream.Length);
                }
            }
            this.Length = (ushort)buffer.Length;
            if (this.Length > 0x4b0)
            {
                throw new ArgumentException("Message too long!", "Length");
            }
            using (MemoryStream stream2 = new MemoryStream())
            {
                using (BinaryBitWriter writer2 = new BinaryBitWriter((Stream)stream2))
                {
                    writer2.Write(SIGNATURE);
                    writer2.Write((bool)(this.Direction == Iridium360.Connect.Framework.Messaging.Direction.MO));
                    writer2.Write((bool)(this.Composite == Iridium360.Connect.Framework.Messaging.Composite.Complex));
                    writer2.Write(false);
                    writer2.Write(false);
                    writer2.Write(false);
                    writer2.Write((bool)((((this.Length & 0x700) >> 8) & 4) == 4));
                    writer2.Write((bool)((((this.Length & 0x700) >> 8) & 2) == 2));
                    writer2.Write((bool)((((this.Length & 0x700) >> 8) & 1) == 1));


                    if (this.Composite == Iridium360.Connect.Framework.Messaging.Composite.Complex)
                    {
                        writer2.Write(this.Group);
                        writer2.Write(this.Parts);
                        writer2.Write(this.Part);
                    }

                    writer2.Write((byte)this.Type);
                    writer2.Write((byte)(this.Length & 0xff));
                    writer2.Write(buffer);
                    byte[] array = stream2.GetBuffer();
                    byte num = 0;
                    int index = 0;
                    while (true)
                    {
                        if (index >= stream2.Length)
                        {
                            writer2.Write(num);
                            writer2.Flush();
                            array = stream2.GetBuffer();
                            Array.Resize<byte>(ref array, (int)stream2.Length);
                            buffer3 = array;
                            break;
                        }
                        num += array[index];
                        index++;
                    }
                }
            }
            return buffer3;
        }

        protected abstract void unpack(BinaryBitReader reader);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        protected static Message Unpack(Dictionary<MessageType, Type> knownTypes, byte[] buffer)
        {
            Message message2;
            using (MemoryStream stream = new MemoryStream(buffer))
            {
                using (BinaryBitReader reader = new BinaryBitReader((Stream)stream))
                {
                    if (!CheckSignature(reader.ReadByte()))
                        throw new FormatException("Invalid signature!");

                    var direction = reader.ReadBoolean() ? Iridium360.Connect.Framework.Messaging.Direction.MO : Iridium360.Connect.Framework.Messaging.Direction.MT;
                    var composite = reader.ReadBoolean() ? Iridium360.Connect.Framework.Messaging.Composite.Complex : Iridium360.Connect.Framework.Messaging.Composite.Simple;
                    bool flag = reader.ReadBoolean();
                    flag = reader.ReadBoolean();
                    flag = reader.ReadBoolean();

                    var count = (0 + (reader.ReadBoolean() ? ((ushort)0x400) : ((ushort)0)))
                        + (reader.ReadBoolean() ? ((ushort)0x200) : ((ushort)0))
                        + (reader.ReadBoolean() ? ((ushort)0x100) : ((ushort)0));

                    int parts = 0;
                    int part = 0;
                    int group = 0;
                    if (composite == Iridium360.Connect.Framework.Messaging.Composite.Complex)
                    {
                        group = reader.ReadByte();
                        parts = reader.ReadByte();
                        part = reader.ReadByte();
                    }
                    MessageType messageType = (MessageType)reader.ReadByte();
                    count += reader.ReadByte();
                    byte[] payload = reader.ReadBytes(count);
                    byte sum = reader.ReadByte();
                    int index = 0;
                    while (true)
                    {
                        if (index >= (stream.Length - 1L))
                        {
                            if (sum != 0)
                            {
                                throw new FormatException("Invalid checksum!");
                            }

                            var type = knownTypes.Where(t => t.Key == messageType).FirstOrDefault();
                            Message message = (Message)Activator.CreateInstance(type.Value, true);

                            message.Composite = composite;
                            message.Group = (byte)group;
                            message.Part = (byte)part;
                            message.Parts = (byte)parts;
                            message.Length = (ushort)count;

                            using (MemoryStream stream2 = new MemoryStream(payload))
                            {
                                using (BinaryBitReader reader2 = new BinaryBitReader((Stream)stream2))
                                {
                                    message.unpack(reader2);
                                }
                            }

                            message2 = message;
                            break;
                        }
                        sum -= buffer[index];
                        index++;
                    }
                }
            }
            return message2;
        }

        public abstract Iridium360.Connect.Framework.Messaging.Direction Direction { get; }

        public virtual Iridium360.Connect.Framework.Messaging.Composite Composite { get; private set; }

        public byte Group { get; private set; }

        public byte Parts { get; private set; }

        public byte Part { get; private set; }

        public ushort Length { get; protected set; }

        public abstract MessageType Type { get; }


        /// <summary>
        /// 
        /// </summary>
        protected static Dictionary<MessageType, System.Type> KnownMOTypes
        {
            get
            {
                if ((knownMOTypes?.Count ?? 0) == 0)
                {
                    fetchKnownTypes(Direction.MO, ref knownMOTypes);
                }
                return knownMOTypes;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected static Dictionary<MessageType, System.Type> KnownMTTypes
        {
            get
            {
                if ((knownMTTypes?.Count ?? 0) == 0)
                {
                    fetchKnownTypes(Direction.MT, ref knownMTTypes);
                }
                return knownMTTypes;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="knownTypes"></param>
        static void fetchKnownTypes(Direction direction, ref Dictionary<MessageType, Type> knownTypes)
        {
            var types = typeof(Message).Assembly.GetTypes().Where(type => (typeof(Message).IsAssignableFrom(type) && !type.IsAbstract)).ToList();

            foreach (System.Type type in types)
            {
                try
                {
                    Message message = (Message)Activator.CreateInstance(type, true);
                    if (message.Direction == direction)
                        knownTypes.Add(message.Type, type);
                }
                catch (Exception ex)
                {
#if DEBUG
                    Debugger.Break();
#endif
                }
            }
        }


    }
}