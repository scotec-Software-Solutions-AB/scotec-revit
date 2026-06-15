// Copyright (c) 2023 - 2026 Olaf Meyer
// Copyright (c) 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using Autodesk.Revit.DB.Events;
using System;

namespace Scotec.Revit.EventHandler;

public abstract class RevitSingleEventHandler<TSender, TEventArgs> : RevitEventHandler<TSender, TEventArgs>
    where TSender : class
    where TEventArgs : RevitAPISingleEventArgs
{
    protected RevitSingleEventHandler(Guid addInId) : base(addInId)
    {
    }
}
