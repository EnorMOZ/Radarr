using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;


namespace NzbDrone.Core.MediaFiles
{
    public interface IMediaFileRepository : IBasicRepository<MovieFile>
    {
        List<MovieFile> GetFilesByMovie(int movieId);
        List<MovieFile> GetFilesWithoutMediaInfo();
    }


    public class MediaFileRepository : BasicRepository<MovieFile>, IMediaFileRepository
    {
        public MediaFileRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public List<MovieFile> GetFilesByMovie(int movieId)
        {
            return Query(Builder().WhereEqual<MovieFile>(x => x.MovieId, movieId));
        }

        public List<MovieFile> GetFilesWithoutMediaInfo()
        {
            return Query(Builder().WhereNull<MovieFile>(x => x.MediaInfo));
        }
    }
}
