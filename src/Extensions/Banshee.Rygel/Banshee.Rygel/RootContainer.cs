//
// RootContainer.cs
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
using org.freedesktop.DBus;
using Banshee;

namespace Banshee.Rygel
{
	  public class RootContainer : MediaContainer
	  {
		    private static ObjectPath banshee_object_path = new ObjectPath("/org/gnome/UPnP/MediaServer2/Banshee");
		    private static string banshee_service_name = "org.gnome.UPnP.MediaServer2.Banshee";
		    private bool owner;
		
		    public RootContainer () : base(null, "Banshee", banshee_object_path)
		    {
			      owner = false;
			      MediaContainer songs = new MediaContainer(this, "Songs", new ObjectPath("/org/gnome/UPnP/MediaServer2/Banshee/Containers/Songs"));
			      AddObject(songs, false);
		    }
		
		    ~RootContainer ()
		    {
		        if (owner)
				        Bus.Session.ReleaseName (banshee_service_name);
		    }
		
		    public static ObjectPath BansheeBaseObjectPath { get { return banshee_object_path; } }
		
		    public bool Request ()
		    {
			      if (!owner)
				        owner = Bus.Session.RequestName (banshee_service_name) == RequestNameReply.PrimaryOwner;
			
			      return owner;
		    }

        public void Release ()
        {
            Bus.Session.ReleaseName (banshee_service_name);
        }

        private string GenerateSafeUUID(string ID)
        {
            // TODO need a better way to create a safe ObjectPath UUID
            return ID.GetHashCode().ToString().Replace("-", "n");
        }
	  }
}
