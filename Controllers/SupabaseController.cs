namespace BlazorApp.Controllers
{
    public class SupabaseController
    {
        private readonly Supabase.Client _client;

        public SupabaseController(Supabase.Client client)
        {
            _client = client;
        }

        public Supabase.Client GetClient()
        {
            return _client;
        }

    }
}
