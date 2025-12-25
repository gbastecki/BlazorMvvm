using System;

namespace BlazorMvvm;

/// <summary>
/// Attribute that enables source generation for messenger registration.
/// When applied to a class that implements <see cref="IBlazorRecipient{TMessage}"/>,
/// the source generator will create RegisterMessenger and UnregisterMessenger methods.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class BlazorMessengerAttribute : Attribute
{
}