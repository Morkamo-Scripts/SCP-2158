using Exiled.API.Interfaces;

namespace SCP_2158
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
    }
}