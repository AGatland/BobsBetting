namespace BobsBetting.Services {
    using BobsBetting.CacheModels;
    using BobsBetting.DBModels;

    public class LobbyService
    {
        private readonly Dictionary<int, Lobby> _lobbies = new Dictionary<int, Lobby>();

        public Lobby CreateLobby()
        {
            var newLobby = new Lobby { LobbyId = GenerateLobbyId() };
            _lobbies.Add(newLobby.LobbyId, newLobby);
            return newLobby;
        }

        public void JoinLobby(int lobbyId, User user)
        {
            if (_lobbies.TryGetValue(lobbyId, out var lobby))
            {
                lobby.Users.Add(user);
            }
            else
            {
                throw new ArgumentException("Lobby not found.");
            }
        }

        private int GenerateLobbyId()
        {
            // Simple ID generation, should be replaced with a more robust method
            return _lobbies.Keys.Count + 1;
        }
    }
}