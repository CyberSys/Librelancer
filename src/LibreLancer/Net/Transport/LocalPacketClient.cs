// MIT License - Copyright (c) Callum McGing
// This file is subject to the terms and conditions defined in
// LICENSE, which is part of this source code package

using System;
using System.Collections.Concurrent;
using LiteNetLib;

namespace LibreLancer
{
    public class LocalPacketClient : IPacketClient
    {
        public ConcurrentQueue<IPacket> Packets = new ConcurrentQueue<IPacket>();
        public void SendPacket(IPacket packet, PacketDeliveryMethod method, bool force = false)
        {
            #if DEBUG
            LibreLancer.Packets.CheckRegistered(packet);
            #endif
            Packets.Enqueue(packet);
        }

        public void SendPacketWithEvent(IPacket packet, Action onAck, PacketDeliveryMethod method)
        {
            #if DEBUG
            LibreLancer.Packets.CheckRegistered(packet);
            #endif
            Packets.Enqueue(packet);
            onAck();
        }
    }
}