// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Scoring;

namespace osu.Game.Online.Websocket
{
    public struct JudgementReportPacket : IPacket
    {
        public WebsocketOpCode GetOpCode() => WebsocketOpCode.JudgementReport;

        public int Index;
        public HitResult Result;

        public int PlayerNum;
    }
}
