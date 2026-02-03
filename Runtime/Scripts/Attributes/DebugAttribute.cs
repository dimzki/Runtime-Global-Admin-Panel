using System;

#if ODIN_INSPECTOR
// If Odin is present, this attribute simply becomes an alias for Odin's own ButtonAttribute.
// This is the cleanest and most compatible way to handle buttons.
using Sirenix.OdinInspector;
public class DebugAttribute : ButtonAttribute { }
#else
// If Odin is NOT present, this is a standard attribute for our fallback editor to find.
[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public class DebugAttribute : Attribute { }
#endif