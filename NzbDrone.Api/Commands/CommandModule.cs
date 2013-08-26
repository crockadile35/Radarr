﻿using System;
using System.Linq;
using Nancy;
using NzbDrone.Api.Extensions;
using NzbDrone.Common.Composition;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Api.Commands
{
    public class CommandModule : NzbDroneRestModule<CommandResource>
    {
        private readonly IMessageAggregator _messageAggregator;
        private readonly IContainer _container;

        public CommandModule(IMessageAggregator messageAggregator, IContainer container)
        {
            _messageAggregator = messageAggregator;
            _container = container;

            Post["/"] = x => RunCommand(ReadResourceFromRequest());

        }

        private Response RunCommand(CommandResource resource)
        {
            var commandType =
                _container.GetImplementations(typeof(ICommand))
                          .Single(c => c.Name.Replace("Command", "")
                          .Equals(resource.Command, StringComparison.InvariantCultureIgnoreCase));

            dynamic command = Request.Body.FromJson(commandType);
            _messageAggregator.PublishCommand(command);

            return resource.AsResponse(HttpStatusCode.Created);
        }
    }
}