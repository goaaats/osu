// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osuTK.Input;

namespace osu.Game.Online.Websocket
{
    public struct GameplayInfoPacket : IPacket
    {
        WebsocketOpCode IPacket.GetOpCode() => WebsocketOpCode.GameplayInfo;

        public Key LastKey;
        public Vector2 LastMousePos;

        public int PlayerNum;
    }
}
