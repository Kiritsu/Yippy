using System.Text;
using System.Text.Json;
using Blazored.LocalStorage;
using Yippy.Web.Authentication.Models;

namespace Yippy.Web.Authentication;

public interface IAuthService
{
    Task<bool> SendEmailCodeAsync(string email);
    
    Task<AccessTokenResponse?> ValidateCodeAsync(Guid accessKey);
    
    Task<AuthState?> GetAuthStateAsync();
    
    Task<bool> RefreshTokenAsync();
    
    Task LogoutAsync();
    
    event Action<AuthState?> AuthStateChanged;
}

public class AuthService(
    HttpClient httpClient,
    ILocalStorageService localStorage,
    ILogger<AuthService> logger)
    : IAuthService
{
    private AuthState? _authState = new();

    public event Action<AuthState?> AuthStateChanged = delegate { };
    
    public async Task<bool> SendEmailCodeAsync(string email)
    {
        try
        {
            var request = new EmailRequest(email);
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await httpClient.PostAsync("/auth/token", content);
            response.EnsureSuccessStatusCode();
            
            // Store email for later use
            await localStorage.SetItemAsStringAsync("pending_email", email);

            return true;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to send email code for {Email}", email);
            return false;
        }
    }
    
    public async Task<AccessTokenResponse?> ValidateCodeAsync(Guid accessKey)
    {
        try
        {
            var request = new TokenValidationRequest(accessKey);
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await httpClient.PostAsync("/auth/validate", content);
            response.EnsureSuccessStatusCode();
            
            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<AccessTokenResponse>(responseJson, JsonSerializerOptions.Web);

            if (result != null)
            {
                await SetAuthenticationAsync(result);
            }

            return result ?? throw new InvalidOperationException("Invalid response from server");
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to validate access key");
            return null;
        }
    }
    
    public async Task<AuthState?> GetAuthStateAsync()
    {
        if (_authState?.IsAuthenticated == true)
        {
            return _authState;
        }

        // Try to restore from storage
        var token = await localStorage.GetItemAsStringAsync("auth_token");
        var refreshToken = await localStorage.GetItemAsync<Guid?>("refresh_token");
        var expiry = await localStorage.GetItemAsync<DateTime?>("token_expiry");
        var email = await localStorage.GetItemAsStringAsync("user_email");

        if (string.IsNullOrEmpty(token) || !refreshToken.HasValue || !expiry.HasValue)
        {
            return _authState;
        }
        
        if (expiry.Value > DateTime.UtcNow)
        {
            _authState = new AuthState
            {
                IsAuthenticated = true,
                Token = token,
                RefreshToken = refreshToken,
                TokenExpiry = expiry.Value,
                Email = email,
                Claims = ParseJwtClaims(token)
            };

            httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            // Token expired, try to refresh
            await RefreshTokenAsync();
        }

        return _authState;
    }
    
    public async Task<bool> RefreshTokenAsync()
    {
        try
        {
            var refreshToken = await localStorage.GetItemAsStringAsync("refresh_token");
            if (string.IsNullOrEmpty(refreshToken))
            {
                return false;
            }

            // Implement refresh logic if your API supports it
            // For now, we'll redirect to login
            await LogoutAsync();
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to refresh token");
            await LogoutAsync();
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        _authState = new AuthState();
        
        await localStorage.RemoveItemAsync("auth_token");
        await localStorage.RemoveItemAsync("refresh_token");
        await localStorage.RemoveItemAsync("token_expiry");
        await localStorage.RemoveItemAsync("user_email");
        await localStorage.RemoveItemAsync("pending_email");

        httpClient.DefaultRequestHeaders.Authorization = null;
        
        AuthStateChanged.Invoke(_authState);
    }
    
    private async Task SetAuthenticationAsync(AccessTokenResponse response)
    {
        var email = await localStorage.GetItemAsStringAsync("pending_email");
        var tokenExpiry = DateTime.UtcNow.AddMinutes(response.Duration);
        
        _authState = new AuthState
        {
            IsAuthenticated = true,
            Token = response.Token,
            RefreshToken = response.RefreshToken,
            TokenExpiry = tokenExpiry,
            Email = email,
            Claims = ParseJwtClaims(response.Token)
        };

        // Store tokens securely
        await localStorage.SetItemAsStringAsync("auth_token", response.Token);
        await localStorage.SetItemAsync("refresh_token", response.RefreshToken);
        await localStorage.SetItemAsync("token_expiry", tokenExpiry);
        await localStorage.SetItemAsStringAsync("user_email", email!);
        
        // Clean up pending email
        await localStorage.RemoveItemAsync("pending_email");

        // Configure HTTP client for authenticated requests
        httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", response.Token);

        AuthStateChanged.Invoke(_authState);
    }
    
    private static Dictionary<string, object> ParseJwtClaims(string token)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length != 3)
            {
                return new Dictionary<string, object>();
            }

            var payload = parts[1];
            
            // Add padding if needed
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
            }

            var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
            return JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? new Dictionary<string, object>();
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }
}