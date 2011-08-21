//
// TrackInfo.cs
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

namespace Banshee.Rygel
{
	  public class TrackInfo
	  {
        public TrackInfo (IDictionary<string, object> properties)
        {
            if (!properties.ContainsKey("name"))
                throw new ArgumentNullException("name");

            if (!properties.ContainsKey("URI"))
                throw new ArgumentNullException("URI");

            Artist = "Unknown Artist";
            Album = "Unknown Album";
            MIMEType = "audio/mpeg";

            foreach (var property in properties) {
                switch (property.Key) {
                    case "name":
                        DisplayName = property.Value.ToString();
                        break;
                    case "URI":
                        URLs = new string [] { property.Value.ToString() };
                        break;
                    case "artist":
                        Artist = property.Value.ToString();
                        break;
                    case "album":
                        Album = property.Value.ToString();
                        break;
                    case "mime-type":
                        MIMEType = property.Value.ToString();
                        break;
                }
            }

            ID = properties["URI"].GetHashCode().ToString();
            ID = ID.Replace('-', 'n');
        }
		
		    public string ID { get; protected set; }
		    public string DisplayName { get ; protected set; }
		
		    public string[] URLs { get; protected set; }
		    public string MIMEType { get; protected set; }
		
		    // The following are optional, return of null is considered omitting them.
		
		    // Applies to all items
		    public String Artist { get; protected set;  }
		    public String Album { get; protected set;  }
		    public String Date { get; protected set;  }
		    public String Genre { get; protected set;  }
		    public String DLNAProfile { get; protected set;  }
		
		    // video and audio/music
		    public int? Duration { get; protected set;  } // in units of seconds
		    public int? Bitrate { get; protected set;  }
		    public int? SampleRate { get; protected set;  }
		    public int? BitsPerSample { get; protected set;  }
		
		    // video and images
		    public int? Width { get; protected set;  }
		    public int? Height { get; protected set;  }
		    public int? ColorDepth { get; protected set;  }
		    public int? PixelWidth { get; protected set;  }
		    public int? PixelHeight { get; protected set;  }
	  }
}

