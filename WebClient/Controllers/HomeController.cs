using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using WebClient.Models;
using WebClient.Services;

namespace WebClient.Controllers
{
    public class HomeController : Controller
    {
        private readonly ITokenService _tokenService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ITokenService tokenService ,ILogger<HomeController> logger)
        {
            _tokenService = tokenService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
        [Authorize]
        public async Task<IActionResult> Users()
        {
            using var client = new HttpClient();

            // for client credentials flow
            //var token = await _tokenService.GetToken("OnlineStore.read");
            
            // for code flow, getting the token from user request
            var token = await HttpContext.GetTokenAsync("access_token");
            var idToken = await HttpContext.GetTokenAsync("id_token");
            //var refreshToken = await HttpContext.GetTokenAsync("refresh_token");

            //var claims = User.Claims.ToList();
            // return jwt format of the token
            var _accessToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
            
            var _idToken = new JwtSecurityTokenHandler().ReadJwtToken(idToken);
            //getting claims from userInfo endpoint
            var claim = User.Claims.ToList();
            //Set the Auth header 
            
            client.SetBearerToken(token);

            var result = await client.GetAsync("https://localhost:5001/api/users");

            if (result.IsSuccessStatusCode)
            {
                var model = await result.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<List<User>>(model);
                return View(data);
            }

            throw new Exception("Unable to get data.");


        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}