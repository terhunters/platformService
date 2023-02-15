using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using WebApplication3.Data;
using WebApplication3.DTOs;

namespace WebApplication3.AsyncDataServices.SignalR
{
    public class ServerHub : Hub
    {
        private readonly IPlatformRepo _repository;
        private readonly IMapper _mapper;

        public ServerHub(IPlatformRepo repo, IMapper mapper)
        {
            _repository = repo;
            _mapper = mapper;
        }
        
        public async Task GetAllPlatforms()
        {
            await Clients.Caller.SendAsync("ReceivePlatforms", _mapper.Map<IEnumerable<PlatformReadDto>>(_repository.GetAllPlatforms()));
        }

        public async Task SendNewPlatformToAll(PlatformReadDto platform)
        {
            await Clients.All.SendAsync("CreatedNewPlatform", platform);
        }

        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"{Context.ConnectionId} connected to hub");
            await Clients.Caller.SendAsync("Notify", "Connected");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            Console.WriteLine($"{Context.ConnectionId} disconnected");
            await Clients.Caller.SendAsync("Notify", "Disconnected");
            await base.OnDisconnectedAsync(exception);
        }
    }
}