using System;
using System.Collections.Generic;
using System.Linq;
using CommandLineParser.Compatiblity;
using CommandLineParser.Exceptions;

namespace CommandLineParser.Arguments
{
    /// <summary>
    /// Use EnumeratedValueArgument for an argument whose values must be from certain finite set 
    /// (see <see cref="AllowedValues"/>)
    /// </summary>
    /// <typeparam name="TValue">Type of the value</typeparam>
    public class EnumeratedValueArgument<TValue> : CertifiedValueArgument<TValue>
    {
        #region property backing fields

        private ICollection<TValue> _allowedValues;

        private bool _ignoreCase;

        #endregion
        
        #region constructor 

        /// <summary>
        /// Creates new command line argument with a <see cref="Argument.ShortName">short name</see>,
        /// <see cref="Argument.LongName">long name</see> and <see cref="Argument.Description">description</see>
        /// </summary>
        /// <param name="shortName">Short name of the argument</param>
        /// <param name="longName">Long name of the argument </param>
        /// <param name="description">Description of the argument</param>
        /// <param name="allowedValues">Allowed values</param>
        public EnumeratedValueArgument(char? shortName = null, string longName = null, string description = null, ICollection<TValue> allowedValues = null)
            : base(shortName, longName, description)
        {
            _allowedValues = allowedValues;
        }

        #endregion

        #region properties 

        /// <summary>
        /// Set of values that are allowed for the argument.
        /// </summary>
        public ICollection<TValue> AllowedValues
        {
            get { return _allowedValues; }
            set { _allowedValues = value; }
        }

        /// <summary>
        /// String arguments will be accepted even with differences in capitalisation (e.g. INFO will be accepted for info).
        /// </summary>
        public bool IgnoreCase
        {
            get { return _ignoreCase; }
            set
            {
                if (!(typeof(TValue) == typeof(string)) && value)
                {
                    throw new ArgumentException(string.Format("Ignore case can be used only for string arguments, type of TValue is {0}", typeof(TValue)));
                }
                _ignoreCase = value;
            }
        }

        #endregion

        #region methods

        /// <summary>
        /// Initilazes <see cref="AllowedValues"/> by a string of values separated by commas or semicolons.
        /// </summary>
        /// <param name="valuesString">Allowed values (separated by comas or semicolons)</param>
        public void InitAllowedValues(string valuesString)
        {
            string[] splitted = valuesString.Split(';', ',');
            TValue[] typedValues = new TValue[splitted.Length];
            int i = 0;
            foreach (string value in splitted)
            {
                typedValues[i] = Convert(value);
                i++;
            }
            AllowedValues = typedValues;
        }

        /// <summary>
        /// Checks whether the specified value belongs to 
        /// the set of <see cref="AllowedValues">allowed values</see>. 
        /// </summary>
        /// <param name="value">value to certify</param>
        /// <exception cref="CommandLineArgumentOutOfRangeException">thrown when <paramref name="value"/> does not belong to the set of allowed values.</exception>
        protected override void Certify(TValue value)
        {
            bool ok;
            if (IgnoreCase && typeof(TValue) == typeof(string) && value is string)
            {
                TValue found = _allowedValues.FirstOrDefault(av => StringComparer.CurrentCultureIgnoreCase.Compare(value.ToString(), av.ToString()) == 0);
                ok = found != null;
                if (ok)
                {
                    base.Value = found;
                }
            }
            else
            {
                ok = _allowedValues.Contains(value);
            }
            if (!ok)
            {
                throw new CommandLineArgumentOutOfRangeException(String.Format(Messages.EXC_ARG_ENUM_OUT_OF_RANGE, Value, Name), Name);
            }
        }

        #endregion
    }

    /// <summary>
    /// <para>
    /// Attribute for declaring a class' field a <see cref="EnumeratedValueArgument{TValue}"/> and 
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
    public sealed class EnumeratedValueArgumentAttribute : ArgumentAttribute
    {                
        /// <summary>
		/// Creates new instance of EnumeratedValueArgument. EnumeratedValueArgument
		/// uses underlying <see cref="EnumeratedValueArgument{TValue}"/>.
        /// </summary>
		/// <param name="type">Type of the generic parameter of <see cref="EnumeratedValueArgument{TValue}"/>.</param>
        /// <remarks>
        /// Parameter <paramref name="type"/> has to be either built-in 
        /// type or has to define a static Parse(String, CultureInfo) 
        /// method for reading the value from string.
        /// </remarks>
        public EnumeratedValueArgumentAttribute(Type type)
            : base(typeof(EnumeratedValueArgument<>).MakeGenericType(type)) { }

        /// <summary>
		/// Creates new instance of EnumeratedValueArgument. EnumeratedValueArgument
		/// uses underlying <see cref="EnumeratedValueArgument{TValue}"/>.
        /// </summary>		        
        /// <remarks>
        /// TValue will be inferred from the field/property where the argument is applied.
        /// TValue has to be either built-in 
        /// type or has to define a static Parse(String, CultureInfo) 
        /// method for reading the value from string.
        /// </remarks>    
        public EnumeratedValueArgumentAttribute(): base(typeof(LazyArgument))
        {
            ((LazyArgument)Argument).GenericArgumentType = typeof(EnumeratedValueArgument<>);
        }

        private string allowedValues;
        /// <summary>
        /// Allowed values of the argument, separated by commas or semicolons.
        /// </summary>
        public string AllowedValues
        {
            get
            {
                return allowedValues;
            }
            set
            {
                allowedValues = value;
                if (Argument is LazyArgument)
                {
                    _underlyingArgumentType.SetPropertyValue("AllowedValues", Argument, value);
                }
                else
                {
                    _underlyingArgumentType.InvokeMethod("InitAllowedValues", Argument, value);
                }
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

        /// <summary>
        /// String arguments will be accepted even with differences in capitalisation (e.g. INFO will be accepted for info).
        /// </summary>
        public bool IgnoreCase
        {
            get
            {
                return _underlyingArgumentType.GetPropertyValue<bool>("IgnoreCase", Argument);
            }
            set
            {
                _underlyingArgumentType.SetPropertyValue("IgnoreCase", Argument, value);
            }
        }
    }
}