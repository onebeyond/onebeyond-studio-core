using System;
using OneBeyond.Studio.Core.Mediator;

namespace OneBeyond.Studio.Application.SharedKernel.Tests.CommandHandlers;

public static class DerivedCommandFactory
{
    public static IRequest GetCommand(string commandName)
    {
        if (commandName == nameof(DerivedCommand1))
        {
            return new DerivedCommand1(); 
        }

        if (commandName == nameof(DerivedCommand2))
        {
            return new DerivedCommand2();
        }

        throw new Exception($"Command {commandName} not recognised.");
    }
}
