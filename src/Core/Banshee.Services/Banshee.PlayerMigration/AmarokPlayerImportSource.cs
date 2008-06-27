//
// AmarokPlayerImportSource.cs
//
// Author:
//   Sebastian Dröge <slomo@circular-chaos.org>
//   Scott Peterson <lunchtimemama@gmail.com>
//
// Copyright (C) 2006 Sebastian Dröge, Scott Peterson
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
using System.Data;
using System.IO;

using Mono.Unix;

using Hyena.Data.Sqlite;

using Banshee.Base;
using Banshee.Collection.Database;
using Banshee.Library;
using Banshee.ServiceStack;

namespace Banshee.PlayerMigration
{
    public sealed class AmarokPlayerImportSource : ThreadPoolImportSource
    {
        private static readonly string amarok_db_path = Paths.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), ".kde", "share", "apps", "amarok", "collection.db");

        protected override void ImportCore ()
        {
            LibraryImportManager import_manager = ServiceManager.Get<LibraryImportManager> ();
            HyenaSqliteConnection conn;

            try {
                conn = new HyenaSqliteConnection (amarok_db_path);
            } catch (Exception e) {
                LogError (amarok_db_path, String.Format (
                    "Unable to open Amarok database: {0}", e.Message));
                return;
            }
            
            int count = 0;
            try {
                count = conn.Query<int> ("SELECT COUNT(*) FROM tags");
            } catch (Exception) {}

            try {
                conn.Execute (@"
                    CREATE TEMP TABLE devices_tmp
                           (id INTEGER PRIMARY KEY,
                            lastmountpoint VARCHAR(255));
                    INSERT INTO devices_tmp (id, lastmountpoint)
                           SELECT devices.id,
                                  devices.lastmountpoint
                           FROM devices;
                    INSERT OR IGNORE INTO devices_tmp (id, lastmountpoint)
                           VALUES (-1, '/');"
                );
                
                HyenaSqliteCommand cmd = new HyenaSqliteCommand (@"
                    SELECT DISTINCT
                           devices_tmp.lastmountpoint,
                           tags.url,
                           tags.title,
                           artist.name,
                           genre.name,
                           album.name,
                           year.name,
                           tags.track,
                           tags.length,
                           tags.deviceid
                     FROM  tags,
                           devices_tmp,
                           artist,
                           album,
                           genre,
                           year
                     WHERE tags.deviceid = devices_tmp.id
                       AND tags.artist = artist.id
                       AND tags.album = album.id
                       AND tags.genre = genre.id
                       AND tags.year = year.id"
                );
                
                HyenaSqliteCommand stats_cmd = new HyenaSqliteCommand (@"
                                                     SELECT DISTINCT rating/2, playcounter
                                                     FROM   statistics
                                                     WHERE  accessdate > 0 AND url = ? AND deviceid = ?");

                int processed = 0;

                IDataReader reader = conn.Query (cmd);
                while (reader.Read ()) {
                     if (CheckForCanceled ())
                         break;

                     processed++;

                     try {
                         string mountpoint = (string) reader[0];
                         string path = (string) reader[1];
                         
                         SafeUri uri = null;
                         if (path.StartsWith ("./")) {
                             uri = new SafeUri (Path.Combine (mountpoint, path.Substring (2)));
                         } else if (path.StartsWith ("/")) {
                             uri = new SafeUri (path);
                         } else {
                             continue;
                         }

                         string title = (string) reader[2];
                         string artist = (string) reader[3];
                         
                         // the following fields are not critical and can be skipped if something goes wrong
                         int rating = 0, playcount = 0;

                         // Try to read stats
                         try {
                             int deviceid = Convert.ToInt32 (reader [9]);
                             IDataReader stats_reader = conn.Query (stats_cmd, path, deviceid);

                             while (stats_reader.Read ()) {
                                 rating = Convert.ToInt32 (stats_reader[0]);
                                 playcount = Convert.ToInt32 (stats_reader[1]);
                             }
                             stats_reader.Close ();
                         } catch (Exception) {}

                         UpdateUserJob (processed, count, artist, title);
                     
                         try {
                             DatabaseTrackInfo track = import_manager.ImportTrack (uri);
                            
                             if (track == null) {
                                 throw new Exception (String.Format (Catalog.GetString ("Unable to import track: {0}"), uri.AbsoluteUri));
                             }
                            
                             if (rating > 0 || playcount > 0) {
                                 track.Rating = rating;
                                 track.PlayCount = playcount;
                                 track.Save (false);
                             }
                         } catch (Exception e) {
                             LogError (SafeUri.UriToFilename (uri), e);
                         }
                     } catch (Exception e) {
                         Hyena.Log.Exception (e);
                         // something went wrong, skip entry
                     }
                 }
                 reader.Close ();
                 
                 // TODO migrating more than the podcast subscriptions (eg whether to auto sync them etc) means 1) we need to have those features
                 // and 2) we need to depend on Migo and/or the Podcast extension
                 DBusCommandService cmd_service = ServiceManager.Get<DBusCommandService> ();
                 if (cmd_service != null && ServiceManager.DbConnection.TableExists ("PodcastSyndications")) {
                     foreach (string podcast_url in conn.QueryEnumerable<string> ("SELECT url FROM podcastchannels")) {
                         cmd_service.PushFile (podcast_url.Replace ("http:", "feed:"));
                     }
                 }
                 
            } catch (Exception e) {
                Hyena.Log.Exception (e);
                LogError (amarok_db_path, Catalog.GetString ("Importing from Amarok failed"));
            } finally {
                conn.Dispose ();
            }
        }
        
        public static new bool CanImport {
            get { return Banshee.IO.File.Exists (new SafeUri (amarok_db_path)); }
        }
        
        public override string Name {
            get { return Catalog.GetString ("Amarok"); }
        }

        public override string [] IconNames {
            get { return new string [] { "system-search" }; }
        }
    }
}
