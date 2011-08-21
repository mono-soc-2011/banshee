//
// BansheeIndexer.cs
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
using Banshee.Collection.Indexer.RemoteHelper;

namespace Banshee.Rygel
{
    public delegate void ResultHandler (List<TrackInfo> tracks);

	  class CollectionIndexer : SimpleIndexerClient
	  {
		    private DateTime last_change;
		    private List<TrackInfo> tracks;
		    private Object mutex;
        private ResultHandler handler;
		
		    readonly string[] export_fields = new [] {"name", "artist", "album", "track-number", "URI", "local-path", "mime-type", "file-size", "bits-per-sample", "sample-rate", "length", "media-attributes"};
		
		    public CollectionIndexer(ResultHandler handler)
		    {
			      mutex = new Object();
			      last_change = DateTime.MinValue;
			      tracks = new List<TrackInfo>();
            this.handler = handler;
			
			      AddExportField(export_fields);
			      IndexWhenCollectionChanged = false;
		    }
		
		    #region implemented abstract members of Banshee.Collection.Indexer.RemoteHelper.SimpleIndexerClient
		    protected override void IndexResult (IDictionary<string, object> result)
		    {
			      lock (mutex) {
				        tracks.Add(new TrackInfo(result));
				        if (tracks.Count % 500 == 0)
					          Console.WriteLine("Working ...");
			      }
		    }
		
		    protected override void OnStarted ()
		    {
		    }

		    protected override void OnBeginUpdateIndex ()
		    {
		    }
		
		    protected override void OnEndUpdateIndex ()
		    {
			      lock (mutex) {
				        last_change = DateTime.UtcNow;
                handler (tracks);
			      }
		    }
		
		    protected override void OnShutdownWhileIndexing ()
		    {
		    }
		
		    protected override int CollectionCount {
			      get { lock (mutex) return tracks.Count; }
		    }
		
		
		    protected override DateTime CollectionLastModified {
			      get { lock (mutex) return last_change; }
		    }
		    #endregion
	  }
}

