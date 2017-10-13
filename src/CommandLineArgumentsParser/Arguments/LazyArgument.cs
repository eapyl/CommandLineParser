using System;
using System.Collections.Generic;

namespace CommandLineParser.Arguments
{
    internal class LazyArgument : Argument
    {
        internal ArgumentAttribute.ConstructorParameter[] ConstructorParams { get; set; }

        internal Dictionary<string, object> PropertyValues { get; private set; }

        internal Type GenericArgumentType { get; set; }

        public LazyArgument()
        {
            PropertyValues = new Dictionary<string, object>();
        }

        public override void PrintValueInfo()
        {
            throw new NotImplementedException();
        }

        public override void UpdateBoundObject()
        {
            throw new NotImplementedException();
        }
    }
}
