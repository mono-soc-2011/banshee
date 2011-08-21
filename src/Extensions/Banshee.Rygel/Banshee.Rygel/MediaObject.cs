//
// MediaObject.cs
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
using System.Reflection;
using System.Collections.Generic;

using DBus;
using org.gnome.UPnP;

namespace Banshee.Rygel
{
	  public class MediaObject : MediaObject2, org.freedesktop.DBus.Properties
	  {
		    public MediaObject (MediaContainer parent, string displayname, string type, ObjectPath path)
		    {
			      if (path == null)
				        throw new ArgumentNullException("path");
			
			      Parent = (parent == null) ? path : parent.Path; // If parent is null, parent is current object (should only happen on root)
			      DisplayName = displayname;
			      Type = type;
			      Path = path;
			
			      Bus.Session.Register (Path, this);
		    }
		
		    ~MediaObject ()
		    {
			      Bus.Session.Unregister (Path);
		    }
		
		    #region MediaObject2 implementation
		    public ObjectPath Parent { get ; protected set;	}
		    public string DisplayName { get ; protected set; }
		    public string Type { get ; protected set; }
		    public ObjectPath Path { get ; protected set; }
		    #endregion
		
		    #region Properties implementation
		    public virtual object Get (string @interface, string propname)
		    {
			      try {
				        foreach (var t in GetType().GetInterfaces()) {
					          if ((t.Namespace + "." + t.Name) == @interface) {
						            var property = t.GetProperty(propname);
						            if (property == null)
							              throw new ArgumentException(propname);
						            else
							              return property.GetGetMethod().Invoke(this, null);
					          }
				        }
			      } catch (Exception e) {
				        Console.Error.WriteLine(DisplayName + "::Get::Exception " + e.ToString());
				        return new Dictionary<string, object>();
			      }

			      throw new ArgumentException(@interface);
		    }
		
		    public void Set (string @interface, string propname, object value)
		    {
			      throw new System.NotImplementedException();
		    }
		
		    public virtual IDictionary<string, object> GetAll (string @interface)
		    {
			      try {
				        Dictionary<string, object> dict = new Dictionary<string, object>();
				        foreach (var t in GetType().GetInterfaces()) {
					          if (@interface.Equals(t.Namespace + "." + t.Name)) {
						            foreach (var property in t.GetProperties()) {	
							              var val = property.GetGetMethod().Invoke(this, null);
							              if (val != null)
								                dict.Add(property.Name, val);
						            }
						
						            return dict;
					          }
				        }
			      } catch (Exception e) {
				        Console.Error.WriteLine(DisplayName + "::GetAll::Exception " + e.ToString());
				        return new Dictionary<string, object>();
			      }

			      throw new ArgumentException(@interface);
		    }
		    #endregion
		
		    public IDictionary<string, object> Get(string []filter)
		    {
			      if (filter == null)
				        throw new ArgumentNullException("filter");
			
			      if (filter.Length == 0)
				        throw new ArgumentException("string array needs to be of length atleast 1", "filter");
			
			      try {
				        var type = GetType();
				        Dictionary<string, object> dict = new Dictionary<string, object>();
				
				        if (filter.Length == 1 && filter[0].Equals("*")) {
					          foreach (var property in type.GetProperties()) {
						            var val = property.GetGetMethod().Invoke(this, null);
						            if (val != null)
							              dict.Add(property.Name, val);
					          }
				        } else {
					          foreach (var propname in filter) {
						            var property = type.GetProperty(propname);
						            if (property == null) {
							              Console.Error.WriteLine(DisplayName + "::Get(filter) Failed to find this property " + propname);
						            } else {
						                var val = property.GetGetMethod().Invoke(this, null);
						                if (val != null)
							                  dict.Add(property.Name, val);
						            }
					          }
				        }
				        return dict;
			      } catch (Exception e) {
				        Console.Error.WriteLine(DisplayName + "::Get(filter)::Exception " + e.ToString());
				        return new Dictionary<string, object>();
			      }
		    }
	  }
}

