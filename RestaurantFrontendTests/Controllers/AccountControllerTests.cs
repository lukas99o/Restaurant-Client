using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ResturangFrontEnd.Controllers;
using ResturangFrontEnd.Models;
using Restaurant_Frontend_Tests.Helpers;

namespace Restaurant_Frontend_Tests.Controllers
{
    public class AccountControllerTests
    {
        [Fact]
        public async Task Login_WithValidCredentials_RedirectsAndSetsCookie()
        {
            var admin = new Customer
            {
                Email = "admin@example.com",
                Password = "secret",
                Name = "Admin",
                CustomerID = 1
            };

            var handler = new DelegatingHandlerStub((request, _) =>
            {
                request.RequestUri!.AbsoluteUri.Should().Contain("api/Customers");

                var json = JsonConvert.SerializeObject(new List<Customer> { admin });
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            });

            var httpClient = new HttpClient(handler);

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "JwtKey", "supersecretkeyvalue1234567890123456" }
                })
                .Build();

            var controller = new AccountController(httpClient, configuration)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            var result = await controller.Login(admin.Email, admin.Password);

            var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            redirect.ControllerName.Should().Be("Home");

            controller.HttpContext.Response.Headers["Set-Cookie"].ToString().Should().Contain("jwt=");
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsViewWithError()
        {
            var handler = new DelegatingHandlerStub((request, _) =>
            {
                request.RequestUri!.AbsoluteUri.Should().Contain("api/Customers");

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("[]", Encoding.UTF8, "application/json")
                };
            });

            var httpClient = new HttpClient(handler);

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "JwtKey", "supersecretkeyvalue1234567890123456" }
                })
                .Build();

            var controller = new AccountController(httpClient, configuration)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            var result = await controller.Login("wrong@example.com", "badpassword");

            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().BeNull();
            ((string)controller.ViewBag.Error).Should().Be("Fel användarnamn eller lösenord");
            controller.HttpContext.Response.Headers["Set-Cookie"].ToString().Should().BeEmpty();
        }

        [Fact]
        public async Task Login_WithWrongPassword_ReturnsViewWithError()
        {
            var admin = new Customer
            {
                Email = "admin@example.com",
                Password = "correct-password",
                Name = "Admin",
                CustomerID = 1
            };

            var handler = new DelegatingHandlerStub((request, _) =>
            {
                request.RequestUri!.AbsoluteUri.Should().Contain("api/Customers");

                var json = JsonConvert.SerializeObject(new List<Customer> { admin });
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            });

            var httpClient = new HttpClient(handler);

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "JwtKey", "supersecretkeyvalue1234567890123456" }
                })
                .Build();

            var controller = new AccountController(httpClient, configuration)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            var result = await controller.Login(admin.Email, "wrong-password");

            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().BeNull();
            ((string)controller.ViewBag.Error).Should().Be("Fel användarnamn eller lösenord");
            controller.HttpContext.Response.Headers["Set-Cookie"].ToString().Should().BeEmpty();
        }

        [Fact]
        public void LogOut_RemovesCookieAndRedirects()
        {
            var handler = new DelegatingHandlerStub((_, _) => new HttpResponseMessage(HttpStatusCode.OK));
            var httpClient = new HttpClient(handler);

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "JwtKey", "supersecretkeyvalue1234567890123456" }
                })
                .Build();

            var controller = new AccountController(httpClient, configuration)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            controller.HttpContext.Request.Headers["Cookie"] = "jwt=old-token";

            var result = controller.LogOut();

            var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            redirect.ControllerName.Should().Be("Home");

            controller.HttpContext.Response.Headers["Set-Cookie"].ToString().Should().Contain("jwt=");
            controller.HttpContext.Response.Headers["Set-Cookie"].ToString().Should().Contain("expires");
        }
    }
}
