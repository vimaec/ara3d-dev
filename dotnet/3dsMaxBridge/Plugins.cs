/*
    Ara 3D Geometry Plugin
    Copyright 2018, Ara 3D, Inc.
    Usage licensed under terms of MIT Licenese
*/
namespace Ara3D
{
    public interface IPluginDescriptor
    {
        string Name { get; }
        string Author { get; }
        string Url { get; }
        string Tooltip { get; }
        string Description { get; }
        int MajorVersion { get; }
        int MinorVersion { get; }
        int BuildVersion { get; }
        IParameterDescriptor[] Parameters { get; }
    }

    public interface IParameterDescriptor
    {
        IPluginDescriptor Descriptor { get; }
        string Label { get; }
        string Tooltip { get; }
        string Guid { get; }
        string Name { get; }
        string Description{ get; }
        int Index { get; }
        string Help { get; }
        object Default { get; }
    }

    public interface IGeometryPlugin
    {
        IGeometry Evaluate(IGeometryPluginHost host);
    }

    public interface IParameter
    {
        object Value { get; }
        IParameterDescriptor Descriptor { get; }
    }

    public interface IGeometryPluginHost
    {
        object[] Args { get; }
        int Time { get; }
        int LastTime { get; }
        IGeometry LastValue { get; }
    }

    public interface IUtilityPlugin
    {
        void Evaluate();
    }
}
