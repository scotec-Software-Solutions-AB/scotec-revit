// Copyright (c) 2023 - 2026 Olaf Meyer
// Copyright (c) 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using Autodesk.Revit.DB.Events;
using System;

namespace Scotec.Revit.EventHandler;

public abstract class RevitPreEventHandler<TEventArgs> : RevitEventHandler<TEventArgs>
    where TEventArgs : RevitAPIPreEventArgs
{
    protected RevitPreEventHandler(Guid addInId) : base(addInId)
    {
    }

    public void Cancel()
    {
        if(EventArgs is null)
        {
            throw new InvalidOperationException("EventArgs not set. This method can only be called during event handling.");
        }

        EventArgs.Cancel();
    }
}
