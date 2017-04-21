using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CommandLineParser.Arguments;
using CommandLineParser.Compatiblity;
using CommandLineParser.Exceptions;
using CommandLineParser.Validation;
using CommandLineParser.Extensions;

namespace CommandLineParser
{
    /// <summary>
    /// CommandLineParser allows user to define command line arguments and then parse
    /// the arguments from the command line.
    /// </summary>
    public class CommandLineParser
    {
        #region fields

        private readonly OutputFormatter _outputFormatter;

        private Dictionary<string, Argument> _lookupDictionary;

        readonly Dictionary<string, Argument> _ignoreCaseLookupDirectory = new Dictionary<string, Argument>();

        internal string[] _argsNotParsed;

        private static readonly Regex lettersOnly = new Regex("^[a-zA-Z]$");

        #endregion

        #region constructor 

        public CommandLineParser()
        {
            _outputFormatter = new OutputFormatter(this);
        }

        #endregion

        #region properties 

        /// <summary>
        /// Defined command line arguments
        /// </summary>
        public List<Argument> Arguments { get; set; } = new List<Argument>();

        /// <summary>
        /// Set of <see cref="ArgumentCertification">certifications</see> - certifications can be used to define 
        /// which argument combinations are allowed and such type of validations. 
        /// </summary>
        /// <seealso cref="CheckArgumentCertifications"/>
        /// <seealso cref="ArgumentCertification"/>
        /// <seealso cref="ArgumentGroupCertification"/>
        /// <seealso cref="DistinctGroupsCertification"/>
        public List<ArgumentCertification> Certifications { get; set; } = new List<ArgumentCertification>();

        /// <summary>
        /// Allows more specific definition of additional arguments 
        /// (arguments after those with - and -- prefix).
        /// </summary>
        public AdditionalArgumentsSettings AdditionalArgumentsSettings { get; } = new AdditionalArgumentsSettings();

        /// <summary>
        /// Text printed in the beginning of 'show usage'
        /// </summary>
        public string ShowUsageHeader { get; set; }

        /// <summary>
        /// Text printed in the end of 'show usage'
        /// </summary>
        public string ShowUsageFooter { get; set; }

        /// <summary>
        /// Arguments that directly invoke <see cref="ShowUsage()"/>. By default this is --help and /?.
        /// </summary>
        public IList<string> ShowUsageCommands { get; set; } = new List<string> { "--help", "/?", "/help" };

        /// <summary>
        /// When set to true, usage help is printed on the console when command line is without arguments.
        /// Default is false. 
        /// </summary>
        public bool ShowUsageOnEmptyCommandline { get; set; }

        /// <summary>
        /// When set to true, <see cref="MandatoryArgumentNotSetException"/> is thrown when some of the non-optional argument
        /// is not found on the command line. Default is true.
        /// See: <see cref="Argument.Optional"/>
        /// </summary>
        public bool CheckMandatoryArguments { get; set; } = true;

        /// <summary>
        /// When set to true, arguments are certified (using set of <see cref="Certifications"/>) after parsing. 
        /// Default is true.
        /// </summary>
        public bool CheckArgumentCertifications { get; set; } = true;

        /// <summary>
        /// When set to true (default) <see cref="SwitchArgument">switch arguments</see> can be grouped on the command line. 
        /// (e.g. -a -b -c can be written as -abc). When set to false and such a group is found, <see cref="CommandLineFormatException"/> is thrown.
        /// </summary>
        public bool AllowShortSwitchGrouping { get; set; } = true;

        /// <summary>
        /// Allows arguments in /a and /arg format
        /// </summary>
        public bool AcceptSlash { get; set; } = true;

        /// <summary>
        /// Allows arguments in -a and --arg format
        /// </summary>
        public bool AcceptHyphen { get; set; } = true;

        /// <summary>
        /// Argument names case insensitive (--OUTPUT or --output are treated equally)
        /// </summary>
        public bool IgnoreCase { get; set; }

        /// <summary>
        /// When set to true, values of <see cref="ValueArgument{TValue}"/> are separeted by space, 
        /// otherwise, they are separeted by equal sign and enclosed in quotation marks
        /// </summary>
        /// <example>
        /// --output="somefile.txt"
        /// </example>
        public bool AcceptEqualSignSyntaxForValueArguments { get; set; }

        public bool PreserveValueQuotesForEqualsSignSyntax { get; set; }

        public char[] EqualsSignSyntaxValuesSeparators { get; set; } = { ',', ';' };

        /// <summary>
        /// Value is set to true after parsing finishes successfuly 
        /// </summary>
        public bool ParsingSucceeded { get; private set; }

        #endregion 

        /// <summary>
        /// Fills lookup dictionaries with arguments names and aliases 
        /// </summary>
        private void InitializeArgumentLookupDictionaries()
        {
            _lookupDictionary = new Dictionary<string, Argument>();
            foreach (Argument argument in Arguments)
            {
                if (argument.ShortName.HasValue)
                {
                    _lookupDictionary.Add(argument.ShortName.Value.ToString(), argument);
                }
                _lookupDictionary.AddUnderKeys(argument, argument.ShortAliases.Select(c => c.ToString()));
                if (!string.IsNullOrEmpty(argument.LongName))
                {
                    _lookupDictionary.Add(argument.LongName, argument);
                }
                _lookupDictionary.AddUnderKeys(argument, argument.LongAliases);
            }

            _ignoreCaseLookupDirectory.Clear();
            if (IgnoreCase)
            {
                foreach (KeyValuePair<string, Argument> keyValuePair in _lookupDictionary)
                {
                    var icString = keyValuePair.Key.ToUpper();
                    if (_ignoreCaseLookupDirectory.ContainsKey(icString))
                    {
                        throw new ArgumentException("Clash in ignore case argument names: " + icString);
                    }
                    _ignoreCaseLookupDirectory.Add(icString, keyValuePair.Value);
                }
            }
        }

        /// <summary>
        /// Resolves arguments from the command line and calls <see cref="Argument.Parse"/> on each argument. 
		/// Additional arguments are stored in AdditionalArgumentsSettings.AdditionalArguments 
		/// if AdditionalArgumentsSettings.AcceptAdditionalArguments is set to true. 
        /// </summary>
        /// <exception cref="CommandLineFormatException">Command line arguments are not in correct format</exception>
        /// <param name="args">Command line arguments</param>
        public void ParseCommandLine(string[] args)
        {
            ParsingSucceeded = false;
            Arguments.ForEach(action => action.Init());
            List<string> argsList = new List<string>(args);
            InitializeArgumentLookupDictionaries();
            ExpandValueArgumentsWithEqualSigns(argsList);
            ExpandShortSwitches(argsList);
            AdditionalArgumentsSettings.AdditionalArguments = new string[0];

            _argsNotParsed = args;

            if ((args.Length == 0 && ShowUsageOnEmptyCommandline) ||
                (args.Length == 1 && ShowUsageCommands.Contains(args[0])))
            {
                ShowUsage();
                return;
            }

            if (args.Length > 0)
            {
                int argIndex;

                for (argIndex = 0; argIndex < argsList.Count;)
                {
                    string curArg = argsList[argIndex];
                    Argument argument = ParseArgument(curArg);
                    if (argument == null)
                        break;

                    argument.Parse(argsList, ref argIndex);
                    argument.UpdateBoundObject();
                }

                ParseAdditionalArguments(argsList, argIndex);
            }

            foreach (Argument argument in Arguments)
            {
                if (argument is IArgumentWithDefaultValue && !argument.Parsed)
                {
                    argument.UpdateBoundObject();
                }
            }

            PerformMandatoryArgumentsCheck();
            PerformCertificationCheck();
            ParsingSucceeded = true; 
        }

        /// <summary>
        /// Searches <paramref name="parsingTarget"/> for fields with 
        /// <see cref="ArgumentAttribute">ArgumentAttributes</see> or some of its descendats. Adds new argument
        /// for each such a field and defines binding of the argument to the field. 
        /// Also adds <see cref="ArgumentCertification"/> object to <see cref="Certifications"/> collection 
        /// for each <see cref="ArgumentCertificationAttribute"/> of <paramref name="parsingTarget"/>.
        /// </summary>
        /// <seealso cref="Argument.Bind"/>
        /// <param name="parsingTarget">object where you with some ArgumentAttributes</param>
        public void ExtractArgumentAttributes(object parsingTarget)
        {
            Type targetType = parsingTarget.GetType();

            MemberInfo[] fields = targetType.GetFields();

            MemberInfo[] properties = targetType.GetProperties();

            List<MemberInfo> fieldAndProps = new List<MemberInfo>(fields);
            fieldAndProps.AddRange(properties);

            foreach (MemberInfo info in fieldAndProps)
            {
                var attrs = info.GetCustomAttributes(typeof(ArgumentAttribute), true).ToArray();

                if (attrs.Length == 1 && attrs[0] is ArgumentAttribute)
                {
                    Arguments.Add(((ArgumentAttribute)attrs[0]).Argument);
                    ((ArgumentAttribute)attrs[0]).Argument.Bind =
                        new FieldArgumentBind(parsingTarget, info.Name);
                }
            }

            object[] typeAttrs = targetType.GetTypeInfo().GetCustomAttributes(typeof(ArgumentCertificationAttribute), true).ToArray();
            foreach (object certificationAttr in typeAttrs)
            {
                Certifications.Add(((ArgumentCertificationAttribute)certificationAttr).Certification);
            }
        }

        /// <summary>
        /// Parses one argument on the command line, lookups argument in <see cref="Arguments"/> using 
        /// lookup dictionaries.
        /// </summary>
        /// <param name="curArg">argument string (including '-' or '--' prefixes)</param>
        /// <returns>Look-uped Argument class</returns>
        /// <exception cref="CommandLineFormatException">Command line is in the wrong format</exception>
        /// <exception cref="UnknownArgumentException">Unknown argument found.</exception>
        private Argument ParseArgument(string curArg)
        {
            if (curArg[0] == '-')
            {
                if (AcceptHyphen)
                {
                    if (curArg.Length > 1)
                    {
                        string argName;
                        if (curArg[1] == '-')
                        {
                            //long name
                            argName = curArg.Substring(2);
                            if (argName.Length == 1)
                            {
                                throw new CommandLineFormatException(String.Format(
                                    Messages.EXC_FORMAT_SHORTNAME_PREFIX, argName));
                            }

                        }
                        else
                        {
                            //short name
                            argName = curArg.Substring(1);
                            if (argName.Length != 1)
                            {
                                throw new CommandLineFormatException(
                                    String.Format(Messages.EXC_FORMAT_LONGNAME_PREFIX, argName));
                            }
                        }

                        Argument argument = LookupArgument(argName);
                        if (argument != null) return argument;
                        else
                            throw new UnknownArgumentException(string.Format(Messages.EXC_ARG_UNKNOWN, argName), argName);
                    }
                    else
                    {
                        throw new CommandLineFormatException(Messages.EXC_FORMAT_SINGLEHYPHEN);
                    }
                }
                else
                    return null;
            }
            else if (curArg[0] == '/')
            {
                if (AcceptSlash)
                {
                    if (curArg.Length > 1)
                    {
                        if (curArg[1] == '/')
                        {
                            throw new CommandLineFormatException(Messages.EXC_FORMAT_SINGLESLASH);
                        }
                        string argName = curArg.Substring(1);
                        Argument argument = LookupArgument(argName);
                        if (argument != null) return argument;
                        else throw new UnknownArgumentException(string.Format(Messages.EXC_ARG_UNKNOWN, argName), argName);
                    }
                    else
                    {
                        throw new CommandLineFormatException(Messages.EXC_FORMAT_DOUBLESLASH);
                    }
                }
                else
                    return null;
            }
            else
                /*
                 * curArg does not start with '-' character and therefore it is considered additional argument.
                 * Argument parsing ends here.
                 */
                return null;
        }

        /// <summary>
        /// Checks whether or non-optional arguments were defined on the command line. 
        /// </summary>
        /// <exception cref="MandatoryArgumentNotSetException"><see cref="Argument.Optional">Non-optional</see> argument not defined.</exception>
        /// <seealso cref="CheckMandatoryArguments"/>, <seealso cref="Argument.Optional"/>
        private void PerformMandatoryArgumentsCheck()
        {
            Arguments.ForEach(arg =>
            {
                if (!arg.Optional && !arg.Parsed)
                    throw new MandatoryArgumentNotSetException(
                        string.Format(Messages.EXC_MISSING_MANDATORY_ARGUMENT, arg.Name), arg.Name);
            });
        }

        /// <summary>
        /// Performs certifications
        /// </summary>
        private void PerformCertificationCheck()
        {
            Certifications.ForEach(c => c.Certify(this));
        }

        /// <summary>
        /// Parses the rest of the command line for additional arguments
        /// </summary>
        /// <param name="argsList">list of thearguments</param>
        /// <param name="i">index of the first additional argument in <paramref name="argsList"/></param>
        /// <exception cref="CommandLineFormatException">Additional arguments found, but they are 
        /// not accepted</exception>
        private void ParseAdditionalArguments(List<string> argsList, int i)
        {
            if (AdditionalArgumentsSettings.AcceptAdditionalArguments)
            {
                AdditionalArgumentsSettings.AdditionalArguments = new string[argsList.Count - i];
                if (i < argsList.Count)
                {
                    Array.Copy(argsList.ToArray(), i, AdditionalArgumentsSettings.AdditionalArguments, 0, argsList.Count - i);
                }
                AdditionalArgumentsSettings.ProcessArguments();
            }
            else if(i < argsList.Count)
            {
                // only throw when there are any additional arguments
                throw new CommandLineFormatException(
                    Messages.EXC_ADDITIONAL_ARGUMENTS_FOUND);
            }
        }

        /// <summary>
        /// If <see cref="AllowShortSwitchGrouping"/> is set to true,  each group of switch arguments (e. g. -abcd) 
        /// is expanded into full format (-a -b -c -d) in the list.
        /// </summary>
        /// <exception cref="CommandLineFormatException">Argument of type differnt from SwitchArgument found in one of the groups. </exception>
        /// <param name="argsList">List of arguments</param>
        /// <exception cref="CommandLineFormatException">Arguments that are not <see cref="SwitchArgument">switches</see> found 
        /// in a group.</exception>
        /// <seealso cref="AllowShortSwitchGrouping"/>
        private void ExpandShortSwitches(IList<string> argsList)
        {
            if (AllowShortSwitchGrouping)
            {
                for (int i = 0; i < argsList.Count; i++)
                {
                    string arg = argsList[i];
                    if (arg.Length > 2)
                    {
                        if (arg[0] == '/' && arg[1] != '/' && AcceptSlash && _lookupDictionary.ContainsKey(arg.Substring(1)))
                            continue;
                        if (arg.Contains('='))
                            continue;
                        if (ShowUsageCommands.Contains(arg))
                            continue;

                        char sep = arg[0];
                        if ((arg[0] == '-' && AcceptHyphen && lettersOnly.IsMatch(arg.Substring(1)))
                            || (arg[0] == '/' && AcceptSlash && lettersOnly.IsMatch(arg.Substring(1))))
                        {
                            argsList.RemoveAt(i);
                            //arg ~ -xyz
                            foreach (char c in arg.Substring(1))
                            {
                                if (_lookupDictionary.ContainsKey(c.ToString()) && !(_lookupDictionary[c.ToString()] is SwitchArgument))
                                {
                                    throw new CommandLineFormatException(
                                        string.Format(Messages.EXC_BAD_ARG_IN_GROUP, c));
                                }

                                argsList.Insert(i, sep.ToString() + c);
                                i++;
                            }
                        }
                    }
                }
            }
        }        

        private void ExpandValueArgumentsWithEqualSigns(IList<string> argsList)
        {
            if (AcceptEqualSignSyntaxForValueArguments)
            {
                for (int i = 0; i < argsList.Count; i++)
                {
                    string arg = argsList[i];
				
                    Regex r = new Regex("([^=]*)=(.*)");
                    if (AcceptEqualSignSyntaxForValueArguments && r.IsMatch(arg))
                    {
                        Match m = r.Match(arg);
                        string argNameWithSep = m.Groups[1].Value;
                        string argName = argNameWithSep;
                        while (argName.StartsWith("-") && AcceptHyphen)
                            argName = argName.Substring(1);
                        while (argName.StartsWith("/") && AcceptSlash)
                            argName = argName.Substring(1);
                        string argValue = m.Groups[2].Value;
                        if (!PreserveValueQuotesForEqualsSignSyntax && !string.IsNullOrEmpty(argValue) && argValue.StartsWith("\"") && argValue.EndsWith("\""))
                        {
                            argValue = argValue.Trim('"');
                        }

                        Argument argument = LookupArgument(argName);
                        if (argument is IValueArgument)
                        {
                            argsList.RemoveAt(i);
                            if (argument.AllowMultiple)
                            {
                                var splitted = argValue.Split(EqualsSignSyntaxValuesSeparators);
                                foreach (var singleValue in splitted)
                                {
                                    argsList.Insert(i, argNameWithSep);
                                    i++;
                                    if (!string.IsNullOrEmpty(singleValue))
                                    {
                                        argsList.Insert(i, singleValue);
                                        i++;
                                    }                                    
                                }
                                i--;
                            }
                            else
                            {
                                argsList.Insert(i, argNameWithSep);
                                i++;
                                argsList.Insert(i, argValue);
                            }                            
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns argument of given name
        /// </summary>
        /// <param name="argName">Name of the argument (<see cref="Argument.ShortName"/>, <see cref="Argument.LongName"/>, or alias)</param>
        /// <returns>Found argument or null when argument is not present</returns>
        public Argument LookupArgument(string argName)
        {
            if (_lookupDictionary.ContainsKey(argName))
            {
                return _lookupDictionary[argName];
            }
            if (IgnoreCase && _ignoreCaseLookupDirectory.ContainsKey(argName.ToUpper()))
            {
                return _ignoreCaseLookupDirectory[argName.ToUpper()];
            }
            // argument not found anywhere
            return null;
        }
        
        /// <summary>
        /// <para>
        /// Fills FullDescription of all the difined arguments from a resource file. 
        /// For each argument selects a string from a resource that has the same resource key
        /// as is the currrent value of the argument's FullDescription. 
        /// </para>
        /// <para>
        /// This way the value of FullDescription's can be set to keys and these keys are replaced by 
        /// the resource values when the method is called. 
        /// </para>
        /// </summary>
        /// <param name="resource">The resource.</param>
        public void FillDescFromResource(IResource resource)
        {
            foreach (Argument argument in Arguments)
            {
                if (!string.IsNullOrEmpty(argument.FullDescription))
                {
                    string ld = resource.ResourceManager.GetString(argument.FullDescription);
                    argument.FullDescription = ld;
                }
            }

            foreach (Argument argument in AdditionalArgumentsSettings.TypedAdditionalArguments)
            {
                if (!string.IsNullOrEmpty(argument.FullDescription))
                {
                    string ld = resource.ResourceManager.GetString(argument.FullDescription);
                    argument.FullDescription = ld;
                }
            }
        }

        public void Clear()
        {
            Arguments.Clear();
            Certifications.Clear();
            _lookupDictionary.Clear();
            _ignoreCaseLookupDirectory.Clear();
        }
            
        #region delegated to output formatter 

        /// <summary>
        /// Prints arguments information and usage information to the console. 
        /// </summary>
        public void ShowUsage()
        {
            _outputFormatter.PrintUsage(Console.Out);
        }

        /// <summary>
        /// Prints arguments information and usage information to
        /// the <paramref name="outputStream"/>. 
        /// </summary>
        public void PrintUsage(TextWriter outputStream)
        {
            _outputFormatter.PrintUsage(outputStream);
        }

        /// <summary>
        /// Prints values of parsed arguments. Can be used for debugging. 
        /// </summary>
        public void ShowParsedArguments(bool showOmittedArguments = false)
        {
            _outputFormatter.ShowParsedArguments(showOmittedArguments);
        }

        #endregion 
    }
}
