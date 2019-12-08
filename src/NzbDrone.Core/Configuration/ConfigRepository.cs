using Dapper;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;


namespace NzbDrone.Core.Configuration
{
    public interface IConfigRepository : IBasicRepository<Config>
    {
        Config Get(string key);
        Config Upsert(string key, string value);
    }

    public class ConfigRepository : BasicRepository<Config>, IConfigRepository
    {
        public ConfigRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public Config Get(string key)
        {
            using (var conn = _database.OpenConnection())
            {
                return conn.QuerySingleOrDefault<Config>($"SELECT * FROM {_table} WHERE Key = @Key", new { Key = key });
            }
        }

        public Config Upsert(string key, string value)
        {
            var dbValue = Get(key);

            if (dbValue == null)
            {
                return Insert(new Config {Key = key, Value = value});
            }

            dbValue.Value = value;

            return Update(dbValue);
        }
    }
}
