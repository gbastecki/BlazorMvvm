using System;

namespace BlazorMvvm
{
    [AttributeUsage(AttributeTargets.Field)]
    public class BlazorObservablePropertyAttribute : Attribute
    {
        /// <summary>
        /// Optional custom name for the generated property.
        /// If null, the property name is derived from the field name.
        /// The derived field naming conventions supported are: `_camelCase`, `m_camelCase`, or `camelCase`.
        /// </summary>
        public string? Name { get; set; }
    }
}