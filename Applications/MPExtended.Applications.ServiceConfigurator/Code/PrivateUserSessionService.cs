﻿#region Copyright (C) 2011-2013 MPExtended
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
using System.ServiceModel;
using System.Text;
using System.Windows;
using MPExtended.Applications.ServiceConfigurator.Pages;
using MPExtended.Libraries.Service.Strings;
using MPExtended.Services.Common.Interfaces;
using MPExtended.Services.MetaService.Interfaces;
using MPExtended.Services.UserSessionService.Interfaces;

namespace MPExtended.Applications.ServiceConfigurator.Code
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true, InstanceContextMode = InstanceContextMode.Single)]
    internal class PrivateUserSessionService : IPrivateUserSessionService
    {
        private Dictionary<string, SelectUserDialog> accessRequestDialogs = new Dictionary<string, SelectUserDialog>();

        public WebBoolResult OpenConfigurator()
        {
            Application.Current.MainWindow.Show();
            return true;
        }

        public WebBoolResult RequestAccess(string token, string clientName, string ipAddress, List<string> users)
        {
            string msg = String.Format(UI.AccessRequest, clientName, ipAddress);
            accessRequestDialogs[token] = new SelectUserDialog("MPExtended", msg, users);
            accessRequestDialogs[token].Width = 360;
            accessRequestDialogs[token].Height = 220;
            accessRequestDialogs[token].Show();
            accessRequestDialogs[token].Focus();

            return true;
        }

        public WebAccessRequestResponse GetAccessRequestStatus(string token)
        {
            WebAccessRequestResponse response = new WebAccessRequestResponse();
            if (accessRequestDialogs[token] != null)
            {
                response.UserHasResponded = accessRequestDialogs[token].UserHasResponded;
                response.Username = accessRequestDialogs[token].SelectedUser;
                response.IsAllowed = accessRequestDialogs[token].SelectedUser != null;
            }

            return response;
        }

        public WebBoolResult CancelAccessRequest(string token)
        {
            if (accessRequestDialogs.ContainsKey(token))
            {
                accessRequestDialogs[token].Close();
                accessRequestDialogs.Remove(token);
                return true;
            }
            return false;
        }
    }
}
