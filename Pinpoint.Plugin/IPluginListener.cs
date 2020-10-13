﻿namespace Pinpoint.Plugin
{
    public interface IPluginListener<in Plugin, in Target> where Plugin : IPlugin
    {
        void PluginChange_Added(object sender, Plugin plugin, Target target);

        void PluginChange_Removed(object sender, Plugin plugin, Target target);
    }
}
