using Microsoft.AspNetCore.Mvc;

namespace TicTacToe.Controllers
{
	public class TicTacToeController : Controller
	{
		[Route("/")]
		public IActionResult Index()
		{
			return View();
		}
		
	}
}
