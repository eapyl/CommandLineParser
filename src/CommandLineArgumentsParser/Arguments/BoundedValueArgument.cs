using System;
using CommandLineParser.Compatiblity;
using CommandLineParser.Exceptions;

namespace CommandLineParser.Arguments
{
    /// <summary>
    /// Use BoundedValueArgument for an argument whose value must belong to an interval.
    /// </summary>
    /// <typeparam name="TValue">Type of the value, must support comparison</typeparam>
    public class BoundedValueArgument<TValue> : CertifiedValueArgument<TValue>
        where TValue : IComparable
    {
        #region property backing fields

        private TValue _minValue;

        private TValue _maxValue;

        #endregion

        #region constructor

        /// <summary>
        /// Creates new value argument with a <see cref="Argument.ShortName">short name</see>,
        /// <see cref="Argument.LongName">long name</see> and <see cref="Argument.Description">description</see> and specified minimal and maximal value. 
        /// </summary>
        /// <param name="shortName">Short name of the argument</param>
        /// <param name="longName">Long name of the argument </param>
        /// <param name="description">Description of the argument</param>
        /// <param name="minValue">Minimal value of the argument</param>
        /// <param name="maxValue">Maximal value of the argument</param>
        public BoundedValueArgument(char? shortName = null, string longName = null, string description = null, TValue minValue = default(TValue), TValue maxValue = default(TValue))
            : base(shortName, longName, description)
        {
            _maxValue = maxValue;
            UseMaxValue = !Equals(maxValue, default(TValue));
            _minValue = minValue;
            UseMinValue = !Equals(minValue, default(TValue)); ;
        }
        
        #endregion 

        #region properties 

        /// <summary>
        /// Minimal allowed value (inclusive)
        /// </summary>
        public TValue MinValue
        {
            get { return _minValue; }
            set
            {
                _minValue = value;
                UseMinValue = true;
            }
        }

        /// <summary>
        /// Maximal allowed value (inclusive) 
        /// </summary>
        public TValue MaxValue
        {
            get { return _maxValue; }
            set
            {
                _maxValue = value;
                UseMaxValue = true;
            }
        }

        /// <summary>
        /// When set to true, value is checked for being greater than or equal to <see cref="MinValue"/>
        /// </summary>
        public bool UseMinValue { get; set; }

        /// <summary>
        /// When set to true, value is checked for being lesser than or equal to <see cref="MaxValue"/>
        /// </summary>
        public bool UseMaxValue { get; set; }

        #endregion
        
        /// <summary>
        /// Checks whether the value belongs to the [<see cref="MinValue"/>, <see cref="MaxValue"/>] interval
        /// (when <see cref="UseMinValue"/> and <see cref="UseMaxValue"/> are set).
        /// </summary>
        /// <param name="value">value to certify</param>
        /// <exception cref="CommandLineArgumentOutOfRangeException">Thrown when <paramref name="value"/> lies outside the interval. </exception>
        protected override void Certify(TValue value)
        {
            if (UseMinValue && MinValue.CompareTo(value) == 1)
            {
                throw new CommandLineArgumentOutOfRangeException(
                    string.Format(Messages.EXC_ARG_BOUNDED_LESSER_THAN_MIN, value, _minValue), Name);
            }

            if (UseMaxValue && MaxValue.CompareTo(value) == -1)
            {
                throw new CommandLineArgumentOutOfRangeException(
                    string.Format(Messages.EXC_ARG_BOUNDED_GREATER_THAN_MAX, value, _maxValue), Name);
            }
        }
    }

    /// <summary>
    /// <para>
    /// Attribute for declaring a class' field a <see cref="BoundedValueArgument{TValue}"/> and 
    /// thus binding a field's value to a certain command line switch argument.
    /// </para>
    /// <para>
    /// Instead of creating an argument explicitly, you can assign a class' field an argument
    /// attribute and let the CommandLineParse take care of binding the attribute to the field.
    /// </para>
    /// </summary>
    /// <remarks>Appliable to fields and properties (public).</remarks>
    /// <remarks>Use <see cref="CommandLineParser.ExtractArgumentAttributes"/> for each object 
    /// you where you have delcared argument attributes.</remarks>
    public class BoundedValueArgumentAttribute : ArgumentAttribute
    {
        /// <summary>
        /// Creates new instance of BoundedValueArgument. BoundedValueArgument
        /// uses underlying <see cref="BoundedValueArgument{TValue}"/>.
        /// </summary>
        /// <param name="type">Type of the generic parameter of <see cref="BoundedValueArgument{TValue}"/>.</param>
        /// <remarks>
        /// Parameter <paramref name="type"/> has to be either built-in 
        /// type or has to define a static Parse(String, CultureInfo) 
        /// method for reading the value from string.
        /// </remarks>
        public BoundedValueArgumentAttribute(Type type)
            : base(typeof(BoundedValueArgument<>).MakeGenericType(type)) { }

        /// <summary>
        /// Creates new instance of BoundedValueArgument. BoundedValueArgument
        /// uses underlying <see cref="BoundedValueArgument{TValue}"/>.
        /// </summary>        
        /// <remarks>
        /// TValue will be inferred from the field/property where the argument is applied.
        /// TValue has to be either built-in 
        /// type or has to define a static Parse(String, CultureInfo) 
        /// method for reading the value from string.
        /// </remarks>        
        public BoundedValueArgumentAttribute(): base(typeof(LazyArgument))
        {
            ((LazyArgument)Argument).GenericArgumentType = typeof(BoundedValueArgument<>);
        }

        /// <summary>
        /// Maximal allowed value (inclusive) 
        /// </summary>
        public object MaxValue
        {
            get
            {
                return _underlyingArgumentType.GetPropertyValue<object>("MaxValue", Argument);
            }
            set
            {
                _underlyingArgumentType.SetPropertyValue("MaxValue", Argument, value);
            }
        }

        /// <summary>
        /// Minimal allowed value (inclusive)
        /// </summary>
        public object MinValue
        {
            get
            {
                return _underlyingArgumentType.GetPropertyValue<object>("MinValue", Argument);
            }
            set
            {
                _underlyingArgumentType.SetPropertyValue("MinValue", Argument, value);
            }
        }

        /// <summary>
        /// When set to true, value is checked for being lesser than or equal to <see cref="MaxValue"/>
        /// </summary>
        public bool UseMaxValue
        {
            get
            {
                return _underlyingArgumentType.GetPropertyValue<bool>("UseMaxValue", Argument);
            }
            set
            {
                _underlyingArgumentType.SetPropertyValue("UseMaxValue", Argument, value);
            }
        }

        /// <summary>
        /// When set to true, value is checked for being greater than or equal to <see cref="MinValue"/>
        /// </summary>
        public bool UseMinValue
        {
            get
            {
                return _underlyingArgumentType.GetPropertyValue<bool>("UseMinValue", Argument);
            }
            set
            {
                _underlyingArgumentType.SetPropertyValue("UseMinValue", Argument, value);
            }
        }

        /// <summary>
        /// Default value
        /// </summary>
        public object DefaultValue
        {
            get
            {
                return _underlyingArgumentType.GetPropertyValue<object>("DefaultValue", Argument);
            }
            set
            {
                _underlyingArgumentType.SetPropertyValue("DefaultValue", Argument, value);
            }
        }

        /// <summary>
        /// When set to true, argument can appear on the command line with or without value, e.g. both is allowed: 
        /// <code>
        /// myexe.exe -Arg Value
        /// OR
        /// myexe.exe -Arg
        /// </code>
        /// </summary>
        public bool ValueOptional
        {
            get
            {
                return _underlyingArgumentType.GetPropertyValue<bool>("ValueOptional", Argument);
            }
            set
            {
                _underlyingArgumentType.SetPropertyValue("ValueOptional", Argument, value);
            }
        }
    }
}