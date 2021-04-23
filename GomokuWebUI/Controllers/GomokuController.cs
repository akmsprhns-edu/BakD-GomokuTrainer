using GomokuWebUI.Dtos.GomokuController;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GomokuWebUI.Controllers
{
    [ApiController]
    [Route("Api/[controller]")]
    public class GomokuController : ControllerBase
    {
        private readonly ILogger<GomokuController> _logger;
        private readonly Dictionary<string, PvEGameSession> _gameSessions;

        public GomokuController(ILogger<GomokuController> logger, Dictionary<string, PvEGameSession> gameSessions)
        {
            _logger = logger;
            _gameSessions = gameSessions;
        }

        [HttpPost("[action]")]
        public IActionResult NewGame()
        {
            var newGameSession = new PvEGameSession(Guid.NewGuid().ToString(), AIType.PureMonteCarlo);
            _gameSessions[newGameSession.Guid] = newGameSession;

            return Ok(GetModelJson(newGameSession));
        }

        [HttpPost("[action]")]
        public IActionResult MakeMove([FromBody] MoveRequest move)
        {
            if (!_gameSessions.TryGetValue(move.GameSessionGuid, out var gameSession))
            {
                return BadRequest($"Game session with guid {move.GameSessionGuid} not found");
            };

            gameSession.MakePlayerMove(move.Row, move.Column);
            var gameResult = gameSession.GameState.IsGameOver();
            if (gameResult == null)
            {
                gameSession.MakeComputerMove();
            }

            return Ok(GetModelJson(gameSession));
        }

        private string GetModelJson(PvEGameSession gameSession)
        {
            var gameSessionResponse = new GameSessionStateResponse()
            {
                Guid = gameSession.Guid,
                Board = gameSession.GameState.Get2DArrary(),
                GameResult = gameSession.GameState.IsGameOver()
            };

            return JsonConvert.SerializeObject(gameSessionResponse, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        }
    }
}
