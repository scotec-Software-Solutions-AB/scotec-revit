// Copyright (c) 2023 - 2026 Olaf Meyer
// Copyright (c) 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using Autodesk.Revit.DB.Events;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Scotec.Revit.EventHandler;

public abstract class RevitPostDocumentEventHandler<TEventArgs> : RevitPostEventHandler<TEventArgs>
    where TEventArgs : RevitAPIPostDocEventArgs
{
    protected RevitPostDocumentEventHandler(Guid addInId) : base(addInId)
    {
    }

    protected override void RegisterEventContext(IServiceCollection services, object sender, TEventArgs args)
    {
        base.RegisterEventContext(services, sender, args);
        var context = new RevitContext(args.Document);

        services.AddScoped<IRevitContext>(_ => context);
    }
}
