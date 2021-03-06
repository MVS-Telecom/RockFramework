﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Iridium360.Connect.Framework.Messaging
{
    public class MessageSentMO : MessageMO
    {
        public override MessageType Type => MessageType.Sent;

        public byte SentGroup { get; private set; }


        protected override void pack(BinaryBitWriter writer)
        {
            writer.Write(SentGroup);
        }

        protected override void unpack(BinaryBitReader reader)
        {
            SentGroup = reader.ReadByte();
        }


        public static MessageSentMO Create(ProtocolVersion version, byte sentGroup)
        {
            var mo = Create<MessageSentMO>(version);
            mo.SentGroup = sentGroup;
            return mo;
        }
    }
}
