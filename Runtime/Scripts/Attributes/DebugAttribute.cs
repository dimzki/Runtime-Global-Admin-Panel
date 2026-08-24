using System;

namespace Alzaki.GlobalSettings
{
    /// <summary>
    /// Attribute to expose methods as clickable buttons in the inspector.
    /// Works with or without Odin Inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class DebugAttribute : Attribute { }
}
