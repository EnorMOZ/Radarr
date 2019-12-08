using System.Collections.Generic;
using System.Linq;
using Dapper;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Blacklisting
{
    public interface IBlacklistRepository : IBasicRepository<Blacklist>
    {
        List<Blacklist> BlacklistedByTitle(int movieId, string sourceTitle);
        List<Blacklist> BlacklistedByTorrentInfoHash(int movieId, string torrentInfoHash);
        List<Blacklist> BlacklistedByMovie(int movieId);
    }

    public class BlacklistRepository : BasicRepository<Blacklist>, IBlacklistRepository
    {
        public BlacklistRepository(IMainDatabase database, IEventAggregator eventAggregator) :
            base(database, eventAggregator)
        {
        }

        public List<Blacklist> BlacklistedByTitle(int movieId, string sourceTitle)
        {
            return Query(Builder()
                         .WhereEqual<Blacklist>(x => x.MovieId, movieId)
                         .WhereContains<Blacklist>(x => x.SourceTitle, sourceTitle));
        }

        public List<Blacklist> BlacklistedByTorrentInfoHash(int movieId, string torrentInfoHash)
        {
            return Query(Builder()
                         .WhereEqual<Blacklist>(x => x.MovieId, movieId)
                         .WhereContains<Blacklist>(x => x.TorrentInfoHash, torrentInfoHash));
        }

        public List<Blacklist> BlacklistedByMovie(int movieId)
        {
            return Query(Builder().WhereEqual<Blacklist>(x => x.MovieId, movieId));
        }

        private IEnumerable<Blacklist> SelectJoined(SqlBuilder.Template sql)
        {
            using (var conn = _database.OpenConnection())
            {
                return conn.Query<Blacklist, Movie, Blacklist>(
                    sql.RawSql,
                    (bl, movie) => {
                        bl.Movie = movie;
                        return bl;
                    },
                    sql.Parameters)
                    .ToList();
            }
        }

        protected override SqlBuilder GetPagedBuilder() => new SqlBuilder().Join("Movies ON Movies.Id = Blacklist.MovieId");
        protected override IEnumerable<Blacklist> GetPagedSelector(SqlBuilder.Template sql) => SelectJoined(sql);
    }
}
