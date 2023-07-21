using StackExchange.Redis;

namespace ExtratoClubeAPI.Manager
{
    public class RedisCacheManager
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _database;

        public RedisCacheManager()
        {
            _redis = ConnectionMultiplexer.Connect("localhost");
            _database = _redis.GetDatabase();
        }

        public bool TryGetBeneficios(string numeroMatricula, out string jsonBeneficios)
        {
            jsonBeneficios = _database.StringGet(numeroMatricula);

            return !string.IsNullOrWhiteSpace(jsonBeneficios);
        }

        public void SetBeneficios(string numeroMatricula, string jsonBeneficios)
        {
            _database.StringSet(numeroMatricula, jsonBeneficios);
        }
    }
}
