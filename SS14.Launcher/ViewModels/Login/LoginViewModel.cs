using System;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SS14.Launcher.Models;

namespace SS14.Launcher.ViewModels.Login
{
    public class LoginViewModel : BaseLoginViewModel
    {
        private readonly ConfigurationManager _cfg;
        private readonly MainWindowLoginViewModel _parentVm;
        private readonly AuthApi _authApi;

        [Reactive] public string EditingUsername { get; set; } = "";
        [Reactive] public string EditingPassword { get; set; } = "";

        [Reactive] public bool IsInputValid { get; private set; }

        public LoginViewModel(ConfigurationManager cfg, MainWindowLoginViewModel parentVm, AuthApi authApi)
        {
            _authApi = authApi;
            _cfg = cfg;
            _parentVm = parentVm;

            this.WhenAnyValue(x => x.EditingUsername, x => x.EditingPassword)
                .Subscribe(s => { IsInputValid = !string.IsNullOrEmpty(s.Item1) && !string.IsNullOrEmpty(s.Item2); });
        }

        public async void OnLogInButtonPressed()
        {
            if (!IsInputValid)
            {
                return;
            }

            Busy = true;
            try
            {
                // TODO: Remove Task.Delay here.
                await Task.Delay(1000);
                var resp = await _authApi.AuthenticateAsync(EditingUsername, EditingPassword);

                if (resp.IsSuccess)
                {
                    var loginInfo = resp.LoginInfo;
                    if (_cfg.Logins.Lookup(loginInfo.UserId).HasValue)
                    {
                        // Already had a login like this??
                        // TODO: Immediately sign out the token here.
                        _cfg.SelectedLoginId = loginInfo.UserId;
                        return;
                    }

                    _cfg.AddLogin(loginInfo);
                    _cfg.SelectedLoginId = loginInfo.UserId;
                }
                else
                {
                    // TODO: Display errors
                }
            }
            finally
            {
                Busy = false;
            }
        }

        public void OnRegisterButtonPressed()
        {
            _parentVm.SwitchToRegister();
        }

        public void OnForgotPasswordPressed()
        {
            _parentVm.SwitchToForgotPassword();
        }

        public void OnResendConfirmationPressed()
        {
            _parentVm.SwitchToResendConfirmation();
        }
    }
}
