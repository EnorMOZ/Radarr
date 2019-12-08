using System;
using System.Collections.Generic;
using Dapper;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Messaging.Commands
{
    public interface ICommandRepository : IBasicRepository<CommandModel>
    {
        void Trim();
        void OrphanStarted();
        List<CommandModel> FindCommands(string name);
        List<CommandModel> FindQueuedOrStarted(string name);
        List<CommandModel> Queued();
        List<CommandModel> Started();
        void Start(CommandModel command);
        void End(CommandModel command);
    }

    public class CommandRepository : BasicRepository<CommandModel>, ICommandRepository
    {
        public CommandRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public void Trim()
        {
            var date = DateTime.UtcNow.AddDays(-1);

            Delete(Builder().WhereLessThan<CommandModel>(x => x.EndedAt, date));
        }

        public void OrphanStarted()
        {
            var sql = @"UPDATE Commands SET Status = @Orphaned, EndedAt = @Ended WHERE Status = @Started";
            var args = new
                {
                    Orphaned = (int) CommandStatus.Orphaned,
                    Started = (int) CommandStatus.Started,
                    Ended = DateTime.UtcNow
                };

            using (var conn = _database.OpenConnection())
            {
                conn.Execute(sql, args);
            }
        }

        public List<CommandModel> FindCommands(string name)
        {
            return Query(Builder().WhereEqual<CommandModel>(x => x.Name, name));
        }

        public List<CommandModel> FindQueuedOrStarted(string name)
        {
            return Query(Builder()
                         .WhereEqual<CommandModel>(x => x.Name, name)
                         .WhereIn<CommandModel>(x => x.Status, new [] { (int)CommandStatus.Queued, (int)CommandStatus.Started }));
        }

        public List<CommandModel> Queued()
        {
            return Query(Builder().WhereEqual<CommandModel>(x => x.Status, (int)CommandStatus.Queued));
        }

        public List<CommandModel> Started()
        {
            return Query(Builder().WhereEqual<CommandModel>(x => x.Status, (int)CommandStatus.Started));
        }

        public void Start(CommandModel command)
        {
            SetFields(command, c => c.StartedAt, c => c.Status);
        }

        public void End(CommandModel command)
        {
            SetFields(command, c => c.EndedAt, c => c.Status, c => c.Duration, c => c.Exception);
        }
    }
}
