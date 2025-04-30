using BlazorApp.Controllers;
using BlazorApp.Models;
using BlazorApp.Singletons;
using MudBlazor;
using Supabase.Gotrue;
using BlazorApp.Models;

namespace BlazorApp.Services
{
    public class AuthService
    {
        private readonly Supabase.Client _supabaseClient;
        private Supabase.Gotrue.User? currentUser; // Fully qualified name
        private readonly ISnackbar _snackbar;

        public AuthService(SupabaseController supabaseController, ISnackbar snackbar)
        {
            _supabaseClient = supabaseController.GetClient(); // Access Supabase.Client through SupabaseController
            _snackbar = snackbar;
        }

        public async Task<bool> SignUp(string email, string password)
        {
            try
            {
                // Check if the email already exists
                var existingUser = await _supabaseClient.Auth.SignIn(email, password);
                if (existingUser?.User != null)
                {
                    _snackbar.Add("Try to login, this email is already registered.", Severity.Warning);
                    return false;
                }
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("Invalid login credentials")) // Ignore login failure, as user may not exist
                {
                    _snackbar.Add($"Error checking existing user: {ex.Message}", Severity.Error);
                    return false;
                }
            }

            try
            {
                var response = await _supabaseClient.Auth.SignUp(email, password);
                if (response?.User != null)
                {
                    await _supabaseClient.Auth.SignOut(); // Ensure user is logged out until verification
                    _snackbar.Add("Signup successful! Check your email to verify your account.", Severity.Success);
                    return true;
                }
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message.ToLower(); // Normalize message for easier handling

                if (errorMessage.Contains("weak_password") || errorMessage.Contains("password should be at least 6 characters"))
                {
                    _snackbar.Add("Enter a strong password. It should be more than 6 characters.", Severity.Error);
                }
                else if (errorMessage.Contains("over_email_send_rate_limit"))
                {
                    _snackbar.Add("Too many signup attempts! Please wait a few minutes before trying again.", Severity.Warning);
                }
                else
                {
                    _snackbar.Add($"Signup failed: {errorMessage}", Severity.Error);
                }
            }
            return false;
        }




        public async Task<string?> SignIn(string email, string password, UserStateManager userState)
        {
            try
            {
                var response = await _supabaseClient.Auth.SignIn(email, password);
                currentUser = response?.User;

                if (currentUser == null)
                {
                    _snackbar.Add("Incorrect email or password. Please try again.", Severity.Error);
                    return "Invalid login credentials.";
                }

                if (!currentUser.EmailConfirmedAt.HasValue)
                {
                    await _supabaseClient.Auth.SignOut();
                    _snackbar.Add("Email not verified. Please check your inbox.", Severity.Warning);
                    return "Email not verified.";
                }

                var loggedInUser = new BlazorApp.Models.User(currentUser.Email, null, new List<Favourite>());
                userState.SetUser(loggedInUser);
                _snackbar.Add("Login successful!", Severity.Success);
                return null;
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;

                // Handle invalid credentials error
                if (errorMessage.Contains("invalid_credentials") || errorMessage.Contains("Invalid login credentials"))
                {
                    _snackbar.Add("Incorrect email or password. Please try again.", Severity.Error);
                    return "Invalid login credentials.";
                }
                else
                {
                    _snackbar.Add($"Login failed: {errorMessage}", Severity.Error);
                    return $"Login failed: {errorMessage}";
                }
            }
        }



        public async Task<string> SignOut(UserStateManager userState)
        {
            await _supabaseClient.Auth.SignOut();
            currentUser = null;
            userState.ClearUser(); // Clear user from state
            _snackbar.Add("User signed out successfully!", Severity.Success);
            return null;
        }

        public Session GetSession()
        {
            return _supabaseClient.Auth.CurrentSession;
        }

        public Supabase.Gotrue.User? GetUser() // Fully qualified name
        {
            return currentUser;
        }

        public async Task<bool> ChangePassword(string currentPassword, string newPassword)
        {
            try
            {
                var session = _supabaseClient.Auth.CurrentSession;
                if (session == null || session.User == null)
                {
                    _snackbar.Add("User not logged in. Please log in again.", Severity.Warning);
                    return false;
                }

                var email = session.User.Email;

                // Try to authenticate with the current password
                var reauthResponse = await _supabaseClient.Auth.SignIn(email, currentPassword);

                if (reauthResponse?.User == null)
                {
                    _snackbar.Add("Incorrect current password. Please try again.", Severity.Error);
                    return false;
                }

                // Proceed to update the password
                var userAttributes = new UserAttributes { Password = newPassword };
                await _supabaseClient.Auth.Update(userAttributes);

                _snackbar.Add("Password updated successfully!", Severity.Success);
                return true;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("invalid_credentials"))
                {
                    _snackbar.Add("Incorrect current password. Please try again.", Severity.Error);
                }
                else if (ex.Message.Contains("same_password"))
                {
                    _snackbar.Add("New password must be different from the current password.", Severity.Warning);
                }
                else
                {
                    _snackbar.Add($"Failed to change password: {ex.Message}", Severity.Error);
                }
                return false;
            }
        }






        public async Task SendPasswordResetLink(string email)
        {
            try
            {
                await _supabaseClient.Auth.ResetPasswordForEmail(email);
                _snackbar.Add("Password reset email sent. Check your inbox.", Severity.Success);
            }
            catch (Exception ex)
            {
                _snackbar.Add($"Failed to send password reset email: {ex.Message}", Severity.Error);
            }
        }

        public async Task<bool> ResetPassword(string newPassword, string accessToken)
        {
            try
            {
                if (string.IsNullOrEmpty(accessToken))
                {
                    _snackbar.Add("Invalid or expired password reset link.", Severity.Error);
                    return false;
                }

                // Set the session using the access token (ensure correct refreshToken handling)
                var session = await _supabaseClient.Auth.SetSession(accessToken, "");

                if (session == null || session.User == null)
                {
                    _snackbar.Add("Invalid or expired reset link.", Severity.Error);
                    return false;
                }

                // Proceed to update the password
                var userAttributes = new UserAttributes { Password = newPassword };
                await _supabaseClient.Auth.Update(userAttributes);

                _snackbar.Add("Password updated successfully! You can now log in.", Severity.Success);
                return true;
            }
            catch (Exception ex)
            {
                _snackbar.Add($"Error updating password: {ex.Message}", Severity.Error);
                return false;
            }
        }




        public async Task<bool> ConfirmEmail()
        {
            try
            {
                var session = _supabaseClient.Auth.CurrentSession;
                if (session == null || session.AccessToken == null)
                {
                    _snackbar.Add("Session expired or invalid. Please log in again.", Severity.Warning);
                    return false;
                }

                var user = await _supabaseClient.Auth.GetUser(session.AccessToken);
                if (user != null && user.EmailConfirmedAt.HasValue)
                {
                    _snackbar.Add("Email verified successfully. You can now log in.", Severity.Success);
                    return true;
                }

                _snackbar.Add("Email verification failed. Try again or request a new confirmation email.", Severity.Warning);
                return false;
            }
            catch (Exception ex)
            {
                _snackbar.Add($"Email confirmation check failed: {ex.Message}", Severity.Error);
                return false;
            }
        }

        public async Task<bool> UpdatePassword(string newPassword)
        {
            try
            {
                var session = _supabaseClient.Auth.CurrentSession;
                if (session == null || session.User == null)
                {
                    _snackbar.Add("Session expired or invalid. Please request a new reset link.", Severity.Warning);
                    return false;
                }

                var userAttributes = new UserAttributes { Password = newPassword };
                await _supabaseClient.Auth.Update(userAttributes); // Correct usage

                _snackbar.Add("Password updated successfully! You can now log in.", Severity.Success);
                return true;
            }
            catch (Exception ex)
            {
                _snackbar.Add($"Error updating password: {ex.Message}", Severity.Error);
                return false;
            }
        }


    }
}
