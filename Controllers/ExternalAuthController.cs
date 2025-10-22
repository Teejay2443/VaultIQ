//using Microsoft.AspNetCore.Authentication;
//using Microsoft.AspNetCore.Authentication.Google;
//using Microsoft.AspNetCore.Mvc;

//namespace VaultIQ.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class ExternalAuthController : ControllerBase
//    {
//        [HttpGet("google-login")]
//        public IActionResult GoogleLogin()
//        {
//            var redirectUrl = Url.Action(nameof(GoogleResponse));
//            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
//            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
//        }

//        [HttpGet("google-response")]
//        public async Task<IActionResult> GoogleResponse()
//        {
//            var result = await HttpContext.AuthenticateAsync();
//            if (!result.Succeeded)
//                return BadRequest("Google authentication failed.");

//            var claims = result.Principal.Identities.FirstOrDefault()?.Claims
//                .Select(c => new { c.Type, c.Value });

//            return Ok(claims);
//        }
//    }
//}
