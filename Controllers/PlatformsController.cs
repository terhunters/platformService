using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WebApplication3.AsyncDataServices;
using WebApplication3.AsyncDataServices.SignalR;
using WebApplication3.Data;
using WebApplication3.DTOs;
using WebApplication3.Models;
using WebApplication3.SyncDataServices.http;

namespace WebApplication3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlatformsController : ControllerBase
    {
        private readonly ICommandDataClients _commandDataClient;
        private readonly IMapper _mapper;
        private readonly IMessageBusClient _messageBusClient;
        private readonly IPlatformRepo _repository;
        private readonly IHubContext<ServerHub> _serverHubContext;

        public PlatformsController(
            IPlatformRepo repository,
            IMapper mapper,
            ICommandDataClients commandDataClients,
            IMessageBusClient messageBusClient,
            IHubContext<ServerHub> serverHubContext)
        {
            Console.WriteLine("constructor");
            _mapper = mapper;
            _repository = repository;
            _commandDataClient = commandDataClients;
            _messageBusClient = messageBusClient;
            _serverHubContext = serverHubContext;
        }

        [HttpGet]
        public ActionResult<IEnumerable<PlatformReadDto>> GetPlatforms()
        {
            Console.WriteLine("Getting platforms");
            return Ok(
                _mapper.Map<IEnumerable<PlatformReadDto>>(
                    _repository.GetAllPlatforms())
            );
        }

        [HttpGet("{id}", Name = "GetPlatformById")]
        public ActionResult<PlatformReadDto> GetPlatformById(int id)
        {
            Console.WriteLine("Getting platform by id");
            var result = _repository.GetPlatformById(id);
            if (result != null) return Ok(_mapper.Map<PlatformReadDto>(result));

            return NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<PlatformReadDto>> CreatePlatform(PlatformCreateDto platform)
        {
            Console.WriteLine("Create platform");
            var platformModel = _mapper.Map<Platform>(platform);
            _repository.CreateNewPlatform(platformModel);
            if (_repository.SaveChanges())
            {
                var resultDto = _mapper.Map<PlatformReadDto>(platformModel);

                try
                {
                    await _commandDataClient.SendPlatformToCommand(resultDto);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"--> Could not send synchronously: {ex.Message}");
                }

                try
                {
                    var publishedDto = _mapper.Map<PlatformPublishedDto>(resultDto);
                    publishedDto.Event = "Platform_Published";
                    _messageBusClient.PublishNewPlatform(publishedDto);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"--> Could not send asynchronously: {ex.Message}");
                }
                
                try
                {
                    Console.WriteLine("SignalR send to all clients ");
                    Console.WriteLine($"{_serverHubContext.Clients.All.ToString()}");
                    await _serverHubContext.Clients.All.SendAsync("CreatedNewPlatform", platform).ConfigureAwait(false);
                    Console.WriteLine("Finish sending");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"--> Could not send asynchronously SignalR: {ex.Message}");
                }

                return CreatedAtRoute(nameof(GetPlatformById), new { resultDto.Id }, resultDto);
            }

            return StatusCode(500);
        }
    }
}