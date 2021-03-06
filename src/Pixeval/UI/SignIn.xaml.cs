﻿#region Copyright (C) 2019-2020 Dylech30th. All rights reserved.

// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019-2020 Dylech30th
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Threading.Tasks;
using System.Windows;
using Pixeval.Objects.Exceptions.Logger;
using Pixeval.Objects.I18n;
using Pixeval.Objects.Primitive;
using Pixeval.Persisting;
using Refit;

namespace Pixeval.UI
{
    public partial class SignIn
    {
        public static SignIn Instance;

        public SignIn()
        {
            Instance = this;
            InitializeComponent();
        }

        private async void Login_OnClick(object sender, RoutedEventArgs e)
        {
            if (Email.Text.IsNullOrEmpty() || Password.Password.IsNullOrEmpty())
            {
                ErrorMessage.Text = AkaI18N.EmptyEmailOrPasswordIsNotAllowed;
                return;
            }

            Login.Disable();

            try
            {
                await Task.WhenAll(Authentication.AppApiAuthenticate(Email.Text, Password.Password), Authentication.WebApiAuthenticate(Email.Text, Password.Password));
            }
            catch (Exception exception)
            {
                SetErrorHint(exception);
                ExceptionDumper.WriteException(exception);
                Login.Enable();
                return;
            }

            var mainWindow = new MainWindow();
            mainWindow.Show();

            Close();
        }

        private async void SignIn_OnInitialized(object sender, EventArgs e)
        {
            if (Session.ConfExists())
            {
                try
                {
                    DialogHost.OpenControl();
                    await Session.RefreshIfRequired();
                }
                catch (Exception exception)
                {
                    SetErrorHint(exception);
                    ExceptionDumper.WriteException(exception);
                    DialogHost.CurrentSession.Close();
                    return;
                }

                DialogHost.CurrentSession.Close();

                var mainWindow = new MainWindow();
                mainWindow.Show();
                Close();
            }
        }

        private async void SetErrorHint(Exception exception)
        {
            ErrorMessage.Text = exception is ApiException aException && await IsPasswordOrAccountError(aException) ? AkaI18N.EmailOrPasswordIsWrong : exception.Message;
        }

        private static async ValueTask<bool> IsPasswordOrAccountError(ApiException exception)
        {
            var eMess = await exception.GetContentAsAsync<dynamic>();
            return eMess.errors.system.code == 1508;
        }
    }
}