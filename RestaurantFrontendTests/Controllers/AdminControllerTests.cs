using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ResturangFrontEnd.Controllers;
using ResturangFrontEnd.Models;
using Restaurant_Frontend_Tests.Helpers;

namespace Restaurant_Frontend_Tests.Controllers
{
    public class AdminControllerTests
    {
        [Fact]
        public void Login_Get_ReturnsViewWithLoginModel()
        {
            var controller = CreateController();

            var result = controller.Login();

            var view = result.Should().BeOfType<ViewResult>().Subject;
            controller.ViewData["Title"].Should().Be("Admin Login");
            view.Model.Should().BeOfType<Login>();
        }

        [Fact]
        public async Task Login_Post_InvalidModel_ReturnsViewWithModel()
        {
            var controller = CreateController();
            controller.ModelState.AddModelError("Username", "Required");

            var login = new Login { Username = "", Password = "" };

            var result = await controller.Login(login);

            var view = result.Should().BeOfType<ViewResult>().Subject;
            view.Model.Should().BeSameAs(login);
        }

        [Fact]
        public async Task Login_Post_InvalidCredentials_ReturnsViewWithError()
        {
            var handler = new DelegatingHandlerStub((request, _) =>
            {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            });

            var controller = CreateController(handler);

            var login = new Login { Username = "admin", Password = "wrongpassword" };

            var result = await controller.Login(login);

            result.Should().BeOfType<ViewResult>();
            controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Login_Post_ValidCredentials_PasswordNotChanged_RedirectsToChangePassword()
        {
            var handler = new DelegatingHandlerStub((request, _) =>
            {
                var response = new { Token = "test-token", ChangedPassword = false };
                var json = JsonConvert.SerializeObject(response);
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            });

            var controller = CreateController(handler);
            SetupControllerContext(controller);

            var login = new Login { Username = "admin", Password = "admin123" };

            var result = await controller.Login(login);

            var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("ChangePassword");
        }

        [Fact]
        public async Task Login_Post_ValidCredentials_PasswordChanged_RedirectsToIndex()
        {
            var handler = new DelegatingHandlerStub((request, _) =>
            {
                var response = new { Token = "test-token", ChangedPassword = true };
                var json = JsonConvert.SerializeObject(response);
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            });

            var controller = CreateController(handler);
            SetupControllerContext(controller);

            var login = new Login { Username = "admin", Password = "newpassword" };

            var result = await controller.Login(login);

            var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
        }

        [Fact]
        public void ChangePassword_Get_ReturnsViewWithModel()
        {
            var controller = CreateController();

            var result = controller.ChangePassword();

            var view = result.Should().BeOfType<ViewResult>().Subject;
            controller.ViewData["Title"].Should().Be("Change Password");
            view.Model.Should().BeOfType<ChangePassword>();
        }

        [Fact]
        public async Task ChangePassword_Post_InvalidModel_ReturnsViewWithModel()
        {
            var controller = CreateController();
            SetupControllerContext(controller);
            controller.ModelState.AddModelError("NewPassword", "Required");

            var model = new ChangePassword { NewPassword = "", ConfirmPassword = "" };

            var result = await controller.ChangePassword(model);

            var view = result.Should().BeOfType<ViewResult>().Subject;
            view.Model.Should().BeSameAs(model);
        }

        [Fact]
        public async Task ChangePassword_Post_NoToken_RedirectsToLogin()
        {
            var controller = CreateController();
            SetupControllerContext(controller);

            var model = new ChangePassword { NewPassword = "newpass123", ConfirmPassword = "newpass123" };

            var result = await controller.ChangePassword(model);

            var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Login");
        }

        [Fact]
        public async Task ChangePassword_Post_Success_RedirectsToLogin()
        {
            var handler = new DelegatingHandlerStub((request, _) =>
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("\"Password changed successfully.\"", Encoding.UTF8, "application/json")
                };
            });

            var controller = CreateController(handler);
            SetupControllerContextWithToken(controller, "test-token");

            var model = new ChangePassword { NewPassword = "newpass123", ConfirmPassword = "newpass123" };

            var result = await controller.ChangePassword(model);

            var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Login");
        }

        [Fact]
        public void Logout_DeletesCookieAndRedirectsToLogin()
        {
            var controller = CreateController();
            SetupControllerContext(controller);

            var result = controller.Logout();

            var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Login");
        }

        private static AdminController CreateController(HttpMessageHandler? handler = null)
        {
            var httpClient = handler is null ? new HttpClient() : new HttpClient(handler);
            return new AdminController(httpClient);
        }

        private static void SetupControllerContext(AdminController controller)
        {
            var httpContext = new DefaultHttpContext();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            controller.TempData = new FakeTempDataDictionary();
        }

        private static void SetupControllerContextWithToken(AdminController controller, string token)
        {
            var httpContext = new DefaultHttpContext();

            // Populate Request.Cookies by providing a Cookie header.
            httpContext.Request.Headers.Cookie = $"jwt={token}";

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            controller.TempData = new FakeTempDataDictionary();
        }

        private sealed class FakeTempDataDictionary : Dictionary<string, object?>, Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataDictionary
        {
            public void Keep() { }
            public void Keep(string key) { }
            public void Load() { }
            public object? Peek(string key) => TryGetValue(key, out var value) ? value : null;
            public void Save() { }
        }
    }
}

