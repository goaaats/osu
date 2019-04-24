// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osuTK;
using System.Threading.Tasks;
using osu.Game.Online.Chat;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osuTK.Input;
using WebSocketSharp;

namespace osu.Game.Online.Websocket
{
    // cursed shall be who dares to go beyond this point
    public class TagModManager
    {
        public static TagModManager Instance { get; } = new TagModManager();

        private WebSocketSharp.WebSocket _socket;

        public bool GamePrepared = false;
        public bool GameReady = false;
        public int ThisPlayerNum = -1;
        public int PlayersMax = -1;

        public int PreparedMapId = -1;

        private Key lastKey = Key.Unknown;
        private Vector2 lastMousePos = Vector2.Zero;

        private Thread _gameplayInfoUpdateThread;
        private volatile bool _gameplayInfoUpdateThreadRunning = true;

        public Dictionary<int, Tuple<Key, Vector2>> PlayerLastPositionMap = new Dictionary<int, Tuple<Key, Vector2>>();

        public void Reconnect(string url)
        {
            _socket?.Close();
            _socket = new WebSocket(url);
            _socket.OnMessage += socketOnMessage;

            _socket.Connect();
        }

        private void updateGameplayInfoLoop()
        {
            while (_gameplayInfoUpdateThreadRunning)
            {
                sendPacket(new GameplayInfoPacket
                {
                    LastKey = lastKey,
                    LastMousePos = lastMousePos,
                    PlayerNum = ThisPlayerNum
                });

                Thread.Sleep(30);
            }
        }

        public void HandleChatCommand(string[] parameters, Chat.Channel targetChannel)
        {
            parameters = parameters[1].Split(' ');

            switch (parameters[0])
            {
                case "c":
                    //Reconnect(parameters[1]);
                    Reconnect("ws://localhost/osutag");
                    targetChannel.AddNewMessages(new InfoMessage($"Connected to websocket!"));
                    break;

                case "init":
                    var packet = new InitMapPacket();
                    packet.MapId = int.Parse(parameters[1]);
                    packet.Players = byte.Parse(parameters[2]);

                    sendPacket(packet);
                    break;

                case "reset":
                    GamePrepared = false;
                    GameReady = false;
                    ThisPlayerNum = -1;
                    PlayersMax = -1;
                    PreparedMapId = -1;

                    targetChannel.AddNewMessages(new InfoMessage($"TagModManager reset."));
                    break;
            }
        }

        public void Reset()
        {
            GamePrepared = false;
            GameReady = false;
            ThisPlayerNum = -1;
            PlayersMax = -1;
            PreparedMapId = -1;

            _gameplayInfoUpdateThreadRunning = false;
        }

        private void socketOnMessage(object sender, MessageEventArgs e)
        {
            Framework.Logging.Logger.Log($"[WebSocket] Recv: {e.RawData.ToHexString()}");
            var opCode = (WebsocketOpCode)e.RawData[0];

            Framework.Logging.Logger.Log($"[WebSocket] Got packet: {opCode}");

            var data = e.RawData.Skip(1).ToArray();

            switch (opCode)
            {
                case WebsocketOpCode.InitMap:
                    var initMapPacket = data.ToObject<InitMapPacket>();

                    PlayersMax = initMapPacket.Players;
                    PreparedMapId = initMapPacket.MapId;
                    ThisPlayerNum = initMapPacket.ThisPlayerNum;

                    PlayerLastPositionMap.Clear();
                    for (int i = 0; i < PlayersMax; i++)
                    {
                        PlayerLastPositionMap.Add(i, new Tuple<Key, Vector2>(Key.Unknown, Vector2.Zero));
                    }

                    GamePrepared = true;

                    Framework.Logging.Logger.Log($"[WebSocket] Game prepared! {initMapPacket.MapId} Player: {initMapPacket.ThisPlayerNum}");
                    break;

                case WebsocketOpCode.ReadyForStart:
                    Framework.Logging.Logger.Log("[WebSocket] Ready for start!");

                    _gameplayInfoUpdateThreadRunning = true;
                    _gameplayInfoUpdateThread = new Thread(updateGameplayInfoLoop) { IsBackground = true };
                    _gameplayInfoUpdateThread.Start();
                    GameReady = true;
                    break;

                case WebsocketOpCode.GameplayInfo:
                    var giPacket = data.ToObject<GameplayInfoPacket>();

                    if (PlayerLastPositionMap.ContainsKey(giPacket.PlayerNum))
                        PlayerLastPositionMap[giPacket.PlayerNum] = new Tuple<Key, Vector2>(giPacket.LastKey, giPacket.LastMousePos);
                    break;
            }
        }

        public void NotifyGameReady()
        {
            sendPacket(new ReadyForStartPacket());
        }

        public void ReportKey(Key key)
        {
            lastKey = key;
        }

        public void ReportMouseMovement(Vector2 position)
        {
            lastMousePos = position;
        }

        public void ReportJudgement(HitObject hitObject, JudgementResult result)
        {
            var reportPacket = new JudgementReportPacket { PlayerNum = ThisPlayerNum, Index = hitObject.Index, Result = result.Type };

            sendPacket(reportPacket);
        }

        private void sendPacket(IPacket packet)
        {
            var packetData = packet.GetBytes();

            var sendBuffer = new byte[packetData.Length + 1];

            Array.Copy(packetData, 0, sendBuffer, 1, packetData.Length);
            sendBuffer[0] = (byte)packet.GetOpCode();

            Framework.Logging.Logger.Log($"[WebSocket] Send: {packet.GetOpCode()} {sendBuffer.ToHexString()}");

            if (!_socket.IsAlive)
                return;

            Task.Run(delegate { _socket?.Send(sendBuffer); });
        }
    }
}
