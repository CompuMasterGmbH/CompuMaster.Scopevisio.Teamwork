using CompuMaster.Scopevisio.CenterDeviceApi;
using NUnit.Framework;
using RestSharp;
using System;
using System.Net;

namespace CompuMaster.Scopevisio.Teamwork.AsyncTests
{
    public class ServerErrorDiagnosticTests
    {
        [TestCase(500), TestCase(502), TestCase(503)]
        public void ServerFailureRetainsStatusAndTransportCauseWithoutPrintingTheResponse(int status)
        {
            var cause = new InvalidOperationException("Isolated transport cause");
            var response = new RestResponse { StatusCode = (HttpStatusCode)status, ErrorException = cause,
                ResponseStatus = ResponseStatus.Completed, Content = "response-private-data" };
            var handler = new TeamworkClientErrorHandler("fixture", null);
            var error = Assert.Throws<WebException>((Action)(() => handler.ValidateResponse(response)));
            Assert.That(error.Message, Does.Contain("HTTP " + status));
            Assert.That(error.Data["CompuMaster.Scopevisio.Teamwork.HttpStatusCode"], Is.EqualTo(status));
            Assert.That(error.Data["CompuMaster.Scopevisio.Teamwork.ResponseStatus"], Is.EqualTo("Completed"));
            Assert.That(error.InnerException, Is.SameAs(cause));
            Assert.That(error.Message, Does.Not.Contain("response-private-data"));
            Assert.That(error.Status, Is.EqualTo(WebExceptionStatus.UnknownError));
        }

        [Test]
        public void RequestValidationFailureRemainsOwnedByTheSdkStatusHandler()
        {
            var handler = new TeamworkClientErrorHandler("fixture", null);
            Assert.DoesNotThrow((Action)(() => handler.ValidateResponse(new RestResponse { StatusCode = HttpStatusCode.BadRequest })));
        }
    }
}
