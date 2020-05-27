using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Host.Web.Mvc.Models;
using PulseCheck.Constants;
using IdentityServer3.Core.Extensions;
using Microsoft.Owin;
using System.Net.Http;
using IdentityModel.Client;
using System.Text;
using Newtonsoft.Json;
using Host.Configuration;
using PulseCheck.Domain;
using PulseCheck.ILogic;
using PulseCheck.Logic;

namespace Host.Web.Mvc.Controllers
{
    public class LoginController : Controller
    {
        private readonly UserAccountManager _userAccountSvc;
        private readonly ISiteManager _siteSvc;
        private readonly IUserManager _userService;

        public LoginController(UserAccountManager userAccountSvc, ISiteManager siteSvc, IUserManager userService)
        {
            _userAccountSvc = userAccountSvc;
            _siteSvc = siteSvc;
            _userService = userService;
        }

        [Route("core/custom/login")]
        public ActionResult Index(string id)
        {
            var model = new LoginModel()
            {
                Id = id
            };

            return View("Index", model);
        }

        [Route("core/custom/login")]
        [HttpPost]
        public async Task<ActionResult> Index(string id, string username, string password)
        {
            var env = Request.GetOwinContext().Environment;
            var context = new OwinContext(env);

            if (_userAccountSvc.Authenticate(username, password))
            {
                var user = _userAccountSvc.GetByUsername(username);

                //Check Claims
                var userIdClaims =
                    user.Claims.Where(
                        x =>
                            string.Equals(x.Type, PulseCheckClaims.PulseCheckUserId,
                                StringComparison.CurrentCultureIgnoreCase)).ToList();
                
                // TODO: This probably doesn't need to be here anymore.
                if (userIdClaims.Count == 1)
                {
                    var token = await GetUserTokenAsync(username, password);

                    if (token.IsError) return Redirect("http://www.google.com");

                    var claims = await GetClaimsAsync(token);

                    var cookieId = new ClaimsIdentity(claims, "Cookies");
                    Request.GetOwinContext().Authentication.SignIn(cookieId);

                    return Redirect("https://localhost:44318/api/v1/sites");
                }

                // TODO: This blows up with a "No Partial Login" error.
                else if (userIdClaims.Count > 1)
                {/*
                    var claimsToAdd = new List<Claim>
                    {
                        new Claim("sub", user.ID.ToString()),
                        new Claim("name", user.Username)
                    };

                    await env.UpdatePartialLoginClaimsAsync(claimsToAdd);

                    //Else redirect to page where the user can select which user to login as.    
                    return Redirect("~/core/custom/sites?id=" + id); */
                }

                // TODO: What do we do when the user has no ID claims? Just fall through to "Invalid Login"?
                else
                {

                }
            }

            var model = new LoginModel()
            {
                Id = id
            };

            ModelState.AddModelError("", "Invalid Login");

            return View("Index", model);
        }


        [Route("core/custom/sites")]
        public async Task<ActionResult> Sites(string id)
        {
            var env = Request.GetOwinContext().Environment;
            var context = new OwinContext(env);
            var partial = await env.GetIdentityServerPartialLoginAsync();
            if (partial == null || !partial.IsAuthenticated)
                return Redirect("~/core/custom/login?id=" + id);

            var username =
                partial.Claims.FirstOrDefault(
                    x => string.Equals(x.Type, "name", StringComparison.CurrentCultureIgnoreCase));

            if (string.IsNullOrWhiteSpace(username.Value))
                return Redirect("~/core/custom/login?id=" + id);

            var user = _userAccountSvc.GetByUsername(username.Value);
            var sites = await _siteSvc.GetSitesBySubjectIdAsync(user.ID.ToString());

            var model = new Models.Sites()
            {
                Id = id,
                AvailableSites = sites
            };

            return View("Sites", model);
        }

        [Route("core/custom/sites")]
        [HttpPost]
        public async Task<ActionResult> Sites(string id, byte siteId)
        {
            var env = Request.GetOwinContext().Environment;
            var context = new OwinContext(env);

            var partial = await env.GetIdentityServerPartialLoginAsync();


            if (partial == null || !partial.IsAuthenticated)
                return Redirect("~/core/custom/login?id=" + id);

            var username =
                partial.Claims.FirstOrDefault(
                    x => string.Equals(x.Type, "name", StringComparison.CurrentCultureIgnoreCase));

            if (string.IsNullOrWhiteSpace(username.Value))
                return Redirect("~/core/custom/login?id=" + id);

            var user = _userAccountSvc.GetByUsername(username.Value);
            var sites = await _siteSvc.GetSitesBySubjectIdAsync(user.ID.ToString());
            
            var site = sites.FirstOrDefault(s => s.Id == siteId);

            if (site != null)
            {
                //Get the User.Id that contains the SiteId
                int dummy;
                var userIds =
                    user.Claims.Where(
                        x =>
                            string.Equals(x.Type, PulseCheckClaims.PulseCheckUserId,
                                StringComparison.CurrentCultureIgnoreCase))
                        .Where(y => int.TryParse(y.Value, out dummy))
                        .Select(z => int.Parse(z.Value))
                        .ToList();

                var u = (await _userService.GetUsersByIdAsync(userIds)).FirstOrDefault(x => x.SiteId == siteId);

                var claimsToAdd = new List<Claim>
                    {
                        new Claim("sub", user.ID.ToString()),
                        new Claim("name", user.Username),
                        new Claim("idp", "idsrv"),
                        new Claim("auth_time", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
                        new Claim("amr", IdentityServer3.Core.Constants.AuthenticationMethods.Password),
                    };

                if (u != null)
                    claimsToAdd.Add(new Claim(PulseCheckClaims.PulseCheckUserId, u?.Id.ToString()));

                await env.UpdatePartialLoginClaimsAsync(claimsToAdd);

                var resumeUrl = await env.GetPartialLoginResumeUrlAsync();            
                return Redirect(resumeUrl);
            }

            var model = new Models.Sites()
            {
                Id = id,
                AvailableSites = sites,               
            };

            ModelState.AddModelError("", "There was an error.");
            return View("Sites", model);
        }

        private async Task<IEnumerable<Claim>> GetClaimsAsync(IdentityModel.Client.TokenResponse token)
        {
            var result = new List<Claim>();
            
            var client = new UserInfoClient(string.Format(Endpoints.UserInfoEndpoint, Addresses.GetIDServerBaseAddress()));
            var userInfo = await client.GetAsync(token.AccessToken);

            userInfo.Claims.ToList().ForEach(ui => result.Add(new Claim(ui.Type, ui.Value)));
            result.Add(new Claim("token", token.AccessToken));

            return result;
        }

        public async Task<IdentityModel.Client.TokenResponse> GetUserTokenAsync(string username, string password)
        {
            var credentials = new AuthCredentials();
            credentials.UserName = username;
            credentials.Password = password;

            var client = new HttpClient();

            var result = await client.PostAsync(Addresses.GetAPIBaseAddress() + "api/auth/login", 
                new StringContent(JsonConvert.SerializeObject(credentials), Encoding.UTF8, "application/json")
            );
            var json = await result.Content.ReadAsStringAsync();

            return new IdentityModel.Client.TokenResponse(json);
        }
    }
}