﻿using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Notifications.Pushover
{
    public class Pushover : NotificationBase<PushoverSettings>
    {
        private readonly IPushoverProxy _proxy;
        
        public Pushover(IPushoverProxy proxy)
        {
            _proxy = proxy;
        }

        public override string Link => "https://pushover.net/";

        public override void OnGrab(GrabMessage grabMessage)
        {
            const string title = "Movie Grabbed";

            _proxy.SendNotification(title, grabMessage.Message, Settings);
        }

        public override void OnDownload(DownloadMessage message)
        {
            const string title = "Movie Downloaded";

            _proxy.SendNotification(title, message.Message, Settings);
        }

        public override void OnMovieRename(Movie movie)
        {
        }
		
        public override string Name => "Pushover";

        public override bool SupportsOnRename => false;

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_proxy.Test(Settings));

            return new ValidationResult(failures);
        }
    }
}
