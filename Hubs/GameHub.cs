namespace BobsBetting.Hub {
    using System.Collections.Generic;
    using BobsBetting.CacheModels;
    using BobsBetting.DataService;
    using BobsBetting.DBModels;
    using BobsBetting.Models;
    using BobsBetting.Services;
    using Microsoft.AspNetCore.SignalR;

    internal class GameHub(BBDb _db, SharedDb shared, PokerGameService pkg) : Hub
    {
        private readonly BBDb db = _db;
        private readonly SharedDb _shared = shared;
        private readonly PokerGameService _pkg = pkg;

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
                if (gameState.GameEnded && !gameState.PublicPlayerStates.Any(p => p.LastAction.ActionType == ActionType.Raise || p.LastAction.ActionType == ActionType.AllIn)) {
                    List<WinnerData> winnerDatas = _pkg.SettleGame(conn.LobbyId);
                    List<PublicPlayer> publicPlayers = gameState.PublicPlayerStates;
                    foreach (PublicPlayer publicPlayer in publicPlayers) {
                        User userToUpdate = await db.Users.FindAsync(publicPlayer.UserId);
                        if (winnerDatas.Any(w => w.UserId == publicPlayer.UserId)) {
                            userToUpdate.Chips += winnerDatas[0].Winnings - publicPlayer.CurrentBet;
                        } else {
                            userToUpdate.Chips -= publicPlayer.CurrentBet;
                        }
                        await db.SaveChangesAsync();
                    }
                    await Clients.Group(conn.LobbyId).SendAsync("ReceiveGameResult", winnerDatas);
                }
            }
        }

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

    }
}