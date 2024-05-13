namespace BobsBetting.Hub {
    using System.Collections.Generic;
    using BobsBetting.CacheModels;
    using BobsBetting.DataService;
    using BobsBetting.DBModels;
    using BobsBetting.Models;
    using BobsBetting.Services;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.SignalR;

    internal class GameHub(BBDb _db, SharedDb shared, PokerGameService pkg) : Hub
    {
        private readonly BBDb db = _db;

        private readonly SharedDb _shared = shared;
        private readonly PokerGameService _pkg = pkg;

/*
        public override async Task OnConnectedAsync() {
            await Clients.All.SendAsync("ReceiveMessage", $"{Context.ConnectionId} has joined");
        }*/

/*
        public async Task JoinLobby(UserConnectionReq conn)
        {
            await Clients.All.SendAsync("ReceiveMessage", $"{conn.UserId} has joined");
        }*/

        public async Task JoinSpecificLobby(UserConnectionReq conn)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, conn.LobbyId);
            User user = await db.Users.FindAsync(conn.UserId);
            UserConnection userConn = new UserConnection(user, conn.LobbyId);

            var existingConnection = _shared.connections.FirstOrDefault(c => c.Value.User.Id == user.Id);
            if (existingConnection.Key != null)
            {
                // Remove the existing connection if it exists
                _shared.connections.TryRemove(existingConnection.Key, out _);
            }
            _shared.connections[Context.ConnectionId] = userConn;

            await Clients.Group(conn.LobbyId).SendAsync("JoinSpecificLobby", "admin", $"{conn.UserId} has joined {conn.LobbyId}");
        }

        Dictionary<GameRounds, string> roundNames = new Dictionary<GameRounds, string> {
            { GameRounds.PREFLOP, "Pre-Flop" },
            { GameRounds.FLOP, "Flop" },
            { GameRounds.TURN, "Turn" },
            { GameRounds.RIVER, "River" }
        };

        public async Task StartGame(UserConnectionReq conn) {
            
            var userConnections = _shared.connections.Where(c => c.Value.LobbyId == conn.LobbyId).ToList();
            List<User> users = userConnections.Select(c => c.Value.User).ToList();
            ActiveGameState gameState = _pkg.StartPokerGame(conn.LobbyId, users);

            // Notify all players in the lobby of the initial game state (public information)
            List<Card> CommunityCards = [];
            await Clients.Group(conn.LobbyId)
                .SendAsync("GameStarted", CommunityCards, roundNames[gameState.CurrentRound], gameState.CurrentPlayerId, gameState.PublicPlayerStates, gameState.CurrentPot);

            // Notify each player of their private cards
            foreach (var user in userConnections)
            {
                Player player = gameState.PlayerStates.Find(c => c.UserId == user.Value.User.Id);
                await Clients.Client(user.Key).SendAsync("ReceiveHand", player.Hand);
            }
        }

        public async Task UserRoundAction(UserConnectionReq conn, PlayerActionReq action)
        {
            User user = _shared.connections[Context.ConnectionId].User;
            ActiveGameState gameState = _pkg.HandleTurn(conn.LobbyId, user, action);
            int cardsToShow = 0;
            switch (gameState.CurrentRound)
            {
                case GameRounds.PREFLOP:
                    cardsToShow = 0; break;
                case GameRounds.FLOP:
                    cardsToShow = 3; break;
                case GameRounds.TURN:
                    cardsToShow = 4; break;
                case GameRounds.RIVER:
                    cardsToShow = 5; break;
            }
            if (gameState.PublicPlayerStates.Count != 0) {
                await Clients.Group(conn.LobbyId)
                    .SendAsync("ReceiveUpdatedGame", gameState.CommunityCards[..cardsToShow], roundNames[gameState.CurrentRound], gameState.CurrentPlayerId, gameState.PublicPlayerStates, gameState.CurrentPot);
                if (gameState.GameEnded) {
                    await Clients.Group(conn.LobbyId).SendAsync("ReceiveGameResult", _pkg.SettleGame(conn.LobbyId));
                }
            }
        }

/*
        public async Task JoinSpecificLobby(string lobbyId, int userId)
        {
            if (_lobbies.ContainsKey(lobbyId))
            {
                _lobbies[lobbyId].Add(new(Context.ConnectionId, userId));
                await Groups.AddToGroupAsync(Context.ConnectionId, lobbyId);
                await Clients.Group(lobbyId).SendAsync("PlayerJoined", Context.ConnectionId);
            }
        }*/

/*
        public async Task LeaveLobby(string lobbyId, int userId)
        {
            if (_lobbies.ContainsKey(lobbyId) && _lobbies[lobbyId].Select(l => l.ConnectionId).Contains(Context.ConnectionId))
            {
                _lobbies[lobbyId].Select(l => l.ConnectionId).ToList().Remove(Context.ConnectionId);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, lobbyId);
                await Clients.Group(lobbyId).SendAsync("PlayerLeft", Context.ConnectionId);
            }
        }*/

/*
        public async Task StartGame(string lobbyId)
        {
            if (_lobbies.ContainsKey(lobbyId))
            {
                var gameState = new ActiveGameState();
                var deck = _deckService.CreateAndShuffleDeck();
                
                // Assuming each player has been added to a lobby
                foreach (var player in _lobbies[lobbyId])
                {
                    var playerState = new PlayerState
                    {
                        ConnectionId = player.ConnectionId
                    };
                    playerState.HandCards.Add(deck.Deal());
                    playerState.HandCards.Add(deck.Deal());
                    gameState.Players.Add(playerState);
                }

                // Deal community cards
                for (int i = 0; i < 5; i++) {
                    gameState.CommunityCards.Add(deck.Deal());
                }

                // Store the game state
                _gameCacheService.SetGameState(gameState.GameId, gameState);

                // Notify each player of their private cards
                foreach (var player in gameState.Players)
                {
                    await Clients.Client(player.ConnectionId).SendAsync("ReceiveHand", player.HandCards);
                }

                // Notify all players in the lobby of the initial game state (public information)
                await Clients.Group(lobbyId).SendAsync("GameStarted", gameState.CommunityCards, gameState.PotSize);
            }
        }*/
    }
}