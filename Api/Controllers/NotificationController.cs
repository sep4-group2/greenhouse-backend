using Api.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController: ControllerBase
{
    //Two endpoints needed 
    //One for fetching the public VAPID key
    //One for saving subscriptions

    
    //Endpoint for clients to fetch the public VAPID key needed to subscribe
    [HttpGet]
    public IActionResult GetPublicKey()
    {
        //The public key needs to be fetched from wherever it is stored
        //TO BE IMPLEMENTED
        var publicKey = new VAPIDKeyDTO();
        return Ok(publicKey);
    }
}