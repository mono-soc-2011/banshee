//
// MediaContainer.cs
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
using org.gnome.UPnP;

namespace Banshee.Rygel
{
	  public class MediaContainer : MediaObject, MediaContainer2
	  {
		    IList<MediaObject> Containers;
		    IList<MediaObject> Items;
		    IList<MediaObject> Children;
		
		    public MediaContainer (MediaContainer parent, string displayname, ObjectPath path) : base (parent, displayname, "container", path)
		    {
			      Containers 	= new List<MediaObject>();
			      Items 		= new List<MediaObject>();
			      Children 	= new List<MediaObject>();
			
			      Searchable 	= false;
		    }
		
		    public void AddObject(MediaObject obj)
		    {
			      AddObject(obj, true);
		    }
		
		    public void AddObject(MediaObject obj, bool announce)
		    {
			      Children.Add(obj);
			
			      if (obj is MediaContainer)
				        Containers.Add(obj as MediaContainer);
			
			      if (obj is MediaItem)
				        Items.Add(obj as MediaItem);
			
			      if (Updated != null && announce)
				        Updated();
		    }
		
		    public void AddObjects(IList<MediaObject> objects)
		    {
			      AddObjects(objects, true);
		    }
		
		    public void AddObjects(IList<MediaObject> objects, bool announce)
		    {
			      foreach (MediaObject obj in objects) {
				        AddObject(obj, false);
			      }
			
			      if (Updated != null && announce)
				        Updated();
		    }
		
		    #region MediaContainer2 implementation
		    public event UpdatedHandler Updated;
		
		    private IDictionary<string, object>[] HandleList (uint offset, uint max, string[] filter, IList<MediaObject> objects)
		    {
			      try {
				        int begin = Math.Min((int)offset, objects.Count);
				        int end = max == 0 ? objects.Count : Math.Min((int)max, objects.Count);
				        int length = end - begin;
				
				        IDictionary<string, object>[] list = new IDictionary<string, object>[length];
				
				        int j = 0;
				        for (int i = begin; i < end; i++) {
					          list[j++] = objects[i].Get(filter);
				        }
				
				        return list;
			      } catch (Exception e) {
				        Console.Error.WriteLine(DisplayName + "::HandleList::Exception: " + e.ToString());
				        return new IDictionary<string, object>[0];
			      }
		    }

		    public IDictionary<string, object>[] ListChildren (uint offset, uint max, string[] filter)
		    {
			      return HandleList(offset, max, filter, Children);
		    }
		
		    public IDictionary<string, object>[] ListContainers (uint offset, uint max, string[] filter)
		    {
			      return HandleList(offset, max, filter, Containers);
		    }
		
		    public IDictionary<string, object>[] ListItems (uint offset, uint max, string[] filter)
		    {
			      return HandleList(offset, max, filter, Items);
		    }
		
		    public IDictionary<string, object>[] SearchObjects (string query, uint offset, uint max, string[] filter)
		    {
			      throw new System.NotImplementedException();
		    }
		
		    public uint ChildCount {
			      get { return (uint)Children.Count; }
		    }
		
		    public uint ItemCount {
			      get { return (uint)Items.Count; }
		    }
		
		    public uint ContainerCount {
			      get { return (uint)Containers.Count; }
		    }
		
		    public bool Searchable { get; protected set; }
		    #endregion
	  }
}

