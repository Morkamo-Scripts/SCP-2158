using Exiled.API.Interfaces;
using SCP_2158.Handlers;

namespace SCP_2158
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;

        public Scp2158Handler Scp2158 { get; set; } = new();
    }
}