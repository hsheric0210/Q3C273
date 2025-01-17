// This code is distributed under MIT license. 
// Copyright (c) 2015 George Mamaladze
// See license.txt or https://mit-license.org/

using System;
using Ton618.MouseKeyHook.WinApi;

namespace Ton618.MouseKeyHook.Implementation
{
    internal abstract class BaseListener : IDisposable
    {
        protected BaseListener(Subscribe subscribe)
        {
            Handle = subscribe(Callback);
        }

        protected HookResult Handle { get; set; }

        public void Dispose()
        {
            Handle.Dispose();
        }

        protected abstract bool Callback(CallbackData data);
    }
}