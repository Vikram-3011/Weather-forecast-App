using BlazorApp.Models;
using Microsoft.AspNetCore.Components;


namespace BlazorApp.Singletons
{
    public class UserStateManager
    {
        public User? User { get; private set; }
        public event Action? OnChange;
        //public string UserEmail { get; set; } = "";
        public string? UserEmail => User?.Email;

        public void SetUser(User? user)
        {
            User = user;
            NotifyStateChanged();
        }



        public void ClearUser()
        {
            User = null;
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();

        public bool IsLoggedIn() => User != null;
    }
}
