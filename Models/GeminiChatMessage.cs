﻿namespace BlazorApp.Models
{
    public class GeminiChatMessage
    {
        public string Role { get; set; } = "user"; // or "model"
        public string Content { get; set; } = string.Empty;
    }
}
