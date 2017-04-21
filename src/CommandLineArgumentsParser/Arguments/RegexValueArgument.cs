using System;
using System.Text.RegularExpressions;
using CommandLineParser.Compatiblity;
using CommandLineParser.Exceptions;

namespace CommandLineParser.Arguments
{
    /// <summary>
    /// Use RegexValueArgument for an argument whose value must match a regular expression. 
    /// </summary>
    public class RegexValueArgument : CertifiedValueArgument<string>
    {
        #region constructor
        
        /// <summary>
        /// Creates new argument with a <see cref="Argument.ShortName">short name</see>,
        /// <see cref="Argument.LongName">long name</see> and <see cref="Argument.Description">description</see>.
        /// </summary>
        /// <param name="shortName">Short name of the argument</param>
        /// <param name="longName">Long name of the argument </param>
        /// <param name="description">description of the argument</param>
        /// <param name="regex">regular expressin which the value must match</param>
        public RegexValueArgument(char? shortName = null, string longName = null, string description = null, Regex regex = null) : base(shortName, longName, description)
        {
            Regex = regex;
        }
        #endregion

        #region properties 

        /// <summary>
        /// Regular expression which the value must match 
        /// </summary>
        public Regex Regex { get; set; }
        
        /// <summary>
        /// Sample value that would be displayed to the user as a suggestion when 
        /// the user enters a wrong value. 
        /// </summary>
        public string SampleValue { get; set; }

        #endregion 

        protected override void Certify(string value)
        {
            // override the Certify method to validate value against regex
            if (Regex != null)
            {
                if (!Regex.IsMatch(value))
                {
                    if (SampleValue == null)
                    {
                        throw new CommandLineArgumentOutOfRangeException(
                            string.Format("Argument '{0}' does not match the regex pattern '{1}'.", value, Regex), Name);
                    }
                    else
                    {
                        throw new CommandLineArgumentOutOfRangeException(
                            string.Format("Argument '{0}' does not match the regex pattern '{1}'. An example of a valid value would be '{2}'.", value, Regex, SampleValue), Name);
                    }                    
                }
            }
        }
    }

    /// <summary>
    /// <para>
    /// Attribute for declaring a class' field a <see cref="RegexValueArgumentAttribute"/> and 
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
    public sealed class RegexValueArgumentAttribute : ArgumentAttribute
    {
        /// <summary>
        /// Creates new instance of RegexValueArgument. RegexValueArgumentAttribute
        /// uses underlying <see cref="RegexValueArgument"/>.
        /// </summary>
        /// <param name="pattern">Regex pattern</param>
        public RegexValueArgumentAttribute(string pattern)
            : base(typeof(RegexValueArgument))
        {
            Pattern = pattern;
        }

        /// <summary>
        /// Regular expression which the value must match 
        /// </summary>
        public string Pattern
        {
            get
            {
                return $"{_underlyingArgumentType.GetPropertyValue<Regex>("Regex", Argument)}";
            }
            set
            {
                _underlyingArgumentType.SetPropertyValue("Regex", Argument, new Regex(value));
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
        /// Sample value that would be displayed to the user as a suggestion when 
        /// the user enters a wrong value. 
        /// </summary>
        public string SampleValue
        {
            get
            {
                return _underlyingArgumentType.GetPropertyValue<string>("SampleValue", Argument);
            }
            set
            {
                _underlyingArgumentType.SetPropertyValue("SampleValue", Argument, value);
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
