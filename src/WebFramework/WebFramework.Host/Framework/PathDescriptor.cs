using System.Reflection;

namespace WebFramework.Host.Framework;

public record PathDescriptor(string[] PathElements, HttpMethod HttpMethod, Type ControllerType, MethodInfo MethodInfo);