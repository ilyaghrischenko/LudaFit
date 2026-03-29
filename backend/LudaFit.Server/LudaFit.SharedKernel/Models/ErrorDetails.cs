using System.Net;

namespace LudaFit.SharedKernel.Models;

public sealed record ErrorDetails(string ErrorMessage, HttpStatusCode StatusCode = HttpStatusCode.BadRequest);
