﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Exceptions
{
    internal class CategoriesLoaderException : Exception
    {
        public CategoriesLoaderException(string message) : base(message)
        { }
    }
}
