#region Copyright (C) 2011-2013 MPExtended
// Copyright (C) 2011-2013 MPExtended Developers, http://www.mpextended.com/
// 
// MPExtended is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MPExtended is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MPExtended. If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Libraries.Service;
using MPExtended.Services.MediaAccessService.Interfaces;

namespace MPExtended.Applications.WebMediaPortal.Models
{
    public class AvailabilityModel
    {
        public bool TAS
        {
            get
            {
                if (Connections.Current.HasTASConnection)
                {
                    return true;
                }
                else
                {
                    Log.Debug("No TAS connection, retrying");
                    return Connections.Current.HasTASConnection;
                }
            }
        }

        public bool MAS
        {
            get
            {
                if (Connections.Current.HasMASConnection)
                {
                    return true;
                }
                else
                {
                    Log.Debug("No MAS connection, retrying");
                    return Connections.Current.HasMASConnection;
                }
            }
        }

        public bool Movies
        {
            get
            {
                if (!MAS)
                {
                    Log.Debug("No MAS connection, Movies unavailable");
                    return false;
                }
                WebMediaServiceDescription serviceDescription = Connections.Current.MAS.GetServiceDescription();
                return Settings.ActiveSettings.MovieProvider == null ? serviceDescription.DefaultMovieLibrary != 0 : serviceDescription.AvailableMovieLibraries.Any(x => x.Id == Settings.ActiveSettings.MovieProvider);
            }
        }

        public bool TVShows
        {
            get
            {
                if (!MAS)
                {
                    Log.Debug("No MAS connection, TVShows unavailable");
                    return false;
                }
                WebMediaServiceDescription serviceDescription = Connections.Current.MAS.GetServiceDescription();
                return Settings.ActiveSettings.TVShowProvider == null ? serviceDescription.DefaultTvShowLibrary != 0 : serviceDescription.AvailableTvShowLibraries.Any(x => x.Id == Settings.ActiveSettings.TVShowProvider);
            }
        }

        public bool Music
        {
            get
            {
                if (!MAS)
                {
                    Log.Debug("No MAS connection, Music unavailable");
                    return false;
                }
                WebMediaServiceDescription serviceDescription = Connections.Current.MAS.GetServiceDescription();
                return Settings.ActiveSettings.MusicProvider == null ? serviceDescription.DefaultMusicLibrary != 0 : serviceDescription.AvailableMusicLibraries.Any(x => x.Id == Settings.ActiveSettings.MusicProvider);
            }
        }

        public bool Authentication
        {
            get
            {
                return Configuration.Authentication.Enabled;
            }
        }
    }
}