// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Online.Websocket
{
    public enum WebsocketOpCode : byte
    {
        InitMap = 0x01,
        ReadyForStart = 0x02,
        GameplayInfo = 0x03,
        JudgementReport = 0x04
    }
}
