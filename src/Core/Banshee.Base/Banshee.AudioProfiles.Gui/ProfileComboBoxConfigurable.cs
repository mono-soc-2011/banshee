/***************************************************************************
 *  ProfileComboBoxConfigurable.cs
 *
 *  Copyright (C) 2006 Novell, Inc.
 *  Written by Aaron Bockover <aaron@abock.org>
 ****************************************************************************/

/*  THIS FILE IS LICENSED UNDER THE MIT LICENSE AS OUTLINED IMMEDIATELY BELOW: 
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a
 *  copy of this software and associated documentation files (the "Software"),  
 *  to deal in the Software without restriction, including without limitation  
 *  the rights to use, copy, modify, merge, publish, distribute, sublicense,  
 *  and/or sell copies of the Software, and to permit persons to whom the  
 *  Software is furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in 
 *  all copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 *  FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 *  DEALINGS IN THE SOFTWARE.
 */
 
using System;
using Gtk;

namespace Banshee.AudioProfiles.Gui
{
    public class ProfileComboBoxConfigurable : VBox
    {
        private ProfileComboBox combo;
        private ProfileConfigureButton button;
        private Label description;
        private string configuration_id;
        
        public ProfileComboBoxConfigurable(ProfileManager manager, string configurationId)
        {
            HBox editor = new HBox();
            
            configuration_id = configurationId;
            combo = new ProfileComboBox(manager);
            combo.Show();
            
            button = new ProfileConfigureButton(configurationId);
            button.ComboBox = combo;
            button.Show();
            
            editor.Spacing = 5;
            editor.PackStart(combo, true, true, 0);
            editor.PackStart(button, false, false, 0);
            editor.Show();
            
            description = new Label();
            description.Xalign = 0.0f;
            description.Wrap = true;
            description.Show();
            
            SetDescription();
            
            Combo.SetActiveProfile(manager.GetConfiguredActiveProfile(configurationId));
            
            Combo.Changed += delegate {
                ProfileConfiguration.SaveActiveProfile(Combo.ActiveProfile, configurationId);
                SetDescription();
            };
            
            Spacing = 5;
            PackStart(editor, true, true, 0);
            PackStart(description, false, false, 0);
        }
        
        private void SetDescription()
        {
            description.Markup = String.Format("<small><i>{0}</i></small>", GLib.Markup.EscapeText(
                Combo.ActiveProfile.Description));
        }
        
        public ProfileComboBox Combo {
            get { return combo; }
        }
        
        public string ConfigurationID {
            get { return configuration_id; }
        }
    }
}
