using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Grpc.Net.Client;
using MyBGList.gRPC;


namespace MyBGList.Controllers;

[Route("[controller]/[action]")]
[ApiController]
public class GrpcController : ControllerBase
{
	[HttpGet("{id}")]
	public async Task<BoardGameResponce> GetBoardGame(int id)
	{
		using var channel = GrpcChannel.ForAddress("https://localhost:40443");

		var client = new gRPC.Grpc.GrpcClient(channel);

		var responce = await client.GetBoardGameAsync(new BoardGameRequest { Id = id});

		return responce;
	}
}
