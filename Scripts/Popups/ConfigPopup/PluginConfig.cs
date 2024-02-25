using BepInEx;
using BepInEx.Configuration;

public class PluginConfig
{
    public string PluginName => BaseUnityPlugin.Info.Metadata.Name;
    public string PluginGUID => BaseUnityPlugin.Info.Metadata.GUID;
    public ConfigFile Config => BaseUnityPlugin.Config;

    private BaseUnityPlugin BaseUnityPlugin;
    
    public PluginConfig(BaseUnityPlugin baseUnityPlugin)
    {
        BaseUnityPlugin = baseUnityPlugin;

    }
}