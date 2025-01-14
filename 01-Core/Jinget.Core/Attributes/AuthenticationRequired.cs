﻿using System;

namespace Jinget.Core.Attributes
{
    /// <summary>
    /// This attribute are useful in scenarios like Service Hub, where prior to calling an API we need to authenticate it
    /// </summary>
    public class AuthenticationRequiredAttribute(bool required) : Attribute
    {
        public bool Required { get; set; } = required;
    }
}
