// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Threading;
using osu.Game.Online.Websocket;
using osu.Game.Rulesets.Scoring;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace osu.TagWsServer
{
    public class OsuTagWebSocketService : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            var opCode = (WebsocketOpCode)e.RawData[0];

            //Console.WriteLine($"[{ID}] RECV: {opCode}");

            var data = e.RawData.Skip(1).ToArray();

            switch (opCode)
            {
                case WebsocketOpCode.InitMap:
                    var initMapPacket = data.ToObject<InitMapPacket>();

                    for (byte i = 0; i < Sessions.ActiveIDs.Count(); i++)
                    {
                        initMapPacket.ThisPlayerNum = i;
                        Sessions.SendTo(getSendBuffer(initMapPacket), Sessions.ActiveIDs.ElementAt(i));
                        Console.WriteLine($"Initialized Player {i} - {Sessions.ActiveIDs.ElementAt(i)}");
                    }

                    break;

                case WebsocketOpCode.ReadyForStart:
                    Thread.Sleep(1000);
                    sendPacket(new ReadyForStartPacket());
                    break;

                case WebsocketOpCode.GameplayInfo:
                    //var giPacket = data.ToObject<GameplayInfoPacket>();
                    //Console.WriteLine($"    -> {giPacket.LastKey.ToString()} {giPacket.LastMousePos} Player: {giPacket.PlayerNum}");

                    foreach (var activeID in Sessions.ActiveIDs)
                    {
                        if (activeID != this.ID)
                            Sessions.SendTo(e.RawData, activeID);
                    }

                    break;

                case WebsocketOpCode.JudgementReport:
                    var rp = data.ToObject<JudgementReportPacket>();
                    Console.WriteLine($"[JUDGE]    -> {rp.Index} = {rp.Result.ToString()} Player: {rp.PlayerNum}");

                    foreach (var activeID in Sessions.ActiveIDs)
                    {
                        if (activeID != this.ID)
                            Sessions.SendTo(e.RawData, activeID);
                    }

                    break;
            }
        }

        private byte[] getSendBuffer(IPacket packet)
        {
            var packetData = packet.GetBytes();

            var sendBuffer = new byte[packetData.Length + 1];

            Array.Copy(packetData, 0, sendBuffer, 1, packetData.Length);
            sendBuffer[0] = (byte)packet.GetOpCode();

            Console.WriteLine($"[WebSocket] Sending: {packet.GetOpCode()} {sendBuffer.ToHexString()}");

            return sendBuffer;
        }

        private void sendPacket(IPacket packet)
        {
            var sendBuffer = getSendBuffer(packet);

            Send(sendBuffer);
        }

        protected override void OnOpen()
        {
            Console.WriteLine($"Client connected: {ID}");
        }
    }
}
