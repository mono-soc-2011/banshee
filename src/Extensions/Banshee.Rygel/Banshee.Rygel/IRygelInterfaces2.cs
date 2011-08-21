//
// IRygelInterfaces2.cs
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
using System.Collections.Generic;
using DBus;

namespace org.gnome.UPnP
{
    [Interface ("org.gnome.UPnP.MediaObject2")]
    public interface MediaObject2
    {
        ObjectPath Parent { get; }
        string DisplayName { get; }
        string Type { get; }
        ObjectPath Path { get; }
    }

    public delegate void UpdatedHandler ();

    [Interface ("org.gnome.UPnP.MediaContainer2")]
    public interface MediaContainer2 : MediaObject2
    {
        event UpdatedHandler Updated;

        uint ChildCount { get; }
        uint ItemCount { get; }
        uint ContainerCount { get; }
        bool Searchable { get; }

        IDictionary<string, object>[] ListChildren (uint offset, uint max, string[] filter);
        IDictionary<string, object>[] ListContainers (uint offset, uint max, string[] filter);
        IDictionary<string, object>[] ListItems (uint offset, uint max, string[] filter);

        IDictionary<string, object>[] SearchObjects (string query, uint offset, uint max, string[] filter);
    }

    [Interface ("org.gnome.UPnP.MediaItem2")]
    public interface MediaItem2 : MediaObject2
    {
        string[] URLs { get; }
        string MIMEType { get; }

        // The following are optional, return of null is considered omitting them.

        // Applies to all items
        //x org.gnome.UPnP.MediaItem2.Size            # in units of bytes
        String Artist { get; }
        String Album { get; }
        String Date { get; } // must be compliant to ISO#8601 and RFC#3339.
        String Genre { get; }
        String DLNAProfile { get; }

        // video and audio/music
        int? Duration { get; } // in units of seconds
        int? Bitrate { get; }
        int? SampleRate { get; }
        int? BitsPerSample { get; }

        // video and images
        int? Width { get; }
        int? Height { get; }
        int? ColorDepth { get; }
        int? PixelWidth { get; }
        int? PixelHeight { get; }
        ObjectPath Thumbnail { get; }

        // audio and music
        ObjectPath AlbumArt { get; }

        /*
        * The objects returned by the "!Thumbnail" and "AlbumArt" props (if provided) must implement 
        * the org.gnome.MediaItem2 interface and the values of 'width', '!Height' and '!Depth' must be 
        * provided and '!Type' must be 'image' for these objects. If you provide "!DLNAProfile" property,
        * it will greatly help avoiding guessing of it's value by Rygel (gupnp-av actually).
        * */
    }
}
