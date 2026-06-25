using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchTemplate.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController(ISender sender) : ControllerBase
{
    protected ISender Sender { get; } = sender;
}
