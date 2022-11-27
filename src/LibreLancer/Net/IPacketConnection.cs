// MIT License - Copyright (c) Callum McGing
// This file is subject to the terms and conditions defined in
// LICENSE, which is part of this source code package

using LibreLancer.Net.Protocol;

namespace LibreLancer.Net
{
    public interface IPacketConnection
    {
        void SendPacket(IPacket packet, PacketDeliveryMethod method);
        bool PollPacket(out IPacket packet);
        void Shutdown();
    }
}