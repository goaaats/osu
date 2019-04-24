// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Online.Websocket
{
    public struct InitMapPacket : IPacket
    {
        public WebsocketOpCode GetOpCode() => WebsocketOpCode.InitMap;

        public int MapId;
        public byte Players;

        public byte ThisPlayerNum;
    }
}
