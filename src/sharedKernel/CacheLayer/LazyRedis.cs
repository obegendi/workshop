using System;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace CacheLayer
{
    public class LazyRedis
    {
        static Lazy<ConnectionMultiplexer> multiplexer = CreateMultiplexer();
        static string connectionString = "TODO: CALL InitializeConnectionString() method with connection string";

        public static ConnectionMultiplexer Connection
        {
            get { return multiplexer.Value; }
        }

        public static void InitializeConnectionString(string cnxString)
        {
            if (string.IsNullOrWhiteSpace(cnxString))
                throw new ArgumentNullException(nameof(cnxString));

            connectionString = cnxString;
        }

        public static int DefaultDb { get; set; }
        private static IDatabase RedisDb
        {
            get { return Connection.GetDatabase(DefaultDb); }
        }

        private static Lazy<ConnectionMultiplexer> CreateMultiplexer()
        {
            return new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(connectionString));
        }

        public static void Add(string key, object value, TimeSpan expireDate)
        {
            if (value == null) return;
            var jsonValue = JsonConvert.SerializeObject(value);

            if (RedisDb.IsConnected(key))
            {
                if (RedisDb.KeyExists(key))
                {
                    RedisDb.KeyDelete(key);
                }
                RedisDb.StringSet(key, jsonValue, expireDate);
            }
        }

        public static T Get<T>(string key)
        {

            if (RedisDb.IsConnected(key))
            {
                string jsonValue = RedisDb.StringGet(key);
                return jsonValue == null
                    ? default(T)
                    : JsonConvert.DeserializeObject<T>(jsonValue);
            }
            return default(T);
        }
    }
}
