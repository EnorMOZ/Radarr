using System;
using System.Linq;
using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Qualities;
using Dapper;
using NzbDrone.Core.Movies.AlternativeTitles;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Profiles;

namespace NzbDrone.Core.Movies
{
    public interface IMovieRepository : IBasicRepository<Movie>
    {
        bool MoviePathExists(string path);
        List<Movie> FindByTitles(List<string> titles);
        List<Movie> FindByTitleInexact(string cleanTitle);
        Movie FindByImdbId(string imdbid);
        Movie FindByTmdbId(int tmdbid);
        List<Movie> FindByTmdbId(List<int> tmdbids);
        Movie FindByTitleSlug(string slug);
        List<Movie> MoviesBetweenDates(DateTime start, DateTime end, bool includeUnmonitored);
        List<Movie> MoviesWithFiles(int movieId);
        PagingSpec<Movie> MoviesWithoutFiles(PagingSpec<Movie> pagingSpec);
        List<Movie> GetMoviesByFileId(int fileId);
        void SetFileId(int fileId, int movieId);
        PagingSpec<Movie> MoviesWhereCutoffUnmet(PagingSpec<Movie> pagingSpec, List<QualitiesBelowCutoff> qualitiesBelowCutoff);
        Movie FindByPath(string path);
        List<string> AllMoviePaths();
    }

    public class MovieRepository : BasicRepository<Movie>, IMovieRepository
    {
        private readonly IProfileRepository _profileRepository;
        public MovieRepository(IMainDatabase database,
                               IProfileRepository profileRepository,
                               IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
            _profileRepository = profileRepository;
        }

        protected override SqlBuilder BuilderBase() => new SqlBuilder()
            .Join("Profiles ON Profiles.Id = Movies.ProfileId")
            .LeftJoin("AlternativeTitles ON AlternativeTitles.MovieId = Movies.Id")
            .LeftJoin("MovieFiles ON MovieFiles.MovieId = Movies.Id");

        private Movie Map(Dictionary<int, Movie> dict, Movie movie, Profile profile, AlternativeTitle altTitle, MovieFile movieFile)
        {
            Movie movieEntry;

            if (!dict.TryGetValue(movie.Id, out movieEntry))
            {
                movieEntry = movie;
                movieEntry.Profile = profile;
                movieEntry.MovieFile = movieFile;
                dict.Add(movieEntry.Id, movieEntry);
            }

            if (altTitle != null)
            {
                movieEntry.AlternativeTitles.Add(altTitle);
            }

            return movieEntry;
        }

        protected override IEnumerable<Movie> GetResults(SqlBuilder.Template sql)
        {
            var movieDictionary = new Dictionary<int, Movie>();

            using (var conn = _database.OpenConnection())
            {
                conn.Query<Movie, Profile, AlternativeTitle, MovieFile, Movie>(
                    sql.RawSql,
                    (movie, profile, altTitle, file) => Map(movieDictionary, movie, profile, altTitle, file),
                    sql.Parameters)
                    .ToList();
            }

            return movieDictionary.Values;
        }

        public override IEnumerable<Movie> All()
        {
            // the skips the join on profile and populates manually
            // to avoid repeatedly deserializing the same profile
            var noProfileTemplate = $"SELECT /**select**/ FROM {_table} /**leftjoin**/ /**where**/ /**orderby**/";
            var sql = Builder().AddTemplate(noProfileTemplate);

            var movieDictionary = new Dictionary<int, Movie>();
            var profiles = _profileRepository.All();

            using (var conn = _database.OpenConnection())
            {
                conn.Query<Movie, AlternativeTitle, MovieFile, Movie>(
                    sql.RawSql,
                    (movie, altTitle, file) => Map(movieDictionary, movie, null, altTitle, file),
                    sql.Parameters);

                return movieDictionary.Values.Join(profiles, m => m.ProfileId, p => p.Id, (movie, profile) => {
                        movie.Profile = profile;
                        return movie;
                    }).ToList();
            }
        }

        public bool MoviePathExists(string path)
        {
            return Query(Builder().WhereEqual<Movie>(x => x.Path, path)).Any();
        }

        public List<Movie> FindByTitles(List<string> titles)
        {
            return Query(Builder().OrWhereIn<Movie>(x => x.CleanTitle, titles)
                         .OrWhereIn<AlternativeTitle>(x => x.CleanTitle, titles));
        }

        public List<Movie> FindByTitleInexact(string cleanTitle)
        {
            return Query(Builder().WhereSubstringOf<Movie>(x => x.Id, cleanTitle));
        }

        public Movie FindByImdbId(string imdbid)
        {
            var imdbIdWithPrefix = Parser.Parser.NormalizeImdbId(imdbid);
            return Query(Builder().WhereEqual<Movie>(x => x.ImdbId, imdbIdWithPrefix)).FirstOrDefault();
        }

        public Movie FindByTmdbId(int tmdbid)
        {
            return Query(Builder().WhereEqual<Movie>(x => x.TmdbId, tmdbid)).FirstOrDefault();
        }

        public List<Movie> FindByTmdbId(List<int> tmdbids)
        {
            return Query(Builder().WhereIn<Movie>(x => x.TmdbId, tmdbids));
        }

        public List<Movie> GetMoviesByFileId(int fileId)
        {
            return Query(Builder().WhereEqual<Movie>(x => x.MovieFileId, fileId));
        }

        public void SetFileId(int fileId, int movieId)
        {
            SetFields(new Movie { Id = movieId, MovieFileId = fileId }, movie => movie.MovieFileId);
        }

        public Movie FindByTitleSlug(string slug)
        {
            return Query(Builder().WhereEqual<Movie>(x => x.TitleSlug, slug)).FirstOrDefault();
        }

        public List<Movie> MoviesBetweenDates(DateTime start, DateTime end, bool includeUnmonitored)
        {
            var builder = Builder()
                .Where($"(([{_table}].[InCinemas] BETWEEN @Start AND @End) OR ([{_table}].[PhysicalRelease] BETWEEN @Start AND @End))",
                       new { Start = start, End = end });

            if (!includeUnmonitored)
            {
                builder.WhereEqual<Movie>(x => x.Monitored, 1);
            }

            return Query(builder);
        }

        public List<Movie> MoviesWithFiles(int movieId)
        {
            return Query(Builder().WhereNotNull<Movie>(x => x.MovieFileId));
        }

        public SqlBuilder GetMoviesWithoutFilesBuilder() => BuilderBase().WhereEqual<Movie>(x => x.MovieFileId, 0);

        public PagingSpec<Movie> MoviesWithoutFiles(PagingSpec<Movie> pagingSpec)
        {
            pagingSpec.Records = GetPagedRecords(GetMoviesWithoutFilesBuilder().SelectAll(), pagingSpec, GetPagedSelector);
            pagingSpec.TotalRecords = GetPagedRecordCount(GetMoviesWithoutFilesBuilder().SelectCount(), pagingSpec);

            return pagingSpec;
        }

        public SqlBuilder GetMoviesWhereCutoffUnmetBuilder(List<QualitiesBelowCutoff> qualitiesBelowCutoff) => BuilderBase()
                .WhereEqual<Movie>(x => x.MovieFileId, 0)
                .Where(BuildQualityCutoffWhereClause(qualitiesBelowCutoff));

        public PagingSpec<Movie> MoviesWhereCutoffUnmet(PagingSpec<Movie> pagingSpec, List<QualitiesBelowCutoff> qualitiesBelowCutoff)
        {
            pagingSpec.Records = GetPagedRecords(GetMoviesWhereCutoffUnmetBuilder(qualitiesBelowCutoff).SelectAll(), pagingSpec, GetPagedSelector);
            pagingSpec.TotalRecords = GetPagedRecordCount(GetMoviesWhereCutoffUnmetBuilder(qualitiesBelowCutoff).SelectCount(), pagingSpec);

            return pagingSpec;
        }

        private string BuildQualityCutoffWhereClause(List<QualitiesBelowCutoff> qualitiesBelowCutoff)
        {
            var clauses = new List<string>();

            foreach (var profile in qualitiesBelowCutoff)
            {
                foreach (var belowCutoff in profile.QualityIds)
                {
                    clauses.Add(string.Format($"([{_table}].[ProfileId] = {profile.ProfileId} AND [MovieFile].[Quality] LIKE '%_quality_: {belowCutoff},%')"));
                }
            }

            return string.Format("({0})", string.Join(" OR ", clauses));
        }

        public Movie FindByPath(string path)
        {
            return Query(Builder().WhereEqual<Movie>(x => x.Path, path)).FirstOrDefault();
        }

        public List<string> AllMoviePaths()
        {
            using (var conn = _database.OpenConnection())
            {
                return conn.Query<string>("SELECT Path FROM Movies").ToList();
            }
        }
    }
}
