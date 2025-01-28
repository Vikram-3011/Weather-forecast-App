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

        public async Task<BlazorApp.Models.User> SignUp(string email, string password, UserStateManager userState)
        {
            var response = await _supabaseClient.Auth.SignUp(email, password);
            currentUser = response?.User; // Use Supabase.Gotrue.User explicitly

            if (currentUser != null)
            {
                var newUser = new BlazorApp.Models.User(currentUser.Email, null, new List<Favourite>());
                userState.SetUser(newUser); // Store user email globally
                _snackbar.Add("User registered successfully!", Severity.Success);
                return newUser;
            }
            else
            {
                _snackbar.Add("Registration failed!", Severity.Error);
                return null;
            }
        }

        public async Task<string> SignIn(string email, string password, UserStateManager userState)
        {
            var response = await _supabaseClient.Auth.SignIn(email, password);
            Supabase.Gotrue.User? currentUser = response.User; // Fully qualified name

            if (currentUser != null)
            {
                var loggedInUser = new BlazorApp.Models.User(currentUser.Email, null, new List<Favourite>());
                userState.SetUser(loggedInUser); // Store user email globally
                _snackbar.Add("User Logged in successfully!", Severity.Success);
                return null;
            }
            else
            {
                _snackbar.Add("Login failed!", Severity.Error);
                return "Login failed!";
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

                if (session == null)
                    throw new Exception("User not logged in.");

                // Create a UserAttributes object and set the new password
                var userAttributes = new UserAttributes
                {
                    Password = newPassword
                };

                // Update the user's attributes
                await _supabaseClient.Auth.Update(userAttributes);

                _snackbar.Add("Password updated successfully!", Severity.Success);
                return true;
            }
            catch (Exception ex)
            {
                _snackbar.Add($"Failed to change password: {ex.Message}", Severity.Error);
                return false;
            }
        }


        public async Task SendPasswordResetLink(string email)
        {
            try
            {
                await _supabaseClient.Auth.ResetPasswordForEmail(email);
                _snackbar.Add("Password reset email sent successfully.", Severity.Success);
            }
            catch (Exception ex)
            {
                _snackbar.Add($"Failed to send password reset email: {ex.Message}", Severity.Error);
            }
        }
    }
}
