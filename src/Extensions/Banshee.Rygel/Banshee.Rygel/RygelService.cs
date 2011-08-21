//
// RygelService.cs
//
// Authors:
//   Tobias Arrskog <tobias.arrskog@gmail.com>
//
// Copyright (C) 2011 Tobias Arrskog
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using org.freedesktop.DBus;
using DBus;
using Hyena;
using Banshee.MediaEngine;
using Banshee.PlaybackController;
using Banshee.Playlist;
using Banshee.ServiceStack;
using Banshee.Sources;

namespace Banshee.Rygel
{
    public class RygelService : IExtensionService, IDisposable
    {
        private RootContainer root;
        private CollectionIndexer indexer;

        public RygelService ()
        {
        }

        void IExtensionService.Initialize ()
        {
            if (!DBusConnection.Enabled) {
                return;
            }

            root = new RootContainer();
            if (!root.Request()) {
                Hyena.Log.Warning ("Rygel service couldn't grab bus name");
                return;
            }

            indexer = new CollectionIndexer(tracks => root.ProcessTracks(tracks));
            indexer.Start();
        }

        void IDisposable.Dispose ()
        {
            root.Release();
            root = null;
            indexer = null;
        }

        string IService.ServiceName {
            get { return "RygelService"; }
        }
    }
}
